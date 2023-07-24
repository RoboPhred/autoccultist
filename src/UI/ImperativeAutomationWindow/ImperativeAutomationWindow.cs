namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;

    public class ImperativeAutomationWindow : AbstractWindow
    {
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
        private DateTime lastUpdate = DateTime.MinValue;
        private int lastHash = 0;

        public IImperative Imperative { get; private set; }

        protected override int DefaultWidth => 400;

        protected override int DefaultHeight => 600;

        public void Attach(IImperative imperative)
        {
            this.Imperative = imperative;
            this.Title = imperative.Name;

            this.lastHash = 0;
            this.lastUpdate = DateTime.MinValue;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            this.Icon.AddImage().SetSprite(ResourcesManager.GetSpriteForUI("autoccultist_gears"));
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (!this.IsOpen)
            {
                return;
            }

            if (DateTime.UtcNow - this.lastUpdate < UpdateInterval)
            {
                return;
            }

            this.lastUpdate = DateTime.UtcNow;

            var state = GameStateProvider.Current;
            var stateHash = state.GetHashCode();
            if (stateHash == this.lastHash)
            {
                return;
            }

            this.lastHash = stateHash;
            this.RebuildContent();
        }

        private void RebuildContent()
        {
            this.Content.Clear();
            this.Footer.Clear();

            if (this.Imperative == null)
            {
                return;
            }

            this.Content.AddScrollRegion("ScrollRegion")
                .SetExpandWidth()
                .SetVertical()
                .AddContent(mountPoint =>
                {
                    mountPoint.AddVerticalLayoutGroup("Goals")
                        .SetExpandWidth()
                        .SetSpreadChildrenVertically()
                        .SetSpacing(10)
                        .SetPadding(10, 0)
                        .AddContent(mountPoint =>
                        {
                            foreach (var child in this.GetImperativeChildren(this.Imperative))
                            {
                                this.AddImperativeRow(mountPoint, child);
                            }
                        });
                });

            this.Footer.AddHorizontalLayoutGroup("FooterButtons")
                .SetExpandWidth()
                .SetChildAlignment(UnityEngine.TextAnchor.MiddleRight)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddTextButton("AbortButton")
                        .OnClick(() => NucleusAccumbens.RemoveImperative(this.Imperative))
                        .SetText("Abort");
                });
        }

        private void AddImperativeRow(WidgetMountPoint mountPoint, IImperative imperative)
        {
            mountPoint.AddVerticalLayoutGroup($"Imperative_{imperative.Id}")
                .SetSpacing(10)
                .SetPadding(10, 0, 0, 0)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddText("Label")
                        .SetExpandWidth()
                        .SetFontSize(20)
                        .SetText(imperative.Name);

                    var children = this.GetImperativeChildren(imperative).ToArray();
                    if (children.Length > 0)
                    {
                        foreach (var child in children)
                        {
                            this.AddImperativeRow(mountPoint, child);
                        }
                    }
                });
        }

        private IEnumerable<IImperative> GetImperativeChildren(IImperative imperative)
        {
            return imperative.GetActiveChildren(GameStateProvider.Current).Where(this.ShouldDisplayImperative).Select(this.TryFlatten);
        }

        private IImperative TryFlatten(IImperative imperative)
        {
            var children = imperative.GetActiveChildren(GameStateProvider.Current).Where(this.ShouldDisplayImperative).ToArray();
            if (children.Length == 1)
            {
                return this.TryFlatten(children[0]);
            }

            return imperative;
        }

        private bool ShouldDisplayImperative(IImperative imperative)
        {
            return imperative is IGoal || imperative is IMotivationConfig;
        }
    }
}
