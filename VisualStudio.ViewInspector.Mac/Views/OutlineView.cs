using System;
using System.Collections.Generic;
using System.Text;
using AppKit;
using Foundation;
using VisualStudio.ViewInspector.Abstractions;

namespace VisualStudio.ViewInspector.Mac.Views
{
	public class MainNode
	{
		public Node Node
		{
			get;
			set;
		}

		public MainNode()
		{
			Node = new Node("main");
		}
	}

	public class OutlineView : NSOutlineView
	{
		public event EventHandler<Node> StartDrag;
		public event EventHandler<ushort> KeyPress;

		public override void KeyDown(NSEvent theEvent)
		{
			base.KeyDown(theEvent);
			KeyPress?.Invoke(this, theEvent.KeyCode);
		}

		readonly MainNode Data = new MainNode ();

		public event EventHandler SelectionNodeChanged;

		Node selectedNode;
		public Node SelectedNode {
			get => selectedNode;
		 	set {
				if (selectedNode == value) {
					return;
				}

				selectedNode = value;
				SelectionNodeChanged?.Invoke (this, EventArgs.Empty);
			}
		}

		NSOutlineViewDelegate outlineViewDelegate;
		OutlineViewDataSource outlineViewDataSource;

		public OutlineView ()
		{
			AllowsExpansionToolTips = true;
			AllowsMultipleSelection = false;
			AutosaveTableColumns = false;
			FocusRingType = NSFocusRingType.None;
			IndentationPerLevel = 16;
			RowHeight = 17;
			NSTableColumn column = new NSTableColumn ("Values");
			column.Title = "View Outline";
			AddColumn (column);
			OutlineTableColumn = column;

			Delegate = outlineViewDelegate = GetDelegate();
			DataSource = outlineViewDataSource = GetDataSource(Data);
			outlineViewDataSource.StartDrag += (sender, e) =>
			{
				StartDrag?.Invoke(this, e as Node);
			};
		}

		public void SetData (Node data, bool expand = true)
		{
			this.Data.Node = data;
			ReloadData ();
			if (expand)
				ExpandItem (ItemAtRow (0), true);
		}

		public virtual OutlineViewDataSource GetDataSource(MainNode node)
		{
			return new OutlineViewDataSource(node);
		}

		public virtual NSOutlineViewDelegate GetDelegate()
        {
			return new OutlineViewDelegate();
		}

		internal void FocusNode (Node node)
		{
			if (this.RowCount < 0 ) {
				return;
			}
			var index = RowForItem (node);
			if (index >= 0) {
				if (node.Parent != null)
                {
					ExpandItem(node.Parent, false);
				}
				ScrollRowToVisible(index);
				SelectRow(index, false);
			}
		}
	}

	public class TreeNodeView : Node
	{
		public bool TryGetImageName(out string value)
        {
			var nativeObject = NativeObject.NativeObject;
			value = null;

			if (NativeObject is IConstrainContainer)
            {
				value = "Constraints.png";
            }
			else if (NativeObject is ITabItem)
			{
				value = "Tab.png";
			}
			else if (NativeObject is IConstrain && nativeObject is NSLayoutConstraint constraint)
			{
                switch (constraint.FirstAttribute)
                {
					case NSLayoutAttribute.Leading:
					case NSLayoutAttribute.Left:
						value = "ConstraintLeft.png";
						break;
					case NSLayoutAttribute.Trailing:
					case NSLayoutAttribute.Right:
						value = "ConstraintRight.png";
						break;
					case NSLayoutAttribute.Top:
						value = "ConstraintTop.png";
						break;
					case NSLayoutAttribute.Bottom:
						value = "ConstraintBottom.png";
						break;
					case NSLayoutAttribute.Width:
						value = "ConstraintWidth.png";
						break;
					case NSLayoutAttribute.Height:
						value = "ConstraintHeight.png";
						break;
					case NSLayoutAttribute.CenterX:
						value = "ConstraintCenterX.png";
						break;
					case NSLayoutAttribute.CenterY:
						value = "ConstraintCenterY.png";
						break;
				}
			}
			else if (nativeObject is NSWindow)
			{
				value = "Window.png";
			}
			else if (nativeObject is NSTabView)
			{
				value = "TabView.png";
			}
			else if (nativeObject is NSComboBox)
			{
				value = "ComboBox.png";
			}
			else if (nativeObject is NSPopUpButton)
			{
				value = "PopupButton.png";
			}
			else if (nativeObject is NSStackView stackView)
			{
				value = stackView.Orientation == NSUserInterfaceLayoutOrientation.Horizontal ? "StackViewHorizontal.png" : "StackViewVertical.png";
			}
			else if (nativeObject is NSSplitView)
			{
				value = "SplitView.png";
			}
			else if (nativeObject is NSSearchField)
			{
				value = "SearchField.png";
			}
			else if (nativeObject is NSSecureTextField)
			{
				value = "SecureTextField.png";
			}
			else if (nativeObject is NSTextField nSTextField)
			{
				value = nSTextField.IsLabel() ? "Label.png" : "TextField.png";
			}
			else if (nativeObject is NSButton button)
			{
                switch (button.BezelStyle)
                {
					case NSBezelStyle.Disclosure:
						value = "Disclosure.png";
						break;
					case NSBezelStyle.HelpButton:
						value = "HelpButton.png";
						break;
					default:
						value = "Button.png";
						break;
                }
			}
			else if (nativeObject is NSView)
			{
				value = "View.png";
			}

			return !string.IsNullOrEmpty(value);
        }

