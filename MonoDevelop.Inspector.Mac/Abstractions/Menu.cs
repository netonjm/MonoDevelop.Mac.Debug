﻿// This file has been autogenerated from a class added in the UI designer.

using AppKit;

namespace MonoDevelop.Inspector.Mac.Abstractions
{
    internal class Menu : IMenu
    {
        private NSMenu submenu;

        public Menu(NSMenu submenu)
        {
            this.submenu = submenu;
        }

        public void InsertItem(IMenuItem menuItem, int index)
        {
            var item = (NSMenuItem)menuItem.NativeObject;
            submenu.InsertItem(item, index);
        }

        public object NativeObject => submenu;
    }
}