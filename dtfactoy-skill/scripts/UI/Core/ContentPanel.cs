using i3D;
using System;

public partial class ContentPanel : Panel
{
    private VBoxContainer[] _panels = new VBoxContainer[5];
    private int _selectedIndex = 0;
    private bool _collapsed = false;
    private float _targetOffsetRight;
    private Button _toggleBtn;
    private const float ANIM_SPEED = 8f;

    public override void iStart()
    {
        AnchorLeft = 1f;
        AnchorRight = 1f;
        AnchorTop = 0f;
        AnchorBottom = 1f;
        OffsetLeft = -UIStyles.RIGHT_W;
        OffsetRight = 0f;
        OffsetTop = UIStyles.TOP_H;
        OffsetBottom = -UIStyles.BOTTOM_H;
        _targetOffsetRight = 0f;
        AddThemeStyleboxOverride("panel", UIStyles.MakePanelStyle());

        MarginContainer margin = new();
        margin.SetAnchorsPreset(LayoutPreset.FullRect);
        margin.SetOffsetsPreset(LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", (int)UIStyles.PADDING);
        margin.AddThemeConstantOverride("margin_right", (int)UIStyles.PADDING);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_bottom", 12);
        AddChild(margin);

        ScrollContainer scroll = new();
        scroll.SetAnchorsPreset(LayoutPreset.FullRect);
        scroll.SetOffsetsPreset(LayoutPreset.FullRect);
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        margin.AddChild(scroll);

        VBoxContainer scrollContent = new();
        scrollContent.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(scrollContent);

        for (int i = 0; i < 5; i++)
        {
            VBoxContainer content = new();
            content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            content.AddThemeConstantOverride("separation", 8);
            content.Visible = false;
            scrollContent.AddChild(content);
            _panels[i] = content;
        }

        CreateToggleButton();

        ColorRect leftLine = new();
        leftLine.SetAnchorsPreset(LayoutPreset.LeftWide);
        leftLine.SetOffsetsPreset(LayoutPreset.LeftWide);
        leftLine.OffsetLeft = 0;
        leftLine.OffsetRight = 1;
        leftLine.Color = UIStyles.DIVIDER;
        AddChild(leftLine);
    }

    private void CreateToggleButton()
    {
        _toggleBtn = new Button();
        _toggleBtn.Text = ">";
        _toggleBtn.SetCustomMinimumSize(new Vector2(24, 48));
        _toggleBtn.AnchorLeft = 0f;
        _toggleBtn.AnchorRight = 0f;
        _toggleBtn.AnchorTop = 0.5f;
        _toggleBtn.AnchorBottom = 0.5f;
        _toggleBtn.OffsetLeft = -24;
        _toggleBtn.OffsetRight = 0;
        _toggleBtn.OffsetTop = -24;
        _toggleBtn.OffsetBottom = 24;
        _toggleBtn.AddThemeFontSizeOverride("font_size", 14);
        _toggleBtn.AddThemeColorOverride("font_color", UIStyles.TEXT_DIM);
        _toggleBtn.AddThemeColorOverride("font_hover_color", UIStyles.TEXT_WHITE);
        _toggleBtn.AddThemeStyleboxOverride("normal",
            UIStyles.MakeStyleBox(new Color(0.05f, 0.1f, 0.2f), UIStyles.DIVIDER, 0, 4));
        _toggleBtn.AddThemeStyleboxOverride("hover",
            UIStyles.MakeStyleBox(UIStyles.ACCENT_BLUE, UIStyles.ACCENT, 0, 4));
        _toggleBtn.Pressed += ToggleCollapse;
        AddChild(_toggleBtn);
    }

    public override void Update(double delta)
    {
        if (_collapsed && OffsetRight > -(UIStyles.RIGHT_W + 24))
        {
            float t = Mathf.Clamp((float)(ANIM_SPEED * delta), 0f, 1f);
            OffsetRight = (int)Mathf.Lerp((float)OffsetRight, -(UIStyles.RIGHT_W + 24), t);
            OffsetLeft = (int)(OffsetRight - UIStyles.RIGHT_W);
        }
        else if (!_collapsed && OffsetRight < _targetOffsetRight - 0.5f)
        {
            float t = Mathf.Clamp((float)(ANIM_SPEED * delta), 0f, 1f);
            OffsetRight = (int)Mathf.Lerp((float)OffsetRight, _targetOffsetRight, t);
            OffsetLeft = (int)(OffsetRight - UIStyles.RIGHT_W);
        }
    }

    public VBoxContainer GetPanel(int index) => _panels[index];

    public void SelectPanel(int index)
    {
        _panels[_selectedIndex].Visible = false;
        _panels[index].Visible = true;
        _selectedIndex = index;
    }

    public void ToggleCollapse()
    {
        _collapsed = !_collapsed;
        _toggleBtn.Text = _collapsed ? "<" : ">";
    }

    public bool IsCollapsed => _collapsed;
}
