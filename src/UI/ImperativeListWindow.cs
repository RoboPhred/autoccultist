namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;
    using AutoccultistNS.Tokens;
    using SecretHistories.Commands;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine;

    public class ImperativeListWindow : AbstractWindow
    {
        private readonly TimeSpan updateInterval = TimeSpan.FromSeconds(.5);
        private readonly Dictionary<IImperative, ImperativeUIElements> imperativeUIs = new();

        private DateTime lastUpdate = DateTime.MinValue;

        protected override int DefaultWidth => 500;
        protected override int DefaultHeight => 800;

        protected override void OnAwake()
        {
            base.OnAwake();
            this.Title = "Automations";
            this.Icon.AddImage().SetSprite(ResourcesManager.GetSpriteForUI("autoccultist_gears"));

            this.RebuildContent();
        }

        protected override void OnUpdate()
        {
            if (DateTime.UtcNow - this.lastUpdate < this.updateInterval)
            {
                return;
            }

            this.lastUpdate = DateTime.UtcNow;

            var state = GameStateProvider.Current;

            var orderedOps =
                from pair in this.imperativeUIs
                let imperative = pair.Key
                let canExecute = imperative.IsConditionMet(state) && !imperative.IsSatisfied(state)
                let isRunning = NucleusAccumbens.CurrentImperatives.Contains(imperative)
                orderby canExecute descending, imperative.Name
                select new { Imperative = imperative, IsRunning = isRunning, CanExecute = canExecute, Elements = pair.Value };

            foreach (var pair in orderedOps)
            {
                pair.Elements.Row.GameObject.transform.SetAsLastSibling();
                pair.Elements.StartButton.SetEnabled(pair.CanExecute);
                pair.Elements.StartButton.SetActive(!pair.IsRunning);
                pair.Elements.RunningIcon.SetActive(pair.IsRunning);
            }
        }

        private void RebuildContent()
        {
            this.Content.Clear();

            this.Content.AddScrollRegion("ScrollRegion")
                .SetVertical()
                .AddContent(
                    mountPoint =>
                    {
                        foreach (var imperative in Library.Arcs.OfType<IImperative>().Concat(Library.Goals))
                        {
                            this.BuildImperativeRow(imperative, mountPoint);
                        }
                    })
                .ScrollToVertical(1);
        }

        private void BuildImperativeRow(IImperative imperative, WidgetMountPoint mountPoint)
        {
            ImageWidget runningIcon = null;
            IconButtonWidget startButton = null;
            var row = mountPoint.AddHorizontalLayoutGroup($"imperative_${imperative.Name}")
                .SpreadChildrenVertically()
                .SetPadding(10, 5)
                .AddContent(mountPoint =>
                {
                    var isRunning = NucleusAccumbens.CurrentImperatives.Contains(imperative);

                    var nameElement = mountPoint.AddText("Recipe")
                        .SetFontSize(14)
                        .SetMinFontSize(10)
                        .SetMaxFontSize(14)
                        .SetExpandWidth()
                        .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
                        .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                        .SetText(imperative.Name);

                    mountPoint.AddSizingLayout("Spacer")
                        .SetMinWidth(10)
                        .SetExpandWidth();

                    runningIcon = mountPoint.AddImage("ActiveIcon")
                        .SetSprite("autoccultist_situation_automation_badge")
                        .SetActive(isRunning)
                        .SetMinWidth(35)
                        .SetMinHeight(35)
                        .SetPreferredWidth(35)
                        .SetPreferredHeight(35);

                    startButton = mountPoint.AddIconButton("StartButton")
                        .SetMinWidth(35)
                        .SetMinHeight(35)
                        .SetPreferredWidth(35)
                        .SetPreferredHeight(35)
                        .SetActive(!isRunning)
                        .Disable()
                        .SetBackground()
                        .SetSprite("autoccultist_play_icon")
                        .OnClick(() => this.OnStartImperativeClick(imperative));
                });

            this.imperativeUIs.Add(imperative, new ImperativeUIElements
            {
                Row = row,
                StartButton = startButton,
                RunningIcon = runningIcon,
            });
        }

        private void OnStartImperativeClick(IImperative imperative)
        {
            if (!imperative.IsConditionMet(GameStateProvider.Current))
            {
                return;
            }

            var tabletop = GameAPI.TabletopSphere;

            // NucleusAccumbens.AddImperative(imperative);

            // TODO: Use drop zone.
            var location = new TokenLocation(Vector3.zero, tabletop);
            var quickDuration = Watchman.Get<Compendium>().GetSingleEntity<Dictum>().DefaultQuickTravelDuration;
            var creationCommand = new AutomationCreationCommand(imperative.Id);
            var zoneTokenCreationCommand = new TokenCreationCommand(creationCommand, location).WithDestination(location, quickDuration);
            zoneTokenCreationCommand.Execute(Context.Unknown(), tabletop);

            this.lastUpdate = DateTime.MinValue;
        }

        private class ImperativeUIElements
        {
            public HorizontalLayoutGroupWidget Row { get; set; }

            public IconButtonWidget StartButton { get; set; }

            public ImageWidget RunningIcon { get; set; }
        }
    }
}
