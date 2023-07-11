namespace AutoccultistNS.UI
{
    using AutoccultistNS.Brain;
    using TMPro;
    using UnityEngine;

    public class OngoingOperationView : IWindowView
    {
        public OperationReaction Reaction { get; private set; }

        public OngoingOperationView(SituationAutomationWindow window, OperationReaction reaction, Transform contentRoot)
        {
            this.Reaction = reaction;

            UIFactories.CreateVeritcalLayoutGroup("VerticalLayout", contentRoot)
                .FillContentHeight()
                .AddContent(transform =>
                {
                    UIFactories.CreateText("OperationText", transform)
                        .FontWeight(FontWeight.Bold)
                        .Text("Operation in progress");
                    UIFactories.CreateText("OperationName", transform)
                        .Text(reaction.Operation.Name);
                    UIFactories.CreateTextButton("Cancel", transform)
                        .Text("Cancel")
                        .OnClick(() =>
                            {
                                reaction.Dispose();
                            });
                });
        }

        public void UpdateContent()
        {
        }
    }
}
