using minecraft.Camera;
using mmd_sd.Helpers;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = minecraft.Sys.Window;

namespace minecraft.Entities.Player;

public class Player : Entity
{
    private const float JumpForce = 6f;
    private const float JumpForceInWater = 5.75f;
    private const float WalkSpeed = 3f;
    private const float WalkSpeedInWater = 1.85f;
    private const float SprintSpeed = 5f;
    
    private float _currentSpeed;
    private float _jumpForce;
    
    private float _breakTimer;
    private float _placeTimer;

    private const float BreakDelay = 0.25f;
    private const float PlaceDelay = 0.25f;
    
    private readonly BlockInteractor _blockInteractor;

    public int SelectedBlockId { get; private set; } = 1;

    public Player() : base(new Vector3(0.6f, 1.8f, 0.6f))
    {
        _blockInteractor = new BlockInteractor(this);
    }
    
    public override void Update(float deltaTime)
    {
        if(Window.CameraMode == CameraMode.Free) return;
        
        HandleInput(deltaTime);
        
        _breakTimer -= deltaTime;
        _placeTimer -= deltaTime;

        if(_breakTimer <= 0f && Input.GetMouseButton(MouseButton.Button1))
        {
            if(_blockInteractor.TryBreakBlock())
                _breakTimer = BreakDelay;
        }

        if(_placeTimer <= 0f && Input.GetMouseButton(MouseButton.Button2))
        {
            if(_blockInteractor.TryPlaceBlock())
                _placeTimer = PlaceDelay;
        }
        
        if(IsInWater && Input.GetKey(Keys.Space))
        {
            MoveVertical(Vector3.UnitY, 5, deltaTime);
        }

        SelectedBlockId = Math.Clamp(SelectedBlockId - Math.Sign(Input.MouseWheelDelta), 1, 7);
    }

    private void HandleInput(float deltaTime)
    {
        var controlPressed = Input.GetKey(Keys.LeftControl);

        _currentSpeed = controlPressed switch
        {
            true when IsGrounded && !IsInWater => SprintSpeed,
            false when IsGrounded && !IsInWater => WalkSpeed,
            _ => IsInWater ? WalkSpeedInWater : WalkSpeed
        };
        
        var flatForward = Window.ActiveCamera.Transform.forward;
        flatForward.Y = 0;
        flatForward = flatForward.Normalized();

        var flatRight = Window.ActiveCamera.Transform.right;
        flatRight.Y = 0;
        flatRight = flatRight.Normalized();

        var inputDir = flatForward * Input.Vertical + flatRight * Input.Horizontal;
        
        inputDir.Y = 0f;

        if (inputDir.LengthSquared > 0.001f)
        {
            MoveHorizontal(inputDir.Normalized(), _currentSpeed, deltaTime);
        }
        
        if (Input.GetKey(Keys.Space)) TryJump();

        if (inputDir.LengthSquared > 0.001f)
            MoveHorizontal(inputDir.Normalized(), _currentSpeed, deltaTime);
    }
    private void TryJump()
    {
        _jumpForce = IsInWater ? JumpForceInWater : JumpForce;
        
        Jump(_jumpForce);
    }
}