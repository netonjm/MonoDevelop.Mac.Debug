﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using VisualStudio.ViewInspector.Mac.Views;
using AppKit;
using Foundation;
using ObjCRuntime;

namespace VisualStudio.ViewInspector.Mac.Windows.Inspector
{
    class PropertyNode : Node
    {
        public PropertyInfo PropertyInfo { get; }
        public EventType EventType { get; }
        public PropertyType PropertyType { get; }
        public object CurrentObject { get; }

        public PropertyNode(PropertyType property, EventType eventType, PropertyInfo propertyInfo, object obj, string name) : base(name)
        {
            PropertyInfo = propertyInfo;
            EventType = eventType;
            CurrentObject = obj;
            PropertyType = property;
        }
    }

    class EventNode : Node
    {
        public EventInfo MethodInfo { get; }
        public EventType EventType { get; }
        public PropertyType PropertyType { get; }
        public object CurrentObject { get; }

        public EventNode(PropertyType property, EventType eventType, EventInfo method, object obj, string name) : base(name)
        {
            MethodInfo = method;
            EventType = eventType;
            CurrentObject = obj;
            PropertyType = property;
        }
    }

    class AssociedEventMethodNode : Node
    {
        public Delegate sourceDelegate { get; }
        public Delegate associedDelegate { get; }
        public FieldInfo eventFieldInfo { get; }
        public object element { get; }

        public AssociedEventMethodNode(Delegate eventDelegate, Delegate associedDelegate, FieldInfo eventFieldInfo, object element, string name) : base(name) 
        {
            this.sourceDelegate = eventDelegate;
            this.associedDelegate = associedDelegate;
            this.eventFieldInfo = eventFieldInfo;
            this.element = element;
        }
    }

    class PropertyCategoryNode : Node
    {
        public PropertyType PropertyType { get; }

        public PropertyCategoryNode(PropertyType propertyType, string name) : base(name)
        {
            PropertyType = propertyType;
        }
    }

    class EventCategoryNode : Node
    {
        public EventType EventType { get; }

        public EventCategoryNode(EventType eventType, string name) : base(name)
        {
            EventType = eventType;
        }
    }

    enum PropertyType
    {
        Event,
        Property,
    }

    enum EventType
    {
        EventHandler,
        Action,
        Func
    }

    class EventLabelRowNode : LabelRowNode
    {
        public void SetData(Node node)
        {
            if (node is EventNode eventNode)
            {
                var type = eventNode.CurrentObject.GetType();

                FieldInfo eventFieldInfo = type.GetField(eventNode.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (eventFieldInfo != null)
                {
                    Delegate eventDelegate = eventFieldInfo.GetValue(eventNode.CurrentObject) as Delegate;
                    if (eventDelegate != null)
                    {
                        var invocation = eventDelegate.GetInvocationList();
                        var name = eventNode.Name;
                        textField.StringValue = string.Format("{0} ({1} associed)", name, invocation.Length);
                        return;
                    }
                }
            }
            textField.StringValue = node.Name;
        }
    }

    class EventImageRowNode : ImageRowNode
    {
        public override void SetData(Node node, string imageName)
        {
            image = NativeViewHelper.GetManifestImageResource(imageName);

            if (node is EventNode eventNode)
            {
                ToolTip = eventNode.ToString();

                var type = eventNode.CurrentObject.GetType();

                FieldInfo eventFieldInfo = type.GetField(eventNode.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (eventFieldInfo != null)
                {
                    Delegate eventDelegate = eventFieldInfo.GetValue(eventNode.CurrentObject) as Delegate;
                    if (eventDelegate != null)
                    {
                        var invocation = eventDelegate.GetInvocationList();
                        textField.StringValue = string.Format("{0} ({1} attached)", node.Name, invocation.Length);
                        return;
                    }
                }
            }
            textField.StringValue = node.Name;
        }
    }

    class EventDelegate : OutlineViewDelegate
    {
        public override nfloat GetRowHeight(NSOutlineView outlineView, NSObject item)
        {
            return 24;
        }

