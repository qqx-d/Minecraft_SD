using mmd_sd.Helpers;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = minecraft.Sys.Window;

namespace minecraft.Entities.Player;

public class Player : Entity
{
    private const float JumpForce = 6f;

    private const float WalkSpeed = 3f;
    private const float SprintSpeed = 5f;
    private float _currentSpeed = 0f;
    
    private float _breakTimer = 0f;
    private float _placeTimer = 0f;

    private const float BreakDelay = 0.25f;
    private const float PlaceDelay = 0.25f;
    
    private BlockInteractor _blockInteractor;

    public int SelectedBlockID { get; private set; }

    public Player() : base(new Vector3(0.6f, 1.8f, 0.6f))
    {
        _blockInteractor = new BlockInteractor(this);
    }
    
    public override void Update(float deltaTime)
    {
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

        SelectedBlockID = Math.Clamp(SelectedBlockID - Math.Sign(Input.MouseWheelDelta), 0, 7);
    }

    private void HandleInput(float deltaTime)
    {
        _currentSpeed = Input.GetKey(Keys.LeftControl) ? SprintSpeed : WalkSpeed;
        
        var flatForward = Window.ActiveCamera.transform.forward;
        flatForward.Y = 0;
        flatForward = flatForward.Normalized();

        var flatRight = Window.ActiveCamera.transform.right;
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
        Jump(JumpForce);
    }
}