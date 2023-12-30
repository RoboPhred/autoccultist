namespace AutoccultistNS.UI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoccultistNS.Config;
    using Roost.Piebald;
    using UnityEngine;

    public class ImperativeFolderView : IWindowView<ImperativeListWindow.IWindowContext>, IViewHasTitle
    {
        private readonly TimeSpan updateInterval = TimeSpan.FromSeconds(1);
        private readonly Dictionary<IImperativeConfig, ImperativeRowFactory.ImperativeUIElements> imperativeUIs = new();
        private readonly string rootTitle;
        private DateTime lastUpdate = DateTime.MinValue;

        private ImperativeListWindow.IWindowContext window;

        private float scrollY = 1;

        private string searchFilter = string.Empty;

        public ImperativeFolderView(string rootTitle, IReadOnlyCollection<IImperativeConfig> collection, string folder)
        {
            this.rootTitle = rootTitle;
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
                    return this.rootTitle;
                }

                var captialized = string.Join("/", this.Folder.Split("\\").Select(x => x.Capitalize()));
                return $"{this.rootTitle}: {captialized.Substring(0, captialized.Length - 1)}";
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
                    mountPoint.AddLayoutItem("Spacer")
                        .SetExpandWidth();

                    mountPoint.AddTextButton("BackButton")
                        .SetText("Back")
                        .OnClick(() => this.window.PopView());

                    mountPoint.AddLayoutItem("Spacer")
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

            ImperativeRowFactory.UpdateImperatives(this.imperativeUIs);
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
                                ImperativeRowFactory.BuildImperativeRow(imperative, mountPoint, this.window.StartImperative);
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
                    this.window.PushView(new ImperativeFolderView(this.rootTitle, this.Collection, this.Folder + folder + Path.DirectorySeparatorChar));
                })
                .AddContent(mountPoint =>
                {
                    mountPoint.AddLayoutItem("IconContainer")
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

            mountPoint.AddLayoutItem("Icon")
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
    }
}
