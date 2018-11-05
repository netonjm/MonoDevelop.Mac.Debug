using System;
using CoreGraphics;
using AppKit;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing;
using Xamarin.PropertyEditing.Tests;
using Xamarin.PropertyEditing.Themes;
using Foundation;

namespace MonoDevelop.Inspector.Mac
{
    class InspectorWindow : MacWindowWrapper, IInspectorWindow
    {
        const ushort DeleteKey = 51;

        public event EventHandler<IViewWrapper> RaiseFirstResponder;
        public event EventHandler<IViewWrapper> RaiseDeleteItem;

        public const int ButtonWidth = 30;
        const int margin = 10;
        const int ScrollViewSize = 240;
        readonly MockEditorProvider editorProvider;
        readonly MockResourceProvider resourceProvider;
        readonly MockBindingProvider bindingProvider;

        PropertyEditorPanel propertyEditorPanel;
        NSLayoutConstraint constraint;

        readonly NSView contentView;
        MethodListView methodListView;
        public OutlineView outlineView { get; private set; }
       
        NSTabView tabView;

        readonly IInspectDelegate inspectorDelegate;
        MacInspectorToolbarView toolbarView;

        public event EventHandler<ToolbarView> RaiseInsertItem;

        public InspectorWindow (IInspectDelegate inspectorDelegate, CGRect frame) : base (frame, NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false)
        {
            this.inspectorDelegate = inspectorDelegate;
            ShowsToolbarButton = false;
            MovableByWindowBackground = false;
            
            propertyEditorPanel = new PropertyEditorPanel();
          
            editorProvider = new MockEditorProvider();
            resourceProvider = new MockResourceProvider();
            bindingProvider = new MockBindingProvider();

            propertyEditorPanel.TargetPlatform = new TargetPlatform(editorProvider, resourceProvider, bindingProvider)
            {
                SupportsCustomExpressions = true,
                SupportsMaterialDesign = true,
            };

            var currentThemeStyle = NSUserDefaults.StandardUserDefaults.StringForKey ("AppleInterfaceStyle") ?? "Light";
            PropertyEditorPanel.ThemeManager.Theme = currentThemeStyle == "Dark" ? PropertyEditorTheme.Dark :  PropertyEditorTheme.Light;

            contentView = ContentView;

            var stackView = NativeViewHelper.CreateVerticalStackView(margin);
            contentView.AddSubview (stackView);

            stackView.LeftAnchor.ConstraintEqualToAnchor(contentView.LeftAnchor, margin).Active = true;
            stackView.RightAnchor.ConstraintEqualToAnchor(contentView.RightAnchor, -margin).Active = true;
            stackView.TopAnchor.ConstraintEqualToAnchor(contentView.TopAnchor, margin).Active = true;

            constraint = stackView.HeightAnchor.ConstraintEqualToConstant(contentView.Frame.Height-margin * 2);
            constraint.Active = true;
            outlineView = new OutlineView ();
            var outlineViewScrollView = new ScrollContainerView (outlineView);
           
            outlineView.SelectionNodeChanged += (s, e) => {
                if (outlineView.SelectedNode is NodeView nodeView) {
                    RaiseFirstResponder?.Invoke (this, nodeView.View);
                }
            };

            outlineView.KeyPress += (sender, e) =>
            {
                if (e == DeleteKey) {
                    if (outlineView.SelectedNode is NodeView nodeView)
                    {
                        RaiseDeleteItem?.Invoke(this, nodeView.View);
                    }
                }
            };

            //TOOLBAR
            var toolbarTab = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false };
            toolbarTab.WantsLayer = true;
            toolbarTab.Layer.BackgroundColor = NSColor.Red.CGColor;

            stackView.AddArrangedSubview(toolbarTab);

            toolbarTab.LeftAnchor.ConstraintEqualToAnchor(contentView.LeftAnchor, margin).Active = true;
            toolbarTab.RightAnchor.ConstraintEqualToAnchor(contentView.RightAnchor, -margin).Active = true;
            toolbarTab.TopAnchor.ConstraintEqualToAnchor(contentView.TopAnchor, margin).Active = true;
            toolbarTab.HeightAnchor.ConstraintEqualToConstant(ScrollViewSize).Active = true;

