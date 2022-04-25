using System;
using CoreGraphics;
using AppKit;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing;
using System.Linq;
using VisualStudio.ViewInspector.Mac.Views;
using VisualStudio.ViewInspector.Abstractions;
using VisualStudio.ViewInspector.Mac.Abstractions;

namespace VisualStudio.ViewInspector.Mac.Windows.Inspector
{
    class InspectorToolWindow : BaseWindow, IInspectorWindow
    {
        public event EventHandler<INativeObject> FirstRespondedChanged;
        public event EventHandler<INativeObject> ItemDeleted;
        public event EventHandler<ToolbarView> ItemInserted;

        public event EventHandler RefreshTreeViewRequested;

        internal void RaiseFirstRespondedChanged(INativeObject nativeObject) => FirstRespondedChanged?.Invoke(this, nativeObject);
        internal void RaiseItemDeleted(INativeObject nativeObject) => ItemDeleted?.Invoke(this, nativeObject);
        internal void RaiseItemInserted(ToolbarView view) => ItemInserted?.Invoke(this, view);

        public void GenerateTree(IWindow window, InspectorViewMode viewMode) => contentViewController.GenerateTree(window, viewMode);
        public void Initialize() => contentViewController.Initialize();
        public void RemoveItem() => contentViewController.RemoveItem();
        public void Select(INativeObject view, InspectorViewMode mode) => contentViewController.Select(view, mode);

        readonly InspectorToolContentViewController contentViewController;

        public InspectorToolWindow(IInspectDelegate inspectorDelegate, CGRect frame) : base(frame, NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false)
        {
            ShowsToolbarButton = false;
            MovableByWindowBackground = false;

            ContentViewController = contentViewController = new InspectorToolContentViewController(inspectorDelegate);
        }

        internal void RequestRefreshTreeView()
        {
            RefreshTreeViewRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    class InspectorToolContentViewController : NSViewController
    {
        public event EventHandler<Tuple<string, string, string, string>> LoadFigma;

        const ushort DeleteKey = 51;
        const int margin = 10;

        public const int ButtonWidth = 30;
       
        readonly PropertyEditorProvider editorProvider;
        readonly IInspectDelegate inspectorDelegate;

        readonly NSSplitView splitView;
        readonly HostResource hostResourceProvider;

        public InspectorOutlineView outlineView { get; private set; }

        PropertyEditorPanel propertyEditorPanel;
        
        EventListView eventOutlineView;
        MethodListView methodListView;

        NSTabView tabView;
        MacInspectorToolbarView toolbarView;
        InspectorToolWindow inspectorToolWindow => (InspectorToolWindow)View.Window;
        ToggleButton compactModeToggleButton;

        EventDelegate eventDelegate;
        MainNode eventMainNode;

        ScrollContainerView eventScrollView;

        IButton invokeButton;

        List<NSLayoutConstraint> visualizedConstraints = new List<NSLayoutConstraint>();

        void ClearVisualizedConstraints()
        {
            visualizedConstraints.Clear();
            CurrentWindow?.VisualizeConstraints(new NSLayoutConstraint[0]);
        }

        public InspectorToolContentViewController (IInspectDelegate inspectorDelegate)
        {
            this.inspectorDelegate = inspectorDelegate;

            View = new NSView() { TranslatesAutoresizingMaskIntoConstraints = false };

            splitView = new NSSplitView() { TranslatesAutoresizingMaskIntoConstraints = false };
            View.AddSubview(splitView);

            splitView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 7).Active = true;
            splitView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -7).Active = true;
            splitView.TopAnchor.ConstraintEqualTo(View.TopAnchor, 7).Active = true;
            splitView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor, -7).Active = true;

            hostResourceProvider = new HostResource();
            propertyEditorPanel = new PropertyEditorPanel(hostResourceProvider);

            editorProvider = new PropertyEditorProvider();

            propertyEditorPanel.TargetPlatform = new TargetPlatform(editorProvider)
            {
                SupportsCustomExpressions = true,
                SupportsMaterialDesign = true,
            };

