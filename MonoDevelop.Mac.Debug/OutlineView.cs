using System;
using System.Collections.Generic;
using AppKit;
using Foundation;

namespace MonoDevelop.Mac.Debug
{
	class OutlineView : NSOutlineView
	{
		readonly MainNode data = new MainNode ();

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

			Delegate = new OutlineViewDelegate ();
			DataSource = new OutlineViewDataSource (data);
		}

		public void SetData (Node data)
		{
			this.data.Node = data;
			ReloadData ();
		}

		// Delegates recieve events associated with user action and determine how an item should be visualized
		class OutlineViewDelegate : NSOutlineViewDelegate
		{
			public OutlineViewDelegate ()
			{
			}

			const string identifer = "myCellIdentifier";
			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				// This pattern allows you reuse existing views when they are no-longer in use.
				// If the returned view is null, you instance up a new view
				// If a non-null view is returned, you modify it enough to reflect the new data
				NSTextField view = (NSTextField)outlineView.MakeView (identifer, this);
				if (view == null) {
					view = new NSTextField () {
						Identifier = identifer,
						Bordered = false,
						Selectable = false,
						Editable = false
					};
				}

				view.StringValue = ((Node)item).Name;
				return view;
			}

			// An example of responding to user input 
			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				((OutlineView) outlineView).SelectedNode = (Node)item;
				Console.WriteLine ("ShouldSelectItem: {0}", ((Node)item).Name);
				return true;
			}
		}
	}

	class MainNode
	{
		public Node Node {
			get;
			set;
		}

		public MainNode ()
		{
			Node = new Node ("test");
		}
	}

	class NodeView : Node
	{
		static string GetName (NSView view)
		{
			return view.GetType ().ToString ();
		}

		public readonly NSView View;

		public NodeView (NSView view) : base (GetName (view))
		{
			this.View = view;
		}
	}

	class Node : NSObject
	{
		public string Name { get; private set; }
		List<Node> Children;

		public Node (string name)
		{
			Name = name;
			Children = new List<Node> ();
		}

		public Node AddChild (string name)
		{
			Node n = new Node (name);
			Children.Add (n);
			return n;
		}

		public void AddChild (Node node)
		{
			Children.Add (node);
		}

		public Node GetChild (int index)
		{
			return Children [index];
		}

		public int ChildCount { get { return Children.Count; } }
		public bool IsLeaf { get { return ChildCount == 0; } }
	}



	// Data sources walk a given data source and respond to questions from AppKit to generate
	// the data used in your Delegate. In this example, we walk a simple tree.
	class OutlineViewDataSource : NSOutlineViewDataSource
	{
		MainNode mainNode;
		public OutlineViewDataSource (MainNode mainNode)
		{
			this.mainNode = mainNode; 
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			// If item is null, we are referring to the root element in the tree
			item = item == null ? mainNode.Node : item;
			return ((Node)item).ChildCount;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			// If item is null, we are referring to the root element in the tree
			item = item == null ? mainNode.Node : item;
			return ((Node)item).GetChild ((int)childIndex);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			// If item is null, we are referring to the root element in the tree
			item = item == null ? mainNode.Node : item;
			return !((Node)item).IsLeaf;
		}
	}


}
