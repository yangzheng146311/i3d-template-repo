using i3D;
using System;

public partial class NavPanel : Panel
{
    private static readonly string[] MODULE_LABELS = { "总 览", "监 控", "告 警", "分 析", "安 全" };
    private static readonly string[] ZONE_NAMES = { "A", "B", "C" };

    private Button[] _menuButtons = new Button[5];
    private int _selectedIndex = 0;
    private StyleBoxFlat _menuNormalStyle;
    private StyleBoxFlat _menuSelectedStyle;
    private VBoxContainer _deviceTreeContainer;
    private LineEdit _searchBox;

    public event Action<int> OnModuleSelected;
    public event Action<int> OnDeviceSelected;

    private SimDataService _data;

    public override void iStart()
    {
        AddThemeStyleboxOverride("panel", UIStyles.MakePanelStyle());

        VBoxContainer mainVBox = new();
        mainVBox.SetAnchorsPreset(LayoutPreset.FullRect);
        mainVBox.SetOffsetsPreset(LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 4);
        AddChild(mainVBox);

        CreateModuleNav(mainVBox);
        CreateSearch(mainVBox);
        CreateDeviceTree(mainVBox);

        ColorRect rightLine = new();
        rightLine.SetAnchorsPreset(LayoutPreset.RightWide);
        rightLine.SetOffsetsPreset(LayoutPreset.RightWide);
        rightLine.OffsetLeft = -1;
        rightLine.OffsetRight = 0;
        rightLine.Color = UIStyles.DIVIDER;
        AddChild(rightLine);
    }

    private void CreateModuleNav(VBoxContainer parent)
    {
        _menuNormalStyle = UIStyles.MakeStyleBox(Colors.Transparent, Colors.Transparent, 0, 6);
        _menuSelectedStyle = UIStyles.MakeStyleBox(UIStyles.ACCENT_BLUE, UIStyles.ACCENT, 0, 6);
        _menuSelectedStyle.SetBorderWidth(Side.Left, 3);

        VBoxContainer navBox = new();
        navBox.AddThemeConstantOverride("separation", 2);

        for (int i = 0; i < MODULE_LABELS.Length; i++)
        {
            Button btn = UIStyles.MakeMenuButton(MODULE_LABELS[i], i == 0);
            int idx = i;
            btn.Pressed += () => SelectModule(idx);
            _menuButtons[i] = btn;
            navBox.AddChild(btn);
        }

        MarginContainer navMargin = new();
        navMargin.AddThemeConstantOverride("margin_left", 6);
        navMargin.AddThemeConstantOverride("margin_right", 6);
        navMargin.AddThemeConstantOverride("margin_top", 8);
        navMargin.AddChild(navBox);

        parent.AddChild(navMargin);

        parent.AddChild(UIStyles.MakeSpacer(4));
        parent.AddChild(UIStyles.MakeDivider());
        parent.AddChild(UIStyles.MakeSpacer(4));
    }

    private void CreateSearch(VBoxContainer parent)
    {
        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 8);
        margin.AddThemeConstantOverride("margin_right", 8);
        margin.AddThemeConstantOverride("margin_bottom", 4);

        _searchBox = new LineEdit();
        _searchBox.PlaceholderText = "搜索设备...";
        _searchBox.Editable = true;
        _searchBox.AddThemeFontSizeOverride("font_size", 12);
        _searchBox.AddThemeColorOverride("font_color", UIStyles.TEXT_WHITE);
        _searchBox.AddThemeColorOverride("font_placeholder_color", UIStyles.TEXT_DIM);
        _searchBox.SetCustomMinimumSize(new Vector2(0, 30));
        _searchBox.TextChanged += OnSearchChanged;
        margin.AddChild(_searchBox);