            outlineView = new InspectorOutlineView();
            var outlineViewScrollView = new ScrollContainerView(outlineView);
            outlineViewScrollView.BackgroundColor = NSColor.Clear;
            outlineViewScrollView.DrawsBackground = false;

            //TOOLBAR
            var tabView = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false };
            splitView.AddArrangedSubview(tabView);

            //toolbarTab.LeftAnchor.ConstraintEqualToAnchor(contentView.LeftAnchor, margin).Active = true;
            //toolbarTab.RightAnchor.ConstraintEqualToAnchor(contentView.RightAnchor, -margin).Active = true;
            //toolbarTab.TopAnchor.ConstraintEqualToAnchor(contentView.TopAnchor, margin).Active = true;

            /////////////////

            var toolbarTabItem = new NSTabViewItem();
            toolbarTabItem.Label = "Toolbar";
         
            var toolbarStackView = NativeViewHelper.CreateVerticalStackView(translatesAutoresizingMaskIntoConstraints: true);
            toolbarStackView.EdgeInsets = new NSEdgeInsets(10, 10, 10, 10);

            var toolbarHorizontalStackView = NativeViewHelper.CreateHorizontalStackView();
            toolbarStackView.AddArrangedSubview(toolbarHorizontalStackView);
          
            toolbarSearchTextField = new NSSearchField() { TranslatesAutoresizingMaskIntoConstraints = false };
           
            toolbarHorizontalStackView.AddArrangedSubview(toolbarSearchTextField);

            compactModeToggleButton = new ToggleButton();
            compactModeToggleButton.Image = inspectorDelegate.GetImageResource("compact-display-16.png").NativeObject as NSImage;
            compactModeToggleButton.ToolTip = "Use compact display";
            toolbarHorizontalStackView.AddArrangedSubview(compactModeToggleButton);
            compactModeToggleButton.WidthAnchor.ConstraintEqualTo(32).Active = true;

            toolbarView = new MacInspectorToolbarView() { TranslatesAutoresizingMaskIntoConstraints = false };
            var toolbarViewScrollView = new ScrollContainerView(toolbarView);
            toolbarStackView.AddArrangedSubview(toolbarViewScrollView);

            toolbarTabItem.View = toolbarStackView;
           
            var outlineTabItem = new NSTabViewItem();
            outlineTabItem.Label = "View Hierarchy";
            outlineTabItem.View = outlineViewScrollView;

            tabView.Add(outlineTabItem);
            tabView.Add(toolbarTabItem);


            //Bottom TabView ==================================================

            var wrapper = new TabViewWrapper(tabView);
            foreach (var module in InspectorContext.Current.Modules)
            {
                if (!module.IsEnabled)
                {
                    continue;
                }
                module.Load(inspectorToolWindow, wrapper);
            }

            //Properties tab
            var tabProperties = new NSTabViewItem() {
                Label = PropertiesTabName,
                View = propertyEditorPanel
            };

            //Method list view
            var methodsStackPanel = CreateMethodTabView();
            var tabMethod = new NSTabViewItem() {
                Label = MethodsTabName
            };

            tabMethod.View.AddSubview(methodsStackPanel);
            methodsStackPanel.LeftAnchor.ConstraintEqualTo(tabMethod.View.LeftAnchor, 0).Active = true;
            methodsStackPanel.RightAnchor.ConstraintEqualTo(tabMethod.View.RightAnchor, 0).Active = true;
            methodsStackPanel.TopAnchor.ConstraintEqualTo(tabMethod.View.TopAnchor, 0).Active = true;
            methodsStackPanel.BottomAnchor.ConstraintEqualTo(tabMethod.View.BottomAnchor, 0).Active = true;


            //Events
            var eventsStackPanel = CreateEventsTabView();
            var tabEvents = new NSTabViewItem() {
                Label = EventsTabName,
            };
            tabEvents.View.AddSubview(eventsStackPanel);
            eventsStackPanel.LeftAnchor.ConstraintEqualTo(tabEvents.View.LeftAnchor, 0).Active = true;
            eventsStackPanel.RightAnchor.ConstraintEqualTo(tabEvents.View.RightAnchor, 0).Active = true;
            eventsStackPanel.TopAnchor.ConstraintEqualTo(tabEvents.View.TopAnchor, 0).Active = true;
            eventsStackPanel.BottomAnchor.ConstraintEqualTo(tabEvents.View.BottomAnchor, 0).Active = true;

