using AppKit;

namespace MonoDevelop.Inspector.Mac
{
    class MacTabWrapper : ITabWrapper
    {
        private NSTabView content;

        public MacTabWrapper (NSTabView content)
        {
            this.content = content;
        }

        public object NativeObject => content;
    }
}
