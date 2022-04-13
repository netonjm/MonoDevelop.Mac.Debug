using System;
using CoreGraphics;
using AppKit;

namespace MonoDevelop.Inspector.Mac
{
    enum KeyCodes
    {
        Enter = 0x24,
        Tab = 0x30,
        Space = 0x31
    }

    [Flags]
    enum KeyModifierFlag
    {
        None = 0x100
    }

    class ToggleButton : NSButton
    {
        public event EventHandler Focused;
        public ToggleButton()
        {
            Title = "";
            BezelStyle = NSBezelStyle.Rounded;
            SetButtonType(NSButtonType.OnOff);
            FocusRingType = NSFocusRingType.Default;
            ImageScaling = NSImageScale.AxesIndependently;
            TranslatesAutoresizingMaskIntoConstraints = false;
        }
        public override bool BecomeFirstResponder()
        {
            Focused?.Invoke(this, EventArgs.Empty);
            return base.BecomeFirstResponder();
        }

        public bool IsToggled
        {
            get => State == NSCellStateValue.On;
            set
            {
                if (IsToggled == value)
                {
                    return;
                }
                State = value ? NSCellStateValue.On : NSCellStateValue.Off;
            }
        }

        public override void KeyDown(NSEvent theEvent)
        {
            base.KeyDown(theEvent);
            if ((int)theEvent.ModifierFlags == (int)KeyModifierFlag.None && (theEvent.KeyCode == (int)KeyCodes.Enter || theEvent.KeyCode == (int)KeyCodes.Space))
            {
                PerformClick(this);
            }
        }
    }
}