﻿// This file has been autogenerated from a class added in the UI designer.

using System.Collections.Generic;
using System.Text;
using AppKit;
using CoreGraphics;
using System.Linq;

namespace MonoDevelop.Inspector.Mac
{
    class TreeViewItemRectangle : IRectangle
    {
        CGRect rect;
        public TreeViewItemRectangle(CGRect rect)
        {
            this.rect = rect;
        }

        public object NativeObject => rect;
    }

    class TreeeViewItemFont : IFontWrapper
    {
        NSFont rect;
        public TreeeViewItemFont(NSFont rect)
        {
            this.rect = rect;
        }

        public object NativeObject => rect;
    }

    class TreeViewItemConstraint : IConstrain
    {
        NSLayoutConstraint item;
        public TreeViewItemConstraint(NSLayoutConstraint item)
        {
            this.item = item;
        }

        public object NativeObject => item;

        string GetNodeName ()
        {
            StringBuilder builder = new StringBuilder();

            if (item.SecondItem != null) {
                builder.Append(string.Format("F:[{0}, {1}]", item.FirstAttribute.ToString(), item.FirstItem.GetType ().Name));
                builder.Append($" {item.Relation}");
                builder.Append(string.Format(" S:[{0}, {1}]", item.SecondAttribute.ToString(), item.SecondItem.GetType().Name));
            }
            else
            {
                builder.Append($"{item.FirstItem.GetType().Name} {item.FirstAttribute.ToString()}");
            }
           
            builder.Append($" C:{item.Constant} M:{item.Multiplier} P:{item.Priority}");
            return builder.ToString();
        }

        public void RemoveFromSuperview()
        {
            if (item.FirstItem is NSView firstItem) {
                if (firstItem.Constraints.Contains (item)) {
                    firstItem.RemoveConstraint(item);
                    return;
                }
            }
            if (item.SecondItem is NSView secondItem)
            {
                if (secondItem.Constraints.Contains(item))
                {
                    secondItem.RemoveConstraint(item);
                    return;
                }
            }
        }

        public string NodeName { get => GetNodeName(); }
        public string Identifier { get => item.Active ? "active" : "inactive"; }

        public IView PreviousValidKeyView {
            get => new TreeViewItemView (item.FirstItem as NSView ?? item.SecondItem as NSView);
        }
    }

    class ConstrainContainer : IConstrainContainer
    {
        IView wrapper;
        NSView view;
        public ConstrainContainer(IView previous)
        {
            this.wrapper = previous;
            view = previous.NativeObject as NSView;
        }

        public string NodeName => "Constraints";

        public IView PreviousValidKeyView => wrapper;

        public object NativeObject => null;

        public void RemoveFromSuperview()
        {
            if (view != null)
            {
                var constraints = view.Constraints;
                view.RemoveConstraints(constraints);
            }
        }
    }

    class TabView : ITab
    {
        NSTabView view;

        public TabView(NSTabView previous)
        {
            this.view = previous;
        }

        public string NodeName => "NSTabView";

        public object NativeObject => view;
    }

    class TreeViewItemView : IView
    {
        public bool Hidden => widget.Hidden;

        public string Identifier => widget.Identifier;

        public IRectangle AccessibilityFrame => new TreeViewItemRectangle(widget.AccessibilityFrame);

        public List<IView> Subviews {
            get {
                List<IView> tmp = new List<IView>();
                foreach (var w in widget.Subviews) {
                    tmp.Add(new TreeViewItemView(w));
                }
                return tmp;
            }
        }

        public IView NextValidKeyView {
            get {
                if (widget.NextValidKeyView != null)
                    return new TreeViewItemView(widget.NextValidKeyView);
                return null;
            }
        }

        public IView PreviousValidKeyView {
            get {
                if (widget.PreviousValidKeyView != null)
                    return new TreeViewItemView(widget.PreviousValidKeyView);
                return null;
            }
        }

        public IRectangle Frame => new TreeViewItemRectangle(widget.Frame);

        public IView Superview {
            get {
                if (widget.Superview != null)
                    return new TreeViewItemView(widget.Superview);
                return null;
            }
        }

        public string AccessibilityTitle {
            get => widget.AccessibilityTitle;
            set => widget.AccessibilityTitle = value;
        }

        public string AccessibilityHelp {
            get => widget.AccessibilityHelp;
            set => widget.AccessibilityHelp = value;
        }

        public object AccessibilityParent {
            get {
                return widget.AccessibilityParent;
            }
            set => widget.AccessibilityParent = value as NSView;
        }

        public bool CanBecomeKeyView {
            get => widget.CanBecomeKeyView;
        }

        public object NativeObject => widget;
        public object View => widget;

        public string NodeName {
            get
            {
                if (widget is NSTextField textField)
                {
                    return string.Format("\"{0}\" {1}", textField.StringValue,
                        textField.IsLabel() ? "Label" : widget.GetType().Name);
                }
                if (widget is NSTextView textView)
                {
                    return string.Format("\"{0}\" {1}", textView.Value,widget.GetType().Name);
                }
                if (widget is NSButton button)
                {
                    return string.Format("\"{0}\" {1}", button.Title, widget.GetType().Name);
                }
                //if (widget is NSTabViewItem tabViewItem)
                //{
                //    return string.Format("{1}({0})", tabViewItem.Title, widget.GetType().Name);
                //}
                return widget.GetType().Name;
            }
        }

        public bool HasConstraints => widget.Constraints.Length > 0;

        public List<IConstrain> Constraints { 
            get {
                var result = new List<IConstrain>();
                foreach (var item in widget.Constraints)
                {
                    var cons = new TreeViewItemConstraint(item);
                    result.Add(cons);
                }
                return result;
            }
        }

        public void RemoveFromSuperview ()
		{
			widget.RemoveFromSuperview ();
		}

        public void Focus()
        {
           widget.Window?.MakeFirstResponder(widget);
        }

        internal NSView widget;
		public TreeViewItemView (NSView widget)
		{
			this.widget = widget;
		}
	}
}
