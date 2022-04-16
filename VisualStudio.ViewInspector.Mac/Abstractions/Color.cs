using AppKit;
using CoreGraphics;
using Foundation;
using System;
using System.Linq;
using VisualStudio.ViewInspector.Abstractions;

namespace VisualStudio.ViewInspector.Mac.Abstractions
{
    public class ObservableWindow : WindowWrapper, IMainWindow
    {
        ObservableWindowDelegate windowDelegate;

        public ObservableWindow(NSWindow window) : base(window)
        {
            this.windowDelegate = new ObservableWindowDelegate(this);
            Window = window;
            //Observer to check window close and deatach

        }

        class ObservableWindowDelegate : NSWindowDelegate
        {
            WeakReference<ObservableWindow> weakWindow;

            public ObservableWindowDelegate(ObservableWindow window)
            {
                this.weakWindow = new WeakReference<ObservableWindow>(window);
            }

            public override void DidResize(NSNotification notification)
            {
                if (weakWindow.TryGetTarget(out var target))
                {
                    target.RaiseResizeRequested();
                    return;
                }
            }

            public override void DidMove(NSNotification notification)
            {
                if (weakWindow.TryGetTarget(out var target))
                {
                    target.RaiseMovedRequested();
                }
            }

            public override void DidResignKey(NSNotification notification)
            {
                if (weakWindow.TryGetTarget(out var target))
                {
                    target.RaiseLostFocus();
                    return;
                }
            }
        }

        public InspectorViewMode ViewMode { get; set; }

        public override NSWindow Window
        {
            get => window;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (window?.Delegate != null)
                {
                    window.Delegate = null;
                }
                this.responder = value;
                this.window.Delegate = windowDelegate;
            }
        }
    }

    public class ResponderWrapper : INativeObject
    {
        protected NSResponder responder;
        public object NativeObject => responder;

        public ResponderWrapper(NSResponder responder)
        {
            this.responder = responder;
        }
    }

    public class WindowWrapper : ResponderWrapper, IWindow
    {
        public event EventHandler LostFocus;
        public event EventHandler ResizeRequested;
        public event EventHandler MovedRequested;

        protected void RaiseLostFocus() => LostFocus?.Invoke(this, EventArgs.Empty);
        protected void RaiseResizeRequested() => ResizeRequested?.Invoke(this, EventArgs.Empty);
        protected void RaiseMovedRequested() => MovedRequested?.Invoke(this, EventArgs.Empty);

        IView IWindow.ContentView
        {
            get
            {
                if (window.ContentView is NSView view)
                {
                    return new ViewWrapper(view);
                }
                return null;
            }
            set
            {
                window.ContentView = value.NativeObject as NSView;
            }
        }

        IView IWindow.FirstResponder
        {
            get
            {
                if (window.FirstResponder is NSView view)
                {
                    return new ViewWrapper(view);
                }
                return null;
            }
        }

        public bool HasParentWindow => window.ParentWindow != null;

        public string Title
        {
            get => window.Title;
            set => window.Title = value;
        }

        public float FrameX => (float)window.Frame.X;
        public float FrameY => (float)window.Frame.Y;
        public float FrameWidth => (float)window.Frame.Width;
        public float FrameHeight => (float)window.Frame.Height;

        public virtual NSWindow Window
        {
            get => window;
            set
            {
                this.responder = value;
            }
        }

        public IWindow ParentWindow
        {
            get
            {
                var paren = window.ParentWindow;
                if (paren == null)
                {
                    return null;
                }
                return new WindowWrapper(paren);
            }
        }

        protected NSWindow window => (NSWindow)base.NativeObject;

        public WindowWrapper(NSWindow window) : base(window)
        {

        }

        public void AddChildWindow(IWindow borderer)
        {
            if (borderer.NativeObject is NSWindow currentWindow)
            {
                if (currentWindow.ParentWindow != null && currentWindow.ParentWindow != window)
                {
                    currentWindow.ParentWindow.RemoveChildWindow(currentWindow);
                }
                window.AddChildWindow(currentWindow, NSWindowOrderingMode.Above);
            }
        }

        public void AlignLeft(IWindow toView, int pixels)
        {
            var toViewWindow = toView.NativeObject as NSWindow;
            var frame = window.Frame;
            frame.Location = new CGPoint(toViewWindow.Frame.Left - window.Frame.Width - pixels, toViewWindow.Frame.Bottom - frame.Height);
            window.SetFrame(frame, true);
        }

        public void AlignRight(IWindow toView, int pixels)
        {
            var toViewWindow = toView.NativeObject as NSWindow;
            var frame = window.Frame;
            frame.Location = new CGPoint(toViewWindow.Frame.Right + pixels, toViewWindow.Frame.Bottom - frame.Height);
            window.SetFrame(frame, true);
        }

        public void AlignTop(IWindow toView, int pixels)
        {
            var toViewWindow = toView.NativeObject as NSWindow;
            var frame = window.Frame;
            frame.Location = new CGPoint(toViewWindow.Frame.Left, toViewWindow.AccessibilityFrame.Y + toViewWindow.Frame.Height + pixels);
            window.SetFrame(frame, true);
        }

        public void Close()
        {
            window.Close();
        }

        public bool ContainsChildWindow(IWindow debugOverlayWindow)
        {
            return window.ChildWindows.Contains(debugOverlayWindow.NativeObject as NSWindow);
        }

        public void RecalculateKeyViewLoop()
        {
            window.RecalculateKeyViewLoop();
        }

        public void SetAppareance(bool isDark)
        {
            window.Appearance = NSAppearance.GetAppearance(isDark ? NSAppearance.NameVibrantDark : NSAppearance.NameVibrantLight);
        }

        public void SetContentSize(int toolbarWindowWidth, int toolbarWindowHeight)
        {
            window.SetContentSize(new CGSize(toolbarWindowWidth, toolbarWindowHeight));
        }
    }

    internal class Color : IColor
    {
        NSColor image;
        public Color(NSColor image)
        {
            this.image = image;
        }

        public object NativeObject => image;
    }
}