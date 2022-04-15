using AppKit;
using System;
using VisualStudio.ViewInspector.Abstractions;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class ImageView : IImageView
    {
        private NSImageView imageView;

        public ImageView(NSImageView imageView)
        {
            this.imageView = imageView;
        }

        public object NativeObject => imageView;
    }

    internal class Button : IButton, IDisposable
    {
        private NSButton button;

        public event EventHandler Pressed;

        public Button(NSButton imageView)
        {
            this.button = imageView;
            this.button.Activated += Button_Activated;
        }

        void Button_Activated(object sender, EventArgs e)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        public object NativeObject => button;

        public void SetTooltip(string v)
        {
            button.ToolTip = v;
        }

        public void SetWidth(int width)
        {
            //TODO: This is wrong 2 time
            button.WidthAnchor.ConstraintEqualTo(width).Active = true;
        }

        public void Dispose()
        {
            this.button.Activated -= Button_Activated;
        }
    }
}