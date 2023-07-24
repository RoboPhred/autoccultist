namespace AutoccultistNS.UI
{
    using UnityEngine;

    public class ViewWindow<TWindowHost> : AbstractWindow, IWindowViewHost<TWindowHost>
        where TWindowHost : class, IWindowViewHost<TWindowHost>
    {
        private IWindowView<TWindowHost> view;

        WidgetMountPoint IWindowViewHost<TWindowHost>.Content => this.Content;

        WidgetMountPoint IWindowViewHost<TWindowHost>.Footer => this.Footer;

        protected virtual Sprite DefaultIcon { get; } = null;

        protected IWindowView<TWindowHost> View
        {
            get
            {
                return this.view;
            }

            set
            {
                this.DetatchView();

                this.view = value;

                if (this.IsOpen)
                {
                    this.AttachView();
                }
            }
        }

        void IWindowViewHost<TWindowHost>.ReplaceView(IWindowView<TWindowHost> view)
        {
            this.View = view;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            this.AttachView();
        }

        protected override void OnClose()
        {
            base.OnClose();

            this.view?.Detatch();
            this.Content.Clear();
            this.Footer.Clear();

            // Preserve this.view so we can re-attach it on open.
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
        }

        private void AttachView()
        {
            this.Icon.Clear();
            var sprite = this.view?.Icon ?? this.DefaultIcon;
            if (sprite != null)
            {
                this.Icon.AddImage("Icon")
                    .SetSprite(sprite);
            }

            this.Content.Clear();
            this.Footer.Clear();

            if (this.view != null)
            {
                this.view.Attach(this as TWindowHost);
            }
        }
    }
}
