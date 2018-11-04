using AppKit;
using System.Collections.Generic;

namespace MonoDevelop.Inspector.Mac
{
    class MacTabView : ITabView
    {
        readonly NSTabView tabView;
        public List<IViewWrapper> Pages { get; } = new List<IViewWrapper>();
        public MacTabView()
        {
            tabView = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false };
        }

        public object NativeObject => tabView;

        List<NSTabViewItem> pages = new List<NSTabViewItem>();

        public void Add(IViewWrapper viewWrapper)
        {
            Pages.Add(viewWrapper);
            var tabItem = new NSTabViewItem() { View = viewWrapper.NativeView as NSView };
            pages.Add(tabItem);
            tabView.Add(tabItem);
        }

        public void Remove(IViewWrapper viewWrapper)
        {
            Pages.Remove(viewWrapper);
            tabView.Remove(viewWrapper.NativeView as NSTabViewItem);
        }
    }
}
