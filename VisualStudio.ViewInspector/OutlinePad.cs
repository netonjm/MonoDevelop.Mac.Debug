using System;
using AppKit;
using MonoDevelop.Components;
using MonoDevelop.Ide.Gui;

namespace VisualStudio.ViewInspector
{
	class OutlinePad : PadContent
	{
		OutlineContentPad widget;

		protected override void Initialize(IPadWindow container)
		{
			widget = OutlineContentPad.Instance;
			widget.CanFocus = true;
			widget.Sensitive = true;
			widget.RaiseFirstResponder += Widget_RaiseFirstResponder;
			widget.DoubleClick += Widget_DoubleClick;

			widget.ShowAll();
		}

		void Widget_DoubleClick(object sender, NSView e)
		{
			e.BecomeFirstResponder();
			//if (e.CanFocus && e.HasFocus)
			//{
			//	e.HasFocus = true;
			//}
		}

		void Widget_RaiseFirstResponder(object sender, NSView e)
		{
			InspectorPropertyPad.Instance.Control.CurrentObject = e;
		}

		public override Control Control
		{
			get { return widget; }
		}
	}
}
