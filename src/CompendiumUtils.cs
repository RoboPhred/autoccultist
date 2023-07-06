namespace AutoccultistNS
{
    using SecretHistories.Entities;
    using SecretHistories.UI;

    public static class CompendiumUtils
    {
        public static bool IsValidElement(string id)
        {
            var element = Watchman.Get<Compendium>().GetEntityById<Element>(id);
            if (element == null)
            {
                return false;
            }

            if (element.IsHidden)
            {
                Autoccultist.LogWarn($"Element {id} is hidden.");
            }

            return true;
        }
    }
}
