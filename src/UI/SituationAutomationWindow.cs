namespace AutoccultistNS.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;
    using SecretHistories.Entities;
    using UnityEngine;

    public class SituationAutomationWindow : SituationWindow
    {
        private readonly Dictionary<OperationConfig, OperationUIElements> operationUIs = new();

        public static SituationAutomationWindow CreateWindow(Situation situation)
        {
            var window = SituationWindow.CreateWindow<SituationAutomationWindow>($"window_${situation.VerbId}_automation");
            window.Attach(situation);
            return window;
        }

        public void Awake()
        {
            this.UpdateContent();
        }

        public void Update()
        {
            this.UpdateContent();
        }

        protected override void BuildContent(Transform parent)
        {
            // FIXME: We want to let the window expand up to a point then stop.
            // var constraint = UIFactories.CreateSizingLayout("Constraint", parent)
            //     .FillContentWidth()
            //     .MaxHeight(600);

            UIFactories.CreateScroll("ScrollRect", parent)
                .Vertical()
                .AddContent(
                    transform =>
                    {
                        foreach (var op in Library.Operations.Where(x => x.Situation == this.Situation.VerbId))
                        {
                            this.BuildOperationRow(op, transform);
                        }
                    });
        }

        private void UpdateContent()
        {
            if (!this.IsVisible)
            {
                return;
            }

            // TODO: Reorder entries based on startability
            var state = GameStateProvider.Current;
            foreach (var op in this.operationUIs.Keys)
            {
                var ui = this.operationUIs[op];

                var canRun = op.IsConditionMet(state);

                var runningImpulse = NucleusAccumbens.CurrentImperatives.OfType<UITriggeredImperative>().FirstOrDefault(x => x.Imperative == op);

                if (runningImpulse == null && !canRun)
                {
                    ui.StartButton.Disable();
                    ui.StartButton.Text("Start");
                }
                else if (runningImpulse != null)
                {
                    ui.StartButton.Enable();
                    ui.StartButton.Text("Abort");
                }
                else
                {
                    ui.StartButton.Enable();
                    ui.StartButton.Text("Start");
                }
            }
        }

        private void BuildOperationRow(OperationConfig operation, Transform parent)
        {
            var layout = UIFactories.CreateHorizontalLayoutGroup($"operation_${operation.Id}", parent)
                .Padding(10, 2)
                .AddContent(transform =>
                {
                    var nameElement = UIFactories.CreateText("Name", transform)
                        .FontSize(12)
                        .MinFontSize(8)
                        .MaxFontSize(12)
                        .HorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left)
                        .VerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                        .Text(operation.Name);

                    UIFactories.CreateSizingLayout("Spacer", transform)
                        .MinWidth(10)
                        .ExpandWidth();

                    var startButton = UIFactories.CreateTextButton("Button", transform)
                        .PreferredHeight(35)
                        .Disable()
                        .Text("Start")
                        .OnClick(() => this.OnOperationButtonClick(operation));

                    this.operationUIs.Add(operation, new OperationUIElements
                    {
                        StartButton = startButton,
                    });
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
            public TextButtonWidget StartButton { get; set; }
        }
    }
}
