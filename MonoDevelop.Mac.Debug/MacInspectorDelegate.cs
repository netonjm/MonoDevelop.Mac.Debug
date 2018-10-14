using AppKit;
using MonoDevelop.Mac.Debug.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.Mac.Debug
{
	internal class MacInspectorDelegate : IInspectDelegate
	{
		public MacInspectorDelegate()
		{
		}

		public void ConvertToNodes(IViewWrapper customView, NodeView node)
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
					ConvertToNodes(item, nodel);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		public FontData GetFont(IViewWrapper view)
		{
			return NativeViewHelper.GetFont(view.Content as NSView);
		}

		public ViewWrapper GetWrapper(IViewWrapper viewSelected)
		{
			NSView view = viewSelected.Content as NSView;
			if (view is NSComboBox comboBox)
			{
				return new ComboBoxWrapper(comboBox);
			}

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

			if (view is NSImageView img)
			{
				return new ImageViewWrapper(img);
			}

			if (view is NSBox box)
			{
				return new BoxViewWrapper(box);
			}

			return new ViewWrapper(view);
		}

		public void SetFont(IViewWrapper view, NSFont font)
		{
			NativeViewHelper.SetFont(view.Content as NSView, font);
		}


		ColorResult BackColorSearch(IViewWrapper view)
		{
			var properties = view.GetType().GetProperties().Where(s => s.Name.StartsWith("BackgroundColor")).ToArray();

			var property = view.GetProperty("BackgroundColor");
			if (property != null)
			{
				var colorFound = property.GetValue(view.Superview) as NSColor;
				return new ColorResult() { View = view, Color = colorFound };
			}

			if (view.Superview is IViewWrapper superView && superView != null)
			{
				var result = BackColorSearch(superView);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		bool IsSelectableView(IViewWrapper customView)
		{
			return !customView.CanBecomeKeyView && !customView.Hidden;
		}

		public void Recursively(IViewWrapper customView, List<DetectedError> detectedErrors)
		{
			if (detectedErrors.Count >= AccessibilityService.MaxIssues)
			{
				return;
			}

			var errorType = DetectedErrorType.None;

			ContrastAnalisys contrastAnalisys = null;
			if (customView is ITextBoxWrapper textField)
			{
				var parentColor = textField.BackgroundColor;
				if (parentColor == null && textField.Superview != null)
				{
					var result = BackColorSearch(textField.Superview);
					if (result != null)
					{
						contrastAnalisys = new ContrastAnalisys(textField.TextColor, result.Color, textField.Font);
						contrastAnalisys.View1 = customView;
						contrastAnalisys.View2 = textField.Superview;
						if (!contrastAnalisys.IsPassed)
						{
							errorType |= DetectedErrorType.Contrast;
						}
					}
				}
			}

			if (IsSelectableView(customView))
			{
				if (string.IsNullOrEmpty(customView.AccessibilityTitle))
				{
					errorType |= DetectedErrorType.AccessibilityTitle;
				}
				if (string.IsNullOrEmpty(customView.AccessibilityHelp))
				{
					errorType |= DetectedErrorType.AccessibilityHelp;
				}
				if (customView.AccessibilityParent == null)
				{
					errorType |= DetectedErrorType.AccessibilityParent;
				}
			}

			if (errorType != DetectedErrorType.None)
			{
				var detectedError = new DetectedError() { View = customView, ErrorType = errorType };
				if (contrastAnalisys != null)
				{
					detectedError.Color1 = contrastAnalisys.Color1.ToHex();
					detectedError.Color2 = contrastAnalisys.Color2.ToHex();
					detectedError.ContrastRatio = contrastAnalisys.Contrast;
					detectedError.View2 = contrastAnalisys.View2;
				}

				detectedErrors.Add(detectedError);
			}

			if (customView.Subviews == null || customView.IsBlockedType())
			{
				return;
			}

			foreach (var item in customView.Subviews)
			{
				Recursively(item, detectedErrors);
			}
		}

	}
}