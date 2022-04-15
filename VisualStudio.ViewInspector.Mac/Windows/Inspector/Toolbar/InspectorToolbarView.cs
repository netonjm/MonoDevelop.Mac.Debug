﻿using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using VisualStudio.ViewInspector.Abstractions;

namespace VisualStudio.ViewInspector.Mac.Windows.Inspector
{
    [Register("MacInspectorToolbarView")]
    class MacInspectorToolbarView : NSCollectionView
    {
        InspectorToolbarViewDataSource dataSource;
        InspectorToolbarViewDelegate collectionViewDelegate;
        InspectorToolbarViewDelegateFlowLayout flowLayout;

        readonly List<CollectionHeaderItem> items = new List<CollectionHeaderItem>();

        public void SetData(List<CollectionHeaderItem> items)
        {
            this.items.Clear();
            this.items.AddRange(items);
        }

        public override void SetFrameSize(CGSize newSize)
        {
            if (Frame.Size != newSize)
            {
                flowLayout.InvalidateLayout();
            }
            base.SetFrameSize(newSize);
        }

        public override NSView MakeSupplementaryView(NSString elementKind, string identifier, NSIndexPath indexPath)
        {
            var item = MakeItem(identifier, indexPath) as InspectorToolbarHeaderViewItem;
            var selectedItem = items[(int)indexPath.Section];
            item.TextField.StringValue = selectedItem.Label ?? "";
            item.TextField.AccessibilityTitle = selectedItem.AccessibilityTitle ?? "";
            item.TextField.AccessibilityHelp = selectedItem.AccessibilityHelp ?? "";
            item.IsCollapsed = flowLayout.SectionAtIndexIsCollapsed((nuint)indexPath.Section);

            item.ExpandButton.Activated += (sender, e) => {
                ToggleSectionCollapse(item.View);
                item.IsCollapsed = flowLayout.SectionAtIndexIsCollapsed((nuint)indexPath.Section);
                ResizeViews();
            };

            return item.View;
        }

        // Called when created from unmanaged code
        public MacInspectorToolbarView() : base()
        {
            Initialize();
        }

        // Called when created from unmanaged code
        public MacInspectorToolbarView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MacInspectorToolbarView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public override void MouseDown(NSEvent theEvent)
        {
            base.MouseDown(theEvent);
            var point = ConvertPointFromView(theEvent.LocationInWindow, null);
            var indexPath = base.GetIndexPath(point);
            if (indexPath != null) {
                SelectedItem = items[(int)indexPath.Section].Items[(int)indexPath.Item];
            }
            if (SelectedItem != null && theEvent.ClickCount > 1)
            {
                OnActivateSelectedItem(EventArgs.Empty);
            }
        }
        public event EventHandler ActivateSelectedItem;
        protected virtual void OnActivateSelectedItem(EventArgs args)
        {
            ActivateSelectedItem?.Invoke(this, args);
        }

        // Shared initialization code
        public void Initialize()
        {
            flowLayout = new InspectorToolbarViewDelegateFlowLayout();
            flowLayout.SectionHeadersPinToVisibleBounds = true;
            flowLayout.MinimumInteritemSpacing = 2;
            flowLayout.MinimumLineSpacing = 0;
            flowLayout.SectionFootersPinToVisibleBounds = false;
            CollectionViewLayout = flowLayout;
            Delegate = collectionViewDelegate = new InspectorToolbarViewDelegate();

            collectionViewDelegate.SelectionChanged += (sender, e) => {
                if (e.Count == 0)
                {
                    return;
                }
                if (e.AnyObject is NSIndexPath indexPath)
                {
                    SelectedItem = items[(int)indexPath.Section].Items[(int)indexPath.Item];
                }
            };

            Selectable = true;
            AllowsEmptySelection = true;
            DataSource = dataSource = new InspectorToolbarViewDataSource(items);
            ShowCategories(false);

        }

        void CollectionViewDelegate_SelectionChanged(object sender, NSSet e)
        {
            if (e.Count == 0)
            {
                return;
            }
            if (e.AnyObject is NSIndexPath indexPath)
            {
                SelectedItem = items[(int)indexPath.Section].Items[(int)indexPath.Item];
            }
        }

        public CollectionItem SelectedItem { get; set; }

        internal void ResizeViews()
        {
            flowLayout.InvalidateLayout();
        }

        public void ShowCategories()
        {
            ShowCategories(!isShowCategories);
        }

        public void ShowCategories(bool value)
        {
            isShowCategories = value;
            collectionViewDelegate.IsShowCategories = value;
            ResizeViews();
        }

        public void ShowOnlyImages (bool value)
        {
            collectionViewDelegate.IsOnlyImage = dataSource.IsOnlyImage = isImageMode = value;
            ReloadData();
        }

        public bool IsImageMode => isImageMode;
        internal bool isShowCategories, isImageMode;
    }

    class CollectionHeaderItem
    {
        public string Label { get; set; }
        public string AccessibilityTitle { get; internal set; }
        public string AccessibilityHelp { get; internal set; }

        public List<CollectionItem> Items = new List<CollectionItem>();
    }

    class CollectionItem
    {
        public ToolbarView TypeOfView { get; set; }
        public string Label { get; set; }
        public IImage Image { get; set; }
        public string ToolTip { get; set; }
        public string AccessibilityLabel { get; set; }
        public string AccessibilityHelp { get; set; }
        public string Description { get; internal set; }
    }
}
