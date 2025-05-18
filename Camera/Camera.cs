using minecraft.Entities;
using OpenTK.Mathematics;

namespace minecraft.Camera;

public class Camera(float aspectRatio)
{
    public Transform Transform { get; private set; } = new();
    
    private float _yaw = -90f;
    private float _pitch;
    private readonly float _fov = MathHelper.DegreesToRadians(90f);
    private readonly float _sensitivity = 0.1f;
    
    public float AspectRatio { private get; set; } = aspectRatio;

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Transform.position, Transform.position + Transform.forward, Transform.up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 3000f);
    }

    public void ProcessMouseMovement(float deltaX, float deltaY)
    {
        _yaw += deltaX * _sensitivity;
        _pitch -= deltaY * _sensitivity;
        _pitch = MathHelper.Clamp(_pitch, -89f, 89f);

        Transform.rotationInDegrees = new Vector3(_pitch, _yaw, 0f);
    }

    public void Move(Vector3 direction, float speed)
    {
        Transform.position += direction * speed;
    }
}