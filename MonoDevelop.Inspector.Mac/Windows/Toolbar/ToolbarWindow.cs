﻿// This file has been autogenerated from a class added in the UI designer.

using System;
using AppKit;
using System.Linq;
using Foundation;
using CoreGraphics;
using System.Globalization;
using System.Threading;

namespace MonoDevelop.Inspector.Mac
{
	class ToolbarWindow : BaseWindow, IToolbarWindow
	{
		public event EventHandler<bool> KeyViewLoop;
		public event EventHandler<bool> NextKeyViewLoop;
		public event EventHandler<bool> PreviousKeyViewLoop;
		public event EventHandler<bool> ThemeChanged;

        public event EventHandler ItemDeleted;
		public event EventHandler ItemImageChanged;
		public event EventHandler<FontData> FontChanged;
        public event EventHandler<CultureInfo> CultureChanged;

        public event EventHandler<InspectorViewMode> InspectorViewModeChanged;

        const int MenuItemSeparation = 3;
		const int LeftPadding = 5;

		readonly NSStackView firstRowStackView;
        readonly NSStackView secondRowStackView;
        readonly IInspectDelegate inspectDelegate;

        ToggleButton toolkitButton;

		NSStackView main;
		NSView rescanSeparator;

        public void ShowToolkitButton (bool value)
        {
            if (!value) {
                toolkitButton.RemoveFromSuperview();
                rescanSeparator.RemoveFromSuperview();
            } else {
                if (!firstRowStackView.Subviews.Contains (toolkitButton)){
                    firstRowStackView.AddArrangedSubview(toolkitButton);
                    firstRowStackView.AddArrangedSubview(rescanSeparator);
                } 
            }
        }

		NSStackView CreateFirstRow()
        {
			var stack = NativeViewHelper.CreateHorizontalStackView(MenuItemSeparation);
			var keyViewLoopButton = CreateToogleButton("overlay-actual.png", "Shows current focused item");
			stack.AddArrangedSubview(keyViewLoopButton);
			keyViewLoopButton.Activated += (s, e) => {
				KeyViewLoop?.Invoke(this, keyViewLoopButton.IsToggled);
			};

			var prevKeyViewLoopButton = CreateToogleButton("overlay-previous.png", "Shows previous focused item");
			stack.AddArrangedSubview(prevKeyViewLoopButton);
			prevKeyViewLoopButton.Activated += (s, e) => {
				PreviousKeyViewLoop?.Invoke(this, prevKeyViewLoopButton.IsToggled);
			};

			var nextKeyViewLoopButton = CreateToogleButton("overlay-next.png", "Shows next focused item");
			stack.AddArrangedSubview(nextKeyViewLoopButton);
			nextKeyViewLoopButton.Activated += (s, e) => {
				NextKeyViewLoop?.Invoke(this, nextKeyViewLoopButton.IsToggled);
			};

			stack.AddVerticalSeparator();

			toolkitButton = CreateToogleButton("rescan-16.png", "Change beetween Toolkits");
			stack.AddArrangedSubview(toolkitButton);
			toolkitButton.Activated += ToolkitButton_Activated; ;

			rescanSeparator = stack.AddVerticalSeparator();

			var themeButton = CreateToogleButton("style-16.png", "Change Style Theme");
			stack.AddArrangedSubview(themeButton);
			themeButton.Activated += ThemeButton_Activated;

			stack.AddVerticalSeparator();

			deleteButton = CreateImageButton("delete-16.png", "Delete selected item");
			stack.AddArrangedSubview(deleteButton);
			deleteButton.Activated += (s, e) =>
			{
				ItemDeleted?.Invoke(this, EventArgs.Empty);
			};

			changeImageButton = CreateImageButton("image-16.png", "Change image from selected item");
			stack.AddArrangedSubview(changeImageButton);

			changeImageButton.Activated += (s, e) =>
			{
				ItemImageChanged?.Invoke(this, EventArgs.Empty);
			};

			stack.AddVerticalSeparator();

			//Visual issues view
			languagesComboBox = new NSComboBox() { TranslatesAutoresizingMaskIntoConstraints = false };
			languagesComboBox.ToolTip = "Change font from selected item";

			cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
			var culturesStr = new NSString[cultureInfos.Length];

			NSString selected = null;
			for (int i = 0; i < cultureInfos.Length; i++)
			{
				culturesStr[i] = new NSString(cultureInfos[i].DisplayName);
				if (i == 0 || cultureInfos[i] == Thread.CurrentThread.CurrentUICulture)
				{
					selected = culturesStr[i];
				}
			}

			languagesComboBox.Add(culturesStr);
			stack.AddArrangedSubview(languagesComboBox);

			languagesComboBox.Select(selected);

			languagesComboBox.Activated += LanguagesComboBox_SelectionChanged;
			languagesComboBox.SelectionChanged += LanguagesComboBox_SelectionChanged;
			//languagesComboBox.WidthAnchor.ConstraintEqualTo(220).Active = true;

			return stack;
		}

