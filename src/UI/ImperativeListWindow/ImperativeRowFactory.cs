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

    public static class ImperativeRowFactory
    {
        public static ImperativeUIElements BuildImperativeRow(IImperativeConfig imperative, WidgetMountPoint mountPoint, Action<IImperativeConfig> startImperative)
        {
            var state = GameStateProvider.Current;
            var canExecute = imperative.IsConditionMet(state) && !imperative.IsSatisfied(state);
            var isRunning = NucleusAccumbens.CurrentImperatives.Contains(imperative);

            Glow glow = null;
            ImageWidget runningIcon = null;
            IconButtonWidget startButton = null;
            var row = mountPoint.AddHorizontalLayoutGroup($"imperative_{imperative.Name}")
                .SetChildAlignment(TextAnchor.MiddleCenter)
                .SetExpandWidth()
                .SetFitContentHeight()
                .SetPadding(20, 5)
                .OnPointerEnter(e => glow.Show())
                .OnPointerExit(e => glow.Hide())
                .AddContent(mountPoint =>
                {
                    mountPoint.AddLayoutItem("IconGlow")
                        .SetMinWidth(40)
                        .SetMinHeight(40)
                        .SetPreferredWidth(40)
                        .SetPreferredHeight(40)
                        .WithGlow(out glow)
                        .AddContent(mountPoint =>
                        {
                            mountPoint.AddImage("Icon")
                                .SetMinWidth(40)
                                .SetMinHeight(40)
                                .SetPreferredWidth(40)
                                .SetPreferredHeight(40)
                                .SetSprite(imperative.UI.GetIcon() ?? ResourceResolver.GetSprite("aspect:memory"));
                        });

                    mountPoint.AddLayoutItem("Spacer")
                    .SetMinWidth(10)
                    .SetPreferredWidth(10);

                    var nameElement = mountPoint.AddText("ImperativeName")
                        .SetFontSize(14)
                        .SetMinFontSize(16)
                        .SetMaxFontSize(32)
                        .SetExpandWidth()
                        .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
                        .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                        .SetText(imperative.Name);

                    mountPoint.AddLayoutItem("Spacer")
                        .SetMinWidth(20)
                        .SetExpandWidth();

                    runningIcon = mountPoint.AddImage("ActiveIcon")
                        .SetSprite("autoccultist_situation_automation_badge")
                        .SetActive(isRunning)
                        .SetMinWidth(50)
                        .SetMinHeight(50)
                        .SetPreferredWidth(50)
                        .SetPreferredHeight(50)
                        .WithSpinAnimation(-360 / 16);

                    startButton = mountPoint.AddIconButton("StartButton")
                        .SetMinWidth(40)
                        .SetMinHeight(40)
                        .SetPreferredWidth(40)
                        .SetPreferredHeight(40)
                        .SetEnabled(canExecute)
                        .SetBackground()
                        .SetSprite("autoccultist_play_icon")
                        .SetActive(!isRunning)
                        .OnClick(() => startImperative(imperative));
                });

            return new ImperativeUIElements
            {
                Row = row,
                StartButton = startButton,
                RunningIcon = runningIcon,
            };
        }

        public static void UpdateImperatives(IReadOnlyDictionary<IImperativeConfig, ImperativeUIElements> imperatives)
        {
            var state = GameStateProvider.Current;

            var orderedOps =
                from pair in imperatives
                let imperative = pair.Key
                let canExecute = imperative.IsConditionMet(state) && !imperative.IsSatisfied(state)
                let isRunning = NucleusAccumbens.CurrentImperatives.Contains(imperative)
                orderby canExecute descending, imperative.Name
                select new { Imperative = imperative, IsRunning = isRunning, CanExecute = canExecute, Elements = pair.Value };

            foreach (var pair in orderedOps.Reverse())
            {
                pair.Elements.Row.GameObject.transform.SetAsFirstSibling();

                pair.Elements.StartButton.SetActive(!pair.IsRunning);
                pair.Elements.StartButton.SetEnabled(pair.CanExecute);
                pair.Elements.RunningIcon.SetActive(pair.IsRunning);
            }
        }
        public class ImperativeUIElements
        {
            public HorizontalLayoutGroupWidget Row { get; set; }

            public IconButtonWidget StartButton { get; set; }

            public ImageWidget RunningIcon { get; set; }
        }
    }
}
