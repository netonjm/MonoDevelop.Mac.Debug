using AppKit;
using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Reflection;
using MonoDevelop.Inspector.Mac.Abstractions;
using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Modules;
using VisualStudio.ViewInspector.Mac.Abstractions;
using VisualStudio.ViewInspector.Mac.Views;
using VisualStudio.ViewInspector.Mac.Windows;
using VisualStudio.ViewInspector.Mac;
using VisualStudio.ViewInspector.Services;
using VisualStudio.ViewInspector.Mac.Windows.Accessibility;
using VisualStudio.ViewInspector.Mac.Windows.Inspector;
using VisualStudio.ViewInspector.Mac.TouchBar;
using VisualStudio.ViewInspector.Mac.Services;
using VisualStudio.ViewInspector.Mac.Windows.Toolbar;

namespace VisualStudio.ViewInspector
{
    class MacInspectorDelegate : IInspectDelegate
    {
        TaskCompletionSource<object> processingCompletion = new TaskCompletionSource<object>();

        public MacInspectorDelegate()
        {
        }

        public IImage GetImageResource(string resource)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(MacInspectorDelegate));
                var resources = assembly.GetManifestResourceNames();
                using (var stream = assembly.GetManifestResourceStream(resource))
                {
                    return new Image(NSImage.FromStream(stream));
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public IMenu GetSubMenu()
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

            return new Menu(submenu);
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

        public InspectorManager CreateInspectorManager()
        {
            var over = new BorderedWindow(CGRect.Empty, NSColor.Green);
            var next = new BorderedWindow(CGRect.Empty, NSColor.Red);
            var previous = new BorderedWindow(CGRect.Empty, NSColor.Blue);
            var acc = new AccessibilityToolWindow(new CGRect(10, 10, 600, 700));
            var ins = new InspectorToolWindow(this, new CGRect(10, 10, 600, 700)); ;
            var tool = new ToolbarWindow(this, new CGRect(10, 10, 100, 700));
            tool.ShowToolkitButton(false);

            var manager = new InspectorManager(this, over, next, previous, acc, ins, tool);
            return manager;
        }

        //public void InitializeManager (InspectorContext context, ToolbarService service = null, bool loadModules = false)
        //{
        //    if (loadModules)
        //    {
        //        LoadModules(context);
        //    }
          
         
        //    context.Initialize(manager, false);

        //    service?.SetDelegate(this);
        //}

        public void SetCultureInfo(CultureInfo e)
        {
            Thread.CurrentThread.CurrentCulture = e;
            Thread.CurrentThread.CurrentUICulture = e;
        }

        public IButton GetImageButton(IImage imageWrapper)
        {
            var invokeButton = new ImageButton();
			invokeButton.Image = (NSImage)imageWrapper.NativeObject;
			return new Button(invokeButton);
        }

        public void ConvertToNodes(IWindow window, INodeView node, InspectorViewMode viewMode)
        {
            var windowNodeView = new NodeView(new TreeNode(window));
            node.AddChild(windowNodeView);

            //window controller node
            var nsWindow = (NSWindow)window.NativeObject;
            
            //if (nsWindow.WindowController != null)
            //{
            //    var windowController = new WindowControllerWrapper(nsWindow.WindowController);
            //    var windowControllerNodeView = new NodeView(new TreeNode(windowController));
            //    windowNodeView.AddChild(windowControllerNodeView);
            //}

            if (nsWindow.ContentViewController != null)
            {
                var contentController = new ViewControllerWrapper(nsWindow.ContentViewController);
                var contentControllerNodeView = new NodeView(new TreeNode(contentController));
                windowNodeView.AddChild(contentControllerNodeView);
                
                windowNodeView = contentControllerNodeView;

                var childControllers = nsWindow.ContentViewController.ChildViewControllers;
                if (childControllers != null && childControllers.Length > 0)
                {
                    //controllers node
                    var contraintContainer = new ViewControllerContainerWrapper();
                    var constraintsContainerNodeView = new NodeView(new TreeNode(contraintContainer));
                    contentControllerNodeView.AddChild(constraintsContainerNodeView);

                    foreach (var controller in childControllers)
                    {
                        var childViewController = new ViewControllerWrapper(controller);
                        var childViewControllerNodeView = new NodeView(new TreeNode(childViewController));
                        constraintsContainerNodeView.AddChild(childViewControllerNodeView);
                    }
                }
            }

            //scan content
            var contentView = new ViewWrapper(nsWindow.ContentView);

            try
            {
                ConvertToNodes(contentView, windowNodeView, viewMode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ConvertToNodes(IView customView, INodeView node, InspectorViewMode viewMode)
        {
            var current = new TreeNode(customView);
            var nodeWrapper = new NodeView(current);
            node.AddChild(nodeWrapper);

            //add constraints
            if (customView.HasConstraints) {
                var contraintContainer = new ConstrainContainerWrapper(customView);
                var constraintsContainerNodeView = new TreeNode(contraintContainer);
                var constraintsContainerNodeWrapper = new NodeView(constraintsContainerNodeView);
                nodeWrapper.AddChild(constraintsContainerNodeWrapper);

                foreach (var item in customView.Constraints)
                {
                    var constraintNodeView = new TreeNode(item);
                    var constraintWrapper = new NodeView(constraintNodeView);
                    constraintsContainerNodeWrapper.AddChild(constraintWrapper);
                }
            }

            //tabview has an special behaviour
            if (customView.NativeObject is NSTabView tabView)
            {
                foreach (var item in tabView.Items)
                {
                    try
                    {
                        var tabViewItem = new TabViewItem(item);
                        var treeNodeView = new TreeNode(tabViewItem);
                        var nodeView = new NodeView(treeNodeView);
                        nodeWrapper.AddChild(nodeView);

                        //now we want scan tabs
                        var tabMainView = new ViewWrapper(item.View);
                        ConvertToNodes(tabMainView, nodeView, viewMode);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            else
            {
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
        }

        public void RemoveAllErrorWindows(IWindow windowWrapper)
        {
            var window = windowWrapper.NativeObject as NSWindow;
            var childWindro = window.ChildWindows.OfType<BorderedWindow>();
            foreach (var item in childWindro)
            {
                item.Close();
            }
        }

        public FontData GetFont(IView view)
        {
            if (view == null)return null;
            return NativeViewHelper.GetFont(view.NativeObject as NSView);
        }

        object GetNativePropertyPanelWrapper(INativeObject viewSelected)
        {
            object nativeObject = viewSelected.NativeObject;
            if (nativeObject is NSComboBox comboBox)
            {
                return new PropertyPanelNSComboBox(comboBox);
            }

            if (nativeObject is NSPopUpButton popUpButton)
            {
                return new PropertyPanelNSPopupButton(popUpButton);
            }

            if (nativeObject is NSStackView stackView)
            {
                return new PropertyPanelNSStackView(stackView);
            }

            if (nativeObject is NSImageView img)
            {
                return new PropertyPanelNSImageView(img);
            }

            if (nativeObject is NSBox box)
            {
                return new PropertyPanelNSBox(box);
            }

            if (nativeObject is NSButton btn)
            {
                return new PropertyPanelNSButton(btn);
            }

            if (nativeObject is NSTextView text)
            {
                return new PropertyPanelNSTextView(text);
            }

            if (nativeObject is NSTextField textfield)
            {
                return new PropertyPanelNSTextField(textfield);
            }

            if (nativeObject is NSWindow window)
            {
                return new PropertyPanelNSWindow(window);
            }

            if (nativeObject is NSView view)
            {
                return new PropertyPanelNSView(view);
            }

            if (nativeObject is NSLayoutConstraint constraint)
            {
                return new PropertyPanelNSLayoutConstraint(constraint);
            }

            //return nativeObject;
            return new PropertyPanelNSResponder(nativeObject as NSResponder);
        }

        public object GetWrapper(INativeObject viewSelected, InspectorViewMode viewMode)
        {
            //if (viewSelected is IView view) {
                //if (viewMode == InspectorViewMode.Xwt)
                //{
                //    return view.View;
                //}
                return GetNativePropertyPanelWrapper(viewSelected);
            //}
            //if (viewSelected is IConstrain constrain)
            //    return constrain;
            //return viewSelected?.NativeObject;
        }

        public void SetFont(IView view, IFont font)
        {
            NativeViewHelper.SetFont(view.NativeObject as NSView, font.NativeObject as NSFont);
        }


        ColorResult BackColorSearch(IView view)
        {
            var properties = view.GetType().GetProperties().Where(s => s.Name.StartsWith("BackgroundColor")).ToArray();

            var property = view.GetProperty("BackgroundColor");
            if (property != null)
            {
                var colorFound = property.GetValue(view.Superview) as NSColor;
                return new ColorResult() { View = view, Color = new Color(colorFound) };
            }

            if (view.Superview is IView superView && superView != null)
            {
                var result = BackColorSearch(superView);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        bool IsSelectableView(IView customView)
        {
            return customView.CanBecomeKeyView && !customView.Hidden;
        }

        public void Recursively(IView customView, List<DetectedError> detectedErrors, InspectorViewMode viewMode)
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

        public void SetButton(IButton button, IImage image)
        {
            var btn = button.NativeObject as NSButton;
            btn.Image = image.NativeObject as NSImage; // ToNSImage(image.ToBitmap ());
        }

        public void SetButton(IImageView imageview, IImage image)
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

        public async Task<IImage> OpenDialogSelectImage(IWindow selectedWindow)
        {
            var panel = new NSOpenPanel();
            panel.AllowedFileTypes = new[] { "png" };
            panel.Prompt = "Select a image";
            IImage rtrn = null;
            processingCompletion = new TaskCompletionSource<object>();

            panel.BeginSheet(selectedWindow.NativeObject as NSWindow, result =>
            {
                if (result == 1 && panel.Url != null)
                {
                    rtrn = new Image(new NSImage(panel.Url.Path));// Xwt.Drawing.Image.FromFile(panel.Url.Path);

                }
                processingCompletion.TrySetResult(null);
            });
            await processingCompletion.Task;
            return rtrn;
        }

        public async Task InvokeImageChanged(IView view, IWindow selectedWindow)
        {
            if (view.NativeObject is NSImageView imageView)
            {
                var image = await OpenDialogSelectImage(selectedWindow);
                if (image != null)
                {
                    SetButton(new ImageView(imageView), image);
                }
            }
            else if (view.NativeObject is NSButton btn)
            {
                var image = await OpenDialogSelectImage(selectedWindow);
                if (image != null)
                {
                    SetButton(new Button(btn), image);
                }
            }
        }

        public IBorderedWindow CreateErrorWindow(IView view)
        {
            return new BorderedWindow(view, NSColor.Red);
        }

        public IFont GetFromName(string selected, int fontSize)
        {
            return new FontWrapper(NSFont.FromFontName(selected, fontSize));
        }

        public void ClearSubmenuItems(List<IMenuItem> menuItems, IMenu submenu)
        {
            var menu = (NSMenu)submenu.NativeObject;
            foreach (var item in menuItems) {
                menu.RemoveItem((NSMenuItem)item.NativeObject);
            }
        }

        public IMenuItem CreateMenuItem(string title, EventHandler menuItemOpenHandler)
        {
            var menuItem = new NSMenuItem(title, menuItemOpenHandler) { Enabled = false };
            return new MenuItem(menuItem);
        }

        public IMenuItem GetShowInspectorWindowMenuItem(EventHandler menuOpenHandler)
        {
            var inspectorMenuItem = new NSMenuItem($"Show Inspector Window", menuOpenHandler);
            inspectorMenuItem.KeyEquivalentModifierMask = NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask;
            inspectorMenuItem.KeyEquivalent = "2";
            return new MenuItem(inspectorMenuItem);
        }

        public IMenuItem GetShowAccessibilityWindowMenuItem(EventHandler menuOpenHandler)
        {
            var inspectorMenuItem = new NSMenuItem($"Show Accessibility Window", menuOpenHandler);
            inspectorMenuItem.KeyEquivalentModifierMask = NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask;
            inspectorMenuItem.KeyEquivalent = "1";
            return new MenuItem(inspectorMenuItem);
        }

        public IMenuItem GetSeparatorMenuItem()
        {
            return new MenuItem(NSMenuItem.SeparatorItem);
        }

        public void SetAppearance(bool isDark, params IWindow[] inspectorWindow)
        {
            //PropertyEditorPanel.ThemeManager.Theme = isDark ? PropertyEditorTheme.Dark : PropertyEditorTheme.Light;
            foreach (var item in inspectorWindow) {
                item?.SetAppareance(isDark);
            }
        }

        public void CreateItem(IView view, ToolbarView e)
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

        public virtual TouchBarBaseDelegate GetTouchBarDelegate(object element)
        {
            if (element is NSView)
            {
                return new ColorPickerDelegate();
            }
            return null;
        }

        public void SetCultureInfo(IWindow selectedWindow, CultureInfo e)
        {

		}

        public IWindowWatcher CreateWatcher()
        {
            return new WindowWatcher();
        }

        public bool TryGetView(INativeObject e, out IView view)
        {
            if (e?.NativeObject is NSView v)
            {
                view = new ViewWrapper(v);
                return true;
            }
            view = null;
            return false;
        }

        public IMainWindow GetTopWindow()
        {
            var currentWindow = NSApplication.SharedApplication.ModalWindow ?? NSApplication.SharedApplication.KeyWindow;

            if (currentWindow != null)
                return new ObservableWindow(currentWindow);
            return null;
        }

        public static bool IsLightAppearance(NSAppearance appearance)
        {
            var appearanceName = appearance?.Name;
            if (appearanceName == NSAppearance.NameVibrantDark ||
                appearanceName == NSAppearance.NameAccessibilityHighContrastVibrantDark ||
                appearanceName == NSAppearance.NameDarkAqua ||
                appearanceName == NSAppearance.NameAccessibilityHighContrastDarkAqua)
                return false;

            return true;
        }

        public bool IsDarkTheme()
        {
            return !IsLightAppearance(NSApplication.SharedApplication.EffectiveAppearance);
        }
    }
}