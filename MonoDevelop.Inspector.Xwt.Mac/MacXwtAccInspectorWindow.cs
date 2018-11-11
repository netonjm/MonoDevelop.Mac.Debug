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
        ToolbarService service;

        public MacXwtAccInspectorWindow()
        {
            touchbar = new NSTouchBar();
            service = ToolbarService.Current;
            context = MacInspectorContext.Current;
            context.Initialize(true);
            service.SetDelegate(context.GetInspectorDelegate());
            NSApplication.SharedApplication.SetAutomaticCustomizeTouchBarMenuItemEnabled(true);
            context.FocusedViewChanged += Context_FocusedViewChanged;
        }

        private void Context_FocusedViewChanged(object s, IViewWrapper e)
        {
            if (e.NativeObject is NSView view)
            {
                RefreshBar(view);
            }
        }

        void RefreshBar(NSView view)
        {
            if (service.GetTouchBarDelegate(view)?.NativeObject is TouchBarBaseDelegate currentDelegate)
            {
                currentDelegate.SetCurrentView(view);
                touchbar.Delegate = currentDelegate;
                touchbar.DefaultItemIdentifiers = currentDelegate.Identifiers;
                view.SetTouchBar(touchbar);
            }
        }

        protected override void Dispose(bool disposing)
        {
            context.FocusedViewChanged -= Context_FocusedViewChanged;
            base.Dispose(disposing);
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
                if (service.GetTouchBarDelegate(focusedView)?.NativeObject is TouchBarBaseDelegate currentDelegate)
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
