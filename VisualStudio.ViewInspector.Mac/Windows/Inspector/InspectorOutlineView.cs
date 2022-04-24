using AppKit;
using Foundation;
using System;
using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Mac.Views;

namespace VisualStudio.ViewInspector.Mac.Windows.Inspector
{
    class ImageRowNode : ImageRowSubView
    {
        public virtual void SetData(Node node, string imageName)
        {
            image = NativeViewHelper.GetManifestImageResource(imageName);
            textField.StringValue = node.Name;
            RefreshStates();
        }
    }

    class LabelRowNode : LabelRowSubView
    {
        public void SetData(Node node)
        {
            textField.StringValue = node.Name;
        }
    }

    class InspectorOutlineView : OutlineView
    {
        class InspectorOutlineViewDelegate : OutlineViewDelegate
        {
            public override ObjCRuntime.nfloat GetRowHeight(NSOutlineView outlineView, NSObject item)
            {
                return 22;
            }

            protected const string imageNodeName = "InspectorImageNode";
            public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
            {
                var data = (TreeNode)item;
                if (data.TryGetImageName(out var imageValue))
                {
                    var view = (ImageRowNode)outlineView.MakeView(imageNodeName, this);
                    if (view == null)
                    {
                        view = new ImageRowNode();
                    }
                    view.SetData(data, imageValue);
                    view.AlphaValue = data.NativeObject is IView v && v.Hidden ? 0.5f : 1;

                    return view;
                }
                else
                {
                    var view = (NSTextField)outlineView.MakeView(identifer, this);
                    if (view == null)
                    {
                        view = NativeViewHelper.CreateLabel(((Node)item).Name);
                    }
                    view.AlphaValue = data.NativeObject is IView v && v.Hidden ? 0.5f : 1;
                    return view;
                }
            }
        }


        public Func<NSEvent, NSMenu> CreateMenuHandler { get; set; }

        public InspectorOutlineView ()
        {
            HeaderView = null;
            BackgroundColor = NSColor.Clear;
        }

        public override NSMenu MenuForEvent(NSEvent theEvent)
        {
            if (CreateMenuHandler != null)
            {
                return CreateMenuHandler.Invoke(theEvent);
            }
           
            return base.MenuForEvent(theEvent);
        }

        public override NSOutlineViewDelegate GetDelegate()
        {
            return new InspectorOutlineViewDelegate();
        }
    }
}
