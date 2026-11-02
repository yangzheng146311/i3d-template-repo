using i3D;
using System;

public static class UIBuilder
{
    public static PanelContainer MakeStatCard(string title, Label valueLabel, Color accentColor)
    {
        PanelContainer card = new();
        card.AddThemeStyleboxOverride("panel", UIStyles.MakeCardStyle(accentColor));
        card.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        VBoxContainer vbox = new();
        vbox.AddThemeConstantOverride("separation", 2);
        card.AddChild(vbox);

        valueLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(valueLabel);

        Label ttl = UIStyles.MakeLabel(title, UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
        ttl.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(ttl);

        return card;
    }

    public static Label MakeStatValue(string text, Color color)
    {
        Label l = UIStyles.MakeLabel(text, color, UIStyles.FONT_BIG);
        l.HorizontalAlignment = HorizontalAlignment.Center;
        return l;
    }

    public static HBoxContainer MakeMonitorRow(string name, ref Label valueRef, Color color)
    {
        HBoxContainer row = new();
        row.SetCustomMinimumSize(new Vector2(0, 32));
        row.AddThemeConstantOverride("separation", 0);

        Label nl = UIStyles.MakeLabel(name, UIStyles.TEXT_DIM, UIStyles.FONT_BODY);
        nl.SetCustomMinimumSize(new Vector2(70, 0));
        nl.VerticalAlignment = VerticalAlignment.Center;
        row.AddChild(nl);

        Control spacer = new();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(spacer);

        Label vl = UIStyles.MakeLabel("--", color, 16);
        vl.VerticalAlignment = VerticalAlignment.Center;
        row.AddChild(vl);
        valueRef = vl;

        return row;
    }

    public static PanelContainer MakeAlertCard(string title, string desc, Color levelColor)
    {
        PanelContainer card = new();
        card.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        StyleBoxFlat style = UIStyles.MakeStyleBox(new Color(0.04f, 0.08f, 0.16f, 0.9f), levelColor, 1, 4);
        style.SetContentMarginAll(10);
        style.SetBorderWidthAll(1);
        card.AddThemeStyleboxOverride("panel", style);

        VBoxContainer vbox = new();
        vbox.AddThemeConstantOverride("separation", 4);
        card.AddChild(vbox);

        vbox.AddChild(UIStyles.MakeLabel(title, levelColor, UIStyles.FONT_SUBTITLE));
        Label d = UIStyles.MakeLabel(desc, UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
        d.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(d);

        return card;
    }

    public static HBoxContainer MakeInfoRow(string key, string val)
    {
        HBoxContainer row = new();
        row.AddThemeConstantOverride("separation", 4);
        Label kl = UIStyles.MakeLabel(key + ":", UIStyles.TEXT_DIM, 12);
        kl.SetCustomMinimumSize(new Vector2(80, 0));
        row.AddChild(kl);
        row.AddChild(UIStyles.MakeLabel(val, Colors.White, 12));
        return row;
    }

    public static HBoxContainer MakeToggleRow(string name, bool defaultOn)
    {
        HBoxContainer row = new();
        row.SetCustomMinimumSize(new Vector2(0, 36));

        row.AddChild(UIStyles.MakeLabel(name, UIStyles.TEXT_DIM, UIStyles.FONT_BODY));

        Control spacer = new();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(spacer);

        CheckBox cb = new();
        cb.ButtonPressed = defaultOn;
        cb.AddThemeColorOverride("font_color", UIStyles.TEXT_WHITE);
        cb.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
        row.AddChild(cb);

        return row;
    }

    public static StyleBoxFlat MakeBtnStyle(Color bg, Color border, int radius)
    {
        StyleBoxFlat s = new();
        s.SetBgColor(bg);
        s.SetBorderColor(border);
        s.SetBorderWidthAll(0);
        s.SetCornerRadiusAll(radius);
        s.SetContentMarginAll(0);
        return s;
    }

    public static HBoxContainer MakeBottomStatChip(string label, string value, Color color)
    {
        HBoxContainer chip = new();
        chip.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        chip.AddThemeConstantOverride("separation", 8);

        Control spacerL = new();
        spacerL.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        chip.AddChild(spacerL);

        chip.AddChild(UIStyles.MakeLabel(label, UIStyles.TEXT_DIM, UIStyles.FONT_SMALL));
        chip.AddChild(UIStyles.MakeLabel(value, color, 16));

        Control spacerR = new();
        spacerR.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        chip.AddChild(spacerR);

        return chip;
    }

    public static HBoxContainer MakeBottomStatChipRef(string label, Label valueLabel, Color color)
    {
        HBoxContainer chip = new();
        chip.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        chip.AddThemeConstantOverride("separation", 8);

        Control spacerL = new();
        spacerL.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        chip.AddChild(spacerL);

        chip.AddChild(UIStyles.MakeLabel(label, UIStyles.TEXT_DIM, UIStyles.FONT_SMALL));
        valueLabel.VerticalAlignment = VerticalAlignment.Center;
        chip.AddChild(valueLabel);

        Control spacerR = new();
        spacerR.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        chip.AddChild(spacerR);

        return chip;
    }

    public static VSeparator MakeBottomSeparator()
    {
        VSeparator sep = new();
        sep.AddThemeColorOverride("color", UIStyles.DIVIDER);
        return sep;
    }
}
