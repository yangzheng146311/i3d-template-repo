using i3D;

public partial class BarChart : Control
{
    private float[] _values = System.Array.Empty<float>();
    private string[] _labels = System.Array.Empty<string>();
    private Color[] _barColors = System.Array.Empty<Color>();
    private bool _grouped;
    private float[][] _groupValues;
    private Color[][] _groupColors;
    private string[] _groupLabels;
    private bool _showValues;
    private float _padding = 24f;
    private bool _dirty = true;

    public void SetData(float[] values, string[] labels, Color[] colors)
    {
        _values = values;
        _labels = labels;
        _barColors = colors;
        _grouped = false;
        _dirty = true;
    }

    public void SetGroupedData(float[][] groupValues, Color[][] groupColors, string[] labels, string[] groupLabels)
    {
        _groupValues = groupValues;
        _groupColors = groupColors;
        _labels = labels;
        _groupLabels = groupLabels;
        _grouped = true;
        _showValues = true;
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
        if (_grouped)
        {
            DrawGrouped();
            return;
        }

        Vector2 size = Size;
        if (size.X < 10 || size.Y < 10 || _values.Length == 0) return;

        float left = _padding;
        float right = size.X - 4f;
        float top = 4f;
        float bottom = size.Y - _padding;
        float chartW = right - left;
        float chartH = bottom - top;

        DrawRect(new Rect2(left, top, chartW, chartH), ChartUtils.BG_DARK, false, 1f);

        float maxVal = ChartUtils.FindMax(_values);
        if (maxVal < 0.01f) maxVal = 1f;
        maxVal *= 1.1f;

        int gridLines = 3;
        for (int i = 0; i <= gridLines; i++)
        {
            float y = top + chartH * i / gridLines;
            DrawLine(new Vector2(left, y), new Vector2(right, y), ChartUtils.GRID_COLOR, 1f, true);
            float val = maxVal - maxVal * i / gridLines;
            DrawString(GetThemeFont("font"), new Vector2(0, y + 4f),
                val.ToString("F1"), HorizontalAlignment.Left, -1, 10, ChartUtils.LABEL_COLOR);
        }

        int n = _values.Length;
        float barGap = chartW * 0.1f / (n + 1);
        float barW = (chartW - barGap * (n + 1)) / n;
        if (barW < 4f) barW = 4f;
        float totalW = barW * n + barGap * (n + 1);
        float startX = left + (chartW - totalW) * 0.5f;

        for (int i = 0; i < n; i++)
        {
            float ratio = _values[i] / maxVal;
            float barH = chartH * ratio;
            float x = startX + barGap + i * (barW + barGap);
            float y = bottom - barH;
            Color c = i < _barColors.Length ? _barColors[i] : ChartUtils.ACCENT;
            DrawRect(new Rect2(x, y, barW, barH), c, true);
            DrawRect(new Rect2(x, y, barW, barH), new Color(1, 1, 1, 0.1f), false, 1f);

            if (i < _labels.Length)
            {
                float lx = x + barW * 0.5f - 10f;
                float ly = bottom + 4f;
                DrawString(GetThemeFont("font"), new Vector2(lx, ly + 10f),
                    _labels[i], HorizontalAlignment.Left, -1, 10, ChartUtils.LABEL_COLOR);
            }
        }
    }

    private void DrawGrouped()
    {
        Vector2 size = Size;
        if (size.X < 10 || size.Y < 10 || _groupValues == null || _groupValues.Length == 0) return;

        float left = _padding;
        float right = size.X - 4f;
        float top = 4f;
        float bottom = size.Y - _padding;
        float chartW = right - left;
        float chartH = bottom - top;

        DrawRect(new Rect2(left, top, chartW, chartH), ChartUtils.BG_DARK, false, 1f);

        float maxVal = float.MinValue;
        for (int g = 0; g < _groupValues.Length; g++)
        {
            if (_groupValues[g] != null)
            {
                float mx = ChartUtils.FindMax(_groupValues[g]);
                if (mx > maxVal) maxVal = mx;
            }
        }
        if (maxVal < 0.01f) maxVal = 1f;
        maxVal *= 1.1f;

        int gridLines = 3;
        for (int i = 0; i <= gridLines; i++)
        {
            float y = top + chartH * i / gridLines;
            DrawLine(new Vector2(left, y), new Vector2(right, y), ChartUtils.GRID_COLOR, 1f, true);
            float val = maxVal - maxVal * i / gridLines;
            DrawString(GetThemeFont("font"), new Vector2(0, y + 4f),
                val.ToString("F1"), HorizontalAlignment.Left, -1, 10, ChartUtils.LABEL_COLOR);
        }

        int n = _groupValues[0]?.Length ?? 0;
        int groups = _groupValues.Length;
        float barGap = chartW * 0.08f / (n + 1);
        float groupGap = 2f;
        float barW = (chartW - barGap * (n + 1) - groupGap * n * (groups - 1)) / (n * groups);
        if (barW < 3f) barW = 3f;
        float totalW = barW * n * groups + barGap * (n + 1) + groupGap * n * (groups - 1);
        float startX = left + (chartW - totalW) * 0.5f;

        for (int i = 0; i < n; i++)
        {
            for (int g = 0; g < groups; g++)
            {
                if (_groupValues[g] == null || i >= _groupValues[g].Length) continue;
                float val = _groupValues[g][i];
                float ratio = val / maxVal;
                float barH = chartH * ratio;
                float x = startX + barGap + i * (barW * groups + groupGap * (groups - 1) + barGap) + g * (barW + groupGap);
                float y = bottom - barH;
                Color c = _groupColors != null && g < _groupColors.Length && i < _groupColors[g].Length
                    ? _groupColors[g][i] : ChartUtils.ACCENT;
                DrawRect(new Rect2(x, y, barW, barH), c, true);

                if (_showValues && barH > 14f)
                {
                    string valText = val.ToString("F0");
                    DrawString(GetThemeFont("font"), new Vector2(x + barW * 0.5f - 8f, y + 12f),
                        valText, HorizontalAlignment.Left, -1, 9, new Color(1, 1, 1, 0.8f));
                }
            }

            if (_labels != null && i < _labels.Length)
            {
                float lx = startX + barGap + i * (barW * groups + groupGap * (groups - 1) + barGap);
                float ly = bottom + 4f;
                DrawString(GetThemeFont("font"), new Vector2(lx, ly + 10f),
                    _labels[i], HorizontalAlignment.Left, -1, 10, ChartUtils.LABEL_COLOR);
            }
        }
    }
}
