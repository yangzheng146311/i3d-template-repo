using i3D;

public partial class GaugeChart : Control
{
    private float _value;
    private float _minVal;
    private float _maxVal = 100f;
    private string _label = "";
    private string _unit = "";
    private bool _dirty = true;

    public void SetValue(float v)
    {
        _value = Mathf.Clamp(v, _minVal, _maxVal);
        _dirty = true;
    }

    public void SetRange(float min, float max)
    {
        _minVal = min;
        _maxVal = max;
        _dirty = true;
    }

    public void SetLabel(string label, string unit)
    {
        _label = label;
        _unit = unit;
        _dirty = true;
    }

    public void MarkDirty() => _dirty = true;

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

        float radius = Mathf.Min(size.X, size.Y) * 0.42f;
        Vector2 center = size * 0.5f;
        float thickness = radius * 0.14f;
        float ratio = (_value - _minVal) / (_maxVal - _minVal);
        if (Mathf.Abs(_maxVal - _minVal) < 0.001f) ratio = 0f;
        ratio = Mathf.Clamp(ratio, 0f, 1f);

        float startAngle = Mathf.Pi * 0.75f;
        float endAngle = Mathf.Pi * 2.25f;
        float sweepTotal = endAngle - startAngle;
        int arcSegments = 64;

        DrawArcBg(center, radius, thickness, startAngle, endAngle, arcSegments);
        if (ratio > 0.001f)
        {
            float sweepValue = startAngle + sweepTotal * ratio;
            DrawArcFg(center, radius, thickness, startAngle, sweepValue, arcSegments, ratio);
        }

        string valText = _value.ToString("F1");
        var font = GetThemeFont("font");
        Vector2 valSize = font.GetStringSize(valText, HorizontalAlignment.Center, -1, 22);
        DrawString(font, new Vector2(center.X - valSize.X * 0.5f, center.Y + 4f),
            valText, HorizontalAlignment.Left, -1, 22, Colors.White);

        if (!string.IsNullOrEmpty(_unit))
        {
            Vector2 unitSize = font.GetStringSize(_unit, HorizontalAlignment.Center, -1, 12);
            DrawString(font, new Vector2(center.X - unitSize.X * 0.5f, center.Y + 20f),
                _unit, HorizontalAlignment.Left, -1, 12, ChartUtils.LABEL_COLOR);
        }

        if (!string.IsNullOrEmpty(_label))
        {
            Vector2 lblSize = font.GetStringSize(_label, HorizontalAlignment.Center, -1, 11);
            DrawString(font, new Vector2(center.X - lblSize.X * 0.5f, center.Y + radius + 14f),
                _label, HorizontalAlignment.Left, -1, 11, ChartUtils.LABEL_COLOR);
        }
    }

    private void DrawArcBg(Vector2 center, float radius, float thickness,
        float start, float end, int segments)
    {
        Color bg = new(0.08f, 0.15f, 0.25f);
        for (int i = 0; i < segments; i++)
        {
            float a1 = start + (end - start) * i / segments;
            float a2 = start + (end - start) * (i + 1) / segments;
            Vector2 from = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            Vector2 to = center + new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * radius;
            DrawLine(from, to, bg, thickness, true);
        }
    }

    private void DrawArcFg(Vector2 center, float radius, float thickness,
        float start, float end, int segments, float ratio)
    {
        Color fgColor = ChartUtils.ColorForValue(ratio);
        for (int i = 0; i < segments; i++)
        {
            float t0 = (float)i / segments;
            float t1 = (float)(i + 1) / segments;
            if (t0 > 1f) break;
            float a1 = start + (end - start) * t0;
            float a2 = start + (end - start) * Mathf.Min(t1, 1f);
            float segRatio = t0 * ratio / (ratio > 0.001f ? ratio : 1f);
            Color c = ChartUtils.ColorForValue(segRatio * ratio);
            Vector2 from = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            Vector2 to = center + new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * radius;
            DrawLine(from, to, c, thickness, true);
        }
    }
}