		NSStackView CreateSecondRow()
		{
			var stackView = NativeViewHelper.CreateHorizontalStackView(MenuItemSeparation);
			//FONTS 

			fontsCombobox = new NSComboBox() { TranslatesAutoresizingMaskIntoConstraints = false };
			fontsCombobox.ToolTip = "Change font from selected item";
			fonts = NSFontManager.SharedFontManager.AvailableFonts
				.Select(s => new NSString(s))
				.ToArray();

			fontsCombobox.Add(fonts);
			fontsCombobox.WidthAnchor.ConstraintGreaterThanOrEqualTo(220).Active = true;

			fontSizeTextView = new NSTextField() { TranslatesAutoresizingMaskIntoConstraints = false };
			fontSizeTextView.ToolTip = "Change font size from selected item";
			fontSizeTextView.WidthAnchor.ConstraintEqualTo(40).Active = true;

			fontsCombobox.SelectionChanged += (s, e) => {
				OnFontChanged();
			};

			fontSizeTextView.Activated += (s, e) => {
				OnFontChanged();
			};
			return stackView;
		}

        class ToolbarImageButton : ImageButton
		{
			public override CGSize IntrinsicContentSize => new CGSize(InspectorToolWindow.ButtonWidth, base.IntrinsicContentSize.Height);
		}

        class ToolbarToogleButton : ToggleButton
		{
			public override CGSize IntrinsicContentSize => new CGSize(InspectorToolWindow.ButtonWidth, base.IntrinsicContentSize.Height);
        }

		ToolbarImageButton CreateImageButton(string resourceName, string tooltip)
        {
			var deleteButton = new ToolbarImageButton()
			{
				Image = (NSImage)inspectDelegate.GetImageResource(resourceName).NativeObject,
				ToolTip = tooltip
			};
			return deleteButton;
		}

		ToolbarToogleButton CreateToogleButton(string resourceName, string tooltip)
        {
			var previousImage = (NSImage)inspectDelegate.GetImageResource(resourceName).NativeObject;
			var prevKeyViewLoopButton = new ToolbarToogleButton() { Image = previousImage };
			prevKeyViewLoopButton.ToolTip = tooltip;
			return prevKeyViewLoopButton;
		}

		public ToolbarWindow (IInspectDelegate inspectDelegate, CGRect frame) : base(frame, NSWindowStyle.Titled | NSWindowStyle.FullSizeContentView, NSBackingStore.Buffered, false)
        {
            this.inspectDelegate = inspectDelegate;
			//BackgroundColor = NSColor.Clear;
			IsOpaque = false;
			TitlebarAppearsTransparent = true;
			TitleVisibility = NSWindowTitleVisibility.Hidden;
			ShowsToolbarButton = false;
			MovableByWindowBackground = false;

			main = NativeViewHelper.CreateVerticalStackView(MenuItemSeparation);
			ContentView = main;

			main.EdgeInsets = new NSEdgeInsets(Margin, Margin, Margin, Margin);

			firstRowStackView = CreateFirstRow();
			main.AddArrangedSubview (firstRowStackView);

			secondRowStackView = CreateSecondRow(); ;
            main.AddArrangedSubview(secondRowStackView);

			main.AddArrangedSubview(new NSView() { TranslatesAutoresizingMaskIntoConstraints = false });
        }

