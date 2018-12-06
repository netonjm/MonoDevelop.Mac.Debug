using AppKit;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoDevelop.Inspector.Services;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing.Themes;
using MonoDevelop.Inspector.Mac.Touchbar;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Reflection;

namespace MonoDevelop.Inspector.Mac
{
    public class MacColorWrapper : IColorWrapper
    {
        NSColor image;
        public MacColorWrapper(NSColor image)
        {
            this.image = image;
        }

        public object NativeObject => image;
    }

    public class MacImageWrapper : IImageWrapper
    {
        NSImage image;
        public MacImageWrapper(NSImage image)
        {
            this.image = image;
        }

        public object NativeObject => image;
    }

    class MacInspectorDelegate : IInspectDelegate
    {
        TaskCompletionSource<object> processingCompletion = new TaskCompletionSource<object>();

        public MacInspectorDelegate()
        {
        }

        public IImageWrapper GetImageResource(string resource)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(MacInspectorDelegate));
                var resources = assembly.GetManifestResourceNames();
                using (var stream = assembly.GetManifestResourceStream(resource))
                {
                    return new MacImageWrapper(NSImage.FromStream(stream));
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public IMenuWrapper GetSubMenu()
        {
            var shared = NSApplication.SharedApplication;
            if (shared.Menu == null)
            {
                shared.Menu = new NSMenu();
            }

            NSMenuItem item;
            if (shared.Menu.Count == 0)
            {
                item = new NSMenuItem("Inspector");
                shared.Menu.AddItem(item);
            }
            else
            {
                item = shared.Menu.ItemAt(0);
            }

            if (item.Submenu == null)
            {
                item.Submenu = new NSMenu();
            }

            var submenu = item.Submenu;
            submenu.AutoEnablesItems = false;

            return new MacMenuWrapper(submenu);
        }

        void LoadModule (string path, InspectorContext context)
        {

			Dictionary<Assembly, string> instanciableTypes = new Dictionary<Assembly, string> ();

			Console.WriteLine("Loading {0}...", path);
            foreach (var file in Directory.EnumerateFiles(path, "*.dll"))
            {
                var fileName = Path.GetFileName(file);
                Console.WriteLine("[{0}] Found.", fileName);
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    instanciableTypes.Add(assembly, file);
                   
                }
                catch (Exception ex)
                {
					Console.WriteLine ("[{0}] Error loading.", fileName);
					//Console.WriteLine(ex);
				}
            }

            foreach (var assemblyTypes in instanciableTypes)
            {
                try
                {
                    var interfaceType = typeof(IInspectorTabModule);
                    var types = assemblyTypes.Key.GetTypes()
                        .Where(interfaceType.IsAssignableFrom);

                    Console.WriteLine("[{0}] Loaded.", assemblyTypes.Value);
                    foreach (var type in types)
                    {
                        Console.WriteLine("[{0}] Creating instance {1}", assemblyTypes.Key, type);
                        try
                        {
                            if (Activator.CreateInstance(type) is IInspectorTabModule element)
                                context.Modules.Add(element);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        Console.WriteLine("[{0}] Loaded", type);
                    }
                }
                catch (Exception ex)
                {

                }
            }
		}

        public void LoadModules (InspectorContext context)
        {
            var modules = InspectorContext.Current.ModulesDirectoryPath;
            Console.WriteLine("Loading modules from: {0}", modules);
            if (!Directory.Exists (modules))
            {
                Console.WriteLine("Error: folder not found.");
                return;
            }

            var modulesDirectories = Directory.EnumerateDirectories(modules);
            Console.WriteLine("{0} module/s found.", modulesDirectories.Count ());
            foreach (var module in modulesDirectories)
            {
                LoadModule(module, context);
            }
        }

        public void InitializeManager (InspectorContext context, ToolbarService service)
        {
            LoadModules(context);
          
            var over = new MacBorderedWindow(CGRect.Empty, NSColor.Green);
            var next = new MacBorderedWindow(CGRect.Empty, NSColor.Red);
            var previous = new MacBorderedWindow(CGRect.Empty, NSColor.Blue);
            var acc = new MacAccessibilityWindow(new CGRect(10, 10, 600, 700));
            var ins = new InspectorWindow(this, new CGRect(10, 10, 600, 700)); ;
            var tool = new MacToolbarWindow(this, new CGRect(10, 10, 100, 700));
            tool.ShowToolkit(false);
            var manager = new InspectorManager(this, over, next, previous, acc, ins, tool);
            context.Initialize(manager, false);
            service.SetDelegate(this);
        }

        public void SetCultureInfo(CultureInfo e)
        {
            Thread.CurrentThread.CurrentCulture = e;
            Thread.CurrentThread.CurrentUICulture = e;
        }

        public IButtonWrapper GetImageButton(IImageWrapper imageWrapper)
        {
            var invokeButton = new ImageButton();
			invokeButton.Image = (NSImage)imageWrapper.NativeObject;
			return new MacButtonWrapper(invokeButton);
        }

        class MacConstrainContainerWrapper : IConstrainContainerWrapper
        {
            IViewWrapper wrapper;
            NSView view;
            public MacConstrainContainerWrapper(IViewWrapper previous)
            {
                this.wrapper = previous;
                view = previous.NativeObject as NSView;
            }

            public string NodeName => "Constraints";

            public IViewWrapper PreviousValidKeyView => wrapper;

            public object NativeObject => null;

            public void RemoveFromSuperview()
            {
                if (view != null ) {
                    var constraints = view.Constraints;
                    view.RemoveConstraints(constraints);
                }
            }
        }

        public void ConvertToNodes(IViewWrapper customView, INodeView node, InspectorViewMode viewMode)
        {
            var current = new NodeView(customView);
            var nodeWrapper = new MacNodeWrapper(current);
            node.AddChild(nodeWrapper);

            if (customView.HasConstraints) {
                var contraintContainer = new MacConstrainContainerWrapper(customView);
                var constraintsContainerNodeView = new NodeView(contraintContainer);
                var constraintsContainerNodeWrapper = new MacNodeWrapper(constraintsContainerNodeView);
                nodeWrapper.AddChild(constraintsContainerNodeWrapper);

                foreach (var item in customView.Constraints)
                {
                    var constraintNodeView = new NodeView(item);
                    var constraintWrapper = new MacNodeWrapper(constraintNodeView);
                    constraintsContainerNodeWrapper.AddChild(constraintWrapper);
                }
            }

            if (customView.Subviews == null)
            {
                return;
            }

            foreach (var item in customView.Subviews)
            {
                try
                {
                    ConvertToNodes(item, nodeWrapper, viewMode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void RemoveAllErrorWindows(IWindowWrapper windowWrapper)
        {
            var window = windowWrapper.NativeObject as NSWindow;
            var childWindro = window.ChildWindows.OfType<MacBorderedWindow>();
            foreach (var item in childWindro)
            {
                item.Close();
            }
        }

        public FontData GetFont(IViewWrapper view)
        {
            return NativeViewHelper.GetFont(view.NativeObject as NSView);
        }

        object GetNativePropertyPanelWrapper(IViewWrapper viewSelected)
        {
            NSView view = viewSelected.NativeObject as NSView;
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

        public object GetWrapper(INativeObject viewSelected, InspectorViewMode viewMode)
        {
            if (viewSelected is IViewWrapper view) {
                if (viewMode == InspectorViewMode.Xwt)
                {
                    return view.View;
                }
                return GetNativePropertyPanelWrapper(view);
            }
            if (viewSelected is IConstrainWrapper constrain)
                return constrain;
            return viewSelected?.NativeObject;
        }

        public void SetFont(IViewWrapper view, IFontWrapper font)
        {
            NativeViewHelper.SetFont(view.NativeObject as NSView, font.NativeObject as NSFont);
        }


        ColorResult BackColorSearch(IViewWrapper view)
        {
            var properties = view.GetType().GetProperties().Where(s => s.Name.StartsWith("BackgroundColor")).ToArray();

            var property = view.GetProperty("BackgroundColor");
            if (property != null)
            {
                var colorFound = property.GetValue(view.Superview) as NSColor;
                return new ColorResult() { View = view, Color = new MacColorWrapper(colorFound) };
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
            return customView.CanBecomeKeyView && !customView.Hidden;
        }

        public void Recursively(IViewWrapper customView, List<DetectedError> detectedErrors, InspectorViewMode viewMode)
        {
            if (detectedErrors.Count >= AccessibilityService.MaxIssues)
            {
                return;
            }

            var errorType = DetectedErrorType.None;

            ContrastAnalysis contrastAnalisys = null;
            if (customView is ITextBoxWrapper textField)
            {
                var parentColor = textField.BackgroundColor;
                if (parentColor == null && textField.Superview != null)
                {
                    var result = BackColorSearch(textField.Superview);
                    if (result != null)
                    {
                        contrastAnalisys = new ContrastAnalysis((NSColor)textField.TextColor.NativeObject, (NSColor)result.Color.NativeObject, (NSFont)textField.Font.NativeObject);
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
                //if (string.IsNullOrEmpty(customView.AccessibilityHelp))
                //{
                //    errorType |= DetectedErrorType.AccessibilityHelp;
                //}
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
                    detectedError.ContrastRatio = (float)contrastAnalisys.Contrast;
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
                Recursively(item, detectedErrors, viewMode);
            }
        }

        public void SetButton(IButtonWrapper button, IImageWrapper image)
        {
            var btn = button.NativeObject as NSButton;
            btn.Image = image.NativeObject as NSImage; // ToNSImage(image.ToBitmap ());
        }

        public void SetButton(IImageViewWrapper imageview, IImageWrapper image)
        {
            var imgView = imageview.NativeObject as NSImageView;
            imgView.Image = image.NativeObject as NSImage; // ToNSImage(image.ToBitmap());
        }

        //public NSImage ToNSImage(BitmapImage img)
        //{
        //	System.IO.MemoryStream s = new System.IO.MemoryStream();
        //	img.Save(s, ImageFileType.Png);
        //	byte[] b = s.ToArray();
        //	CGDataProvider dp = new CGDataProvider(b, 0, (int)s.Length);
        //	s.Flush();
        //	s.Close();
        //	CGImage img2 = CGImage.FromPNG(dp, null, false, CGColorRenderingIntent.Default);
        //	return new NSImage(img2, new CGSize (img2.Width, img2.Height));
        //}

        public async Task<IImageWrapper> OpenDialogSelectImage(IWindowWrapper selectedWindow)
        {
            var panel = new NSOpenPanel();
            panel.AllowedFileTypes = new[] { "png" };
            panel.Prompt = "Select a image";
            IImageWrapper rtrn = null;
            processingCompletion = new TaskCompletionSource<object>();

            panel.BeginSheet(selectedWindow.NativeObject as NSWindow, result =>
            {
                if (result == 1 && panel.Url != null)
                {
                    rtrn = new MacImageWrapper(new NSImage(panel.Url.Path));// Xwt.Drawing.Image.FromFile(panel.Url.Path);

                }
                processingCompletion.TrySetResult(null);
            });
            await processingCompletion.Task;
            return rtrn;
        }

        public async Task InvokeImageChanged(IViewWrapper view, IWindowWrapper selectedWindow)
        {
            if (view.NativeObject is NSImageView imageView)
            {
                var image = await OpenDialogSelectImage(selectedWindow);
                if (image != null)
                {
                    SetButton(new MacImageViewWrapper(imageView), image);
                }
            }
            else if (view.NativeObject is NSButton btn)
            {
                var image = await OpenDialogSelectImage(selectedWindow);
                if (image != null)
                {
                    SetButton(new MacButtonWrapper(btn), image);
                }
            }
        }

        public IBorderedWindow CreateErrorWindow(IViewWrapper view)
        {
            return new MacBorderedWindow(view, NSColor.Red);
        }

        public IFontWrapper GetFromName(string selected, int fontSize)
        {
            return new MacFont(NSFont.FromFontName(selected, fontSize));
        }

        public void ClearSubmenuItems(List<IMenuItemWrapper> menuItems, IMenuWrapper submenu)
        {
            var menu = (NSMenu)submenu.NativeObject;
            foreach (var item in menuItems) {
                menu.RemoveItem((NSMenuItem)item.NativeObject);
            }
        }

        public IMenuItemWrapper CreateMenuItem(string title, EventHandler menuItemOpenHandler)
        {
            var menuItem = new NSMenuItem(title, menuItemOpenHandler) { Enabled = false };
            return new MacMenuItemWrapper(menuItem);
        }

        public IMenuItemWrapper GetShowInspectorWindowMenuItem(EventHandler menuOpenHandler)
        {
            var inspectorMenuItem = new NSMenuItem($"Show Inspector Window", menuOpenHandler);
            inspectorMenuItem.KeyEquivalentModifierMask = NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask;
            inspectorMenuItem.KeyEquivalent = "2";
            return new MacMenuItemWrapper(inspectorMenuItem);
        }

        public IMenuItemWrapper GetShowAccessibilityWindowMenuItem(EventHandler menuOpenHandler)
        {
            var inspectorMenuItem = new NSMenuItem($"Show Accessibility Window", menuOpenHandler);
            inspectorMenuItem.KeyEquivalentModifierMask = NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask;
            inspectorMenuItem.KeyEquivalent = "1";
            return new MacMenuItemWrapper(inspectorMenuItem);
        }

        public IMenuItemWrapper GetSeparatorMenuItem()
        {
            return new MacMenuItemWrapper(NSMenuItem.SeparatorItem);
        }

        public void SetAppearance(bool isDark, params IWindowWrapper[] inspectorWindow)
        {
            PropertyEditorPanel.ThemeManager.Theme = isDark ? PropertyEditorTheme.Dark : PropertyEditorTheme.Light;
            foreach (var item in inspectorWindow) {
                item.SetAppareance(isDark);
            }
        }

        public void CreateItem(IViewWrapper view, ToolbarView e)
        {
            var nativeView = view.NativeObject;
            var createdView = Getview(e);
            if (nativeView is NSStackView stack)
                stack.AddArrangedSubview(createdView);
            else if (nativeView is NSView customView)
                customView.AddSubview(createdView);
        }

        NSView Getview(ToolbarView e)
        {
            switch (e)
            {
                case ToolbarView.ComboBox:
                    return new NSComboBox();
                case ToolbarView.DatePicker:
                    return new NSDatePicker();
                case ToolbarView.ImageBox:
                    return new NSImageView();
                case ToolbarView.Label:
                    return NativeViewHelper.CreateLabel("Label");
                case ToolbarView.WrappingLabel:
                    var label = NativeViewHelper.CreateLabel("Label");
                    label.SetContentCompressionResistancePriority(250, NSLayoutConstraintOrientation.Horizontal);
                    label.UsesSingleLineMode = false;
                    label.LineBreakMode = NSLineBreakMode.ByWordWrapping;
                    return label;
                case ToolbarView.PushButton:
                    return new NSButton() { Title = "Button" };
                case ToolbarView.Search:
                    return new NSSearchField();
            }
            return new NSView();
        }

        public virtual IToolbarWrapperDelegateWrapper GetTouchBarDelegate(object element)
        {
            if (element is NSView)
            {
                return new MacToolbarWrapperDelegateWrapper (new ColorPickerDelegate());
            }
            return null;
        }

        public void SetCultureInfo(IWindowWrapper selectedWindow, CultureInfo e)
        {

		}

    }
}