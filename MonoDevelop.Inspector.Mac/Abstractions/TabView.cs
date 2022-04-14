using AppKit;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class TabViewItem : ITabItem
    {
        private NSTabViewItem content;

        public TabViewItem(NSTabViewItem content)
        {
            this.content = content;
        }

        public object NativeObject => content;

        public string NodeName
        {
            get {
                var label = content.Label;
                if (string.IsNullOrEmpty(label))
                    return label;
                return $"\"{label}\" {content.GetType().FullName}";
            }
        }
    }
}
