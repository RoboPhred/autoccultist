namespace AutoccultistNS
{
    using System;
    using AutoccultistNS.Actor;
    using SecretHistories.Entities;
    using SecretHistories.Fucine;
    using SecretHistories.UI;

    public static class AutoccultistSettings
    {
        private static readonly string SettingActorSort = "autoccultist_actor_sort";

        private static SortSettingReceiver sortSettingReceiver;

        public static TimeSpan ActionDelay
        {
            get
            {
                return TimeSpan.FromSeconds(.25);
            }
        }

        public static bool SortTableOnIdle { get; private set; }

        public static void Initialize()
        {
            sortSettingReceiver = new SortSettingReceiver();
        }

        private class SortSettingReceiver : ISettingSubscriber
        {
            public SortSettingReceiver()
            {
                var setting = Watchman.Get<Compendium>().GetEntityById<Setting>(AutoccultistSettings.SettingActorSort);
                this.WhenSettingUpdated(setting.CurrentValue);
                setting.AddSubscriber(this);
            }

            public void BeforeSettingUpdated(object newValue)
            {
            }

            public void WhenSettingUpdated(object newValue)
            {
                Autoccultist.LogTrace($"SettingActorSort: {newValue}");
                var value = newValue as bool?;
                SortTableOnIdle = value ?? false;
            }
        }
    }
}
