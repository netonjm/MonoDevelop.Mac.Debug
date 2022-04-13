using AppKit;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class Color : IColor
    {
        NSColor image;
        public Color(NSColor image)
        {
            this.image = image;
        }

        public object NativeObject => image;
    }
}