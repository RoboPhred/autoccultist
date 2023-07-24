namespace AutoccultistNS.UI
{
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class ViewWindow<TWindowHost> : AbstractWindow, IWindowViewHost<TWindowHost>
        where TWindowHost : class, IWindowViewHost<TWindowHost>
    {
        private IWindowView<TWindowHost> view;
        private IWindowView<TWindowHost> persistedView;

        private Stack<IWindowView<TWindowHost>> viewStack = new();

        WidgetMountPoint IWindowViewHost<TWindowHost>.Content => this.Content;

        WidgetMountPoint IWindowViewHost<TWindowHost>.Footer => this.Footer;

        protected abstract string DefaultTitle { get; }

        protected virtual Sprite DefaultIcon { get; } = null;

        protected virtual IWindowView<TWindowHost> DefaultView { get; } = null;

        protected virtual bool PersistViewOnClose { get; } = false;

        protected IWindowView<TWindowHost> View
        {
            get
            {
                return this.view;
            }

            set
            {
                if (!this.IsVisible)
                {
                    this.persistedView = value;
                    return;
                }

                this.DetatchView();

                this.view = value;

                if (this.IsVisible)
                {
                    this.AttachView();
                }
            }
        }

        void IWindowViewHost<TWindowHost>.ReplaceView(IWindowView<TWindowHost> view)
        {
            this.ReplaceView(view);
        }

        void IWindowViewHost<TWindowHost>.PushView(IWindowView<TWindowHost> view)
        {
            this.PushView(view);
        }

        void IWindowViewHost<TWindowHost>.PopView()
        {
            this.PopView();
        }

        protected void ReplaceView(IWindowView<TWindowHost> view)
        {
            this.View = view;
        }

        protected void PushView(IWindowView<TWindowHost> view)
        {
            if (this.View != null)
            {
                this.viewStack.Push(this.View);
            }

            this.View = view;
        }

        protected void PopView()
        {
            if (this.viewStack.Count > 0)
            {
                this.View = this.viewStack.Pop();
            }
            else
            {
                this.View = this.DefaultView;
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            if (this.view == null)
            {
                this.view = this.persistedView ?? this.DefaultView;
            }

            this.persistedView = null;

            this.AttachView();
        }

        protected override void OnClose()
        {
            base.OnClose();

            if (this.PersistViewOnClose)
            {
                this.persistedView = this.view;
            }

            this.DetatchView();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            this.view?.Update();
        }

        private void DetatchView()
        {
            if (this.view != null)
            {
                this.view.Detatch();
                this.view = null;
            }

            this.Icon.Clear();
            this.Content.Clear();
            this.Footer.Clear();
        }

        private void AttachView()
        {
            this.Icon.Clear();
            this.Content.Clear();
            this.Footer.Clear();

            var sprite = this.view?.Icon ?? this.DefaultIcon;
            if (sprite != null)
            {
                this.Icon.AddImage("Icon")
                    .SetSprite(sprite);
            }

            this.Title = this.view?.Title ?? this.DefaultTitle;

            if (this.view != null)
            {
                this.view.Attach(this as TWindowHost);
            }
        }
    }
}