		static string GetIdentifier(string nodeName, string identifier)
        {
			if (string.IsNullOrEmpty(identifier))
				return nodeName;
			return string.Format("{0} ({1})", nodeName, identifier);
        }

		static string GetName (IView view)
		{
			var name = GetIdentifier(view.NodeName, view.Identifier);
			if (view.Hidden) {
				name += " (hidden)";
			}
			return name;
		}

		static string GetName(IConstrain view)
        {
            var name = string.Format("{0} ({1})", view.NodeName, view.Identifier ?? " (constraint)");
            return name;
        }

		static string GetName(IWindow window)
		{
			if (string.IsNullOrEmpty(window.Title))
            {
				return "NSWindow";
            }

			var name = string.Format("\"{0}\" NSWindow", window.Title);
			return name;
		}

		public readonly INativeObject NativeObject;

		public TreeNodeView(ITabItem view) : base(view.NodeName)
		{
			this.NativeObject = view;
		}

		public TreeNodeView(IWindow view) : base(GetName(view))
		{
			this.NativeObject = view;
		}

		public TreeNodeView (IView view) : base (GetName (view))
		{
			this.NativeObject = view;
		}

        public TreeNodeView(IConstrainContainer text) : base(text.NodeName)
        {
            this.NativeObject = text;
        }

        public TreeNodeView(IConstrain constrain) : base(GetName(constrain))
        {
            this.NativeObject = constrain;
        }
    }

    public class Node : NSObject
	{
		public string Name { get; private set; }
		List<Node> Children;

		public Node Parent { get; private set; }

		public Node (string name)
		{
			Name = name;
			Children = new List<Node> ();
		}

		public Node AddChild (string name)
		{
			Node n = new Node (name);
			AddChild(n);
			return n;
		}

		public void AddChild (Node node)
		{
			if (!Children.Contains(node))
            {
				Children.Add(node);
				node.Parent = this;
			}
		}

		public void RemoveChild(Node node)
		{
			if (Children.Contains(node))
            {
				Children.Remove(node);
				node.Parent = null;
			}
		}

		public Node GetChild (int index)
		{
			return Children [index];
		}

		public int ChildCount { get { return Children.Count; } }
		public bool IsLeaf { get { return ChildCount == 0; } }
	}

	public abstract class ImageRowSubView : NSView
	{
		internal const string IdentifierId = "PreferencesSubCategoriesCell";

		protected NSImage image;
		protected NSImageView imageView;
		protected NSTextField textField;

		public ImageRowSubView()
		{
			Identifier = IdentifierId;

			imageView = new NSImageView() { TranslatesAutoresizingMaskIntoConstraints = false };
			this.AddSubview(imageView);

			imageView.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
			imageView.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 4).Active = true;

			imageView.WidthAnchor.ConstraintEqualTo(16).Active = true;
			imageView.HeightAnchor.ConstraintEqualTo(16).Active = true;

			textField = NSTextField.CreateLabel(string.Empty);
			textField.TranslatesAutoresizingMaskIntoConstraints = false;
			textField.UsesSingleLineMode = true;
			textField.LineBreakMode = NSLineBreakMode.TruncatingTail;

			this.AddSubview(textField);
			textField.CenterYAnchor.ConstraintEqualTo(this.CenterYAnchor).Active = true;
			textField.LeadingAnchor.ConstraintEqualTo(this.LeadingAnchor, 25).Active = true;
		}

		public override void ViewDidChangeEffectiveAppearance()
		{
			base.ViewDidChangeEffectiveAppearance();
			RefreshStates();
		}

		internal void RefreshStates()
		{
			if (image == null)
				return;

			imageView.Image = image;
		}
	}

	public class OutlineViewDelegate : NSOutlineViewDelegate
	{
		public OutlineViewDelegate()
		{
		}

		protected const string identifer = "myCellIdentifier";
		public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var view = (NSTextField)outlineView.MakeView(identifer, this);
			if (view == null)
			{
				view = NativeViewHelper.CreateLabel(((Node)item).Name);
			}
			return view;
		}

		public override bool ShouldSelectItem(NSOutlineView outlineView, NSObject item)
		{
			((OutlineView)outlineView).SelectedNode = (Node)item;
			return true;
		}
	}

	public class OutlineViewDataSource : NSOutlineViewDataSource
	{
		public event EventHandler<NSObject> StartDrag;

		MainNode mainNode;
		public OutlineViewDataSource (MainNode mainNode)
		{
			this.mainNode = mainNode; 
		}

		public override INSPasteboardWriting PasteboardWriterForItem(NSOutlineView outlineView, NSObject item)
		{
			StartDrag?.Invoke(this, item);
			return null;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			item = item == null ? mainNode.Node : item;
			return ((Node)item).ChildCount;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			item = item == null ? mainNode.Node : item;
			return ((Node)item).GetChild ((int)childIndex);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			item = item == null ? mainNode.Node : item;
			return !((Node)item).IsLeaf;
		}
	}
}
