using System.Windows;
using System.Windows.Media.Media3D;

namespace CSharpPICollision
{
    /// <summary>
    /// Controls PerspectiveCamera rotation and zoom
    /// </summary>
    class CameraController
    {
        /// <summary>
        /// Represents rotation vector
        /// </summary>
        private Vector _rotation;
        /// <summary>
        /// Represents zoom scalar
        /// </summary>
        private double _scalar;
        /// <summary>
        /// Represents controlled camera object
        /// </summary>
        private PerspectiveCamera _camera;

        /// <summary>
        /// Represents rotation vector
        /// </summary>
        public Vector Rotation
        {
            get => _rotation;
        }
        /// <summary>
        /// Represents rotation and zoom transform
        /// </summary>
        public Transform3D Transform
        {
            get => new Transform3DGroup() { Children = { GetRotationTransform(_rotation), GetScaleTransform(_scalar) } };
        }
        /// <summary>
        /// Represents controlled camera object
        /// </summary>
        public PerspectiveCamera Camera
        {
            get => _camera;
        }

        /// <summary>
        /// Represents minimal angle of rotation by x and y axises
        /// </summary>
        public Vector? MinAngle
        {
            get; set;
        }
        /// <summary>
        /// Represents maximal angle of rotation by x and y axises
        /// </summary>
        public Vector? MaxAngle
        {
            get; set;
        }

        /// <summary>
        /// Returns angle delta
        /// </summary>
        /// <param name="movement">Vector of mouse movement</param>
        /// <param name="size">Size of viewport</param>
        /// <returns>Angle delta</returns>
        private static Vector GetAngleFromMovement(Vector movement, Size size)
        {
            var angleX = movement.Y / size.Height * 180;
            var angleY = movement.X / size.Width * 180;

            return new Vector(angleX, -angleY);
        }
        /// <summary>
        /// Returns rotation transform
        /// </summary>
        /// <param name="rotation">Vector of rotation</param>
        /// <returns>Rotation transform</returns>
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
        /// <summary>
        /// Returns scale transform
        /// </summary>
        /// <param name="value">Zoom multiplier</param>
        /// <returns>Scale transform</returns>
        private static Transform3D GetScaleTransform(double value)
        {
            return new ScaleTransform3D(value, value, value);
        }

        /// <summary>
        /// Adds movement vector to rotation
        /// </summary>
        /// <param name="movement">Vector of mouse movement</param>
        /// <param name="size">Size of viewport</param>
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
        /// <summary>
        /// Scales viewport by scalar 
        /// </summary>
        /// <param name="value">Scale multiplier</param>
        public void Scale(double value)
        {
            _scalar *= value;
        }

        /// <summary>
        /// Initializes new instance of CameraController with given camera 
        /// </summary>
        /// <param name="camera">Controlled camera</param>
        public CameraController(PerspectiveCamera camera)
        {
            _camera = camera;
            _rotation = new Vector();
            _scalar = 1;
        }
        /// <summary>
        /// Initializes new instance of CameraController with given camera and initial rotation
        /// </summary>
        /// <param name="camera">Controlled camera</param>
        /// <param name="initialRotation">Initial vector of rotation</param>
        public CameraController(PerspectiveCamera camera, Vector initialRotation) : this(camera)
        {
            _rotation = initialRotation;
        }
    }
}
