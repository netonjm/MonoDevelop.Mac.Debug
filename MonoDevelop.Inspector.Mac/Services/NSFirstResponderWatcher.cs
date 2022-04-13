﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using AppKit;
using Foundation;

namespace MonoDevelop.Inspector.Mac.Services
{
	class NSFirstResponderWatcher : IDisposable
	{
		NSTimer timer;
		NSResponder responder;
		IWindow window;
		public EventHandler<NSResponder> Changed;
		public double Interval { get; set; } = 0.2;

		public NSFirstResponderWatcher (IWindow window)
		{
			this.window = window;
		}

		void TickHandleAction (NSTimer obj)
		{
			if (window.FirstResponder != responder) {
				responder = (AppKit.NSResponder)window.FirstResponder;
				Changed?.Invoke (this, responder);
			}
		}

		public void Start ()
		{
			Stop ();
			timer = NSTimer.CreateRepeatingScheduledTimer (TimeSpan.FromSeconds (Interval), TickHandleAction);
		}

		public void Stop ()
		{
			if (timer != null) {
				timer.Invalidate ();
			}
		}

		public void Dispose ()
		{
			Stop ();
		}
	}
}
