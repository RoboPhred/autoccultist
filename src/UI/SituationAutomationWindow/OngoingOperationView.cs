namespace AutoccultistNS.UI
{
    using AutoccultistNS.Brain;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine;

    public class OngoingOperationView : IWindowView
    {
        private int cachedHistoryItems = -1;
        private ScrollWidget historyScroll;

        public OngoingOperationView(SituationAutomationWindow window, OperationReaction reaction, Transform contentRoot)
        {
            this.Reaction = reaction;

            UIFactories.CreateVeritcalLayoutGroup("VerticalLayout", contentRoot)
                .Padding(10, 2)
                .ExpandWidth()
                .FitContentHeight()
                .AddContent(mountPoint =>
                {
                    mountPoint.AddText("OperationText")
                        .ExpandWidth()
                        .TextAlignment(TMPro.TextAlignmentOptions.Center)
                        .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .FontSize(32)
                        .Text("Operation in progress");

                    mountPoint.AddSizingLayout("Spacer")
                        .PreferredHeight(10);

                    mountPoint.AddText("OperationName")
                        .ExpandWidth()
                        .TextAlignment(TMPro.TextAlignmentOptions.Center)
                        .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .FontSize(20)
                        .MaxFontSize(20)
                        .MinFontSize(12)
                        .Text(reaction.Operation.Name);

                    mountPoint.AddSizingLayout("Spacer")
                        .PreferredHeight(10);

                    this.historyScroll = mountPoint.AddScroll("History")
                        .ExpandWidth()
                        // FIXME: Not working
                        // .ExpandHeight()
                        .PreferredHeight(125)
                        .Vertical();

                    mountPoint.AddSizingLayout("Spacer")
                    .PreferredHeight(10);

                    mountPoint.AddTextButton("Cancel")
                        .Text("Cancel")
                        .OnClick(() =>
                            {
                                reaction.Dispose();
                            });
                });
        }

        public OperationReaction Reaction { get; private set; }

        public void UpdateContent()
        {
            var historyItems = this.Reaction.RecipeHistory;
            if (historyItems.Count == this.cachedHistoryItems)
            {
                NoonUtility.LogWarning($"No history items to update: {historyItems.Count}");
                return;
            }

            this.cachedHistoryItems = historyItems.Count;

            var compendium = Watchman.Get<Compendium>();

            NoonUtility.LogWarning($"Updating history items to {historyItems.Count}");
            this.historyScroll.Clear();
            this.historyScroll.AddContent(
                mountPoint =>
                {
                    foreach (var item in historyItems)
                    {
                        var recipe = compendium.GetEntityById<Recipe>(item);
                        mountPoint.AddText("HistoryItem")
                            .ExpandWidth()
                            .TextAlignment(TMPro.TextAlignmentOptions.Center)
                            .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                            .FontSize(20)
                            .Text(recipe.Label ?? item);
                    }
                });
            this.historyScroll.ScrollToVertical(1);
        }
    }
}
