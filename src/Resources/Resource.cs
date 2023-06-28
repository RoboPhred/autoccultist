namespace AutoccultistNS.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Resource
    {
        private static readonly Dictionary<Type, object> Resources = new();

        public static Resource<T> Of<T>()
            where T : class
        {
            if (!Resources.ContainsKey(typeof(T)))
            {
                Resources[typeof(T)] = new Resource<T>();
            }

            return (Resource<T>)Resources[typeof(T)];
        }

        public static void ClearAll()
        {
            foreach (var resource in Resources.Values)
            {
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            Resources.Clear();
        }
    }

    public class Resource<T> : IDisposable
        where T : class
    {
        private readonly HashSet<IResourceConstraint<T>> constraints = new();

        public void AddConstraint(IResourceConstraint<T> constraint)
        {
            if (this.constraints.Contains(constraint))
            {
                return;
            }

            if (!this.TryAddConstraint(constraint))
            {
                throw new Exception("Constraint cannot be satisfied.");
            }
        }

        public IEnumerable<IResourceConstraint<T>> GetConstraints()
        {
            return this.constraints;
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

        public void Dispose()
        {
            foreach (var constraint in this.constraints)
            {
                constraint.Disposed -= this.OnConstraintDisposed;
                constraint.Dispose();
            }

            this.constraints.Clear();
        }

        private IReadOnlyDictionary<IResourceConstraint<T>, T> GetConstrainedResources()
        {
            // FIXME: We should preserve the order returned by GetCandidates, as that is the priority order.
            // FIXME: This is largely the same logic as ICardChooserExtensions.ChooseAll.  This code can be made generic
            // and called from both places.
            var candidatesByConstraint = this.constraints.ToDictionary(c => c, c => new HashSet<T>(c.GetCandidates()));

            var allCandidates = candidatesByConstraint.SelectMany(c => c.Value).Distinct();
            var weightByCandidate = allCandidates.ToDictionary(c => c, c => candidatesByConstraint.Sum(p => p.Value.Contains(c) ? 1 : 0));

            var choices = new Dictionary<IResourceConstraint<T>, T>();

            foreach (var pair in candidatesByConstraint)
            {
                // FIXME: Sort by priority of the constraint.
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
        }

        private void OnConstraintDisposed(object sender, EventArgs e)
        {
            var constraint = (IResourceConstraint<T>)sender;
            this.constraints.Remove(constraint);
        }
    }
}
