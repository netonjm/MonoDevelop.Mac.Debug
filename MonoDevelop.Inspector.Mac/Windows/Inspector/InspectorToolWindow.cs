using System;
using CoreGraphics;
using AppKit;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Mac;
using Xamarin.PropertyEditing;
using Foundation;
using System.Linq;
using MonoDevelop.Inspector.Mac.Abstractions;

namespace MonoDevelop.Inspector.Mac
{
    class InspectorToolWindow : BaseWindow, IInspectorWindow
    {
        const ushort DeleteKey = 51;

        public event EventHandler<INativeObject> RaiseFirstResponder;
        public event EventHandler<INativeObject> RaiseDeleteItem;

        public const int ButtonWidth = 30;
        const int margin = 10;
        const int ScrollViewSize = 240;
        readonly PropertyEditorProvider editorProvider;

        PropertyEditorPanel propertyEditorPanel;
        //NSLayoutConstraint constraint;

        MethodListView methodListView;
        public OutlineView outlineView { get; private set; }

        NSTabView tabView;

        readonly IInspectDelegate inspectorDelegate;
        MacInspectorToolbarView toolbarView;

        public event EventHandler<ToolbarView> RaiseInsertItem;
        public event EventHandler<Tuple<string, string, string, string>> LoadFigma;
        NSSplitView splitView => (NSSplitView)ContentView;
        readonly HostResource hostResourceProvider;