            /////////////////

            var toolbarTabItem = new NSTabViewItem();
            toolbarTabItem.Label = "Toolbar";
            toolbarTab.Add(toolbarTabItem);
           
            var toolbarStackView = NativeViewHelper.CreateVerticalStackView();
            var toolbarHorizontalStackView = NativeViewHelper.CreateHorizontalStackView();

            toolbarSearchTextField = new NSSearchField() { TranslatesAutoresizingMaskIntoConstraints = false };
            toolbarSearchTextField.Changed += (object sender, EventArgs e) => {
                Search();
            };

            toolbarSearchTextField.SetContentCompressionResistancePriority((int)NSLayoutPriority.DefaultLow, NSLayoutConstraintOrientation.Horizontal);
            toolbarSearchTextField.SetContentHuggingPriorityForOrientation((int)NSLayoutPriority.DefaultLow, NSLayoutConstraintOrientation.Horizontal);
            
            toolbarHorizontalStackView.AddArrangedSubview(toolbarSearchTextField);

            var compactModeToggleButton = new ToggleButton();
            compactModeToggleButton.Image = inspectorDelegate.GetImageResource("compact-display-16.png").NativeObject as NSImage;
            compactModeToggleButton.ToolTip = "Use compact display";
            toolbarHorizontalStackView.AddArrangedSubview(compactModeToggleButton);

            toolbarStackView.AddArrangedSubview(toolbarHorizontalStackView);

            toolbarView = new MacInspectorToolbarView();
            var toolbarViewScrollView = new ScrollContainerView(toolbarView);
            toolbarStackView.AddArrangedSubview(toolbarViewScrollView);

            toolbarTabItem.View = toolbarStackView;

            toolbarView.ActivateSelectedItem += (sender, e) => {
                RaiseInsertItem?.Invoke(this, toolbarView.SelectedItem.TypeOfView);
            };
            //toolbarStackView.LeftAnchor.ConstraintEqualToAnchor(toolbarTabItem.View.LeftAnchor, 0).Active = true;
            //toolbarStackView.RightAnchor.ConstraintEqualToAnchor(toolbarTabItem.View.RightAnchor, 0).Active = true;
            //toolbarStackView.TopAnchor.ConstraintEqualToAnchor(toolbarTabItem.View.TopAnchor, 0).Active = true;
            //toolbarStackView.BottomAnchor.ConstraintEqualToAnchor(toolbarTabItem.View.BottomAnchor, 0).Active = true;


            var outlineTabItem = new NSTabViewItem();
            outlineTabItem.Label = "View Hierarchy";
            outlineTabItem.View.AddSubview(outlineViewScrollView);
            outlineViewScrollView.LeftAnchor.ConstraintEqualToAnchor(outlineTabItem.View.LeftAnchor, 0).Active = true;
            outlineViewScrollView.RightAnchor.ConstraintEqualToAnchor(outlineTabItem.View.RightAnchor, 0).Active = true;
            outlineViewScrollView.TopAnchor.ConstraintEqualToAnchor(outlineTabItem.View.TopAnchor, 0).Active = true;
            outlineViewScrollView.BottomAnchor.ConstraintEqualToAnchor(outlineTabItem.View.BottomAnchor, 0).Active = true;

            toolbarTab.Add(outlineTabItem);

            //===================

            //Method list view
            methodListView = new MethodListView();
            methodListView.AddColumn(new NSTableColumn("col") { Title = "Methods" });
            methodListView.DoubleClick += MethodListView_DoubleClick;

            scrollView = new ScrollContainerView (methodListView);

            var titleContainter = NativeViewHelper.CreateHorizontalStackView();
            //titleContainter.WantsLayer = true;
            //titleContainter.Layer.BackgroundColor = NSColor.Gray.CGColor;

            methodSearchView = new NSSearchField() { TranslatesAutoresizingMaskIntoConstraints = false };
            titleContainter.AddArrangedSubview(methodSearchView);
            methodSearchView.WidthAnchor.ConstraintEqualToConstant(180).Active =true;

