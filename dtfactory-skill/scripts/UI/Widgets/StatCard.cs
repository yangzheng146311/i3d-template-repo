using i3D;

public partial class StatCard : PanelContainer
{
    private Label _valueLabel;

    public override void iStart()
    {
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

    public static StatCard Create(string title, string initialValue, Color accentColor)
    {
        StatCard card = new();
        card.AddThemeStyleboxOverride("panel", UIStyles.MakeCardStyle(accentColor));
        card.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        VBoxContainer vbox = new();
        vbox.AddThemeConstantOverride("separation", 2);
        card.AddChild(vbox);

        card._valueLabel = UIBuilder.MakeStatValue(initialValue, accentColor);
        vbox.AddChild(card._valueLabel);

        Label ttl = UIStyles.MakeLabel(title, UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
        ttl.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(ttl);

        return card;
    }

    public void SetValue(string text)
    {
        if (_valueLabel != null)
            _valueLabel.Text = text;
    }

    public Label GetValueLabel() => _valueLabel;
}
