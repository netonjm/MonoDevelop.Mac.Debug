using System;
using AppKit;

namespace MonoDevelop.Inspector.Mac
{
	public class MacXwtAccInspectorWindow : MacXwtWindowWrapper, IMainWindowWrapper
    {
        public MacXwtAccInspectorWindow()
        {
            MacInspectorContext.Current.Initialize(true);
        }

        protected override void OnBecomeMainWindow (object sender, EventArgs args)
		{
            MacInspectorContext.Current.Attach (this);
		}

		public override void OnFocusChanged (object focused)
		{
			if (focused is NSView focusedView) {
				var wrapperView = new MacViewWrapper (focusedView);
                MacInspectorContext.Current.ChangeFocusedView (wrapperView);
			}
		}
	}
}
