namespace Autoccultist.Brain
{
    /// <summary>
    /// Specifies the priority of a task.
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// Specifies a low priority task performing background work
        /// </summary>
        Maintenance = -10,

        /// <summary>
        /// Specifies a priority without any special characteristics.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Specifies a priority for a task that advances the goal
        /// <summary>
        Goal = 10,

        /// <summary>
        /// Specifies a critical priority, which must be done before anything else
        /// <summary>
        Critical = 20,
    }
}
