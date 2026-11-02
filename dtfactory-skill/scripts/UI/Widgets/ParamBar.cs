using i3D;

public partial class ParamBar : Control
{
    private float _value;
    private float _minVal;
    private float _maxVal = 100f;
    private string _name = "";
    private string _unit = "";
    private bool _dirty = true;
    private Label _valueLabel;

    public void Setup(string name, string unit, float min, float max, float value)
    {
        _name = name;
        _unit = unit;
        _minVal = min;
        _maxVal = max;
        _value = Mathf.Clamp(value, min, max);
        _dirty = true;
    }

    public void SetValue(float v)
    {
        _value = Mathf.Clamp(v, _minVal, _maxVal);
        if (_valueLabel != null)
            _valueLabel.Text = $"{_value:F1} {_unit}";
        _dirty = true;
    }

    public override void Update(double delta)
    {
        if (_dirty)
        {
            QueueRedraw();
            _dirty = false;
        }
    }

    public override void _Draw()
    {
        Vector2 size = Size;
        if (size.X < 10 || size.Y < 10) return;

        float labelW = 70f;
        float valueW = 80f;
        float barLeft = labelW + 4f;
        float barRight = size.X - valueW - 4f;
        float barH = 8f;
        float barY = size.Y * 0.5f - barH * 0.5f;

        DrawString(GetThemeFont("font"), new Vector2(0, barY + barH + 2),
            _name, HorizontalAlignment.Left, -1, UIStyles.FONT_BODY, UIStyles.TEXT_DIM);

        DrawRect(new Rect2(barLeft, barY, barRight - barLeft, barH),
            new Color(0.08f, 0.15f, 0.25f), true);

        float range = _maxVal - _minVal;
        float ratio = range > 0.001f ? (_value - _minVal) / range : 0f;
        ratio = Mathf.Clamp(ratio, 0f, 1f);

        Color barColor;
        if (ratio < 0.6f) barColor = UIStyles.OK;
        else if (ratio < 0.8f) barColor = UIStyles.WARN;
        else barColor = UIStyles.ERR;

        float fillW = (barRight - barLeft) * ratio;
        DrawRect(new Rect2(barLeft, barY, fillW, barH), barColor, true);

        string valText = $"{_value:F1} {_unit}";
        DrawString(GetThemeFont("font"), new Vector2(barRight + 4f, barY + barH + 2),
            valText, HorizontalAlignment.Left, -1, UIStyles.FONT_BODY, UIStyles.TEXT_WHITE);
    }

    public void SetCustomLabel(Label label)
    {
        _valueLabel = label;
    }
}
