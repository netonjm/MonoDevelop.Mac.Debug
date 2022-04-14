﻿// This file has been autogenerated from a class added in the UI designer.

using AppKit;
using CoreGraphics;
using System;

namespace MonoDevelop.Inspector.Mac
{
    public static class Runtime
    {
        internal static InspectorContext Context { get; private set; }

        public static void Initialize (bool hasToolkit)
        {
			var inspectorDelegate = new MacInspectorDelegate();
			var inspectorManager = inspectorDelegate.CreateInspectorManager();
			Context = new InspectorContext();
			Context.Initialize(inspectorDelegate, inspectorManager, hasToolkit);
		}

        public static void Attach(IMainWindow currentWindow)
        {
			Context.Attach(currentWindow);
		}
    }

	//internal class MacInspectorContext : InspectorContext
	//{
	//	public MacInspectorContext ()
	//	{

	//	}

	//	IInspectDelegate macDelegate;
	//	public virtual IInspectDelegate GetInspectorDelegate ()
	//	{
	//		if (macDelegate == null)
	//			macDelegate = new MacInspectorDelegate ();
	//		return macDelegate;
	//	}

	//	protected override InspectorManager GetInitializationContext ()
	//	{
	//		var inspectorDelegate = GetInspectorDelegate ();
	//		var overlayWindow = new BorderedWindow (CGRect.Empty, NSColor.Green);
	//		var nextWindow = new BorderedWindow (CGRect.Empty, NSColor.Red);
	//		var previousWindow = new BorderedWindow (CGRect.Empty, NSColor.Blue);
	//		var accWindow = new AccessibilityToolWindow (new CGRect (10, 10, 600, 700));
	//		var inspectorWindow = new InspectorToolWindow (inspectorDelegate, new CGRect (10, 10, 600, 700)); ;
	//		var macToolbar = new ToolbarWindow (inspectorDelegate, new CGRect (10, 10, 100, 700));
	//		macToolbar.ShowToolkitButton (hasToolkit);

	//		return new InspectorManager (macDelegate, overlayWindow, nextWindow, previousWindow, accWindow, inspectorWindow, macToolbar);
	//	}

	//	public static MacInspectorContext Current { get; set; } = new MacInspectorContext ();
	//}
}
