using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;

namespace MonoDevelop.Inspector.Mac
{
    public class MacToolbarCollectionViewItem : NSCollectionViewItem
    {
        public const int ItemHeight = 60;
        internal const string Name = "LabelViewItem";

        public override string Description => TextField.StringValue;

        public override bool Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                if (contentCollectionView != null)
                {
                    contentCollectionView.IsSelected = value;
                }
            }
        }

        public static NSColor CellBackgroundSelectedColor = NSColor.FromRgba(red: 0.33f, green: 0.55f, blue: 0.92f, alpha: 1.0f);
        public static NSColor CellBorderSelectedColor = NSColor.Black;
        public static NSColor CellBackgroundColor = NSColor.FromRgba(red: 0.25f, green: 0.25f, blue: 0.25f, alpha: 1.0f);

        ContentCollectionViewItem contentCollectionView;
        public override void LoadView()
        {
            View = contentCollectionView = new ContentCollectionViewItem(Name);
            View.AccessibilityElement = false;
            contentCollectionView.BackgroundSelectedColor = CellBackgroundSelectedColor;
            contentCollectionView.BorderSelectedColor = CellBorderSelectedColor;
            contentCollectionView.BackgroundColor = CellBackgroundColor;
            var stackView = NativeViewHelper.CreateHorizontalStackView();
            View.AddSubview(stackView);
            stackView.LeftAnchor.ConstraintEqualToAnchor(View.LeftAnchor, 5).Active = true;
            stackView.CenterYAnchor.ConstraintEqualToAnchor(View.CenterYAnchor, 0).Active = true; ;

            ImageView = new NSImageView { TranslatesAutoresizingMaskIntoConstraints = false };
            ImageView.ImageScaling = NSImageScale.None;
            ImageView.ImageAlignment = NSImageAlignment.Center;

            var container = new NSView() {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            container.AddSubview(ImageView);

          
            stackView.AddArrangedSubview(container);

            ImageView.CenterXAnchor.ConstraintEqualToAnchor(container.CenterXAnchor, 0).Active = true;
            ImageView.CenterYAnchor.ConstraintEqualToAnchor(container.CenterYAnchor, 0).Active = true;

            container.WidthAnchor.ConstraintEqualToConstant (60).Active = true;
            container.HeightAnchor.ConstraintEqualToConstant(60).Active = true;

            ImageView.WidthAnchor.ConstraintEqualToConstant(50).Active = true;
            ImageView.HeightAnchor.ConstraintEqualToConstant(50).Active = true;

            TextField = NativeViewHelper.CreateLabel("", NativeViewHelper.GetSystemFont(false, (int)NSFont.SmallSystemFontSize));
            stackView.AddArrangedSubview(TextField);

            DescriptionTextField = NativeViewHelper.CreateLabel("", NativeViewHelper.GetSystemFont(false, (int)8));
            stackView.AddArrangedSubview(DescriptionTextField);
        }

        class ContainerView : NSView
        {
            public override CGSize IntrinsicContentSize => new CGSize(60, 60);
        }

        public NSTextField DescriptionTextField
        {
            get;
            set;
        }

        public MacToolbarCollectionViewItem(IntPtr handle) : base(handle)
        {

        }
    }

    [Register("HeaderCollectionViewItem")]
    public class HeaderCollectionViewItem : NSCollectionViewItem
    {
        public static NSColor HeaderCellBackgroundSelectedColor = NSColor.FromRgb(0.29f, green: 0.29f, blue: 0.29f);// NSColor.ControlBackground;
        public static NSColor HeaderCellBackgroundColor = HeaderCellBackgroundSelectedColor;

        public static NSImage ExpandedImage = NSImage.ImageNamed("expander-collapse.png");
        public static NSImage CollapsedImage = NSImage.ImageNamed("expander-expand.png");
        public const int ExpandButtonSize = 20;
        public const int SectionHeight = 25;

        internal const string Name = "HeaderViewItem";

        public NSButton ExpandButton { get; private set; }

        bool isCollapsed;
        public bool IsCollapsed
        {
            get => isCollapsed;
            internal set
            {
                isCollapsed = value;
                ExpandButton.Image = value ? CollapsedImage : ExpandedImage;
            }
        }

        ContentCollectionViewItem contentCollectionView;
        public override void LoadView()
        {
            View = contentCollectionView = new ContentCollectionViewItem(Name);
            contentCollectionView.BackgroundColor = HeaderCellBackgroundSelectedColor;
            contentCollectionView.BackgroundSelectedColor = HeaderCellBackgroundColor;

            var stackView = NativeViewHelper.CreateHorizontalStackView();
            View.AddSubview(stackView);
            stackView.LeftAnchor.ConstraintEqualToAnchor(View.LeftAnchor, 10).Active = true; ;
            stackView.CenterYAnchor.ConstraintEqualToAnchor(View.CenterYAnchor, 0).Active = true; ;
            stackView.RightAnchor.ConstraintEqualToAnchor(View.RightAnchor, -7).Active = true; ;
            TextField = NativeViewHelper.CreateLabel("", NativeViewHelper.GetSystemFont(false, (int)NSFont.SmallSystemFontSize));
            stackView.AddArrangedSubview(TextField);

            TextField.SetContentCompressionResistancePriority(250, NSLayoutConstraintOrientation.Horizontal);
            TextField.SetContentHuggingPriorityForOrientation(250, NSLayoutConstraintOrientation.Horizontal);
            ExpandButton = new NSButton()
            {
                Image = ExpandedImage,
                TranslatesAutoresizingMaskIntoConstraints = true,
                Bordered = false,
                BezelStyle = NSBezelStyle.ShadowlessSquare,
                Title = "",
                ImageScaling = NSImageScale.AxesIndependently
            };
            ExpandButton.SetButtonType(NSButtonType.OnOff);


            ExpandButton.HeightAnchor.ConstraintEqualToConstant(ExpandButtonSize).Active = true;
            ExpandButton.WidthAnchor.ConstraintEqualToConstant(ExpandButtonSize).Active = true;

            stackView.AddArrangedSubview(ExpandButton);

            View.WantsLayer = true;
        }

        public HeaderCollectionViewItem(IntPtr handle) : base(handle)
        {

        }
    }

    public class ImageCollectionViewItem : NSCollectionViewItem
    {
        public static NSColor ImageCellBackgroundSelectedColor = NSColor.FromRgba(red: 0.33f, green: 0.55f, blue: 0.92f, alpha: 1.0f);
        public static NSColor ImageCellBorderSelectedColor = NSColor.Black;
        public static NSColor ImageCellBackgroundColor = NSColor.FromRgba(red: 0.25f, green: 0.25f, blue: 0.25f, alpha: 1.0f);

        public static CGSize Size = new CGSize(50, 50);

        internal const string Name = "ImageViewItem";
        const int margin = 5;
        public NSImage Image
        {
            get => ImageView.Image;
            set => ImageView.Image = value;
        }

        public string ToolTip
        {
            get => ImageView.ToolTip;
            set => ImageView.ToolTip = value;
        }

        public string AccessibilityTitle
        {
            get => ImageView.AccessibilityLabel;
            set => ImageView.AccessibilityTitle = value;
        }

        public override bool Selected
        {
            get => base.Selected;
            set
            {
                base.Selected = value;
                if (contentCollectionView != null)
                {
                    contentCollectionView.IsSelected = value;
                }
            }
        }

        ContentCollectionViewItem contentCollectionView;
        public override void LoadView()
        {
            View = contentCollectionView = new ContentCollectionViewItem(Name);

            contentCollectionView.BackgroundSelectedColor = ImageCellBackgroundSelectedColor;
            contentCollectionView.BorderSelectedColor = ImageCellBorderSelectedColor;
            contentCollectionView.BackgroundColor = ImageCellBackgroundColor;

            ImageView = new NSImageView() { TranslatesAutoresizingMaskIntoConstraints = false };
            View.AddSubview(ImageView);

            ImageView.TopAnchor.ConstraintEqualToAnchor(View.TopAnchor, margin).Active = true;
            ImageView.LeftAnchor.ConstraintEqualToAnchor(View.LeftAnchor, margin).Active = true;
            ImageView.RightAnchor.ConstraintEqualToAnchor(View.RightAnchor, -margin).Active = true;
            ImageView.BottomAnchor.ConstraintEqualToAnchor(View.BottomAnchor, -margin).Active = true;
        }

        public ImageCollectionViewItem(IntPtr handle) : base(handle)
        {

        }
    }

    class ContentCollectionViewItem : NSView
    {
        public NSColor BackgroundColor { get; set; } = NSColor.Control;
        public NSColor BackgroundSelectedColor { get; set; } = NSColor.SelectedTextBackground;
        public NSColor BorderSelectedColor { get; internal set; }

        bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value)
                {
                    return;
                }
                isSelected = value;
                NeedsDisplay = true;
            }
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            if (IsSelected) {
                BackgroundSelectedColor.Set();
                NSBezierPath.FillRect(dirtyRect);
            }
        }

        public ContentCollectionViewItem(string identifier)
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            Identifier = identifier;
            WantsLayer = true;
        }
    }

}