namespace AutoccultistNS.UI
{
    using AutoccultistNS.Brain;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine;

    public class OngoingOperationView : IWindowView
    {
        private int cachedHistoryItems = -1;
        private ScrollRegionWidget historyScroll;
        private ScrollRegionWidget recipeDescriptionScroll;
        private TextWidget recipeDescription;

        public OngoingOperationView(SituationAutomationWindow window, OperationReaction reaction, WidgetMountPoint contentMount, WidgetMountPoint footerMount)
        {
            this.Reaction = reaction;

            contentMount.AddVeritcalLayoutGroup("VerticalLayout")
                .Padding(10, 2)
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

                    this.historyScroll = mountPoint.AddScrollRegion("History")
                        .ExpandWidth()
                        .ExpandHeight()
                        .PreferredHeight(100)
                        .Vertical();

                    // TODO: Show current recipe text.
                    // It seems CurrentRecipe description is usually ".".  Are they tokens?  Stored in a situation dominion?
                });

            var test = footerMount.AddHorizontalLayoutGroup("FooterButtons")
                .ChildAlignment(TextAnchor.MiddleRight)
                .Padding(10, 2)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddTextButton("AbortButton")
                        .Text("Abort")
                        .OnClick(() => reaction.Dispose());
                });
        }

        public OperationReaction Reaction { get; private set; }

        public void UpdateContent()
        {
            var historyItems = this.Reaction.RecipeHistory;
            if (historyItems.Count == this.cachedHistoryItems)
            {
                return;
            }

            this.cachedHistoryItems = historyItems.Count;

            var compendium = Watchman.Get<Compendium>();

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
                })
            .ScrollToVertical(0);

            if (historyItems.Count > 0)
            {
                var lastItem = historyItems[historyItems.Count - 1];
                var recipe = compendium.GetEntityById<Recipe>(lastItem);
                this.recipeDescription.Text(recipe?.Description ?? "");
                this.recipeDescriptionScroll.ScrollToVertical(1);
            }
        }
    }
}
