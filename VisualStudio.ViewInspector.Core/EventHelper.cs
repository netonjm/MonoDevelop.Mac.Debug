using System;
namespace VisualStudio.ViewInspector
{
	public static class EventsHelper
	{
        public static bool IsAction(Type type)
        {
            if (type == typeof(System.Action))
                return true;
            if (type.Name.StartsWith("Action`"))
                return true;
            return false;
        }

        public static bool IsFunc(Type type)
        {
            if (type.Name.StartsWith("Func`"))
                return true;
            return false;
        }

        public static bool IsEventHandler(Type type)
        {
            if (type == typeof(EventHandler))
            {
                return true;
            }
            if (type.Name.StartsWith("EventHandler`"))
                return true;
            return false;
        }
    }
}

