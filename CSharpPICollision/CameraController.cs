using System.Windows;
using System.Windows.Media.Media3D;

namespace CSharpPICollision
{
    class CameraController
    {
        private Vector _rotation;
        private double _scalar;
        private PerspectiveCamera _camera;

        public Vector Rotation
        {
            get => _rotation;
        }
        public Transform3D Transform
        {
            get => new Transform3DGroup() { Children = { GetRotationTransform(_rotation), GetScaleTransform(_scalar) } };
        }
        public PerspectiveCamera Camera
        {
            get => _camera;
        }

        public Vector? MinAngle
        {
            get; set;
        }
        public Vector? MaxAngle
        {
            get; set;
        }

        private static Vector GetAngleFromMovement(Vector movement, Size size)
        {
            var angleX = movement.Y / size.Height * 180;
            var angleY = movement.X / size.Width * 180;

            return new Vector(angleX, -angleY);
        }
        private static Transform3D GetRotationTransform(Vector rotation)
        {
            return new Transform3DGroup()
            {
                Children = {
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1,0,0), rotation.X)),
                new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0,1,0), rotation.Y))
                }
            };
        }
        private static Transform3D GetScaleTransform(double value)
        {
            return new ScaleTransform3D(value, value, value);
        }

        public void AddMovement(Vector movement, Size size)
        {
            Vector newAngle = _rotation + GetAngleFromMovement(movement, size);

            if ((!MinAngle.HasValue || (newAngle.X >= MinAngle.Value.X)) && (!MaxAngle.HasValue || newAngle.X <= MaxAngle.Value.X))
            {
                _rotation.X = newAngle.X;
            }

            if ((!MinAngle.HasValue || (newAngle.Y >= MinAngle.Value.Y)) && (!MaxAngle.HasValue || newAngle.Y <= MaxAngle.Value.Y))
            {
                _rotation.Y = newAngle.Y;
            }
        }
        public void Scale(double value)
        {
            _scalar *= value;
        }

        public CameraController(PerspectiveCamera camera)
        {
            _camera = camera;
            _rotation = new Vector();
            _scalar = 1;
        }
        public CameraController(PerspectiveCamera camera, Vector initialRotation) : this(camera)
        {
            _rotation = initialRotation;
        }
    }
}
