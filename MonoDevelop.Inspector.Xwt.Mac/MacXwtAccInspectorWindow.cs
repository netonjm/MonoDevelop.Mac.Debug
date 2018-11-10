using System;
using AppKit;
using Foundation;
using MonoDevelop.Inspector.Mac.Touchbar;

namespace MonoDevelop.Inspector.Mac
{
    class XwtInspectorDelegate : MacInspectorDelegate
    {
        public override IToolbarWrapperDelegateWrapper GetTouchBarDelegate(object element)
        {
            if (element is NSButton)
            {
                return new MacToolbarWrapperDelegateWrapper(new ColorPickerDelegate());
            }
            return null;
        }
    }

    internal class XwtMacInspectorContext : MacInspectorContext
    {
        public override IInspectDelegate GetInspectorDelegate()
        {
            return new XwtInspectorDelegate();
        }
    }

    public class MacXwtAccInspectorWindow : MacXwtWindowWrapper, IMainWindowWrapper
    {
        NSTouchBar touchbar;
        MacInspectorContext context;
        ToolbarService toolbarService;

        public MacXwtAccInspectorWindow()
        {
            touchbar = new NSTouchBar();
            toolbarService = ToolbarService.Current;
            context = MacInspectorContext.Current;
            context.Initialize(true);
            toolbarService.SetDelegate(context.GetInspectorDelegate());
            NSApplication.SharedApplication.SetAutomaticCustomizeTouchBarMenuItemEnabled(true);
        }

        protected override void OnBecomeMainWindow (object sender, EventArgs args)
		{
            context.Attach (this);
		}

		public override void OnFocusChanged (object focused)
		{
			if (focused is NSView focusedView) {
               
                var wrapperView = new MacViewWrapper (focusedView);
                context.ChangeFocusedView (wrapperView);
                if (toolbarService.GetTouchBarDelegate(focusedView)?.NativeObject is TouchBarBaseDelegate currentDelegate)
                {
                    currentDelegate.SetCurrentView(focusedView);
                    touchbar.Delegate = currentDelegate;
                    touchbar.DefaultItemIdentifiers = currentDelegate.Identifiers;
                    focusedView.SetTouchBar(touchbar);
                }
            }
		}
    }
}
