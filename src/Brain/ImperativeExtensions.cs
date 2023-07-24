using System.Collections.Generic;

namespace AutoccultistNS.Brain
{
    public static class ImperativeExtensions
    {
        public static IEnumerable<IImperative> GetChildrenDeep(this IImperative imperative)
        {
            yield return imperative;
            foreach (var child in imperative.Children)
            {
                foreach (var grandchild in child.GetChildrenDeep())
                {
                    yield return grandchild;
                }
            }
        }
    }
}
