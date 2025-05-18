using OpenTK.Mathematics;

namespace minecraft.Entities
{
    public class Transform
    {
        private Vector3 _position = Vector3.Zero;
        private Vector3 _scale = Vector3.One;
        private Vector3 _rotationInDegrees = Vector3.Zero;

        private Vector3 _localPosition = Vector3.Zero;
        private Vector3 _localScale = Vector3.One;
        private Vector3 _localRotationInDegreesInDegrees = Vector3.Zero;

        private Vector3 _forward = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        public Transform? Parent = null;

        public Vector3 forward => _forward;
        public Vector3 up => _up;
        public Vector3 right => _right;

        public Vector3 position
        {
            get
            {
                if (Parent != null)
                {
                    return Vector3.TransformPosition(_localPosition, Parent.GetRotationMatrix()) + Parent.position;
                }
                else
                {
                    return _position;
                }
            }
            set
            {
                if (Parent != null)
                {
                    _localPosition = Vector3.TransformPosition(value - Parent.position, Parent.GetRotationMatrix().Inverted());
                }
                else
                {
                    _position = value;
                }
            }
        }

        public Vector3 scale
        {
            get => Parent != null ? _localScale * Parent.scale : _localScale;
            set => _localScale = Parent != null ? value / Parent.scale : value;
        }

        public Vector3 rotationInDegrees
        {
            get => Parent != null ? Parent.rotationInDegrees + _rotationInDegrees : _rotationInDegrees;
            set
            {
                _rotationInDegrees = value;
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

        public Vector3 localRotationInDegrees
        {
            get => _localRotationInDegreesInDegrees;
            set
            {
                _localRotationInDegreesInDegrees = value;
                UpdateVectors();
            }
        }
        
        public void Rotate(Vector3 angles)
        {
            _rotationInDegrees += angles;
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
            return Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_rotationInDegrees.X))
                 * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotationInDegrees.Y))
                 * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_rotationInDegrees.Z));
        }
    }
}