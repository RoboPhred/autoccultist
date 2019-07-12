namespace Autoccultist
{
    [BepInEx.BepInPlugin("net.robophreddev.CultistSimulator.Autoccultist", "Autoccultist", "0.0.1")]
    public class AutoccultistMod : BepInEx.BaseUnityPlugin
    {
        private bool isRunning = false;

        public static AutoccultistMod Instance
        {
            get;
            private set;
        }

        void Start()
        {
            Instance = this;
            this.Logger.LogInfo("Autoccultist initialized.");
            this.isRunning = true;
        }

        void Update()
        {
            if (!this.isRunning)
            {
                return;
            }
            GameAPI.DoHeartbeat();
        }

        public void Warn(string message)
        {
            this.Logger.LogWarning(message);
            GameAPI.Notify("Autoccultist Warning", message);
        }

        public void Fatal(string message)
        {
            this.Logger.LogError("Fatal - " + message);
            GameAPI.Notify("Autoccultist Fatal", message);
            this.isRunning = false;
        }
    }
}