            IImageWrapper invokeImage = inspectorDelegate.GetImageResource("execute-16.png");
            IButtonWrapper invokeButton = inspectorDelegate.GetImageButton(invokeImage);
            invokeButton.SetTooltip("Invoke Method!");
            invokeButton.SetWidth(ButtonWidth);

            titleContainter.AddArrangedSubview((NSView) invokeButton.NativeObject);
            invokeButton.Pressed += (s, e) => InvokeSelectedView();

            titleContainter.AddArrangedSubview(NativeViewHelper.CreateLabel ("Result: "));

            resultMessage = NativeViewHelper.CreateLabel ("");
            resultMessage.LineBreakMode = NSLineBreakMode.ByWordWrapping;

            titleContainter.AddArrangedSubview(resultMessage);

            var methodStackPanel = NativeViewHelper.CreateVerticalStackView();
            methodStackPanel.AddArrangedSubview(titleContainter);
            titleContainter.LeftAnchor.ConstraintEqualToAnchor(methodStackPanel.LeftAnchor, 0).Active = true;
            titleContainter.RightAnchor.ConstraintEqualToAnchor(methodStackPanel.RightAnchor, 0).Active = true;

            methodStackPanel.AddArrangedSubview(scrollView);
            /////

            var tabPropertyPanel = new NSTabViewItem();
            tabPropertyPanel.View = propertyEditorPanel;
            tabPropertyPanel.Label = "Properties";

            var tabMethod = new NSTabViewItem();

            tabMethod.View.AddSubview(methodStackPanel);
            methodStackPanel.LeftAnchor.ConstraintEqualToAnchor(tabMethod.View.LeftAnchor, 0).Active = true;
            methodStackPanel.RightAnchor.ConstraintEqualToAnchor(tabMethod.View.RightAnchor, 0).Active = true;
            methodStackPanel.TopAnchor.ConstraintEqualToAnchor(tabMethod.View.TopAnchor, 0).Active = true;
            methodStackPanel.BottomAnchor.ConstraintEqualToAnchor(tabMethod.View.BottomAnchor, 0).Active = true;

            tabMethod.Label = "Methods";

            tabView = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false } ;
            tabView.Add(tabPropertyPanel);
            tabView.Add(tabMethod);
            stackView.AddArrangedSubview(tabView as NSView);

            tabView.LeftAnchor.ConstraintEqualToAnchor(stackView.LeftAnchor, 0).Active = true;
            tabView.RightAnchor.ConstraintEqualToAnchor(stackView.RightAnchor, 0).Active = true;

            methodSearchView.Activated += (sender, e) =>
            {
                if (viewSelected != null) {
                    methodListView.SetObject(viewSelected.NativeView, methodSearchView.StringValue);
                }
            };

