namespace AutoccultistNS.UI
{
    using AutoccultistNS.Brain;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine;

    public class OngoingOperationView : IWindowView
    {
        private int cachedHistoryItems = -1;

        private SituationAutomationWindow window;
        private TextButtonWidget lockoutButton;
        private ScrollRegionWidget historyScroll;

        public OngoingOperationView(SituationAutomationWindow window, OperationReaction reaction, WidgetMountPoint contentMount, WidgetMountPoint footerMount)
        {
            this.window = window;
            this.Reaction = reaction;

            contentMount.AddVerticalLayoutGroup("VerticalLayout")
                .Padding(10, 2)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddText("OperationName")
                        .ExpandWidth()
                        .TextAlignment(TMPro.TextAlignmentOptions.Center)
                        .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .MinFontSize(16)
                        .MaxFontSize(32)
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

            footerMount.AddHorizontalLayoutGroup("FooterButtons")
                .ChildAlignment(TextAnchor.MiddleRight)
                .Padding(10, 2)
                .Spacing(5)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddTextButton("AbortButton")
                        .Text("Abort")
                        .OnClick(() => reaction.Dispose());

                    this.lockoutButton = mountPoint.AddTextButton("LockoutButton")
                        .Text("Lockout")
                        .OnClick(() => window.ToggleLockout());
                });
        }

        public OperationReaction Reaction { get; private set; }

        public void UpdateContent()
        {
            this.lockoutButton.SetEnabled(!this.window.IsLockedOut);

            var historyItems = this.Reaction.History;
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
                        var recipe = compendium.GetEntityById<Recipe>(item.SlottedRecipeId);
                        mountPoint.AddVerticalLayoutGroup("HistoryEntry")
                            .SpreadChildrenHorizontally()
                            .ExpandWidth()
                            .FitContentHeight()
                            .Padding(10, 2)
                            .Spacing(3)
                            .AddContent(mountPoint =>
                            {
                                mountPoint.AddText("HistoryItem")
                                    .ExpandWidth()
                                    .TextAlignment(TMPro.TextAlignmentOptions.Center)
                                    .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                                    .FontSize(20)
                                    .Text(recipe.Label ?? item.SlottedRecipeId);

                                foreach (var elementId in item.SlottedElements)
                                {
                                    var element = compendium.GetEntityById<Element>(elementId);

                                    mountPoint.AddHorizontalLayoutGroup("Element")
                                        .ExpandWidth()
                                        .FitContentHeight()
                                        .ChildAlignment(TextAnchor.MiddleCenter)
                                        .Spacing(5)
                                        .AddContent(mountPoint =>
                                        {
                                            mountPoint.AddImage("ElementImage")
                                                .PreferredWidth(30)
                                                .PreferredHeight(30)
                                                .Sprite(ResourcesManager.GetAppropriateSpriteForElement(element));

                                            mountPoint.AddText("ElementName")
                                                .FontSize(16)
                                                .Text(element.Label);
                                        });
                                }
                            });
                    }
                })
                .ScrollToVertical(0);
        }
    }
}