        parent.AddChild(margin);
    }

    private void CreateDeviceTree(VBoxContainer parent)
    {
        MarginContainer margin = new();
        margin.SizeFlagsVertical = SizeFlags.ExpandFill;
        margin.AddThemeConstantOverride("margin_left", 8);
        margin.AddThemeConstantOverride("margin_right", 8);

        ScrollContainer scroll = new();
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        margin.AddChild(scroll);

        _deviceTreeContainer = new VBoxContainer();
        _deviceTreeContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _deviceTreeContainer.AddThemeConstantOverride("separation", 1);
        scroll.AddChild(_deviceTreeContainer);

        parent.AddChild(margin);
    }

    public void SetData(SimDataService data)
    {
        _data = data;
        RefreshDeviceTree();
    }

    public void RefreshDeviceTree()
    {
        if (_data == null) return;
        RefreshDeviceTree(_searchBox?.Text ?? "");
    }

    private void RefreshDeviceTree(string filter)
    {
        while (_deviceTreeContainer.GetChildCount() > 0)
        {
            var child = _deviceTreeContainer.GetChild(0);
            _deviceTreeContainer.RemoveChild(child);
            child.QueueFree();
        }

        string lowerFilter = filter.ToLower();

        for (int z = 0; z < ZONE_NAMES.Length; z++)
        {
            string zoneName = ZONE_NAMES[z] + "区";
            bool zoneHasMatch = false;

            for (int d = 0; d < _data.Devices.Length; d++)
            {
                if (_data.Devices[d].Zone == ZONE_NAMES[z])
                {
                    if (string.IsNullOrEmpty(lowerFilter) ||
                        _data.Devices[d].Name.ToLower().Contains(lowerFilter) ||
                        zoneName.ToLower().Contains(lowerFilter))
                    {
                        zoneHasMatch = true;
                        break;
                    }
                }
            }

            if (!zoneHasMatch) continue;

            Label zoneLabel = UIStyles.MakeLabel("  " + zoneName, UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
            zoneLabel.SetCustomMinimumSize(new Vector2(0, 24));
            zoneLabel.VerticalAlignment = VerticalAlignment.Center;
            _deviceTreeContainer.AddChild(zoneLabel);

            for (int d = 0; d < _data.Devices.Length; d++)
            {
                if (_data.Devices[d].Zone != ZONE_NAMES[z]) continue;

                string devName = _data.Devices[d].Name;
                if (!string.IsNullOrEmpty(lowerFilter) &&
                    !devName.ToLower().Contains(lowerFilter) &&
                    !zoneName.ToLower().Contains(lowerFilter))
                    continue;

                int devIdx = d;
                HBoxContainer row = new();
                row.SetCustomMinimumSize(new Vector2(0, 28));
                row.AddThemeConstantOverride("separation", 6);

                Control padL = new();
                padL.SetCustomMinimumSize(new Vector2(16, 0));
                row.AddChild(padL);

                ColorRect dot = new();
                dot.SetCustomMinimumSize(new Vector2(8, 8));
                dot.SizeFlagsVertical = SizeFlags.ShrinkCenter;
                dot.Color = _data.Devices[d].Online ? UIStyles.OK : UIStyles.ERR;
                row.AddChild(dot);

                Label nameLabel = UIStyles.MakeLabel(devName,
                    _data.Devices[d].Online ? UIStyles.TEXT_WHITE : UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
                nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                nameLabel.VerticalAlignment = VerticalAlignment.Center;
                row.AddChild(nameLabel);

                row.MouseFilter = MouseFilterEnum.Stop;
                row.MouseEntered += () =>
                {
                    nameLabel.AddThemeColorOverride("font_color", UIStyles.ACCENT);
                };
                row.MouseExited += () =>
                {
                    nameLabel.AddThemeColorOverride("font_color",
                        _data.Devices[devIdx].Online ? UIStyles.TEXT_WHITE : UIStyles.TEXT_DIM);
                };
                row.GuiInput += (InputEvent e) =>
                {
                    if (e is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left)
                        OnDeviceSelected?.Invoke(devIdx);
                };

                _deviceTreeContainer.AddChild(row);
            }
        }
    }

    private void OnSearchChanged(string text)
    {
        RefreshDeviceTree(text);
    }

    public void SelectModule(int index)
    {
        for (int i = 0; i < _menuButtons.Length; i++)
        {
            _menuButtons[i].AddThemeStyleboxOverride("normal", i == index ? _menuSelectedStyle : _menuNormalStyle);
            _menuButtons[i].AddThemeColorOverride("font_color", i == index ? UIStyles.TEXT_WHITE : UIStyles.TEXT_DIM);
        }
        _selectedIndex = index;
        OnModuleSelected?.Invoke(index);
    }
}