            compactModeToggleButton.Activated += (sender, e) => {
                toolbarView.ShowOnlyImages(!toolbarView.IsImageMode);
            };
        }
        NSSearchField methodSearchView;
        ScrollContainerView scrollView;
        NodeView data;

        List<CollectionHeaderItem> toolbarData = new List<CollectionHeaderItem>();

        public void Initialize() 
        {
            toolbarView.RegisterClassForItem(typeof(MacInspectorToolbarHeaderCollectionViewItem), MacInspectorToolbarHeaderCollectionViewItem.Name);
            toolbarView.RegisterClassForItem(typeof(MacInspectorToolbarCollectionViewItem), MacInspectorToolbarCollectionViewItem.Name);
            toolbarView.RegisterClassForItem(typeof(MacInspectorToolbarImageCollectionViewItem), MacInspectorToolbarImageCollectionViewItem.Name);

            var toolbarItem = new CollectionHeaderItem() { Label = "Main" };
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.DatePicker, Image = inspectorDelegate.GetImageResource ("view_dateView.png"), Label = "Date Picker", Description ="Provides for visually display and editing an NSDate instance." });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.WrappingLabel, Image = inspectorDelegate.GetImageResource("view_multiline.png"), Label = "Wrapping Label", Description="Display static text that line wraps as needed." });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.ScrollableTextView, Image = inspectorDelegate.GetImageResource("view_scrollable.png"), Label = "Scrollable Text View", Description="A text view enclosed in a scroll view. This configuration is suitable for UI elements typically used in inspectors." });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.TextField, Image = inspectorDelegate.GetImageResource("view_textView.png"), Label = "Text Field", Description ="Displays editable text" });

            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.PushButton, Image = inspectorDelegate.GetImageResource("view_button.png"), Label = "Push Button", Description="For use un window content areas. Use text, not images." });


            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.Label, Image = inspectorDelegate.GetImageResource("view_label.png"), Label = "Label", Description="Display a static text" });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.Search, Image = inspectorDelegate.GetImageResource("view_search.png"), Label = "Search Field", Description ="A text field that is optimized for performing text-based searches" });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.ComboBox, Image = inspectorDelegate.GetImageResource("view_combo.png"), Label = "Combo Box", Description ="Allows you to either enter text directly (as you would with NSTextField) or click the attached arrow at the right of the combo box to select from a displayed pop-up" });

            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.ImageBox, Image = inspectorDelegate.GetImageResource("view_image.png"), Label = "Image Button", Description = "For use in window content areas or toolbar" });

            toolbarData.Add(toolbarItem);

            Search();
        }

        NSSearchField toolbarSearchTextField;

        public void Search ()
        {
            if (string.IsNullOrEmpty(toolbarSearchTextField.StringValue)) {
                toolbarView.SetData(toolbarData);
            } else {
                List<CollectionHeaderItem> collectionHeaderItems = new List<CollectionHeaderItem>();
                for (int i = 0; i < toolbarData.Count; i++)
                {
                    var headerItem = new CollectionHeaderItem();

                    for (int j = 0; j < toolbarData[i].Items.Count; j++)
                    {
                        if (toolbarData[i].Items[j].Label.IndexOf (toolbarSearchTextField.StringValue, StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            headerItem.Items.Add(toolbarData[i].Items[j]);
                        }
                    }

                    if (headerItem.Items.Count > 0)
                    {
                        collectionHeaderItems.Add(headerItem);
                    }
                }

                toolbarView.SetData(collectionHeaderItems);
            }
            toolbarView.ReloadData();
        }

        public void GenerateTree(IWindowWrapper window, InspectorViewMode viewMode)
        {
            data = new NodeView(window.ContentView);
            inspectorDelegate.ConvertToNodes(window.ContentView, new MacNodeWrapper (data), viewMode);
            outlineView.SetData(data);
        }

        NSTextField resultMessage;

        void InvokeSelectedView ()
        {
            if (viewSelected == null)
            {
                return;
            }

            if (methodListView.SelectedItem is MethodTableViewItem itm)
            {
                //itm.MethodInfo
                var method = itm.MethodInfo;
                var parameters = method.GetParameters();

                List<object> arguments = null;
                if (parameters.Count () > 0) {
                    arguments = new List<object> ();
                    foreach (var item in parameters) {
                        arguments.Add (null);
                    }
                }
            
                try
                {
                    var response = method.Invoke(viewSelected.NativeView, parameters);
                    resultMessage.StringValue = response?.ToString() ?? "<null>";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
        }

        void MethodListView_DoubleClick(object sender, EventArgs e) => InvokeSelectedView();

        IViewWrapper viewSelected;

        public void GenerateStatusView (IViewWrapper view, IInspectDelegate inspectDelegate, InspectorViewMode viewMode)
        {
            viewSelected = view;
            propertyEditorPanel.Select(new object[] { inspectDelegate.GetWrapper (viewSelected, viewMode) });
            methodListView.SetObject (view.NativeView, methodSearchView.StringValue);
            if (data != null) {
                var found = data.Search(view);
                if (found != null) {
                    outlineView.FocusNode (found);
                }
            }
        }

        public void RemoveItem()
        {
            if (outlineView.SelectedNode is NodeView nodeView)
            {
                RaiseDeleteItem?.Invoke(this, nodeView.View);
            }
        }

        protected override void Dispose(bool disposing)
        {
            methodListView.DoubleClick -= MethodListView_DoubleClick;
            base.Dispose(disposing);
        }
    }
}
