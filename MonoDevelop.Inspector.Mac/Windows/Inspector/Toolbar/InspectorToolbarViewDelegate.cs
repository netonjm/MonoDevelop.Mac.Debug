using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace MonoDevelop.Inspector.Mac
{
    class InspectorToolbarViewDelegateFlowLayout : NSCollectionViewFlowLayout
    {
        public InspectorToolbarViewDelegateFlowLayout()
        {

        }
    }

    class InspectorToolbarViewDelegate : NSCollectionViewDelegateFlowLayout
    {
        public bool IsOnlyImage { get; set; }
        public bool IsShowCategories { get; set; }

        public event EventHandler<NSSet> SelectionChanged;

        public override void ItemsSelected(NSCollectionView collectionView, NSSet indexPaths)
        {
            SelectionChanged?.Invoke(this, indexPaths);
        }

        public override CGSize SizeForItem(NSCollectionView collectionView, NSCollectionViewLayout collectionViewLayout, NSIndexPath indexPath)
        {
            var delegateFlowLayout = (InspectorToolbarViewDelegateFlowLayout)collectionViewLayout;
            if (delegateFlowLayout.SectionAtIndexIsCollapsed((nuint)indexPath.Section))
            {
                return new CGSize(0, 0);
            }
            if (IsOnlyImage)
            {
                return InspectorToolbarImageViewItem.Size;
            }
            var sectionInset = delegateFlowLayout.SectionInset;
            return new CGSize(collectionView.Frame.Width - sectionInset.Left - sectionInset.Right, InspectorToolbarViewItem.ItemHeight);
        }

        public override NSEdgeInsets InsetForSection(NSCollectionView collectionView, NSCollectionViewLayout collectionViewLayout, nint section)
        {
            return new NSEdgeInsets(0, 0, 0, 0);
        }

        public override CGSize ReferenceSizeForHeader(NSCollectionView collectionView, NSCollectionViewLayout collectionViewLayout, nint section)
        {
            if (!IsShowCategories)
            {
                return CGSize.Empty;
            }
            var delegateFlowLayout = ((InspectorToolbarViewDelegateFlowLayout)collectionViewLayout);
            var sectionInset = delegateFlowLayout.SectionInset;
            return new CGSize(collectionView.Frame.Width, InspectorToolbarHeaderViewItem.SectionHeight);
        }

        public override CGSize ReferenceSizeForFooter(NSCollectionView collectionView, NSCollectionViewLayout collectionViewLayout, nint section)
        {
            return CGSize.Empty;
        }

        public override NSSet ShouldDeselectItems(NSCollectionView collectionView, NSSet indexPaths)
        {
            return indexPaths;
        }

        public override NSSet ShouldSelectItems(NSCollectionView collectionView, NSSet indexPaths)
        {
            return indexPaths;
        }
    }
}