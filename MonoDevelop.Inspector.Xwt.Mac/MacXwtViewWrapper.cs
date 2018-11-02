using AppKit;
using Xwt;
using CoreGraphics;
using System.Collections.Generic;

namespace MonoDevelop.Inspector.Mac
{
    //class XwtMacRectangle : IRectangle
    //{
    //    public object NativeObject => throw new System.NotImplementedException();
    //    Xwt.Rectangle rectangle;
    //    public XwtMacRectangle (Xwt.Rectangle rectangle)
    //    {
    //        this.rectangle = rectangle;
    //    }
    //}

    public class MacXwtViewWrapper : IViewWrapper
	{
		readonly Xwt.Widget widget;
		readonly NSView view;

		public MacXwtViewWrapper (Xwt.Widget widget)
		{
			this.widget = widget;
			view = Xwt.Toolkit.NativeEngine.GetNativeWidget (widget) as NSView;
		}

		public bool Hidden => !widget.Visible;

		public string Identifier => widget.Name;

		public IRectangle AccessibilityFrame => new Mac.MacRectangle ( view.AccessibilityFrame);

		public List<IViewWrapper> Subviews {
			get {
				List<IViewWrapper> tmp = new List<IViewWrapper> ();
				if (widget is IWidgetSurface surface) {
					foreach (var w in surface.Children) {
						tmp.Add (new MacXwtViewWrapper (w));
					}
				}
				return tmp;
			}
		}

		public IViewWrapper NextValidKeyView {
			get {
				//if (view.NextValidKeyView != null)
				//return new MacXwtViewWrapper (widget.NextValidKeyView);
				return null;
			}
		}

		public IViewWrapper PreviousValidKeyView {
			get {
				//if (widget.PreviousValidKeyView != null)
				//return new MacViewWrapper (widget.PreviousValidKeyView);
				return null;
			}
		}

		public IRectangle Frame => new Mac.MacRectangle (view.Frame);

		public IViewWrapper Superview => widget.Parent != null ? new MacXwtViewWrapper (widget.Parent) : null;

		public string AccessibilityTitle {
			get => view.AccessibilityTitle;
			set => view.AccessibilityTitle = value;
		}

		public string AccessibilityHelp {
			get => view.AccessibilityTitle;
			set => view.AccessibilityTitle = value;
		}

		public object AccessibilityParent {
			get => view.AccessibilityParent;
			set => view.AccessibilityParent = value as NSView;
		}

		public bool CanBecomeKeyView => view.CanBecomeKeyView;

		public object NativeView => view;

		public object View => widget;

		public string NodeName => widget.GetType ().Name;

		public void RemoveFromSuperview ()
		{
			view.RemoveFromSuperview ();
		}
	}
}
