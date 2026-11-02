using i3D;

public partial class LineChart : Control
{
    private float[] _data = System.Array.Empty<float>();
    private float[][] _multiData;
    private Color[] _multiColors;
    private bool _multiMode;
    private Color _lineColor = ChartUtils.ACCENT;
    private Color _fillColor = new(0f, 0.83f, 1f, 0.15f);
    private string _yLabelFormat = "{0:F1}";
    private float _yMin;
    private float _yMax;
    private bool _autoRange = true;
    private float _padding = 24f;
    private bool _dirty = true;

    public void SetData(float[] data)
    {
        _data = data;
        _multiMode = false;
        _dirty = true;
    }

    public void SetMultiData(float[][] data, Color[] colors)
    {
        _multiData = data;
        _multiColors = colors;
        _multiMode = true;
        _dirty = true;
    }

    public void SetColor(Color lineColor)
    {
        _lineColor = lineColor;
        _fillColor = new Color(lineColor.R, lineColor.G, lineColor.B, 0.15f);
        _dirty = true;
    }

    public void SetRange(float min, float max)
    {
        _yMin = min;
        _yMax = max;
        _autoRange = false;
        _dirty = true;
    }

    public void SetLabelFormat(string fmt)
    {
        _yLabelFormat = fmt;
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
        if (_multiMode)
        {
            DrawMulti();
            return;
        }

        Vector2 size = Size;
        if (size.X < 10 || size.Y < 10 || _data.Length < 2) return;

        float left = _padding;
        float right = size.X - 4f;
        float top = 4f;
        float bottom = size.Y - _padding;
        float chartW = right - left;
        float chartH = bottom - top;

        DrawRect(new Rect2(left, top, chartW, chartH), ChartUtils.BG_DARK, false, 1f);

        float minVal = _autoRange ? ChartUtils.FindMin(_data) : _yMin;
        float maxVal = _autoRange ? ChartUtils.FindMax(_data) : _yMax;
        float margin = (maxVal - minVal) * 0.1f;
        if (margin < 0.01f) margin = 1f;
        minVal -= margin;
        maxVal += margin;

        int gridLines = 4;
        for (int i = 0; i <= gridLines; i++)
        {
            float y = top + chartH * i / gridLines;
            DrawLine(new Vector2(left, y), new Vector2(right, y), ChartUtils.GRID_COLOR, 1f, true);
            float val = maxVal - (maxVal - minVal) * i / gridLines;
            string label = string.Format(_yLabelFormat, val);
            DrawString(GetThemeFont("font"), new Vector2(0, y + 4f), label,
                HorizontalAlignment.Left, -1, 10, ChartUtils.LABEL_COLOR);
        }

        Vector2[] points = new Vector2[_data.Length];
        for (int i = 0; i < _data.Length; i++)
        {
            float x = left + chartW * i / (_data.Length - 1);
            float y = top + chartH - chartH * ChartUtils.Remap(_data[i], minVal, maxVal, 0f, 1f);
            points[i] = new Vector2(x, y);
        }

        Vector2[] fillPoints = new Vector2[_data.Length + 2];
        for (int i = 0; i < _data.Length; i++)
            fillPoints[i] = points[i];
        fillPoints[_data.Length] = new Vector2(right, bottom);
        fillPoints[_data.Length + 1] = new Vector2(left, bottom);
        DrawPolygon(fillPoints, new[] { _fillColor });

        DrawPolyline(points, _lineColor, 2f, true);

        Vector2 last = points[_data.Length - 1];
        DrawCircle(last, 4f, _lineColor, true);
        DrawCircle(last, 6f, _lineColor, false, 1f);
    }

    private void DrawMulti()
    {
        Vector2 size = Size;
        if (size.X < 10 || size.Y < 10 || _multiData == null || _multiData.Length == 0) return;

        float left = _padding;
        float right = size.X - 4f;
        float top = 4f;
        float bottom = size.Y - _padding;
        float chartW = right - left;
        float chartH = bottom - top;

        DrawRect(new Rect2(left, top, chartW, chartH), ChartUtils.BG_DARK, false, 1f);

        float globalMin = float.MaxValue, globalMax = float.MinValue;
        for (int s = 0; s < _multiData.Length; s++)
        {
            if (_multiData[s] == null || _multiData[s].Length < 2) continue;
            float mn = ChartUtils.FindMin(_multiData[s]);
            float mx = ChartUtils.FindMax(_multiData[s]);
            if (mn < globalMin) globalMin = mn;
            if (mx > globalMax) globalMax = mx;
        }
        float margin = (globalMax - globalMin) * 0.1f;
        if (margin < 0.01f) margin = 1f;
        globalMin -= margin;
        globalMax += margin;

        int gridLines = 4;
        for (int i = 0; i <= gridLines; i++)
        {
            float y = top + chartH * i / gridLines;
            DrawLine(new Vector2(left, y), new Vector2(right, y), ChartUtils.GRID_COLOR, 1f, true);
            float val = globalMax - (globalMax - globalMin) * i / gridLines;
            DrawString(GetThemeFont("font"), new Vector2(0, y + 4f),
                string.Format(_yLabelFormat, val),
                HorizontalAlignment.Left, -1, 10, ChartUtils.LABEL_COLOR);
        }

        for (int s = 0; s < _multiData.Length; s++)
        {
            if (_multiData[s] == null || _multiData[s].Length < 2) continue;
            Color c = s < _multiColors.Length ? _multiColors[s] : ChartUtils.ACCENT;

            Vector2[] points = new Vector2[_multiData[s].Length];
            for (int i = 0; i < _multiData[s].Length; i++)
            {
                float x = left + chartW * i / (_multiData[s].Length - 1);
                float y = top + chartH - chartH * ChartUtils.Remap(_multiData[s][i], globalMin, globalMax, 0f, 1f);
                points[i] = new Vector2(x, y);
            }

            Color fillC = new Color(c.R, c.G, c.B, 0.1f);
            Vector2[] fillPoints = new Vector2[_multiData[s].Length + 2];
            for (int i = 0; i < _multiData[s].Length; i++)
                fillPoints[i] = points[i];
            fillPoints[_multiData[s].Length] = new Vector2(right, bottom);
            fillPoints[_multiData[s].Length + 1] = new Vector2(left, bottom);
            DrawPolygon(fillPoints, new[] { fillC });

            DrawPolyline(points, c, 2f, true);

            Vector2 last = points[_multiData[s].Length - 1];
            DrawCircle(last, 3f, c, true);
        }

        float legendY = top + 12f;
        float legendX = left + 8f;
        string[] legendLabels = { "电", "水", "气" };
        for (int s = 0; s < _multiData.Length && s < legendLabels.Length; s++)
        {
            Color c = s < _multiColors.Length ? _multiColors[s] : ChartUtils.ACCENT;
            DrawRect(new Rect2(legendX, legendY - 8f, 12f, 8f), c, true);
            DrawString(GetThemeFont("font"), new Vector2(legendX + 16f, legendY),
                legendLabels[s], HorizontalAlignment.Left, -1, 10, ChartUtils.LABEL_COLOR);
            legendX += 40f;
        }
    }
}
