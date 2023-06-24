using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoccultistNS.Resources
{
    public static class Resource
    {
        private static readonly Dictionary<Type, object> resources = new();

        public static Resource<T> Of<T>() where T : class
        {
            if (!resources.ContainsKey(typeof(T)))
            {
                resources[typeof(T)] = new Resource<T>();
            }

            return (Resource<T>)resources[typeof(T)];
        }

        public static void ClearAll()
        {
            foreach (var resource in resources.Values)
            {
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            resources.Clear();
        }
    }

    public class Resource<T> : IDisposable where T : class
    {
        private readonly HashSet<IResourceConstraint<T>> constraints = new();

        public void AddConstraint(IResourceConstraint<T> constraint)
        {
            if (constraints.Contains(constraint))
            {
                return;
            }

            if (!TryAddConstraint(constraint))
            {
                throw new Exception("Constraint cannot be satisfied.");
            }
        }

        public bool TryAddConstraint(IResourceConstraint<T> constraint)
        {
            // Verify that the constraint can be met given our other constraints.
            var resolved = this.ResolveConstraint(constraint);
            if (resolved == null)
            {
                return false;
            }

            this.constraints.Add(constraint);
            constraint.Disposed += this.OnConstraintDisposed;
            return true;
        }

        public T ResolveConstraint(IResourceConstraint<T> constraint)
        {
            var resolved = this.GetConstrainedResources();
            if (this.constraints.Contains(constraint))
            {
                if (resolved.ContainsKey(constraint))
                {
                    return resolved[constraint];
                }

                return null;
            }
            else
            {
                return constraint.GetCandidates().FirstOrDefault(x => !resolved.Values.Contains(x));
            }
        }

        public bool IsAvailable(T resource)
        {
            return !this.GetConstrainedResources().Values.Contains(resource);
        }

        private IReadOnlyDictionary<IResourceConstraint<T>, T> GetConstrainedResources()
        {
            return PerfMonitor.Monitor(nameof(GetConstrainedResources), () =>
            {
                var candidatesByConstraint = this.constraints.ToDictionary(c => c, c => new HashSet<T>(c.GetCandidates()));

                var allCandidates = candidatesByConstraint.SelectMany(c => c.Value).Distinct();
                var weightByCandidate = allCandidates.ToDictionary(c => c, c => candidatesByConstraint.Sum(p => p.Value.Contains(c) ? 1 : 0));

                var choices = new Dictionary<IResourceConstraint<T>, T>();

                foreach (var pair in candidatesByConstraint)
                {
                    var choice = pair.Value.Where(c => weightByCandidate.ContainsKey(c)).OrderBy(c => weightByCandidate[c]).FirstOrDefault();
                    if (choice != null)
                    {
                        // Remove the choice from the options
                        weightByCandidate.Remove(choice);

                        // Mark the card as chosen
                        choices.Add(pair.Key, choice);
                    }

                }

                return choices;
            });
        }

        public void Dispose()
        {
            foreach (var constraint in this.constraints)
            {
                constraint.Disposed -= this.OnConstraintDisposed;
                constraint.Dispose();
            }

            this.constraints.Clear();
        }

        private void OnConstraintDisposed(object sender, EventArgs e)
        {
            var constraint = (IResourceConstraint<T>)sender;
            this.constraints.Remove(constraint);
        }
    }
}
