namespace AutoccultistNS.UI
{
    using AutoccultistNS.Brain;
    using Roost.Piebald;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine;

    public class OngoingOperationView : IWindowView<SituationAutomationWindow.IWindowContext>
    {
        private int cachedHistoryItems = -1;

        private SituationAutomationWindow.IWindowContext window;

        private TextButtonWidget lockoutButton;
        private ScrollRegionWidget historyScroll;

        public OngoingOperationView(OperationReaction reaction)
        {
            this.Reaction = reaction;
        }

        public OperationReaction Reaction { get; }

        public void Attach(SituationAutomationWindow.IWindowContext window)
        {
            this.window = window;

            window.Content.AddVerticalLayoutGroup("VerticalLayout")
                .SetPadding(10, 5, 10, 0)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddText("OperationName")
                        .SetExpandWidth()
                        .SetTextAlignment(TMPro.TextAlignmentOptions.Center)
                        .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .SetMinFontSize(16)
                        .SetMaxFontSize(32)
                        .SetText(this.Reaction.Operation.Name);

                    mountPoint.AddSizingLayout("Spacer")
                        .SetPreferredHeight(10);

                    this.historyScroll = mountPoint.AddScrollRegion("History")
                        .SetExpandWidth()
                        .SetExpandHeight()
                        .SetPreferredHeight(100)
                        .SetVertical();

                    // TODO: Show current recipe text.
                    // It seems CurrentRecipe description is usually ".".  Are they tokens?  Stored in a situation dominion?
                });

            window.Footer.AddHorizontalLayoutGroup("FooterButtons")
                .SetChildAlignment(TextAnchor.MiddleRight)
                .SetPadding(10, 2)
                .SetSpacing(5)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddTextButton("AbortButton")
                        .SetText("Abort")
                        .OnClick(() => this.Reaction.Dispose());

                    this.lockoutButton = mountPoint.AddTextButton("LockoutButton")
                        .SetText("Lockout")
                        .OnClick(() => window.ToggleLockout());
                });
        }

        public void Update()
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
                            .SetSpreadChildrenHorizontally()
                            .SetExpandWidth()
                            .SetFitContentHeight()
                            .SetPadding(10, 2)
                            .SetSpacing(3)
                            .AddContent(mountPoint =>
                            {
                                mountPoint.AddText("HistoryItem")
                                    .SetExpandWidth()
                                    .SetTextAlignment(TMPro.TextAlignmentOptions.Center)
                                    .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                                    .SetFontSize(20)
                                    .SetText(recipe.Label ?? item.SlottedRecipeId);

                                foreach (var elementId in item.SlottedElements)
                                {
                                    var element = compendium.GetEntityById<Element>(elementId);

                                    mountPoint.AddHorizontalLayoutGroup("Element")
                                        .SetExpandWidth()
                                        .SetFitContentHeight()
                                        .SetChildAlignment(TextAnchor.MiddleCenter)
                                        .SetSpacing(5)
                                        .AddContent(mountPoint =>
                                        {
                                            mountPoint.AddImage("ElementImage")
                                                .SetPreferredWidth(30)
                                                .SetPreferredHeight(30)
                                                .SetSprite(ResourcesManager.GetAppropriateSpriteForElement(element));

                                            mountPoint.AddText("ElementName")
                                                .SetFontSize(16)
                                                .SetText(element.Label);
                                        });
                                }
                            });
                    }
                })
                .ScrollToVertical(0);
        }

        public void Detatch()
        {
        }
    }
}
