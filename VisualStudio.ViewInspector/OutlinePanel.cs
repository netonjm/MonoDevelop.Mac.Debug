using System;
using AppKit;
using MonoDevelop.Inspector.Mac;
namespace VisualStudio.ViewInspector
{
    public class OutlinePanel
    {
        const ushort DeleteKey = 51;
        public ScrollContainerView EnclosingScrollView { get; }
        public OutlineView View { get; }
        public event EventHandler<NSView> RaiseFirstResponder;
        public event EventHandler<NSView> RaiseDeleteItem;
        public event EventHandler<NSView> DoubleClick;
        public event EventHandler<NSView> StartDrag;

        public OutlinePanel()
        {
            View = new OutlineView();
            EnclosingScrollView = new ScrollContainerView(View);

            View.SelectionNodeChanged += (s, e) =>
            {
                if (View.SelectedNode is WidgetNodeView nodeView)
                {
                    RaiseFirstResponder?.Invoke(this, nodeView.Wrapper);
                }
            };

            View.KeyPress += (sender, e) =>
            {
                if (e == DeleteKey)
                {
                    if (View.SelectedNode is WidgetNodeView nodeView)
                    {
                        RaiseDeleteItem?.Invoke(this, nodeView.Wrapper);
                    }
                }
            };

            View.DoubleClick += (sender, e) =>
            {
                DoubleClick?.Invoke(this, (View.SelectedNode as WidgetNodeView)?.Wrapper);
            };

            View.StartDrag += (sender, e) =>
            {
                StartDrag?.Invoke(this, (e as WidgetNodeView)?.Wrapper);
            };
        }

        public void FocusSelectedView()
        {
            var window = View.Window;
            window.MakeFirstResponder(View);
        }


        public void GenerateTree(Node node)
        {
            //data = new NodeView(window.ContentView);
            //inspectorDelegate.ConvertToNodes(window.ContentView, new NodeWrapper(data), viewMode);
            View.SetData(node);
        }

        public void FocusNode(Node node)
        {
            View.FocusNode(node);
        }
    }
}