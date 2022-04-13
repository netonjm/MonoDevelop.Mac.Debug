﻿using System;
using System.Collections.Generic;
using System.Text;
using AppKit;
using Foundation;

namespace MonoDevelop.Inspector.Mac
{
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

			var outlineViewDataSource = new OutlineViewDataSource(Data);
			DataSource = outlineViewDataSource;
			outlineViewDataSource.StartDrag += (sender, e) =>
			{
				StartDrag?.Invoke(this, e as Node);
			};
		}

		public void SetData (Node data)
		{
			this.Data.Node = data;
			ReloadData ();
			ExpandItem (ItemAtRow (0), true);
		}

		class OutlineViewDelegate : NSOutlineViewDelegate
		{
			public OutlineViewDelegate ()
			{
			}

			const string identifer = "myCellIdentifier";
			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var view = (NSTextField)outlineView.MakeView (identifer, this);
				if (view == null) {
					view = NativeViewHelper.CreateLabel (((Node)item).Name);
				}
				return view;
			}

			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				((OutlineView) outlineView).SelectedNode = (Node)item;
				return true;
			}
		}

		internal void FocusNode (Node node)
		{
			if (this.RowCount < 0 ) {
				return;
			}
			var index = RowForItem (node);
			if (index >= 0) {
				SelectRow (index, false);
				ScrollRowToVisible(index);
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

	public class TreeNodeView : Node
	{
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
				return "Window";
            }

			var name = string.Format("\"{0}\" WINDOW", window.Title);
			return name;
		}


		public readonly INativeObject NativeObject;

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

	class OutlineViewDataSource : NSOutlineViewDataSource
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
