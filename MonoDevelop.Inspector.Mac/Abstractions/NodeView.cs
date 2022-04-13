namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class NodeView : INodeView
    {
        private TreeNodeView nodel;

        public NodeView(TreeNodeView nodel)
        {
            this.nodel = nodel;
        }

        public object NativeObject => nodel;

        public void AddChild(INodeView nodel)
        {
            this.nodel.AddChild(nodel.NativeObject as TreeNodeView);
        }
    }
}