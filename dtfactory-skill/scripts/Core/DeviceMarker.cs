using i3D;
using System;

public partial class DeviceMarker : Item3D
{
    private MeshRender[] _markers = Array.Empty<MeshRender>();
    private int _highlightIdx = -1;
    private float _time;

    public override void iStart()
    {
        AddToGroup("device_marker");

        var dataNode = GetTree().GetFirstItemInGroup("sim_data");
        if (dataNode is not SimDataService data) return;

        _markers = new MeshRender[data.Devices.Length];
        for (int i = 0; i < data.Devices.Length; i++)
        {
            _markers[i] = CreateArrow(data.Devices[i].Position, data.Devices[i].Online);
            _markers[i].Visible = false;
            AddChild(_markers[i]);
        }

        var camNode = GetTree().GetFirstItemInGroup("camera");
        if (camNode is SpringArmCamera cam)
        {
            cam.OnReturnToOverview += ClearHighlight;
            S3.Print("[DeviceMarker] subscribed to camera OnReturnToOverview");
        }

        SetUpdate(true);
    }

    public override void Update(double delta)
    {
        _time += (float)delta;
        for (int i = 0; i < _markers.Length; i++)
        {
            if (_markers[i] == null || !_markers[i].Visible) continue;
            float offset = Mathf.Sin(_time * 3.14f + i * 1.2f) * 0.02f;
            var p = _markers[i].Position;
            _markers[i].Position = new Vector3(p.X, p.Y + offset * (float)delta * 30f, p.Z);
        }
    }

    public void Highlight(int index)
    {
        _highlightIdx = index;
        for (int i = 0; i < _markers.Length; i++)
        {
            if (_markers[i] != null)
                _markers[i].Visible = (i == index);
        }
        S3.Print("[DeviceMarker] Highlight index=" + index);
    }

    public void ClearHighlight()
    {
        _highlightIdx = -1;
        for (int i = 0; i < _markers.Length; i++)
        {
            if (_markers[i] != null)
                _markers[i].Visible = false;
        }
        S3.Print("[DeviceMarker] ClearHighlight");
    }

    private MeshRender CreateArrow(Vector3 devicePos, bool online)
    {
        MeshRender arrow = new();
        arrow.Name = "Arrow";

        float yOff = 0.35f;
        arrow.Position = new Vector3(devicePos.X, devicePos.Y + yOff, devicePos.Z);

        Item3D group = new();
        group.Name = "Group";
        group.RotationDegrees = new Vector3(180, 0, 0);
        arrow.AddChild(group);

        MeshRender shaft = new();
        CylinderMesh shaftMesh = new();
        shaftMesh.SetTopRadius(0.006f);
        shaftMesh.SetBottomRadius(0.006f);
        shaftMesh.SetHeight(0.15f);
        shaftMesh.SetRadialSegments(8);
        shaft.Mesh = shaftMesh;
        shaft.Position = new Vector3(0, 0.075f, 0);
        shaft.MaterialOverride = MakeGlowMat(online);
        group.AddChild(shaft);

        MeshRender head = new();
        CylinderMesh headMesh = new();
        headMesh.SetTopRadius(0f);
        headMesh.SetBottomRadius(0.02f);
        headMesh.SetHeight(0.04f);
        headMesh.SetRadialSegments(8);
        head.Mesh = headMesh;
        head.Position = new Vector3(0, -0.02f, 0);
        head.MaterialOverride = MakeGlowMat(online);
        group.AddChild(head);

        return arrow;
    }

    private StandardMaterial3D MakeGlowMat(bool online)
    {
        StandardMaterial3D mat = new();
        mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        mat.AlbedoColor = online ? new Color(0f, 0.83f, 1f, 0.9f) : new Color(1f, 0.27f, 0.27f, 0.9f);
        mat.EmissionEnabled = true;
        mat.Emission = online ? new Color(0f, 0.6f, 0.8f) : new Color(0.8f, 0.15f, 0.15f);
        mat.EmissionEnergyMultiplier = 2f;
        return mat;
    }
}
