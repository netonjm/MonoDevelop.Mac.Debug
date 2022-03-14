using System;
using System.Collections;
using System.Collections.Generic;
using AppKit;
using MonoDevelop.Components.Commands;
using MonoDevelop.Components.PropertyGrid;
using MonoDevelop.Core;
using MonoDevelop.Core.Serialization;
using MonoDevelop.DesignerSupport;

namespace VisualStudio.ViewInspector
{
    public class PropertyProvider : IPropertyProvider
    {
        public bool SupportsObject(object obj)
        {
            return obj is NSView;
        }

        public object CreateProvider(object obj)
        {
            return obj;
        }
    }

	class PadViewCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Visible = info.Enabled = true;
        }

        protected override void Run()
        {
            var rootWindow = MonoDevelop.Ide.IdeApp.Workbench.RootWindow;
            var widget = rootWindow.Focus;

            if (widget == null)
            {
                MonoDevelop.Ide.MessageService.ShowError("No selected widget");
                return;
            }

   //         var pad = OutlineContentPad.Instance;
   //         if (pad == null)
   //         {
   //             MonoDevelop.Ide.MessageService.ShowError("You need open the Inspector OutlinePad");
   //             return;
   //         }

			////InspectorPropertyPad.Instance.Control.CurrentObject = widget;

			//var topParent = widget.GetTopParent ();
   //         pad.GenerateTree(topParent);
   //         pad.Focus(widget);
        }
    }

    class SearchChildenViewCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Visible = info.Enabled = true;
        }

        protected override void Run()
        {
            var rootWindow = MonoDevelop.Ide.IdeApp.Workbench.RootWindow;
            var widget = rootWindow.Focus;
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Current Gtk Brothers states: ({widget} {IsFocus}|{HasFocus}|{Sensitive})");
            builder.AppendLine("==================================================================");

            if (widget == null)
            {
				builder.AppendLine ("No selected widget");
				LoggingService.LogInfo (builder.ToString ());
				MonoDevelop.Ide.MessageService.ShowError ("No selected widget");
				return;
            }
            if (widget.Parent == null || ! (widget.Parent is Gtk.Container parentContainer))
            {
				builder.AppendLine ("Widget has no parent");
				LoggingService.LogInfo (builder.ToString ());
				MonoDevelop.Ide.MessageService.ShowError ("Widget has no parent");
				return;
            }

            SearchSelectedViewCommandHandler.AppendChildStatus(builder, widget.Parent, 2);
            foreach (var item in parentContainer.Children)
            {
                SearchSelectedViewCommandHandler.AppendChildStatus(builder, item, 2);
            }
			LoggingService.LogInfo (builder.ToString ());
		}
    }

    class SearchSelectedViewCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Visible = info.Enabled = true;
        }

        public static void AppendChildStatus (System.Text.StringBuilder builder, Gtk.Widget current, int level)
        {
            var line = new string('-', level);
            var spaces = $"{line}> {current} ({current.IsFocus}|{current.HasFocus}|{current.Sensitive})";
            builder.AppendLine(spaces);
        }

        protected override void Run()
        {
            var rootWindow = MonoDevelop.Ide.IdeApp.Workbench.RootWindow;
            var widget = rootWindow.Focus;
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Current Gtk Hierarchy states: ({widget} {IsFocus}|{HasFocus}|{Sensitive})");
            builder.AppendLine("==================================================================");

            if (widget == null)
            {
                builder.AppendLine("No selected widget");
				LoggingService.LogInfo (builder.ToString ());
				return;
            }

            var hierarchy = new List<Gtk.Widget>();
            GoParent(widget, hierarchy);

            for (int i = hierarchy.Count - 1; i >= 0; i--)
            {
                var current = hierarchy[i];
                AppendChildStatus(builder, current, hierarchy.Count - i + 1);
            }
			LoggingService.LogInfo (builder.ToString ());
		}

        void GoParent(Gtk.Widget widget, List<Gtk.Widget> widgets)
        {
            widgets.Add(widget);

            if (widget.Parent != null)
            {
                GoParent(widget.Parent, widgets);
            }
        }
    }

}
