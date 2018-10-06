using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace MonoDevelop.Mac.Debug
{
	public static class NativeViewHelpers
	{
		public static NSButton CreateButton(string title) => CreateButton (NSBezelStyle.RoundRect, NSFont.SystemFontOfSize(NSFont.SystemFontSize), title);

		public static NSButton CreateClickableLabel(CGRect rect, NSFont font, NSAttributedString text)
		{
			var label = new NSButton(rect)
			{
				AttributedTitle = text,
				BezelStyle = NSBezelStyle.Inline,
				Bordered = false,
				Font = font,
				ImagePosition = NSCellImagePosition.ImageLeft
			};
			label.SetButtonType(NSButtonType.MomentaryPushIn);

			return label;
		}

		public static NSButton CreateButton(NSBezelStyle bezelStyle, NSFont font, string text, NSControlSize controlSize = NSControlSize.Regular, NSImage image = null, bool bordered = true)
		{
			var button = new NSButton
			{
				BezelStyle = bezelStyle,
				Bordered = bordered,
				ControlSize = controlSize,
				Font = font ?? GetSystemFont(false),
				Title = text
			};
			if (image != null)
			{
				button.Image = image;
			}

			return button;
		}

		public static NSButton CreateCheckbox(CGRect rect, NSFont font, string text, bool active)
		{
			var button = new NSButton(rect)
			{
				Title = text,
				Font = font ?? GetSystemFont(false),
				State = active ? NSCellStateValue.On : NSCellStateValue.Off
			};
			button.SetButtonType(NSButtonType.Switch);

			return button;
		}

		public static NSButton CreateRadioButton(NSFont font, string text, bool active)
		{
			var button = new NSButton(CGRect.Empty)
			{
				Title = text,
				Font = font ?? GetSystemFont(false),
				State = active ? NSCellStateValue.On : NSCellStateValue.Off
			};
			button.SetButtonType(NSButtonType.Radio);

			return button;
		}

		public static NSTextField CreateTextEntry(NSFont font, string text, NSTextAlignment alignment = NSTextAlignment.Left)
		{
			return new NSTextField
			{
				StringValue = text ?? "",
				Font = font ?? GetSystemFont(false),
				Alignment = alignment
			};
		}

		public static NSStackView CreateVerticalStackView(int spacing = 10) => new NSStackView()
		{
			Orientation = NSUserInterfaceLayoutOrientation.Vertical,
			Alignment = NSLayoutAttribute.Leading,
			Spacing = spacing,
			Distribution = NSStackViewDistribution.Fill,
			TranslatesAutoresizingMaskIntoConstraints = false
		};

		public static NSStackView CreateHorizontalStackView(int spacing = 10) => new NSStackView()
		{
			Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
			Alignment = NSLayoutAttribute.CenterY,
			Spacing = spacing,
			Distribution = NSStackViewDistribution.Fill,
			TranslatesAutoresizingMaskIntoConstraints = false
		};

		internal static void ToNodes (this NSView customView, NodeView node)
		{
			if (customView.Subviews == null)
			{
				return;
			}

			foreach (var item in customView.Subviews)
			{
				var nodel = new NodeView(item);
				node.AddChild(nodel);
				try
				{
					ToNodes(item, nodel);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		public static NSFont GetSystemFont(bool bold, float size = 0.0f)
		{
			if (size <= 0)
			{
				size = (float)NSFont.SystemFontSize;
			}
			if (bold)
				return NSFont.BoldSystemFontOfSize(size);
			return NSFont.SystemFontOfSize(size);
		}

		public static NSTextField CreateLabel (string text, NSFont font = null, NSTextAlignment alignment = NSTextAlignment.Left)
		{
			var label = new NSTextField ()
			{
				StringValue = text ?? "",
				Font = font ?? GetSystemFont(false),
				Editable = false,
				Bordered = false,
				Bezeled = false,
				DrawsBackground = false,
				Selectable = false,
				Alignment = alignment
			};
			label.TranslatesAutoresizingMaskIntoConstraints = false;
			return label;
		}

		public static ViewWrapper GetWrapper (this NSView view)
		{
			if (view is NSTextField textfield)
			{
				return new TextFieldViewWrapper(textfield);
			}

			if (view is NSTextView text)
			{
				return new TextViewWrapper(text);
			}

			if (view is NSButton btn)
			{
				return new ButtonViewWrapper(btn);
			}

			return new ViewWrapper(view);
		}

		public static NSAttributedString GetAttributedStringWithImage(string imageName, int imageSize, string text)
		{
			var attrString = new NSMutableAttributedString("");

			if (!String.IsNullOrEmpty(imageName))
			{
				var image = GetImageForSize(imageName, imageSize);
				if (image != null)
				{
					image.AlignmentRect = new CGRect { X = 0, Y = 5, Width = imageSize - 2, Height = imageSize - 2 };
					var cell = new NSTextAttachmentCell(image);
					cell.Alignment = NSTextAlignment.Left;
					attrString.Append(NSAttributedString.FromAttachment(new NSTextAttachment { AttachmentCell = cell }));
					attrString.Append(new NSAttributedString("  "));
				}
			}

			attrString.Append(new NSAttributedString(text));

			var style = new NSMutableParagraphStyle { LineBreakMode = NSLineBreakMode.TruncatingMiddle };
			attrString.AddAttribute(NSStringAttributeKey.ParagraphStyle, style, new NSRange(0, attrString.Length));

			return attrString;
		}

		public static NSAttributedString GetAttributedStringForLabel (string text, NSColor color = null, bool isSelected = false)
		{
			return GetAttributedString(text, NSFont.SmallSystemFontSize, color, isSelected);
		}

		public static NSAttributedString GetAttributedString(string text, nfloat size, NSColor color = null, bool isSelected = false)
		{
			NSFont font = NSFont.SystemFontOfSize(size);

			if (isSelected)
				font = NSFont.BoldSystemFontOfSize(size);

			return GetAttributedString(text, font, color, paragraphStyle: new NSMutableParagraphStyle
			{
				LineBreakMode = NSLineBreakMode.TruncatingMiddle,
				Alignment = NSTextAlignment.Left
			});
		}

		public static NSAttributedString GetAttributedString(string text, NSFont font = null, NSColor foregroundColor = null, NSColor backgroundColor = null, NSParagraphStyle paragraphStyle = default(NSParagraphStyle))
		{
			//There is no need create NSStringAttributes element
			if (font == null && foregroundColor == null && backgroundColor == null && paragraphStyle == default(NSParagraphStyle))
			{
				return new NSAttributedString(text);
			}
			var attributed = new NSAttributedString(text, new NSStringAttributes
			{
				Font = font,
				BackgroundColor = backgroundColor,
				ForegroundColor = foregroundColor,
				ParagraphStyle = paragraphStyle
			});
			return attributed;
		}

		public static NSImage GetImageForSize(string imageName, int size)
		{
			var highResImage = NSImage.ImageNamed(imageName + "-" + size * 2);
			if (highResImage != null)
			{
				highResImage.Size = new CGSize { Width = size, Height = size };

				var image = new NSImage
				{
					Size = new CGSize { Width = size, Height = size }
				};
				image.AddRepresentations(highResImage.Representations());

				var lowResImage = NSImage.ImageNamed(imageName + "-" + size * 2);
				if (lowResImage != null)
				{
					image.AddRepresentations(lowResImage.Representations());
				}

				image.Template = true;

				return image;
			}

			var fallbackImage = NSImage.ImageNamed(imageName);
			fallbackImage.Template = true;
			fallbackImage.Size = new CGSize { Width = size, Height = size };

			return fallbackImage;
		}

	}
}
