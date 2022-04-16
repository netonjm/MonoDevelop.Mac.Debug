using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Mac.Views;

namespace VisualStudio.ViewInspector.Mac.Abstractions
{
    internal class NodeView : INodeView
    {
        private TreeNode nodel;

        public NodeView(TreeNode nodel)
        {
            this.nodel = nodel;
        }

        public object NativeObject => nodel;

        public void AddChild(INodeView nodel)
        {
            this.nodel.AddChild(nodel.NativeObject as TreeNode);
        }
    }
}