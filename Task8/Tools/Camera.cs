using System;
using OpenTK;
using OpenTK.Input;

namespace CompGraph.Tools;

class Camera
{
    private Vector3 _position;
    private Vector3 _viewDir;
    private Vector3 _up;
    private Vector3 _velocity;
    private readonly float _mouseSensitivity;
    public Matrix4 View { get; private set; }
    public Camera(Vector3 position, Vector3 up, float lookX = -90.0f, float lookY = 0.0f, float mouseSensitivity = 0.1f, float speed = 10)
    {
        LookX = lookX;
        LookY = lookY;

        _viewDir.X = MathF.Cos(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));
        _viewDir.Y = MathF.Sin(MathHelper.DegreesToRadians(LookY));
        _viewDir.Z = MathF.Sin(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));

        View = GenerateMatrix(position, _viewDir, up);
        _position = position;
        _up = up;
        _mouseSensitivity = mouseSensitivity;
    }

    public Vector3 GetCameraPos() => _position;
    public void SetCameraVelocity(Vector3 velocity) => _velocity = velocity;
    private float LookX { get; set; }
    private float LookY { get; set; }
    public void ProcessInputs(float dT, out bool frameChanged)
    {
        frameChanged = false;

        var mouseDelta = MouseManager.DeltaPosition;
        if (mouseDelta.X != 0 || mouseDelta.Y != 0)
            frameChanged = true;

        LookX += mouseDelta.X * _mouseSensitivity;
        LookY -= mouseDelta.Y * _mouseSensitivity;

        if (LookY >= 90) LookY = 89.999f;
        if (LookY <= -90) LookY = -89.999f;

        _viewDir.X = MathF.Cos(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));
        _viewDir.Y = MathF.Sin(MathHelper.DegreesToRadians(LookY));
        _viewDir.Z = MathF.Sin(MathHelper.DegreesToRadians(LookX)) * MathF.Cos(MathHelper.DegreesToRadians(LookY));

        var acceleration = Vector3.Zero;
        if (KeyboardManager.IsKeyDown(Key.W))
            acceleration += _viewDir;
            
        if (KeyboardManager.IsKeyDown(Key.S))
            acceleration -= _viewDir;
            
        if (KeyboardManager.IsKeyDown(Key.D))
            acceleration += Vector3.Cross(_viewDir, _up).Normalized();

        if (KeyboardManager.IsKeyDown(Key.A))
            acceleration -= Vector3.Cross(_viewDir, _up).Normalized();

        _velocity += acceleration;
        if (acceleration != Vector3.Zero || _velocity != Vector3.Zero)
            frameChanged = true;

        if (Vector3.Dot(_velocity, _velocity) < 0.01f)
            _velocity = Vector3.Zero;

        _velocity *= 0.95f;
        _velocity += acceleration * dT;
        _position += _velocity * dT;
        View = GenerateMatrix(_position, _viewDir, _up);
    }

    public static Matrix4 GenerateMatrix(Vector3 position, Vector3 viewDir, Vector3 up) => 
        Matrix4.LookAt(position, position + viewDir, up);
}