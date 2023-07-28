namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoccultistNS.Config;
    using Roost.Piebald;

    public class ImperativeArcsGoalsView : IWindowView<ImperativeListWindow.IWindowContext>
    {
        private readonly TimeSpan updateInterval = TimeSpan.FromSeconds(1);
        private readonly Dictionary<IImperativeConfig, ImperativeRowFactory.ImperativeUIElements> imperativeUIs = new();

        private DateTime lastUpdate = DateTime.MinValue;
        private ImperativeListWindow.IWindowContext window;
        private LayoutItemWidget menuGroup;
        private LayoutItemWidget searchGroup;
        private WidgetMountPoint searchItems;

        public void Attach(ImperativeListWindow.IWindowContext window)
        {
            this.window = window;

            window.Content.AddVerticalLayoutGroup("Content")
                .SetExpandWidth()
                .SetExpandHeight()
                .SetPadding(20, 5, 20, 0)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddHorizontalLayoutGroup("SearchField")
                        .SetExpandWidth()
                        .SetFitContentHeight()
                        .SetSpacing(10)
                        .SetChildAlignment(UnityEngine.TextAnchor.MiddleLeft)
                        .AddContent(mountPoint =>
                        {
                            mountPoint.AddText("SearchLabel")
                                .SetFontSize(24)
                                .SetFontStyle(TMPro.FontStyles.Bold)
                                .SetText("Search:");

                            mountPoint.AddInputField("SearchInput")
                                .SetExpandWidth()
                                .SetPreferredHeight(20)
                                .OnChange(this.OnSearch);
                        });

                    mountPoint.AddLayoutItem("Spacer")
                        .SetMinHeight(10);

                    mountPoint.AddLayoutItem("Body")
                        .SetExpandWidth()
                        .SetExpandHeight()
                        .AddContent(mountPoint =>
                        {
                            this.BuildMenuGroup(mountPoint);
                            this.BuildSearchGroup(mountPoint);
                        });
                });
        }

        public void Detatch()
        {
        }

        public void Update()
        {
            if (DateTime.UtcNow - this.lastUpdate < this.updateInterval)
            {
                return;
            }

            this.lastUpdate = DateTime.UtcNow;

            ImperativeRowFactory.UpdateImperatives(this.imperativeUIs);
        }

        private void BuildMenuGroup(WidgetMountPoint mountPoint)
        {
            this.menuGroup = mountPoint.AddVerticalLayoutGroup("MenuGroup")
                .SetBottom(0, 0)
                .SetTop(1, 0)
                .SetLeft(0, 0)
                .SetRight(1, 0)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddLayoutItem("Spacer")
                        .SetExpandHeight();

                    Glow goalsGlow = null;
                    mountPoint.AddHorizontalLayoutGroup("GoalsCategory")
                        .SetExpandWidth()
                        .SetFitContentHeight()
                        .OnPointerEnter(e => goalsGlow.Show())
                        .OnPointerExit(e => goalsGlow.Hide())
                        .WithPointerSounds()
                        .SetSpacing(15)
                        .SetChildAlignment(UnityEngine.TextAnchor.MiddleCenter)
                        .OnPointerClick(e => this.window.PushView(new ImperativeFolderView("Goals", Library.Goals.OfType<IImperativeConfig>().ToList(), string.Empty)))
                        .AddContent(mountPoint =>
                        {
                            mountPoint.AddImage("HitDetector")
                                .SetIgnoreLayout()
                                .SetSprite("empty_bg");

                            mountPoint.AddLayoutItem("ImageContainer")
                                .SetMinWidth(100)
                                .SetMinHeight(100)
                                .WithGlow(out goalsGlow)
                                .AddContent(mountPoint =>
                                {
                                    mountPoint.AddImage("Image")
                                        .SetMinWidth(100)
                                        .SetMinHeight(100)
                                        .SetSprite("aspect:challenge.practical");
                                });

                            mountPoint.AddVerticalLayoutGroup("TextContainer")
                                .SetExpandWidth()
                                .SetSpacing(5)
                                .AddContent(mountPoint =>
                                {
                                    mountPoint.AddText("Title")
                                        .SetFontSize(30)
                                        .SetFontStyle(TMPro.FontStyles.Bold)
                                        .SetText("Goals");

                                    mountPoint.AddText("Description")
                                        .SetFontSize(20)
                                        .SetText("Tasks to automate individual aspects of the game.  These can either accomplish specific tasks, or run perpetually to automate away recurring conditions.");
                                });
                        });

                    mountPoint.AddLayoutItem("Spacer")
                        .SetExpandHeight();

                    Glow arcsGlow = null;
                    mountPoint.AddHorizontalLayoutGroup("ArcsCategory")
                        .SetExpandWidth()
                        .SetFitContentHeight()
                        .OnPointerEnter(e => arcsGlow.Show())
                        .OnPointerExit(e => arcsGlow.Hide())
                        .WithPointerSounds()
                        .SetSpacing(15)
                        .SetChildAlignment(UnityEngine.TextAnchor.MiddleCenter)
                        .OnPointerClick(e => this.window.PushView(new ImperativeFolderView("Arcs", Library.Arcs.OfType<IImperativeConfig>().ToList(), string.Empty)))
                        .AddContent(mountPoint =>
                        {
                            mountPoint.AddImage("HitDetector")
                                .SetIgnoreLayout()
                                .SetSprite("empty_bg");

                            mountPoint.AddLayoutItem("ImageContainer")
                                .SetMinWidth(100)
                                .SetMinHeight(100)
                                .WithGlow(out arcsGlow)
                                .AddContent(mountPoint =>
                                {
                                    mountPoint.AddImage("Image")
                                        .SetMinWidth(100)
                                        .SetMinHeight(100)
                                        .SetSprite("aspect:challenge.knowledge");
                                });

                            mountPoint.AddVerticalLayoutGroup("TextContainer")
                                .SetExpandWidth()
                                .SetSpacing(5)
                                .AddContent(mountPoint =>
                                {
                                    mountPoint.AddText("Title")
                                        .SetFontSize(30)
                                        .SetFontStyle(TMPro.FontStyles.Bold)
                                        .SetText("Arcs");

                                    mountPoint.AddText("Description")
                                        .SetFontSize(20)
                                        .SetText("Fully autonomous playthroughs.  These are intended to run start to finish without human intervention.  They may achieve one of the game's endings, or configure the game into a certain state.");
                                });
                        });

                    mountPoint.AddLayoutItem("Spacer")
                        .SetExpandHeight();
                });
        }

        private void BuildSearchGroup(WidgetMountPoint mountPoint)
        {
            this.searchGroup = mountPoint.AddScrollRegion("SearchGroup")
                .SetVertical()
                .SetBottom(0, 0)
                .SetTop(1, 0)
                .SetLeft(0, 0)
                .SetRight(1, 0)
                .SetActive(false)
                .AddContent(mountPoint =>
                {
                    this.searchItems = mountPoint.AddVerticalLayoutGroup("SearchItems")
                        .SetExpandWidth()
                        .SetExpandHeight()
                        .SetSpacing(10);
                });
        }

        private void OnSearch(string searchContent)
        {
            if (string.IsNullOrEmpty(searchContent))
            {
                this.menuGroup.SetActive(true);
                this.searchGroup.SetActive(false);
                this.imperativeUIs.Clear();
                this.searchItems.Clear();
            }
            else
            {
                this.SetSearchItems(searchContent);
                this.menuGroup.SetActive(false);
                this.searchGroup.SetActive(true);
            }
        }

        private void SetSearchItems(string searchContent)
        {
            searchContent = searchContent.ToLower();

            // Delay the update as we might be constantly typing.
            this.lastUpdate = DateTime.Now;

            this.imperativeUIs.Clear();
            this.searchItems.Clear();

            var imperatives =
                from imperative in Library.Arcs.OfType<IImperativeConfig>().Concat(Library.Goals).OfType<IImperativeConfig>()
                where imperative.UI.Visible
                where imperative.Name.ToLower().Contains(searchContent)
                orderby imperative.Name
                select imperative;

            foreach (var imperative in imperatives)
            {
                ImperativeRowFactory.BuildImperativeRow(imperative, this.searchItems, this.window.StartImperative);
            }
        }
    }
}