            this.tabView = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false };
            this.tabView.Add(tabProperties);
            this.tabView.Add(tabMethod);
            this.tabView.Add(tabEvents);

            splitView.AddArrangedSubview(this.tabView);

            toolbarView.ShowOnlyImages(true);

            outlineView.CreateMenuHandler = (theEvent) =>
            {
                var point = outlineView.ConvertPointFromView(theEvent.LocationInWindow, null);
                var item = outlineView.ItemAtRow(outlineView.GetRow(point));

                var menu = new NSMenu();
                if (item is TreeNode treeNode)
                {
                    bool hasOptions = false;

                    if (treeNode.Content.NativeObject is NSView nSView)
                    {
                        hasOptions = true;

                        if (nSView is NSControl control)
                        {
                            menu.AddItem(new NSMenuItem(control.Enabled ? "Disable" : "Enable", (s, e) =>
                            {
                                control.Enabled = !control.Enabled;
                            }));
                        }

                        menu.AddItem(new NSMenuItem(nSView.Hidden ? "Show" : "Hide", (s, e) =>
                        {
                            nSView.Hidden = !nSView.Hidden;

                            RescanViews();
                        }));
                    }

                    if (visualizedConstraints.Count > 0)
                    {
                        menu.AddItem(new NSMenuItem("Hide Constraints Visualized", (s, e) =>
                        {
                            ClearVisualizedConstraints();
                        }));
                    }

                    if (treeNode.Content.NativeObject is NSLayoutConstraint constraint)
                    {
                        menu.AddItem(new NSMenuItem("Visualize Constraint", (s, e) =>
                        {
                            visualizedConstraints.Add(constraint);

                            CurrentWindow?.VisualizeConstraints(visualizedConstraints.ToArray());
                        }));

                        hasOptions = true;
                    }

                    if (hasOptions)
                    {
                        menu.AddItem(NSMenuItem.SeparatorItem);
                    }

                    menu.AddItem(new NSMenuItem("Delete", (s, e) =>
                    {
                        inspectorToolWindow.RaiseItemDeleted(treeNode.Content);
                    }));
                }
                return menu;
            };
        }

        public override void ViewWillAppear()
        {
            base.ViewWillAppear();

            var defaultSize = new CGSize(400, 650);
            inspectorToolWindow.SetContentSize(defaultSize);
            splitView.SetPositionOfDivider(defaultSize.Height / 2, 0);


            toolbarView.ActivateSelectedItem += ToolbarView_ActivateSelectedItem;
            outlineView.SelectionNodeChanged += OutlineView_SelectionNodeChanged;
            outlineView.KeyPress += OutlineView_KeyPress;

            methodSearchView.Activated += MethodSearchView_Activated;
            compactModeToggleButton.Activated += CompactModeToggleButton_Activated;
            tabView.DidSelect += TabView_DidSelect;
            toolbarSearchTextField.Changed += ToolbarSearchTextField_Changed;


            methodListView.DoubleClick += MethodListView_DoubleClick;

            invokeButton.Pressed += InvokeButton_Pressed;

            outlineView.DragOperationCompleted += OutlineView_DragOperationCompleted;
        }

        void RescanViews () => inspectorToolWindow?.RequestRefreshTreeView();

        private void OutlineView_DragOperationCompleted(object sender, EventArgs e)  => RescanViews ();

        public override void ViewWillDisappear()
        {
            toolbarView.ActivateSelectedItem -= ToolbarView_ActivateSelectedItem;
            outlineView.SelectionNodeChanged -= OutlineView_SelectionNodeChanged;
            outlineView.KeyPress -= OutlineView_KeyPress;

            methodListView.DoubleClick -= MethodListView_DoubleClick;

            methodSearchView.Activated -= MethodSearchView_Activated;
            compactModeToggleButton.Activated -= CompactModeToggleButton_Activated;
            tabView.DidSelect -= TabView_DidSelect;
            toolbarSearchTextField.Changed -= ToolbarSearchTextField_Changed;

            outlineView.DragOperationCompleted -= OutlineView_DragOperationCompleted;
            invokeButton.Pressed -= InvokeButton_Pressed;

            base.ViewWillDisappear();
        }

        private void InvokeButton_Pressed(object sender, EventArgs e) => InvokeSelectedView();

        private void OutlineView_SelectionNodeChanged(object sender, EventArgs e)
        {
            if (outlineView.SelectedNode is TreeNode treeNode)
            {
                if (treeNode.Content is IView view)
                {
                    inspectorToolWindow.RaiseFirstRespondedChanged(view);
                }
                else if (treeNode.Content is IConstrain constrain)
                {
                    inspectorToolWindow.RaiseFirstRespondedChanged(constrain);
                    Select(treeNode.Content, viewModeSelected);
                }
                else
                {
                    inspectorToolWindow.RaiseFirstRespondedChanged(null);
                    Select(treeNode.Content, viewModeSelected);
                }
            }
        }

        private void OutlineView_KeyPress(object sender, ushort e)
        {
            if (e == DeleteKey)
            {
                if (outlineView.SelectedNode is TreeNode nodeView)
                {
                    inspectorToolWindow.RaiseItemDeleted(nodeView.Content);
                }
            }
        }

        private void ToolbarView_ActivateSelectedItem(object sender, EventArgs e)
        {
            if (View.Window is InspectorToolWindow insWindow)
            {
                insWindow?.RaiseItemInserted(toolbarView.SelectedItem.TypeOfView);
            }
        }

        private void ToolbarSearchTextField_Changed(object sender, EventArgs e)
        {
            Search();
        }

        private void CompactModeToggleButton_Activated(object sender, EventArgs e)
        {
            toolbarView.ShowOnlyImages(!toolbarView.IsImageMode);
        }

        private void MethodSearchView_Activated(object sender, EventArgs e)
        {
            if (viewSelected != null)
            {
                methodListView.SetObject(viewSelected.NativeObject, methodSearchView.StringValue);
            }
        }

        NSStackView CreateEventsTabView()
        {
            eventOutlineView = new EventListView();

            eventMainNode = new MainNode();
           
            eventScrollView = new ScrollContainerView(eventOutlineView);
            eventScrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            var eventsStackPanel = NativeViewHelper.CreateVerticalStackView();
            eventsStackPanel.TranslatesAutoresizingMaskIntoConstraints = false;
            eventsStackPanel.AddArrangedSubview(eventScrollView);

            return eventsStackPanel;
        }

        NSStackView CreateMethodTabView()
        {
            methodListView = new MethodListView();
            methodListView.AddColumn(new NSTableColumn("col") { Title = "Methods" });
         
            methodScrollView = new ScrollContainerView(methodListView);
            methodScrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            var titleContainter = NativeViewHelper.CreateHorizontalStackView();
            titleContainter.EdgeInsets = new NSEdgeInsets(0, margin, 0, margin);
            //titleContainter.WantsLayer = true;
            //titleContainter.Layer.BackgroundColor = NSColor.Gray.CGColor;

            methodSearchView = new NSSearchField() { TranslatesAutoresizingMaskIntoConstraints = false };
            titleContainter.AddArrangedSubview(methodSearchView);
            methodSearchView.WidthAnchor.ConstraintEqualTo(180).Active = true;

            IImage invokeImage = inspectorDelegate.GetImageResource("execute-16.png");

            invokeButton = inspectorDelegate.GetImageButton(invokeImage);
            invokeButton.SetTooltip("Invoke Method!");
            invokeButton.SetWidth(ButtonWidth);

            titleContainter.AddArrangedSubview((NSView)invokeButton.NativeObject);

            titleContainter.AddArrangedSubview(NativeViewHelper.CreateLabel("Result: "));

            resultMessage = NativeViewHelper.CreateLabel("");
            resultMessage.LineBreakMode = NSLineBreakMode.TruncatingTail;
            resultMessage.Cell.UsesSingleLineMode = true;

            titleContainter.AddArrangedSubview(resultMessage);

            var methodStackPanel = NativeViewHelper.CreateVerticalStackView();
            methodStackPanel.TranslatesAutoresizingMaskIntoConstraints = false;
            methodStackPanel.AddArrangedSubview(titleContainter);
            titleContainter.LeftAnchor.ConstraintEqualTo(methodStackPanel.LeftAnchor, 0).Active = true;
            titleContainter.RightAnchor.ConstraintEqualTo(methodStackPanel.RightAnchor, 0).Active = true;

            methodStackPanel.AddArrangedSubview(methodScrollView);

            methodListView.WidthAnchor.ConstraintEqualTo(methodStackPanel.WidthAnchor).Active = true;

            return methodStackPanel;
        }

        const string PropertiesTabName = "Properties";
        const string MethodsTabName = "Methods";
        const string EventsTabName = "Events";

        private void TabView_DidSelect(object sender, NSTabViewItemEventArgs e)
        {
            if (e.Item.Label == PropertiesTabName)
            {
                RefreshPropertyEditor();
            }
            else if (e.Item.Label == MethodsTabName)
            {
                RefreshMethodList();
            }
            else if (e.Item.Label == EventsTabName)
            {
                RefreshEventsList();
            }
        }

        NSSearchField methodSearchView;
        ScrollContainerView methodScrollView;
        TreeNode data;

        List<CollectionHeaderItem> toolbarData = new List<CollectionHeaderItem>();

        public void Initialize()
        {
            toolbarView.RegisterClassForItem(typeof(InspectorToolbarHeaderViewItem), InspectorToolbarHeaderViewItem.Name);
            toolbarView.RegisterClassForItem(typeof(InspectorToolbarViewItem), InspectorToolbarViewItem.Name);
            toolbarView.RegisterClassForItem(typeof(InspectorToolbarImageViewItem), InspectorToolbarImageViewItem.Name);

            var toolbarItem = new CollectionHeaderItem() { Label = "Main" };
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.DatePicker, Image = inspectorDelegate.GetImageResource("view_dateView.png"), Label = "Date Picker", Description = "Provides for visually display and editing an NSDate instance." });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.WrappingLabel, Image = inspectorDelegate.GetImageResource("view_multiline.png"), Label = "Wrapping Label", Description = "Display static text that line wraps as needed." });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.ScrollableTextView, Image = inspectorDelegate.GetImageResource("view_scrollable.png"), Label = "Scrollable Text View", Description = "A text view enclosed in a scroll view. This configuration is suitable for UI elements typically used in inspectors." });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.TextField, Image = inspectorDelegate.GetImageResource("view_textView.png"), Label = "Text Field", Description = "Displays editable text" });

            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.PushButton, Image = inspectorDelegate.GetImageResource("view_button.png"), Label = "Push Button", Description = "For use un window content areas. Use text, not images." });

            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.Label, Image = inspectorDelegate.GetImageResource("view_label.png"), Label = "Label", Description = "Display a static text" });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.Search, Image = inspectorDelegate.GetImageResource("view_search.png"), Label = "Search Field", Description = "A text field that is optimized for performing text-based searches" });
            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.ComboBox, Image = inspectorDelegate.GetImageResource("view_combo.png"), Label = "Combo Box", Description = "Allows you to either enter text directly (as you would with NSTextField) or click the attached arrow at the right of the combo box to select from a displayed pop-up" });

            toolbarItem.Items.Add(new CollectionItem() { TypeOfView = ToolbarView.ImageBox, Image = inspectorDelegate.GetImageResource("view_image.png"), Label = "Image Button", Description = "For use in window content areas or toolbar" });

            toolbarData.Add(toolbarItem);

            Search();
        }

        NSSearchField toolbarSearchTextField;

        public void Search()
        {
            if (string.IsNullOrEmpty(toolbarSearchTextField.StringValue))
            {
                toolbarView.SetData(toolbarData);
            }
            else
            {
                List<CollectionHeaderItem> collectionHeaderItems = new List<CollectionHeaderItem>();
                for (int i = 0; i < toolbarData.Count; i++)
                {
                    var headerItem = new CollectionHeaderItem();

                    for (int j = 0; j < toolbarData[i].Items.Count; j++)
                    {
                        if (toolbarData[i].Items[j].Label.IndexOf(toolbarSearchTextField.StringValue, StringComparison.OrdinalIgnoreCase) > -1)
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

        public void GenerateTree(IWindow window, InspectorViewMode viewMode)
        {
            data = new TreeNode(window.ContentView);
            inspectorDelegate.ConvertToNodes(window, new NodeView(data), viewMode);
            outlineView.SetData(data, false);
            ExpandNodes(data);

            if (viewSelected != null)
            {
                Select(viewSelected, viewModeSelected);
            }
        }

        bool IsExpandibleNode(TreeNode nodeView)
        {
            if (nodeView.Content is IConstrainContainer)
            {
                return false;
            }

            if (nodeView.Content is IView view && view.NativeObject is NSView nsView)
            {
                if (nsView is NSTextField)
                    return false;
                if (nsView is NSTextView)
                    return false;
                if (nsView is NSComboBox)
                    return false;
                if (nsView is NSPopUpButton)
                    return false;
                if (nsView is NSButton)
                    return false;
                if (nsView is NSImageView)
                    return false;
                if (nsView is NSView v && v.Hidden)
                    return false;
            }

            return true;
        }

        void ExpandNodes(TreeNode nodeView)
        {
            if (!IsExpandibleNode(nodeView))
            {
                return;
            }

            outlineView.ExpandItem(nodeView, false);

            for (int i = 0; i < nodeView.ChildCount; i++)
            {
                var child =(TreeNode) nodeView.GetChild(i);
                ExpandNodes(child);
            }
        }

        NSTextField resultMessage;

        void InvokeSelectedView()
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
                if (parameters.Count() > 0)
                {
                    arguments = new List<object>();
                    foreach (var item in parameters)
                    {
                        arguments.Add(null);
                    }
                }

                try
                {
                    var response = method.Invoke(viewSelected.NativeObject, parameters);
                    resultMessage.StringValue = response?.ToString() ?? "<null>";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };
        }

        void MethodListView_DoubleClick(object sender, EventArgs e) => InvokeSelectedView();

        NSWindow CurrentWindow
        {
            get
            {
                if (viewSelected != null)
                {
                    if (viewSelected.NativeObject is NSLayoutConstraint constraint)
                    {
                        return GetWindow(constraint);
                    }
                    if (viewSelected.NativeObject is NSView view)
                    {
                        return view.Window;
                    }
                }
                return null;
            }
        }

        NSWindow GetWindow(NSLayoutConstraint constraint)
        {
            return (constraint.FirstItem as NSView)?.Window ?? (constraint.SecondItem as NSView)?.Window;
        }
            
        INativeObject viewSelected;
        InspectorViewMode viewModeSelected;

        public void Select(INativeObject view, InspectorViewMode viewMode)
        {
            viewSelected = view;
            viewModeSelected = viewMode;

            if (data != null && view != null)
            {
                var found = data.Search(view);
                if (found != null)
                {
                    outlineView.FocusNode(found);
                }
            }

            switch (tabView.Selected.Label)
            {
                case PropertiesTabName:
                    RefreshPropertyEditor();
                    break;
                case MethodsTabName:
                    RefreshMethodList();
                    break;
                case EventsTabName:
                    RefreshEventsList();
                    break;
            }
        }

        void RefreshMethodList()
        {
            methodListView.SetObject(viewSelected?.NativeObject, methodSearchView.StringValue);
        }

        void RefreshEventsList()
        {
            eventOutlineView.SetObject(viewSelected?.NativeObject, methodSearchView.StringValue);
        }

        void RefreshPropertyEditor()
        {
            if (viewSelected != null)
            {
                var wrapper = inspectorDelegate.GetWrapper(viewSelected, viewModeSelected);
                propertyEditorPanel.Select(new object[] { wrapper });
            }
            else
            {
                propertyEditorPanel.Select(new object[0]);
            }
        }

        public void RemoveItem()
        {
            if (outlineView.SelectedNode is TreeNode nodeView)
            {
                inspectorToolWindow.RaiseItemDeleted(nodeView.Content);
            }
        }
    }
}
