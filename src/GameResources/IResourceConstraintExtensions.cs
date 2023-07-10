namespace AutoccultistNS.GameResources
{
    public static class IResourceConstraintExtensions
    {
        public static T Resolve<T>(this IResourceConstraint<T> constraint)
            where T : class
        {
            return Resource.Of<T>().ResolveConstraint(constraint);
        }
    }
}
