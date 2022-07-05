using AppKit;
using CoreAnimation;
using CoreGraphics;
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

    class DropSelectionView : NSView
    {
        const float Offset = 1;
        CALayer circleLayer, lineLayer;
        public NSColor Color { get; set; } = NSColor.ControlAccent;

        public override bool WantsDefaultClipping => false;

        public DropSelectionView()
        {
            WantsLayer = true;
            Layer.MasksToBounds = false;
            circleLayer = new CALayer();
            circleLayer.MasksToBounds = false;
            Layer.AddSublayer(circleLayer);

            lineLayer = new CALayer();
            lineLayer.MasksToBounds = false;
            Layer.AddSublayer(lineLayer);
        }

        void Recreate()
        {
            var radius = Frame.Height;
            circleLayer.Frame = new CGRect(0, 0, radius, radius);
            circleLayer.CornerRadius = radius / 2;
            circleLayer.BackgroundColor = NSColor.Clear.CGColor;

            circleLayer.BorderWidth = radius / 3.5f;
            circleLayer.BorderColor = Color.CGColor;

            var lineHeight = radius / 4;
            var lineWidth = Math.Max(0, Frame.Width - radius);
            var middle = radius / 2 - lineHeight / 2;
            lineLayer.Frame = new CGRect(radius - Offset, middle, lineWidth + Offset, lineHeight);
            lineLayer.BackgroundColor = Color.CGColor;
        }

        public override void SetFrameSize(CGSize newSize)
        {
            base.SetFrameSize(newSize);
            Recreate();
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
        internal const string ExtensibleDragNodeType = "com.microsoft.cocoaextensiblepadcellview";
        internal const string NSFinderNodeType = "com.apple.finder.node";

        class InspectorImageRowCell : ImageRowNode
        {
            internal const float LeftRowMargin = 3;
            internal const float DefaultIconSizeWidth = 16;
            
            WeakReference<InspectorOutlineView> weakOutlineView;

            public InspectorImageRowCell(InspectorOutlineView outlineView)
            {
                weakOutlineView = new WeakReference<InspectorOutlineView>(outlineView);

            }

            internal TreeNode Data;

            public override void SetData(Node node, string imageName)
            {
                Data = (TreeNode)node;

                UnregisterDraggedTypes();

                base.SetData(node, imageName);

                if (Data.Content is IView v)
                {
                    AlphaValue = v.Hidden ? 0.5f : 1;
                    RegisterForDraggedTypes(new string[] { ExtensibleDragNodeType, NSFinderNodeType });
                }
            }

            [Export("prepareForDragOperation:")]
            public bool PrepareForDragOperation(INSDraggingInfo sender)
            {
                if (weakOutlineView.TryGetTarget(out var target)) {
                    return target.AllowsDropOperation(Data);
                }
                return false;
            }

            [Export("draggingEntered:")]
            public NSDragOperation DraggingEntered(INSDraggingInfo sender)
            {
                var value = DraggingUpdated(sender);
                return value;
            }

            bool isDragging = false;
            internal bool IsDragging
            {
                get => isDragging;
                set
                {
                    if (isDragging == value)
                    {
                        return;
                    }
                    isDragging = value;
                    if (weakOutlineView.TryGetTarget(out var target))
                    {
                        target.OnCellDragStateChanged(this);
                    }
                }
            }

            [Export("draggingUpdated:")]
            public NSDragOperation DraggingUpdated(INSDraggingInfo sender)
            {
                if (weakOutlineView.TryGetTarget(out var target))
                {
                    if (target.AllowsDropOperation(Data))
                    {
                        IsDragging = true;
                        return sender.DraggingSourceOperationMask;
                    }
                }
                return NSDragOperation.None;
            }

            [Export("draggingEnded:")]
            public void DraggingEnded(INSDraggingInfo sender)
            {
                IsDragging = false;
                if (weakOutlineView.TryGetTarget(out var target))
                {
                    target.PerformDragEnded();
                }
            }

            [Export("draggingExited:")]
            public void DraggingExited(INSDraggingInfo sender)
            {
                IsDragging = false;
            }

            [Export("performDragOperation:")]
            public bool PerformDragOperation(INSDraggingInfo sender)
            {
                IsDragging = false;
                if (weakOutlineView.TryGetTarget(out var target))
                {
                    target.PerformDragOperation(sender, this);
                }
                return true;
            }
        }

        readonly WeakReference<DropSelectionView> weakDropSelectionView = new(null);

        const float dropSelectionViewHeight = 8;
        const float dropSelectionViewHeightOffset = 4;

        public event EventHandler DragOperationCompleted;

        #region Drag & Drop

        private void OnCellDragStateChanged(InspectorImageRowCell cocoaExtensiblePadCellView)
        {
            if (cocoaExtensiblePadCellView.IsDragging)
            {
                if (!weakDropSelectionView.TryGetTarget(out var dropSelectionView))
                {
                    dropSelectionView = new DropSelectionView();
                    weakDropSelectionView.SetTarget(dropSelectionView);
                }
                AddSubview(dropSelectionView);

                var frame = cocoaExtensiblePadCellView.ConvertRectFromView(cocoaExtensiblePadCellView.Frame, this);
                var x = InspectorImageRowCell.LeftRowMargin + InspectorImageRowCell.DefaultIconSizeWidth / 2f - dropSelectionViewHeight / 2f;
                dropSelectionView.Frame = new CGRect(cocoaExtensiblePadCellView.Frame.X + x, frame.Y + frame.Height - dropSelectionViewHeightOffset, cocoaExtensiblePadCellView.Frame.Width - x, dropSelectionViewHeight);
            }
            else
            {
                if (weakDropSelectionView.TryGetTarget(out var dropSelectionView))
                {
                    dropSelectionView.RemoveFromSuperview();
                    weakDropSelectionView.SetTarget(null);
                }
            }
        }

        internal TreeNode DragData;

        private void PerformDragStarted()
        {
            DragData = (TreeNode) SelectedNode;
        }

        private void PerformDragEnded()
        {
            //Drag ended
            DragData = null;
        }

        NSView GetNativeView (TreeNode treeNode)
        {
            if (treeNode.Content is IView iView && iView.NativeObject is NSView view)
            {
                return view;
            }
            return null;
        }

        private void PerformDragOperation(INSDraggingInfo sender, InspectorImageRowCell inspectorImageRowCell)
        {
            var origin = GetNativeView(DragData);
            var target = GetNativeView(inspectorImageRowCell.Data);
            if (origin != null && target != null)
            {
                origin.RemoveFromSuperview();

                if (target is NSStackView stackView)
                {
                    stackView.AddArrangedSubview(origin);
                }
                else
                {
                    target.AddSubview(origin);
                }

                DragOperationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool AllowsStartDragOperation(TreeNode node)
        {
            if (node.Content is IView)
            {
                return true;
            }
            return false;
        }

        private bool AllowsDropOperation(TreeNode node)
        {
            if (node.Content is IView iView)
            {
                if (iView.NativeObject == DragData.Content.NativeObject)
                    return false;

                if (iView.NativeObject is NSView view)
                {
                    //parent stackview allows 
                    //if (view.Superview is NSStackView)
                    //{
                    //    return true;
                    //}

                    if (view is NSOutlineView)
                        return false;
                    if (view is NSTableView)
                        return false;
                    if (view is NSTextField)
                        return false;
                    if (view is NSButton)
                        return false;
                    if (view is NSComboBox)
                        return false;
                    if (view is NSPopUpButton)
                        return false;

                    return true;
                }
            }
            return false;
        }

        #endregion

        class InspectorOutlineViewDelegate : OutlineViewDelegate
        {
            public override nfloat GetRowHeight(NSOutlineView outlineView, NSObject item)
            {
                return 22;
            }

            protected const string imageNodeName = "InspectorImageNode";
            public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
            {
                var data = (TreeNode)item;
                if (data.TryGetImageName(out var imageValue))
                {
                    var view = (InspectorImageRowCell)outlineView.MakeView(imageNodeName, this);
                    if (view == null)
                    {
                        view = new InspectorImageRowCell((InspectorOutlineView) outlineView);
                    }
                    view.SetData(data, imageValue);
                   
                    return view;
                }
                else
                {
                    var view = (NSTextField)outlineView.MakeView(identifer, this);
                    if (view == null)
                    {
                        view = NativeViewHelper.CreateLabel(((Node)item).Name);
                    }
                    view.AlphaValue = data.Content is IView v && v.Hidden ? 0.5f : 1;
                    return view;
                }
            }
        }

        class InspectorOutlineViewDataSource : OutlineViewDataSource, INSPasteboardItemDataProvider
        {
            public InspectorOutlineViewDataSource(MainNode mainNode) : base(mainNode)
            {
            }

            public override INSPasteboardWriting PasteboardWriterForItem(NSOutlineView outlineView, NSObject item)
            {
                var row = outlineView.RowForItem(item);
                if (!outlineView.SelectedRows.Contains((nuint)row))
                {
                    outlineView.SelectRow(row, false);
                }

                if (outlineView is InspectorOutlineView extensibleOutlineView)
                {
                    extensibleOutlineView.PerformDragStarted();

                    if (extensibleOutlineView.AllowsStartDragOperation((TreeNode)item))
                    {
                        var pbItem = new NSPasteboardItem();
                        pbItem.SetDataProviderForTypes(this, new string[] { ExtensibleDragNodeType });
                        return pbItem;
                    }
                }

                return null;
            }

            public void ProvideDataForType(NSPasteboard pasteboard, NSPasteboardItem item, string type)
            {
                //not used
            }

            public void FinishedWithDataProvider(NSPasteboard pasteboard)
            {
                //not used
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

        public override OutlineViewDataSource GetDataSource(MainNode node)
        {
            return new InspectorOutlineViewDataSource(node);
        }

        public override NSOutlineViewDelegate GetDelegate()
        {
            return new InspectorOutlineViewDelegate();
        }
    }
}
