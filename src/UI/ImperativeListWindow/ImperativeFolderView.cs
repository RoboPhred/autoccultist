namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoccultistNS.Brain;
    using AutoccultistNS.Config;
    using AutoccultistNS.GameState;
    using Roost.Piebald;
    using UnityEngine;

    public class ImperativeFolderView : IWindowView<ImperativeListWindow.IWindowContext>, IViewHasTitle
    {
        private readonly TimeSpan updateInterval = TimeSpan.FromSeconds(1);
        private readonly Dictionary<IImperative, ImperativeUIElements> imperativeUIs = new();

        private DateTime lastUpdate = DateTime.MinValue;

        private ImperativeListWindow.IWindowContext window;

        private float scrollY = 1;

        private string searchFilter = string.Empty;

        public ImperativeFolderView(IReadOnlyCollection<IImperativeConfig> collection, string folder)
        {
            this.Collection = collection;
            this.Folder = folder;
        }

        public IReadOnlyCollection<IImperativeConfig> Collection { get; }

        public string Folder { get; }

        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(this.Folder))
                {
                    return "Automations";
                }

                var captialized = string.Join("/", this.Folder.Split("\\").Select(x => x.Capitalize()));
                return "Automations: " + captialized.Substring(0, captialized.Length - 1);
            }
        }

        public void Attach(ImperativeListWindow.IWindowContext window)
        {
            this.window = window;

            window.Footer.AddHorizontalLayoutGroup("Footer")
                .SetExpandWidth()
                .SetChildAlignment(TextAnchor.MiddleCenter)
                .AddContent(mountPoint =>
                {
                    mountPoint.AddSizingLayout("Spacer")
                        .SetExpandWidth();

                    mountPoint.AddTextButton("BackButton")
                        .SetText("Back")
                        .OnClick(() => this.window.PopView());

                    mountPoint.AddSizingLayout("Spacer")
                        .SetExpandWidth();
                });

            this.RebuildContent();
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

            var state = GameStateProvider.Current;

            var orderedOps =
                from pair in this.imperativeUIs
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

        private void RebuildContent()
        {
            this.imperativeUIs.Clear();

            var folders =
                from imperative in this.Collection
                where this.FilterImperative(imperative, false)
                let path = imperative.GetLibraryPath()
                where path != null
                let relative = path.Substring(this.Folder.Length)
                let split = relative.IndexOf(Path.DirectorySeparatorChar)
                where split != -1
                let folder = relative.Substring(0, split)
                group imperative by folder.ToLower() into g
                orderby g.Key
                select g.Key;

            var imperatives =
                from imperative in this.Collection
                where this.FilterImperative(imperative, true)
                select imperative;

            this.window.Content.Clear();
            var scrollRegion = this.window.Content.AddScrollRegion("ScrollRegion")
                .SetExpandWidth()
                .SetExpandHeight()
                .SetVertical()
                .AddContent(mountPoint =>
                {
                    mountPoint.AddVerticalLayoutGroup("Items")
                        .SetExpandWidth()
                        .SetFitContentHeight()
                        .SetSpreadChildrenHorizontally()
                        .SetPadding(0, 10, 0, 0)
                        .SetSpacing(30)
                        .AddContent(mountPoint =>
                        {
                            foreach (var imperative in imperatives)
                            {
                                this.BuildImperativeRow(imperative, mountPoint);
                            }

                            foreach (var partition in folders.Partition(3))
                            {
                                mountPoint.AddHorizontalLayoutGroup()
                                    .SetExpandWidth()
                                    .SetFitContentHeight()
                                    .SetSpacing(30)
                                    .SetChildAlignment(TextAnchor.UpperCenter)
                                    .AddContent(mountPoint =>
                                    {
                                        foreach (var item in partition)
                                        {
                                            this.BuildFolderItem(item, mountPoint);
                                        }
                                    });
                            }
                        });
                });

            scrollRegion.ScrollToVertical(this.scrollY);
            scrollRegion.OnScrollValueChanged(v => this.scrollY = v.y);
        }

        private void BuildFolderItem(string folder, WidgetMountPoint mountPoint)
        {
            mountPoint.AddVerticalLayoutGroup($"Folder_{folder}")
                .SetSpacing(10)
                .SetChildAlignment(TextAnchor.MiddleCenter)
                .WithPointerSounds()
                .OnPointerClick((e) =>
                {
                    this.window.PushView(new ImperativeFolderView(this.Collection, this.Folder + folder + Path.DirectorySeparatorChar));
                })
                .AddContent(mountPoint =>
                {
                    mountPoint.AddSizingLayout("IconContainer")
                        .SetPreferredWidth(100)
                        .SetPreferredHeight(100)
                        .WithBehavior<GlowOnHover>()
                        .AddContent(mountPoint =>
                        {
                            this.BuildFolderIcon(folder, mountPoint);
                        });

                    mountPoint.AddText()
                        .SetFontSize(20)
                        .SetPreferredWidth(120)
                        .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                        .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                        .SetText(folder.Capitalize());
                });
        }

        private void BuildFolderIcon(string folder, WidgetMountPoint mountPoint)
        {
            var imperativesForFolder = this.Collection.Where(x => x.UI.Visible && (x.GetLibraryPath()?.StartsWith(this.Folder + folder) ?? false));
            var iconsForFolder = imperativesForFolder.Select(x => x.UI.Icon).Distinct().Take(4).Select(x => ResourceResolver.GetSprite(x) ?? ResourceResolver.GetSprite("aspect:memory")).Distinct().ToArray();

            if (iconsForFolder.Length == 0)
            {
                iconsForFolder = new[] { ResourceResolver.GetSprite("aspect:memory") };
            }

            mountPoint.AddSizingLayout("Icon")
                .SetMinWidth(100)
                .SetMinHeight(100)
                .SetPreferredWidth(100)
                .SetPreferredHeight(100)
                .AddContent(mountPoint =>
                {
                    if (iconsForFolder.Length == 1)
                    {
                        mountPoint.AddImage("Icon")
                            .SetIgnoreLayout()
                            .SetSprite(iconsForFolder[0]);
                    }
                    else if (iconsForFolder.Length == 2)
                    {
                        mountPoint.AddImage("Icon1")
                            .SetIgnoreLayout()
                            .SetLeft(0, 0)
                            .SetRight(0, 50)
                            .SetBottom(1, -50)
                            .SetTop(1, 0)
                            .SetSprite(iconsForFolder[0]);
                        mountPoint.AddImage("Icon2")
                            .SetIgnoreLayout()
                            .SetLeft(1, -50)
                            .SetRight(1, 0)
                            .SetBottom(1, -50)
                            .SetTop(1, 0)
                            .SetSprite(iconsForFolder[1]);
                        mountPoint.AddImage("Icon3")
                            .SetIgnoreLayout()
                            .SetLeft(0, 0)
                            .SetRight(0, 50)
                            .SetBottom(0, 0)
                            .SetTop(0, 50)
                            .SetSprite(iconsForFolder[1]);
                        mountPoint.AddImage("Icon4")
                            .SetIgnoreLayout()
                            .SetLeft(1, -50)
                            .SetRight(1, 0)
                            .SetBottom(0, 0)
                            .SetTop(0, 50)
                            .SetSprite(iconsForFolder[0]);
                    }
                    else if (iconsForFolder.Length == 3)
                    {
                        mountPoint.AddImage("Icon1")
                            .SetIgnoreLayout()
                            .SetLeft(0, 0)
                            .SetRight(0, 50)
                            .SetBottom(1, -50)
                            .SetTop(1, 0)
                            .SetSprite(iconsForFolder[0]);
                        mountPoint.AddImage("Icon2")
                            .SetIgnoreLayout()
                            .SetLeft(1, -50)
                            .SetRight(1, 0)
                            .SetBottom(1, -50)
                            .SetTop(1, 0)
                            .SetSprite(iconsForFolder[1]);
                        mountPoint.AddImage("Icon3")
                            .SetIgnoreLayout()
                            .SetLeft(0, 0)
                            .SetRight(0, 50)
                            .SetBottom(0, 0)
                            .SetTop(0, 50)
                            .SetSprite(iconsForFolder[2]);
                        mountPoint.AddImage("Icon4")
                            .SetIgnoreLayout()
                            .SetLeft(1, -50)
                            .SetRight(1, 0)
                            .SetBottom(0, 0)
                            .SetTop(0, 50)
                            .SetSprite(iconsForFolder[0]);
                    }
                    else
                    {
                        mountPoint.AddImage("Icon1")
                            .SetIgnoreLayout()
                            .SetLeft(0, 0)
                            .SetRight(0, 50)
                            .SetBottom(1, -50)
                            .SetTop(1, 0)
                            .SetSprite(iconsForFolder[0]);
                        mountPoint.AddImage("Icon2")
                            .SetIgnoreLayout()
                            .SetLeft(1, -50)
                            .SetRight(1, 0)
                            .SetBottom(1, -50)
                            .SetTop(1, 0)
                            .SetSprite(iconsForFolder[1]);
                        mountPoint.AddImage("Icon3")
                            .SetIgnoreLayout()
                            .SetLeft(0, 0)
                            .SetRight(0, 50)
                            .SetBottom(0, 0)
                            .SetTop(0, 50)
                            .SetSprite(iconsForFolder[2]);
                        mountPoint.AddImage("Icon4")
                            .SetIgnoreLayout()
                            .SetLeft(1, -50)
                            .SetRight(1, 0)
                            .SetBottom(0, 0)
                            .SetTop(0, 50)
                            .SetSprite(iconsForFolder[3]);
                    }
                });
        }

        private void BuildImperativeRow(IImperativeConfig imperative, WidgetMountPoint mountPoint)
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
                    mountPoint.AddSizingLayout("IconGlow")
                        .SetMinWidth(40)
                        .SetMinHeight(40)
                        .SetPreferredWidth(40)
                        .SetPreferredHeight(40)
                        .WithGlow(g => glow = g)
                        .AddContent(mountPoint =>
                        {
                            mountPoint.AddImage("Icon")
                                .SetMinWidth(40)
                                .SetMinHeight(40)
                                .SetPreferredWidth(40)
                                .SetPreferredHeight(40)
                                .SetSprite(imperative.UI.GetIcon() ?? ResourceResolver.GetSprite("aspect:memory"));
                        });

                    mountPoint.AddSizingLayout("Spacer")
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

                    mountPoint.AddSizingLayout("Spacer")
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
                        .OnClick(() => this.window.StartImperative(imperative));
                });

            this.imperativeUIs.Add(imperative, new ImperativeUIElements
            {
                Row = row,
                StartButton = startButton,
                RunningIcon = runningIcon,
            });
        }

        private bool FilterImperative(IImperativeConfig imperative, bool exactFolder)
        {
            if (!imperative.UI.Visible)
            {
                return false;
            }

            if (!imperative.Name.ToLower().Contains(this.searchFilter))
            {
                return false;
            }

            var path = imperative.GetLibraryPath();
            if (path == null)
            {
                return string.IsNullOrEmpty(this.Folder);
            }

            if (exactFolder)
            {
                var endOfFolder = path.LastIndexOf(Path.DirectorySeparatorChar);
                if (endOfFolder == -1)
                {
                    if (!string.IsNullOrEmpty(this.Folder))
                    {
                        return false;
                    }
                }
                else
                {
                    var fullPath = path.Substring(0, endOfFolder + 1);

                    if (fullPath.ToLower() != this.Folder.ToLower())
                    {
                        return false;
                    }
                }
            }
            else if (!path.ToLower().StartsWith(this.Folder.ToLower()))
            {
                return false;
            }

            return true;
        }

        private class ImperativeUIElements
        {
            public HorizontalLayoutGroupWidget Row { get; set; }

            public IconButtonWidget StartButton { get; set; }

            public ImageWidget RunningIcon { get; set; }
        }
    }
}
