namespace AutoccultistNS.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;
    using UnityEngine;

    public class NewOperationView : IWindowView
    {
        private readonly SituationAutomationWindow window;
        private readonly Transform contentRoot;
        private readonly Dictionary<OperationConfig, OperationUIElements> operationUIs = new();

        public NewOperationView(SituationAutomationWindow window, Transform contentRoot)
        {
            this.window = window;
            this.contentRoot = contentRoot;

            // FIXME: We want to let the window expand up to a point then stop.
            // var constraint = UIFactories.CreateSizingLayout("Constraint", parent)
            //     .FillContentWidth()
            //     .MaxHeight(600);

            UIFactories.CreateScroll("ScrollRect", contentRoot)
                .Vertical()
                .AddContent(
                    transform =>
                    {
                        foreach (var op in Library.Operations.Where(x => x.Situation == window.Situation.VerbId))
                        {
                            this.BuildOperationRow(op, transform);
                        }
                    })
                .ScrollToVertical(0);
        }

        public void UpdateContent()
        {
            var state = GameStateProvider.Current;

            var orderedOps =
                from pair in this.operationUIs
                let op = pair.Key
                let canExecute = op.IsConditionMet(state)
                orderby canExecute descending, op.Name
                select new { Operation = op, CanExecute = canExecute, Elements = pair.Value };

            foreach (var pair in orderedOps)
            {
                pair.Elements.Row.GameObject.transform.SetAsLastSibling();
                pair.Elements.StartButton.SetEnabled(pair.CanExecute);
            }
        }

        private void BuildOperationRow(OperationConfig operation, Transform parent)
        {
            TextButtonWidget startButton = null;
            var row = UIFactories.CreateHorizontalLayoutGroup($"operation_${operation.Id}", parent)
                .Padding(10, 2)
                .AddContent(mountPoint =>
                {
                    var nameElement = mountPoint.AddText("Name")
                        .FontSize(12)
                        .MinFontSize(8)
                        .MaxFontSize(12)
                        .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
                        .VerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                        .Text(operation.Name);

                    mountPoint.AddSizingLayout("Spacer")
                        .MinWidth(10)
                        .ExpandWidth();

                    startButton = mountPoint.AddTextButton("Button")
                        .PreferredHeight(35)
                        .Disable()
                        .Text("Start")
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

        private class OperationUIElements
        {
            public HorizontalLayoutGroupWidget Row { get; set; }

            public TextButtonWidget StartButton { get; set; }
        }
    }
}
