using System;
using AppKit;

namespace MonoDevelop.Mac.Debug
{
	public class MacXwtAccInspectorWindow : MacXwtWindowWrapper
	{
		protected override void OnBecomeMainWindow (object sender, EventArgs args)
		{
			InspectorContext.Attach (this);
		}

		public override void OnFocusChanged (object focused)
		{
			if (focused is NSView focusedView) {
				var wrapperView = new MacViewWrapper (focusedView);
				InspectorContext.ChangeFocusedView (wrapperView);
			}
		}
	}
}