        public InspectorToolWindow(IInspectDelegate inspectorDelegate, CGRect frame) : base(frame, NSWindowStyle.Titled | NSWindowStyle.Resizable, NSBackingStore.Buffered, false)
        {
            this.inspectorDelegate = inspectorDelegate;
            ShowsToolbarButton = false;
            MovableByWindowBackground = false;

            ContentView = new NSSplitView();

            hostResourceProvider = new HostResource();
            propertyEditorPanel = new PropertyEditorPanel(hostResourceProvider);

            editorProvider = new PropertyEditorProvider();

            propertyEditorPanel.TargetPlatform = new TargetPlatform(editorProvider)
            {
                SupportsCustomExpressions = true,
                SupportsMaterialDesign = true,
            };

            outlineView = new OutlineView();
            var outlineViewScrollView = new ScrollContainerView(outlineView);

            outlineView.SelectionNodeChanged += (s, e) =>
            {
                if (outlineView.SelectedNode is TreeNodeView nodeView)
                {
                    if (nodeView.NativeObject is IConstrain constrain)
                    {
                        Console.WriteLine("IConstrain");
                        Select(constrain, InspectorViewMode.Native);
                    }
                    else if (nodeView.NativeObject is IWindow window)
                    {
                        Console.WriteLine("IWindow");
                        Select(window, InspectorViewMode.Native);
                    }
                    else
                    {
                        Console.WriteLine(nodeView.NativeObject.GetType().FullName);
                        RaiseFirstResponder?.Invoke(this, nodeView.NativeObject);
                    }
                }
            };

            outlineView.KeyPress += (sender, e) =>
            {
                if (e == DeleteKey)
                {
                    if (outlineView.SelectedNode is TreeNodeView nodeView)
                    {
                        RaiseDeleteItem?.Invoke(this, nodeView.NativeObject);
                    }
                }
            };

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
            toolbarSearchTextField.Changed += (object sender, EventArgs e) =>
            {
                Search();
            };
            toolbarHorizontalStackView.AddArrangedSubview(toolbarSearchTextField);

            var compactModeToggleButton = new ToggleButton();
            compactModeToggleButton.Image = inspectorDelegate.GetImageResource("compact-display-16.png").NativeObject as NSImage;
            compactModeToggleButton.ToolTip = "Use compact display";
            toolbarHorizontalStackView.AddArrangedSubview(compactModeToggleButton);
            compactModeToggleButton.WidthAnchor.ConstraintEqualTo(32).Active = true;

            toolbarView = new MacInspectorToolbarView() { TranslatesAutoresizingMaskIntoConstraints = false };
            var toolbarViewScrollView = new ScrollContainerView(toolbarView);
            toolbarStackView.AddArrangedSubview(toolbarViewScrollView);

            toolbarTabItem.View = toolbarStackView;
            toolbarView.ActivateSelectedItem += (sender, e) =>
            {
                RaiseInsertItem?.Invoke(this, toolbarView.SelectedItem.TypeOfView);
            };

            var outlineTabItem = new NSTabViewItem();
            outlineTabItem.Label = "View Hierarchy";
            outlineTabItem.View = outlineViewScrollView;

            tabView.Add(outlineTabItem);
            tabView.Add(toolbarTabItem);

            var wrapper = new TabView(tabView);
            foreach (var module in InspectorContext.Current.Modules)
            {
                if (!module.IsEnabled)
                {
                    continue;
                }
                module.Load(this, wrapper);
            }

            //===================

            //Method list view
            methodListView = new MethodListView();
            methodListView.AddColumn(new NSTableColumn("col") { Title = "Methods" });
            methodListView.DoubleClick += MethodListView_DoubleClick;

            scrollView = new ScrollContainerView(methodListView);
            scrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            var titleContainter = NativeViewHelper.CreateHorizontalStackView();
            titleContainter.EdgeInsets = new NSEdgeInsets(margin, 0, margin, 0);
            //titleContainter.WantsLayer = true;
            //titleContainter.Layer.BackgroundColor = NSColor.Gray.CGColor;

            methodSearchView = new NSSearchField() { TranslatesAutoresizingMaskIntoConstraints = false };
            titleContainter.AddArrangedSubview(methodSearchView);
            methodSearchView.WidthAnchor.ConstraintEqualTo(180).Active = true;

            IImage invokeImage = inspectorDelegate.GetImageResource("execute-16.png");
            IButton invokeButton = inspectorDelegate.GetImageButton(invokeImage);
            invokeButton.SetTooltip("Invoke Method!");
            invokeButton.SetWidth(ButtonWidth);

            titleContainter.AddArrangedSubview((NSView)invokeButton.NativeObject);
            invokeButton.Pressed += (s, e) => InvokeSelectedView();

            titleContainter.AddArrangedSubview(NativeViewHelper.CreateLabel("Result: "));

            resultMessage = NativeViewHelper.CreateLabel("");
            resultMessage.LineBreakMode = NSLineBreakMode.ByWordWrapping;

            titleContainter.AddArrangedSubview(resultMessage);

            var methodStackPanel = NativeViewHelper.CreateVerticalStackView();
            methodStackPanel.TranslatesAutoresizingMaskIntoConstraints = false;
            methodStackPanel.AddArrangedSubview(titleContainter);
            titleContainter.LeftAnchor.ConstraintEqualTo(methodStackPanel.LeftAnchor, 0).Active = true;
            titleContainter.RightAnchor.ConstraintEqualTo(methodStackPanel.RightAnchor, 0).Active = true;

            methodStackPanel.AddArrangedSubview(scrollView);
            /////

            var tabPropertyPanel = new NSTabViewItem();
            tabPropertyPanel.View = propertyEditorPanel;
            tabPropertyPanel.Label = "Properties";

            var tabMethod = new NSTabViewItem();

            tabMethod.View.AddSubview(methodStackPanel);
            methodStackPanel.LeftAnchor.ConstraintEqualTo(tabMethod.View.LeftAnchor, 0).Active = true;
            methodStackPanel.RightAnchor.ConstraintEqualTo(tabMethod.View.RightAnchor, 0).Active = true;
            methodStackPanel.TopAnchor.ConstraintEqualTo(tabMethod.View.TopAnchor, 0).Active = true;
            methodStackPanel.BottomAnchor.ConstraintEqualTo(tabMethod.View.BottomAnchor, 0).Active = true;

            tabMethod.Label = "Methods";

            this.tabView = new NSTabView() { TranslatesAutoresizingMaskIntoConstraints = false };
            this.tabView.Add(tabPropertyPanel);
            this.tabView.Add(tabMethod);
            splitView.AddArrangedSubview(this.tabView);

            methodSearchView.Activated += (sender, e) =>
            {
                if (viewSelected != null)
                {
                    methodListView.SetObject(viewSelected.NativeObject, methodSearchView.StringValue);
                }
            };
          
            compactModeToggleButton.Activated += (sender, e) =>
            {
                toolbarView.ShowOnlyImages(!toolbarView.IsImageMode);
            };

            toolbarView.ShowOnlyImages(true);
            splitView.SetPositionOfDivider(300,0);
        }

        NSSearchField methodSearchView;
        ScrollContainerView scrollView;
        TreeNodeView data;

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
            data = new TreeNodeView(window.ContentView);
            inspectorDelegate.ConvertToNodes(window, new NodeView(data), viewMode);
            outlineView.SetData(data);
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

        INativeObject viewSelected;

        public void Select(INativeObject view, InspectorViewMode viewMode)
        {
            viewSelected = view;
            if (viewSelected != null)
            {
                propertyEditorPanel.Select(new object[] { inspectorDelegate.GetWrapper(viewSelected, viewMode) });
            }
            else
            {
                propertyEditorPanel.Select(new object[0]);
            }

            methodListView.SetObject(view?.NativeObject, methodSearchView.StringValue);
            if (data != null && view != null)
            {
                var found = data.Search(view);
                if (found != null)
                {
                    outlineView.FocusNode(found);
                }
            }
        }

        public void RemoveItem()
        {
            if (outlineView.SelectedNode is TreeNodeView nodeView)
            {
                RaiseDeleteItem?.Invoke(this, nodeView.NativeObject);
            }
        }

        protected override void Dispose(bool disposing)
        {
            methodListView.DoubleClick -= MethodListView_DoubleClick;
            base.Dispose(disposing);
        }
    }
}
