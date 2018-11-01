using AppKit;

namespace MonoDevelop.Inspector.Mac
{
    public class MacImageViewWrapper : IImageViewWrapper
    {
        private NSImageView imageView;

        public MacImageViewWrapper(NSImageView imageView)
        {
            this.imageView = imageView;
        }

        public object NativeObject => imageView;
    }

    public class MacButtonWrapper : IButtonWrapper
    {
        private NSButton button;

        public MacButtonWrapper(NSButton imageView)
        {
            this.button = imageView;
        }

        public object NativeObject => button;
    }
}