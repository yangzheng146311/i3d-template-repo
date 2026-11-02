using i3D;
using System;

public partial class SpringArmCamera : Item3D
{
    [Export] private float _rotateSpeed = 0.005f;
    [Export] private float _zoomSpeed = 0.1f;
    [Export] private float _damping = 8f;
    [Export] private float _minDistance = 0.2f;
    [Export] private float _maxDistance = 1000f;
    [Export] private ItemPath _targetPath;

    private Camera _camera;
    private Vector3 _orbitCenter;
    private float _distance;
    private float _yaw, _pitch;
    private float _targetDistance, _targetYaw, _targetPitch;
    private bool _isRotating;
    private bool _isFlying;
    private Vector2 _lastMousePos;
    private Vector3 _initialCenter;
    private float _initialDistance;
    private float _initialYaw, _initialPitch;

    public event Action OnReturnToOverview;

    public override void iStart()
    {
        AddToGroup("camera");
        S3.Print("[SpringArmCamera] iStart, added to group 'camera'");

        _camera = GetItem<Camera>("Camera");
        _camera.MakeCurrent();

        if (_targetPath != null && !_targetPath.IsEmpty)
        {
            Item3D target = GetItem<Item3D>(_targetPath);
            _orbitCenter = target.GlobalPosition;
        }
        else
        {
            _orbitCenter = Vector3.Zero;
        }

        Vector3 offset = GlobalPosition - _orbitCenter;
        _targetDistance = _distance = offset.Length();
        if (_distance > 0.001f)
        {
            _targetYaw = _yaw = Mathf.Atan2(offset.X, offset.Z);
            _targetPitch = _pitch = Mathf.Asin(Mathf.Clamp(offset.Y / _distance, -1f, 1f));
        }

        _initialCenter = _orbitCenter;
        _initialDistance = _targetDistance;
        _initialYaw = _targetYaw;
        _initialPitch = _targetPitch;

        Input.MouseMode = Input.MouseModeEnum.Visible;
        UpdatePosition();
    }

    public override void Update(double delta)
    {
        float currentDamping = _isFlying ? 3f : _damping;
        float t = Mathf.Clamp((float)(currentDamping * delta), 0f, 1f);
        _distance = Mathf.Lerp(_distance, _targetDistance, t);
        _yaw = Mathf.LerpAngle(_yaw, _targetYaw, t);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, t);
        UpdatePosition();
    }

    public override void OnInput(InputEvent e)
    {
        if (e is InputEventMouseButton btn)
        {
            if (btn.ButtonIndex == MouseButton.Left && !btn.IsDoubleClick())
            {
                _isRotating = btn.Pressed;
                if (_isRotating && _isFlying)
                {
                    _isFlying = false;
                    S3.Print("[SpringArmCamera] FlyTo exited by user rotation");
                }
                _lastMousePos = btn.Position;
            }
            else if (btn.ButtonIndex == MouseButton.Right && btn.Pressed)
            {
                ReturnToOverview();
            }
            else if (btn.ButtonIndex == MouseButton.WheelUp && btn.Pressed)
                _targetDistance *= (1f - _zoomSpeed);
            else if (btn.ButtonIndex == MouseButton.WheelDown && btn.Pressed)
                _targetDistance *= (1f + _zoomSpeed);
            _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
        }
        else if (e is InputEventMouseMotion mm && _isRotating)
        {
            Vector2 d = mm.Position - _lastMousePos;
            _lastMousePos = mm.Position;
            _targetYaw -= d.X * _rotateSpeed;
            _targetPitch = Mathf.Clamp(_targetPitch - d.Y * _rotateSpeed, -1.55f, 1.55f);
        }
    }

    public void FlyTo(Vector3 target, float dist = 0.5f)
    {
        _orbitCenter = target;
        _targetDistance = Mathf.Clamp(dist, _minDistance, _maxDistance);
        _targetPitch = 0.4f;
        _isFlying = true;
        S3.Print("[SpringArmCamera] FlyTo " + target + " dist=" + dist);
    }

    public void ReturnToOverview()
    {
        _orbitCenter = _initialCenter;
        _targetDistance = _initialDistance;
        _targetYaw = _initialYaw;
        _targetPitch = _initialPitch;
        _isFlying = false;
        OnReturnToOverview?.Invoke();
        S3.Print("[SpringArmCamera] ReturnToOverview");
    }

    private void UpdatePosition()
    {
        float x = _distance * Mathf.Cos(_pitch) * Mathf.Sin(_yaw);
        float y = _distance * Mathf.Sin(_pitch);
        float z = _distance * Mathf.Cos(_pitch) * Mathf.Cos(_yaw);
        GlobalPosition = _orbitCenter + new Vector3(x, y, z);
        LookAt(_orbitCenter);
    }
}