        public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
        {
            if (item is PropertyCategoryNode categoryEventNode)
            {
                var view = (ImageRowNode)outlineView.MakeView(identifer, this);
                if (view == null)
                {
                    view = new ImageRowNode();

                    view.SetData(categoryEventNode, categoryEventNode.PropertyType == PropertyType.Event ? "Events.png" : "Properties.png");
                }
                return view;
            }
            else if (item is AssociedEventMethodNode associedEventMethod)
            {
                var view = (ImageRowNode)outlineView.MakeView(identifer, this);
                if (view == null)
                {
                    view = new ImageRowNode();
                    view.SetData(associedEventMethod, "Method.png");
                }
                return view;
            }
            else if (item is EventCategoryNode eventCategoryNode)
            {
                var view = (ImageRowNode)outlineView.MakeView(identifer, this);
                if (view == null)
                {
                    view = new ImageRowNode();
                    string imageName = string.Concat("E_" + eventCategoryNode.EventType, ".png");
                    view.SetData(eventCategoryNode, imageName);
                }
                return view;
            }
            else if (item is EventNode eventNode)
            {
                var view = (EventLabelRowNode)outlineView.MakeView(identifer, this);
                if (view == null)
                {
                    view = new EventLabelRowNode();
                }
                //string imageName =
                //    string.Concat(eventNode.PropertyType == PropertyType.Event ? "E_" : string.Empty, eventNode.EventType, ".png");

                view.SetData(eventNode);
                return view;
            }
            else if (item is PropertyNode propertyNode)
            {
                var view = (EventImageRowNode)outlineView.MakeView(identifer, this);
                if (view == null)
                {
                    view = new EventImageRowNode();
                }

                string imageName =
                    string.Concat(propertyNode.PropertyType == PropertyType.Event ? "E_" : string.Empty, propertyNode.EventType, ".png");

                view.SetData(propertyNode, imageName);
                return view;
            }
            throw new NotImplementedException();
        }
    }

    class EventWindow : NSWindow
    {
        NSStackView content;
        public EventWindow()
        {
            //this.StyleMask = NSWindowStyle.FullSizeContentView;
            var size = new CoreGraphics.CGSize(400, 200);
            SetContentSize(size);

            content = NativeViewHelper.CreateVerticalStackView();
            ContentView = content;
        }

        public override void ResignKeyWindow()
        {
            Close();
        }

        void Clean ()
        {
            foreach (var item in content.ArrangedSubviews)
            {
                item.RemoveFromSuperview();
            }
        }

        internal void SetNode(EventNode eventNode)
        {
            Clean();

            var type = eventNode.CurrentObject.GetType();

            FieldInfo eventFieldInfo = type.GetField(eventNode.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (eventFieldInfo != null)
            {
                Delegate eventDelegate = eventFieldInfo.GetValue(eventNode.CurrentObject) as Delegate;
                if (eventDelegate != null)
                {
                    var invocation = eventDelegate.GetInvocationList();
                    foreach (var attached in invocation)
                    {
                        var fullname = attached.Target.GetType().FullName;
                        var label = NSTextField.CreateLabel($"{fullname} : {attached.Method}");
                        label.TranslatesAutoresizingMaskIntoConstraints = false;
                        content.AddArrangedSubview(label);
                    }
                }
            }
        }
    }

    class EventListView : OutlineView
    {
        MainNode mainNode;

        EventDelegate eventDelegate;

        //EventWindow window;

        public bool ShowAllEvents { get; set; }

        public EventListView()
        {
            //window = new EventWindow();

            TranslatesAutoresizingMaskIntoConstraints = false;
            HeaderView = null;
            BackgroundColor = NSColor.Clear;
        }

