using OpenTK.Mathematics;

namespace minecraft
{
    public class Transform
    {
        private Vector3 _position = Vector3.Zero;
        private Vector3 _scale = Vector3.One;
        private Vector3 _eulerRotation = Vector3.Zero;

        private Vector3 _localPosition = Vector3.Zero;
        private Vector3 _localScale = Vector3.One;
        private Vector3 _localEulerRotation = Vector3.Zero;

        private Vector3 _forward = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        public Transform? parent = null;

        public Vector3 forward => _forward;
        public Vector3 up => _up;
        public Vector3 right => _right;

        public Vector3 position
        {
            get
            {
                if (parent != null)
                {
                    return Vector3.TransformPosition(_localPosition, parent.GetRotationMatrix()) + parent.position;
                }
                else
                {
                    return _position;
                }
            }
            set
            {
                if (parent != null)
                {
                    _localPosition = Vector3.TransformPosition(value - parent.position, parent.GetRotationMatrix().Inverted());
                }
                else
                {
                    _position = value;
                }
            }
        }

        public Vector3 scale
        {
            get => parent != null ? _localScale * parent.scale : _localScale;
            set => _localScale = parent != null ? value / parent.scale : value;
        }

        public Vector3 eulerRotation
        {
            get => parent != null ? parent.eulerRotation + _eulerRotation : _eulerRotation;
            set
            {
                _eulerRotation = value;
                UpdateVectors();
            }
        }

        public Vector3 localPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                UpdateVectors();
            }
        }

        public Vector3 localScale
        {
            get => _localScale;
            set => _localScale = value;
        }

        public Vector3 localEulerRotation
        {
            get => _localEulerRotation;
            set
            {
                _localEulerRotation = value;
                UpdateVectors();
            }
        }
        
        public void Rotate(Vector3 angles)
        {
            _eulerRotation += angles;
            UpdateVectors();
        }

        private void UpdateVectors()
        {
            var rotationMatrix = GetRotationMatrix();
            _forward = Vector3.TransformNormal(-Vector3.UnitZ, rotationMatrix);
            _up = Vector3.TransformNormal(Vector3.UnitY, rotationMatrix);
            _right = Vector3.TransformNormal(Vector3.UnitX, rotationMatrix);
        }

        private Matrix4 GetRotationMatrix()
        {
            return Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_eulerRotation.X))
                 * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_eulerRotation.Y))
                 * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_eulerRotation.Z));
        }
    }
}