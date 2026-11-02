using i3D;
using System;

public partial class TimeRangeSelector : HBoxContainer
{
    private static readonly string[] LABELS = { "1h", "6h", "24h", "7d" };
    private Button[] _buttons = new Button[4];
    private int _selectedIndex = 2;

    public event Action<int> OnRangeChanged;

    public override void iStart()
    {
        AddThemeConstantOverride("separation", 4);

        for (int i = 0; i < LABELS.Length; i++)
        {
            Button btn = new();
            btn.Text = LABELS[i];
            btn.SetCustomMinimumSize(new Vector2(40, 28));
            btn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
            int idx = i;
            btn.Pressed += () => Select(idx);
            _buttons[i] = btn;
            AddChild(btn);
        }

        ApplyStyles();
    }

    private void Select(int index)
    {
        _selectedIndex = index;
        ApplyStyles();
        OnRangeChanged?.Invoke(index);
    }

    private void ApplyStyles()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            bool sel = i == _selectedIndex;
            _buttons[i].AddThemeStyleboxOverride("normal",
                UIStyles.MakeStyleBox(
                    sel ? UIStyles.ACCENT_BLUE : new Color(0.03f, 0.07f, 0.14f),
                    sel ? UIStyles.ACCENT : UIStyles.DIVIDER, 0, 4));
            _buttons[i].AddThemeColorOverride("font_color",
                sel ? UIStyles.TEXT_WHITE : UIStyles.TEXT_DIM);
        }
    }

    public int SelectedIndex => _selectedIndex;
}
