using AppKit;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class TabView : ITab
    {
        private NSTabView content;

        public TabView (NSTabView content)
        {
            this.content = content;
        }

        public object NativeObject => content;
    }
}
