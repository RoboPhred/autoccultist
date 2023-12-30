namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;
    using Roost.Piebald;
    using UnityEngine;

    public class OperationsListView : IWindowView<SituationAutomationWindow.IWindowContext>
    {
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(.5);

        private readonly Dictionary<OperationConfig, OperationUIElements> operationUIs = new();

        private SituationAutomationWindow.IWindowContext window;
        private ScrollRegionWidget scrollWidget;

        private DateTime lastUpdate = DateTime.MinValue;

        public void Attach(SituationAutomationWindow.IWindowContext window)
        {
            this.window = window;

            // FIXME: We want to let the window expand up to a point then stop.
            // See the nonfunctional ConstrainedLayoutElement
            this.scrollWidget = window.Content.AddScrollRegion("ScrollRegion")
                .SetExpandWidth()
                .SetExpandHeight()
                .SetVertical()
                .AddContent(mountPoint =>
                {
                    foreach (var op in Library.Operations.Where(this.FilterOperation))
                    {
                        this.BuildOperationRow(op, mountPoint);
                    }
                });

            window.Footer.AddHorizontalLayoutGroup("FooterButtons")
                .SetChildAlignment(TextAnchor.MiddleRight)
                .SetPadding(10, 2)
                .SetSpacing(5)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddTextButton("LockoutButton")
                        .SetText(window.IsLockedOut ? "Unlock" : "Lockout")
                        .OnClick(() => window.ToggleLockout());
                });
        }

        public void Update()
        {
            if (DateTime.UtcNow - this.lastUpdate < UpdateInterval)
            {
                return;
            }

            this.lastUpdate = DateTime.UtcNow;

            var state = GameStateProvider.Current;

            var orderedOps =
                from pair in this.operationUIs
                let op = pair.Key
                let canExecute = op.IsConditionMet(state) && !op.IsSatisfied(state)
                orderby canExecute descending, op.Name
                select new { Operation = op, CanExecute = canExecute, Elements = pair.Value };

            foreach (var pair in orderedOps)
            {
                pair.Elements.Row.GameObject.transform.SetAsLastSibling();
                pair.Elements.StartButton.SetEnabled(pair.CanExecute);
            }
        }

        public void Detatch()
        {
        }

        private void BuildOperationRow(OperationConfig operation, WidgetMountPoint mountPoint)
        {
            IconButtonWidget startButton = null;
            var row = mountPoint.AddHorizontalLayoutGroup($"operation_${operation.Id}")
                .SetChildAlignment(TextAnchor.MiddleLeft)
                .SetPadding(10, 5)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddImage("Icon")
                        .SetMinWidth(40)
                        .SetMinHeight(40)
                        .SetPreferredWidth(40)
                        .SetPreferredHeight(40)
                        .SetSprite(operation.UI.GetIcon() ?? ResourceResolver.GetSprite("empty_bg"));

                    mountPoint.AddLayoutItem("Spacer")
                    .SetMinWidth(10)
                    .SetPreferredWidth(10);

                    var nameElement = mountPoint.AddText("OperationName")
                        .SetFontSize(16)
                        .SetExpandWidth()
                        .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
                        .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                        .SetText(operation.Name);

                    mountPoint.AddLayoutItem("Spacer")
                        .SetMinWidth(10)
                        .SetExpandWidth();

                    startButton = mountPoint.AddIconButton("StartButton")
                        .SetMinWidth(35)
                        .SetMinHeight(35)
                        .SetPreferredWidth(35)
                        .SetPreferredHeight(35)
                        .Disable()
                        .SetBackground()
                        .SetSprite("autoccultist_play_icon")
                        .OnClick(() => this.OnOperationButtonClick(operation));
                });

            this.operationUIs.Add(operation, new OperationUIElements
            {
                Row = row,
                StartButton = startButton,
            });
        }

        private void OnOperationButtonClick(OperationConfig operation)
        {
            var runningImpulse = NucleusAccumbens.CurrentImperatives.OfType<UITriggeredImperative>().FirstOrDefault(x => x.Imperative == operation);

            if (runningImpulse != null)
            {
                NucleusAccumbens.RemoveImperative(operation);
                runningImpulse.Abort();
            }
            else
            {
                NucleusAccumbens.AddImperative(new UITriggeredImperative(operation));
            }
        }

        private bool FilterOperation(OperationConfig operation)
        {
            if (operation.Situation != this.window.Situation.VerbId)
            {
                return false;
            }

            if (!operation.UI.Visible)
            {
                return false;
            }

            return true;
        }

        private class OperationUIElements
        {
            public HorizontalLayoutGroupWidget Row { get; set; }

            public IconButtonWidget StartButton { get; set; }
        }
    }
}
