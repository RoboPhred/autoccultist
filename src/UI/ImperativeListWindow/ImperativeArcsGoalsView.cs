namespace AutoccultistNS.UI
{
    using System.Linq;
    using AutoccultistNS.Config;
    using Roost.Piebald;
    using UnityEngine;

    public class ImperativeArcsGoalsView : IWindowView<ImperativeListWindow.IWindowContext>
    {
        public void Attach(ImperativeListWindow.IWindowContext window)
        {
            window.Content.AddHorizontalLayoutGroup("Buttons")
                .SetExpandWidth()
                .SetExpandHeight()
                .SetChildAlignment(TextAnchor.UpperCenter)
                .SetPadding(0, 10, 0, 0)
                .SetSpacing(30)
                .AddContent((mountPoint) =>
                {
                    mountPoint.AddVerticalLayoutGroup("Arcs")
                        .SetSpacing(10)
                        .WithPointerSounds()
                        .OnPointerClick(e => window.PushView(new ImperativeFolderView(Library.Arcs.OfType<IImperativeConfig>().ToList(), string.Empty)))
                        .AddContent(mountPoint =>
                        {
                            mountPoint.AddLayoutItem("ArcsButton")
                                .SetPreferredWidth(100)
                                .SetPreferredHeight(100)
                                .WithBehavior<GlowOnHover>()
                                .AddContent(mountPoint =>
                                {
                                    mountPoint.AddImage("ArcsIcon")
                                        .SetPreferredWidth(100)
                                        .SetPreferredHeight(100)
                                        .SetSprite(ResourcesManager.GetSpriteForAspect("memory"));
                                });

                            mountPoint.AddText("ArcsText")
                                .SetFontSize(20)
                                .SetPreferredWidth(100)
                                .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                                .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                                .SetText("Arcs");
                        });

                    mountPoint.AddVerticalLayoutGroup("Goals")
                        .SetSpacing(10)
                        .WithPointerSounds()
                        .OnPointerClick(e => window.PushView(new ImperativeFolderView(Library.Goals.OfType<IImperativeConfig>().ToList(), string.Empty)))
                        .AddContent(mountPoint =>
                        {
                            mountPoint.AddLayoutItem("GoalsButton")
                                .SetPreferredWidth(100)
                                .SetPreferredHeight(100)
                                .WithBehavior<GlowOnHover>()
                                .AddContent(mountPoint =>
                                {
                                    mountPoint.AddImage("GoalsIcon")
                                        .SetPreferredWidth(100)
                                        .SetPreferredHeight(100)
                                        .SetSprite(ResourcesManager.GetSpriteForAspect("memory"));
                                });

                            mountPoint.AddText("GoalsText")
                                .SetFontSize(20)
                                .SetPreferredWidth(100)
                                .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                                .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                                .SetText("Goals");
                        });
                });
        }

        public void Detatch()
        {
        }

        public void Update()
        {
        }
    }
}
