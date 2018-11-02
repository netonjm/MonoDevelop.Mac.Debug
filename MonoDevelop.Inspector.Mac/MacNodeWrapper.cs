namespace MonoDevelop.Inspector.Mac
{
    internal class MacNodeWrapper : INodeView
    {
        private NodeView nodel;

        public MacNodeWrapper(NodeView nodel)
        {
            this.nodel = nodel;
        }

        public object NativeObject => nodel;

        public void AddChild(INodeView nodel)
        {
            this.nodel.AddChild(nodel.NativeObject as NodeView);
        }
    }
}