using Autoccultist.Brain.Util;
using System;
using System.Collections.Generic;

namespace Autoccultist.Brain.Config
{
    /**
     * A TaskSet represents a singular, overarching goal that is usually comprised of smaller Goals, Imperatives, or TaskSets.
     * Like a Goal, the "aspiration" of a TaskSet can change depending on the board state.
     * 
     * In addition to being an ITask, a TaskSet has the following members:
     * CriticalTasks: Goals that must be satisfied and Imperatives that must be non-required before the TaskSet will suggest any other action.
     * GoalTasks: Tasks that must be completed for the TaskSet to be satisfied.
     * MaintenanceTasks: Tasks that will be done only if there are spare resources available.
     * Subtasks: Additional TaskSets 
     */
    class TaskSet : ITask
    {
        public string Name { get; }

        public List<ITask> CriticalTasks { get; }
        public List<ITask> GoalTasks { get; }
        public List<ITask> MaintenanceTasks { get; }

        public TaskSet(String name)
        {
            Name = name;
            CriticalTasks = new List<ITask>();
            GoalTasks = new List<ITask>();
            MaintenanceTasks = new List<ITask>();
        }
        
        public bool ShouldExecute(IGameState state)
        {
            foreach (ITask task in CriticalTasks)
            {
                if (task.ShouldExecute(state)) return true;
            }

            foreach (ITask task in GoalTasks)
            {
                if (task.ShouldExecute(state)) return true;
            }

            foreach (ITask task in MaintenanceTasks)
            {
                if (task.ShouldExecute(state)) return true;
            }
            return false;
        }

        public bool IsSatisfied(IGameState state)
        {
            foreach (ITask t in CriticalTasks)
            {
                if(!t.IsSatisfied(state)) { return false; }
            }
            foreach (ITask t in GoalTasks)
            {
                if(!t.IsSatisfied(state)) { return false; }
            }
            foreach (ITask t in MaintenanceTasks)
            {
                if(!t.IsSatisfied(state)) { return false; }
            }
            return true;
        }

        public IList<Imperative> GetImperatives() => throw new NotImplementedException();
    }
}
