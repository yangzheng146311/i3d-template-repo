using i3D;

public static class UIStyles
{
    public static readonly Color BG = new(0.04f, 0.086f, 0.157f);
    public static readonly Color PANEL_BG = new(0.05f, 0.12f, 0.24f, 0.85f);
    public static readonly Color ACCENT = new(0f, 0.83f, 1f);
    public static readonly Color ACCENT_BLUE = new(0.1f, 0.45f, 0.91f);
    public static readonly Color TEXT_WHITE = Colors.White;
    public static readonly Color TEXT_DIM = new(0.545f, 0.639f, 0.78f);
    public static readonly Color OK = new(0f, 0.9f, 0.46f);
    public static readonly Color WARN = new(1f, 0.6f, 0f);
    public static readonly Color ERR = new(1f, 0.27f, 0.27f);
    public static readonly Color DIVIDER = new(0.1f, 0.23f, 0.36f);
    public static readonly Color CARD_BG = new(0.03f, 0.07f, 0.14f, 0.9f);
    public static readonly Color CARD_BG_HOVER = new(0.06f, 0.12f, 0.24f, 0.95f);
    public static readonly Color OVERLAY = new(0, 0, 0, 0.6f);

    public const int FONT_BIG = 20;
    public const int FONT_TITLE = 17;
    public const int FONT_SUBTITLE = 14;
    public const int FONT_BODY = 13;
    public const int FONT_SMALL = 11;
    public const int FONT_TINY = 10;

    public const float TOP_H = 52f;
    public const float LEFT_W = 180f;
    public const float RIGHT_W = 360f;
    public const float BOTTOM_H = 40f;
    public const float PADDING = 16f;

    public static StyleBoxFlat MakeStyleBox(Color bg, Color border, int borderWidth, int cornerRadius)
    {
        StyleBoxFlat s = new();
        s.SetBgColor(bg);
        s.SetBorderColor(border);
        s.SetBorderWidthAll(borderWidth);
        s.SetCornerRadiusAll(cornerRadius);
        s.SetContentMarginAll(0);
        s.SetAntiAliased(true);
        return s;
    }

    public static StyleBoxFlat MakePanelStyle()
    {
        return MakeStyleBox(PANEL_BG, DIVIDER, 0, 0);
    }

    public static StyleBoxFlat MakeCardStyle(Color accentColor, int borderWidth = 1)
    {
        StyleBoxFlat s = MakeStyleBox(CARD_BG, accentColor, borderWidth, 6);
        s.SetContentMarginAll(10);
        return s;
    }

    public static Label MakeLabel(string text, Color color, int fontSize)
    {
        Label l = new();
        l.Text = text;
        l.AddThemeColorOverride("font_color", color);
        l.AddThemeFontSizeOverride("font_size", fontSize);
        return l;
    }

    public static Label MakeSectionTitle(string text, int fontSize = FONT_TITLE)
    {
        Label l = MakeLabel(text, TEXT_WHITE, fontSize);
        l.SetCustomMinimumSize(new Vector2(0, 28));
        l.VerticalAlignment = VerticalAlignment.Center;
        return l;
    }

    public static ColorRect MakeDivider()
    {
        ColorRect r = new();
        r.SetCustomMinimumSize(new Vector2(0, 1));
        r.Color = DIVIDER;
        return r;
    }

    public static Control MakeSpacer(float height)
    {
        Control c = new();
        c.SetCustomMinimumSize(new Vector2(0, height));
        return c;
    }

    public static Button MakeButton(string text, int fontSize = FONT_BODY)
    {
        Button btn = new();
        btn.Text = text;
        btn.AddThemeFontSizeOverride("font_size", fontSize);
        return btn;
    }

    public static Button MakeMenuButton(string text, bool selected)
    {
        Button btn = new();
        btn.Text = text;
        btn.SetCustomMinimumSize(new Vector2(0, 44));
        btn.AddThemeFontSizeOverride("font_size", 15);
        btn.AddThemeColorOverride("font_color", selected ? TEXT_WHITE : TEXT_DIM);
        btn.AddThemeColorOverride("font_hover_color", TEXT_WHITE);
        btn.Alignment = HorizontalAlignment.Center;

        StyleBoxFlat normal = MakeStyleBox(Colors.Transparent, Colors.Transparent, 0, 6);
        StyleBoxFlat selectedStyle = MakeStyleBox(ACCENT_BLUE, ACCENT, 0, 6);
        selectedStyle.SetBorderWidth(Side.Left, 3);
        btn.AddThemeStyleboxOverride("normal", selected ? selectedStyle : normal);
        btn.AddThemeStyleboxOverride("hover", selectedStyle);
        btn.AddThemeStyleboxOverride("pressed", selectedStyle);

        return btn;
    }
}
