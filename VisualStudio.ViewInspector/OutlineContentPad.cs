using System;
using System.Collections.Generic;
using AppKit;
using MonoDevelop.Inspector;
using MonoDevelop.Inspector.Mac;

namespace VisualStudio.ViewInspector
{
    public class OutlineContentPad : NSStackView
    {
        public event EventHandler<NSView> RaiseFirstResponder;
        public event EventHandler<NSView> RaiseDeleteItem;
        public event EventHandler<NSView> DoubleClick;

        static OutlineContentPad instance;

        public static OutlineContentPad Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OutlineContentPad();
                }

                return instance;
            }
        }

        #region SELECTION

        IView SelectedView;

        readonly IBorderedWindow viewSelectedOverlayWindow;
        bool IsViewSelected;
        internal bool IsFirstResponderOverlayVisible
        {
            get => IsViewSelected;
            set
            {
                IsViewSelected = value;

                if (viewSelectedOverlayWindow != null)
                {
                    //viewSelectedOverlayWindow.SetParentWindow(selectedWindow);
                    viewSelectedOverlayWindow.Visible = value;
                    if (SelectedView != null)
                    {
                        viewSelectedOverlayWindow.AlignWith(SelectedView);
                    }
                    viewSelectedOverlayWindow.OrderFront();
                }
            }
        }

        #endregion

        public void RefreshCurrentView(NSView widget)
        {
            if (widget == null)
            {
                return;
            }
            SelectedView = new TreeViewItemView(widget);
            viewSelectedOverlayWindow.AlignWith(SelectedView);
        }

        OutlinePanel outlinePanel;
        public OutlineContentPad()
        {
            viewSelectedOverlayWindow = new BorderedWindow(CoreGraphics.CGRect.Empty, AppKit.NSColor.Blue);
            outlinePanel = new OutlinePanel();

            outlinePanel.RaiseFirstResponder += (s, e) =>
            {
                SelectedView = new TreeViewItemView(e);
                IsFirstResponderOverlayVisible = true;
                RaiseFirstResponder?.Invoke(s, e);
            };
            outlinePanel.RaiseDeleteItem += (s, e) =>
            {
                RaiseDeleteItem?.Invoke(s, e);
            };

            outlinePanel.DoubleClick += (s, e) =>
            {
                DoubleClick?.Invoke(s, e);
            };

            //var widget = new Gtk.GtkNSViewHost(outlinePanel.EnclosingScrollView, false);
            //widget.Focused += FigmaDesignerOutlinePad_Focused;

            AddArrangedSubview(outlinePanel.EnclosingScrollView);
            //PackStart(widget, true, true, 0);
            //ShowAll();

            //Focused += FigmaDesignerOutlinePad_Focused;
        }

        void FigmaDesignerOutlinePad_Focused(object o, Gtk.FocusedArgs args)
        {
            outlinePanel.FocusSelectedView();
        }

        public void Focus(NSView model)
        {
            if (data != null && model != null)
            {
                var found = Search(data, model);
                if (found != null)
                {
                    outlinePanel.FocusNode(found);
                }
            }
            RefreshCurrentView(model);
        }

        static WidgetNodeView Search(WidgetNodeView nodeView, NSView view)
        {
            if (nodeView.Wrapper != null && nodeView.Wrapper == view)
            {
                return nodeView;
            }

            if (nodeView.ChildCount == 0)
            {
                return null;
            }

            for (int i = 0; i < nodeView.ChildCount; i++)
            {
                var node = (WidgetNodeView)nodeView.GetChild(i);
                var found = Search(node, view);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        WidgetNodeView data;
        public void GenerateTree(NSView mainView)
        {
            Items.Clear();
            data = new WidgetNodeView(mainView);
            ConvertToNodes(mainView, data);
            outlinePanel.GenerateTree(data);
        }

        public List<WidgetNodeView> Items = new List<WidgetNodeView>();

        void ConvertToNodes(NSView figmaNode, WidgetNodeView node)
        {
            var current = new WidgetNodeView(figmaNode);
            node.AddChild(current);
            Items.Add(current);

            foreach (var item in figmaNode.Subviews)
            {
                try
                {
                    ConvertToNodes(item, current);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }

    public class WidgetNodeView : Node
    {
        static string GetName(NSView view)
        {
            var name = view.GetType().ToString(); //string.Format(view.GetType(), , view.id ?? "N.I", view.type);
            if (view.Hidden)
            {
                name += $" (hidden)";
            }
            return name;// + $"({view.}|{view.HasFocus}|{view.Sensitive})";
        }

        public readonly NSView Wrapper;

        public WidgetNodeView(NSView view) : base(GetName(view))
        {
            this.Wrapper = view;
        }
    }
}