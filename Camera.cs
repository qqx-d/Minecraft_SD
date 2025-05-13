using minecraft;
using OpenTK.Mathematics;

public class Camera
{
    public Transform transform { get; private set; } = new Transform();
    
    private float _yaw = -90f;
    private float _pitch = 0f;
    private float _fov = MathHelper.DegreesToRadians(90f);
    private float _sensitivity = 0.1f;

    public float AspectRatio { private get; set; }

    public Camera(float aspectRatio)
    {
        AspectRatio = aspectRatio;
    }
    
    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(transform.position, transform.position + transform.forward, transform.up);
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

        transform.eulerRotation = new Vector3(_pitch, _yaw, 0f);
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }
}