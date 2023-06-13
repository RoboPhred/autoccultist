namespace AutoccultistNS
{
    using System;
    using AutoccultistNS.Actor;
    using SecretHistories.Entities;
    using SecretHistories.Fucine;
    using SecretHistories.UI;

    public static class AutoccultistSettings
    {
        // private static readonly string SettingActorDelay = "autoccultist_actor_delay";
        private static readonly string SettingActorSort = "autoccultist_actor_sort";

        private static SortSettingReceiver sortSettingReceiver;
        // private static DelaySettingReceiver delaySettingReceiver;

        public static TimeSpan ActionDelay
        {
            get
            {
                // return AutoccultistActor.ActionDelay;
                return TimeSpan.FromSeconds(0.25);
            }
        }

        public static void Initialize()
        {
            sortSettingReceiver = new SortSettingReceiver();
            // delaySettingReceiver = new DelaySettingReceiver();
        }

        // private class DelaySettingReceiver : ISettingSubscriber
        // {
        //     public DelaySettingReceiver()
        //     {
        //         var setting = Watchman.Get<Compendium>().GetEntityById<Setting>(AutoccultistSettings.SettingActorDelay);
        //         this.WhenSettingUpdated(setting.CurrentValue);
        //         setting.AddSubscriber(this);
        //     }

        //     public void BeforeSettingUpdated(object newValue)
        //     {
        //     }

        //     public void WhenSettingUpdated(object newValue)
        //     {
        //         var value = newValue as float?;
        //         if (value != null)
        //         {
        //             AutoccultistActor.ActionDelay = TimeSpan.FromSeconds(value.Value);
        //         }
        //     }
        // }

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
                Autoccultist.Instance.LogTrace($"SettingActorSort: {newValue}");
                var value = newValue as bool?;
                AutoccultistActor.SortTableOnIdle = value ?? false;
            }
        }
    }
}