		const int Margin = 5;

        int GetSelectedLanguage ()
        {
            for (int i = 0; i < cultureInfos.Length; i++)
            {
                if (cultureInfos[i] == Thread.CurrentThread.CurrentUICulture)
                {
                    return i;
                }
            }
            return 0;
        }

        CultureInfo[] cultureInfos;
        NSComboBox languagesComboBox;
        void LanguagesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            var currentIndex = (int)languagesComboBox.SelectedIndex;
            if (currentIndex > -1)
            {
                var selected = cultureInfos[currentIndex];
                CultureChanged?.Invoke(this, selected);
            }
        }

		bool handleChange;

        public void ChangeView (InspectorManager manager, IView viewWrapper)
        {
			handleChange = true;

			bool showImage = false;
            bool showFont = false;
            //NSPopUpButton
            var fontData = manager.Delegate.GetFont(viewWrapper);
            if (fontData?.Font != null)
            {
                var currentFontName = ((NSFont)fontData.Font.NativeObject).FontName;
                if (currentFontName == ".AppleSystemUIFont")
                {
                    currentFontName = "HelveticaNeue";
                }
                var name = fonts.FirstOrDefault(s => s.ToString() == currentFontName);
                fontsCombobox.Select(name);

                fontSizeTextView.IntValue = (int)fontData.Size;
                showFont = true;
            }

            if (viewWrapper.NativeObject is NSImageView || viewWrapper.NativeObject is NSButton)
            {
                showImage = true;
            }

            imageButtonVisible = showImage;
            fontButtonsVisible = showFont;

			handleChange = false;

		}

        void ToolkitButton_Activated (object sender, EventArgs e)
		{
			InspectorViewModeChanged?.Invoke (this, toolkitButton.State == NSCellStateValue.On ? InspectorViewMode.Xwt : InspectorViewMode.Native);
		}

		bool fontButtonsVisible
		{
			get => firstRowStackView.Subviews.Contains(fontsCombobox);
			set
			{
				if (fontButtonsVisible == value)
				{
					return;
				}

				if (value)
				{
					secondRowStackView.AddArrangedSubview(fontsCombobox);
                    secondRowStackView.AddArrangedSubview(fontSizeTextView);
				}
				else
				{
					fontSizeTextView.RemoveFromSuperview();
					fontsCombobox.RemoveFromSuperview();
                }
			}
		}

		bool imageButtonVisible
		{
			get => firstRowStackView.Subviews.Contains(changeImageButton);
			set
			{
				if (imageButtonVisible == value)
				{
					return;
				}

				if (value)
				{
					firstRowStackView.AddArrangedSubview(changeImageButton);
				}
				else
				{
					changeImageButton.RemoveFromSuperview();
				}
			}
		}

		void OnFontChanged ()
		{
			if (handleChange) {
				return;
			}
			var currentIndex = (int)fontsCombobox.SelectedIndex;
			if (currentIndex >= -1)
			{
				var selected = fonts[currentIndex].ToString();
				var fontSize = fontSizeTextView.IntValue;
                IFontWrapper font = inspectDelegate.GetFromName(selected, fontSize);
                FontChanged?.Invoke(this, new FontData (font, fontSize));
			}
		}

		NSString[] fonts;
		NSComboBox fontsCombobox;
		NSTextField fontSizeTextView;
		//public override bool CanBecomeKeyWindow => false;
		//public override bool CanBecomeMainWindow => false;

		ImageButton deleteButton, changeImageButton;

		public bool ImageChangedEnabled
		{
			get => changeImageButton.Enabled;
			set => changeImageButton.Enabled = value;
		}

		void ThemeButton_Activated (object sender, EventArgs e)
		{
			if (sender is ToggleButton btn) {
				ThemeChanged?.Invoke (this, btn.IsToggled);
			}
		}
	}
}