        public Func<NSEvent, NSMenu> CreateMenuHandler { get; set; }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public override NSMenu MenuForEvent(NSEvent theEvent)
        {
            var point = ConvertPointFromView(theEvent.LocationInWindow, null);
            var item = ItemAtRow(GetRow(point));

            var menu = new NSMenu();
            if (item is AssociedEventMethodNode associedEventMethodNode)
            {
                menu.AddItem(new NSMenuItem("Execute", (s, e) =>
                {
                    try
                    {
                        var assocDelegate = associedEventMethodNode.associedDelegate;

                        List<object> parametersValues = new List<object>();

                        var parameters = assocDelegate.Method.GetParameters();
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var paramType = parameters[i].ParameterType;
                            var defaultValue = GetDefault(paramType);

                            parametersValues.Add(defaultValue);
                        }

                        assocDelegate.Method.Invoke(assocDelegate.Target, parametersValues.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                   
                }));

                menu.AddItem(NSMenuItem.SeparatorItem);

                var menuItem = new NSMenuItem("Delete", (s, e) =>
                {
                    //associedEventMethodNode.eventDelegate
                    var currentDelete = System.Delegate.RemoveAll(associedEventMethodNode.sourceDelegate, associedEventMethodNode.associedDelegate);
                    associedEventMethodNode.eventFieldInfo.SetValue(associedEventMethodNode.element, currentDelete);
                    //inspectorToolWindow.RaiseItemDeleted(treeNode.NativeObject);
                    ((InspectorToolWindow)Window).RequestRefreshTreeView();
                });
                menu.AddItem(menuItem);
            }
            return menu;
        }

        //void ShowEventWindow(EventNode node)
        //{
        //    window.SetNode(node);
        //    if (!window.IsVisible)
        //    {
        //        this.Window.AddChildWindow(window, NSWindowOrderingMode.Above);
        //        window.OrderFront(window);
        //    }
        //}

        public override NSOutlineViewDelegate GetDelegate()
        {
            return eventDelegate = new EventDelegate();
        }

        EventType GetEventType (EventInfo info)
        {
            if (EventsHelper.IsEventHandler(info.EventHandlerType)) {
                return EventType.EventHandler;
            }

            if (EventsHelper.IsFunc(info.EventHandlerType))
            {
                return EventType.Func;
            }

            if (EventsHelper.IsAction(info.EventHandlerType))
            {
                return EventType.Action;
            }

            throw new NotImplementedException("");
        }

        public void SetObject(object element, string filter)
        {
            mainNode = new MainNode();

            if (element == null)
            {
                SetData(mainNode.Node);
                return;
            }

            List<EventInfo> methodInfos;

            var type = element.GetType();
            if (element != null)
            {
                methodInfos = type.GetEvents(BindingFlags.Public | BindingFlags.Instance).ToList();
            }
            else
            {
                methodInfos = new List<EventInfo>();
            }

            var events = new PropertyCategoryNode(PropertyType.Event, "Events");

            var events_eventHandlers = new EventCategoryNode(EventType.EventHandler, "EventHandlers");
            var events_actions = new EventCategoryNode(EventType.Action, "Actions");
            var events_functions = new EventCategoryNode(EventType.Func, "Functions");

            foreach (var method in methodInfos)
            {
                if (string.IsNullOrEmpty(filter) || method.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    var eventType = GetEventType(method);

                    EventNode eventNode = null;
                    if (eventType == EventType.EventHandler)
                    {
                        eventNode = new EventNode(PropertyType.Event, eventType, method, element, method.Name);
                        events_eventHandlers.AddChild(eventNode);
                    }
                    else if (eventType == EventType.Action)
                    {
                        eventNode = new EventNode(PropertyType.Event, eventType, method, element, method.Name);
                        events_actions.AddChild(eventNode);
                    }
                    else if (eventType == EventType.Func)
                    {
                        eventNode = new EventNode(PropertyType.Event, eventType, method, element, method.Name);
                        events_functions.AddChild(eventNode);
                    }

                    if (eventNode != null)
                    {
                        //there is children?
                        FieldInfo eventFieldInfo = type.GetField(eventNode.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (eventFieldInfo != null)
                        {
                            Delegate eventDelegate = eventFieldInfo.GetValue(eventNode.CurrentObject) as Delegate;
                            if (eventDelegate != null)
                            {
                                var invocation = eventDelegate.GetInvocationList();

                                foreach (var associedDelegate in invocation)
                                {
                                    var name = associedDelegate.Method.ToString();

                                    if (associedDelegate.Target != null)
                                    {
                                        var firstSpace = name.IndexOf(' ');
                                        if (firstSpace > -1)
                                        {
                                            //name.Substring(0, firstSpace), " ", 
                                            name = string.Concat(associedDelegate.Target.GetType().FullName, name.Substring(firstSpace + 1));
                                        }
                                    }

                                    var associedMethod = new AssociedEventMethodNode(eventDelegate, associedDelegate, eventFieldInfo, element, name);
                                    eventNode.AddChild(associedMethod);
                                }
                            }
                        }
                    }
                }
            }

            if (events_eventHandlers.ChildCount > 0 || events_actions.ChildCount > 0 || events_functions.ChildCount > 0)
            {
                mainNode.Node.AddChild(events);

                if (events_eventHandlers.ChildCount > 0)
                    events.AddChild(events_eventHandlers);
                if (events_actions.ChildCount > 0)
                    events.AddChild(events_actions);
                if (events_functions.ChildCount > 0)
                    events.AddChild(events_functions);
            }

            var p_actions = new EventCategoryNode(EventType.Action, "Actions");

            var p_eventHandlers = new EventCategoryNode(EventType.EventHandler, "EventHandlers");

            var p_functions = new EventCategoryNode(EventType.Func, "Functions");

            foreach (var property in type.GetProperties())
            {
                if (EventsHelper.IsAction(property.PropertyType))
                {
                    p_actions.AddChild(new PropertyNode(PropertyType.Property, EventType.Action, property, element, property.Name));
                }
                else if (EventsHelper.IsFunc(property.PropertyType))
                {
                    p_functions.AddChild(new PropertyNode(PropertyType.Property, EventType.Func, property, element, property.Name));
                }
                else if (EventsHelper.IsEventHandler(property.PropertyType))
                {
                    p_eventHandlers.AddChild(new PropertyNode(PropertyType.Property, EventType.EventHandler, property, element, property.Name));
                }
            }

            if (p_actions.ChildCount > 0 || p_functions.ChildCount > 0 || p_eventHandlers.ChildCount > 0)
            {
                if (p_eventHandlers.ChildCount > 0)
                {
                    mainNode.Node.AddChild(p_eventHandlers);
                }
                if (p_actions.ChildCount > 0)
                {
                    mainNode.Node.AddChild(p_actions);
                }
                if (p_functions.ChildCount > 0)
                {
                    mainNode.Node.AddChild(p_functions);
                }
            }

            SetData(mainNode.Node);
        }
    }
}
