using AppKit;
using VisualStudio.ViewInspector.Abstractions;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class Image : IImage
    {
        NSImage image;
        public Image(NSImage image)
        {
            this.image = image;
        }

        public object NativeObject => image;
    }
}