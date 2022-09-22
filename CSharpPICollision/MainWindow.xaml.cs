using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;

namespace CSharpPICollision
{
    readonly record struct Segment(double Point1, double Point2);
    readonly record struct Collision(PhysicalObject Object1, PhysicalObject Object2, double Time);
    readonly record struct CollisionResponse(double Position, double Velocity);

    record class HorizontalAxis(double Y);

    delegate void CollisionsHandler();

    enum CollisionTarget : int
    {
        First = 0,
        Second = 1
    }

    abstract class PhysicalObject
    {
        public abstract double GetVelocity();
        public abstract double GetPosition(double? timeDelta = null);
        public abstract double Distance(PhysicalObject obj, double? timeDelta = null);
        public abstract double ProcessCollision(PhysicalObject obj);
    }

    interface MaterialPointReadonly
    {
        double Impulse
        {
            get;
        }
        double Mass
        {
            get;
        }
        double Velocity
        {
            get;
        }
        double Position
        {
            get;
        }
    }

    class MaterialPoint : MaterialPointReadonly
    {
        public double Impulse
        {
            get
            {
                return Mass * Velocity;
            }
        }
        public double Mass
        {
            get; set;
        }
        public double Velocity
        {
            get; set;
        }
        public double Position
        {
            get; set;
        }

        public override string ToString()
        {
            return $"Impulse - {Impulse}, Mass - {Mass}, Velocity - {Velocity}, Position - {Position}";
        }

        public MaterialPoint(double mass, double velocity, double position)
        {
            this.Mass = mass;
            this.Velocity = velocity;
            this.Position = position;
        }
    }

    class CollisionsCount
    {
        private Block _source;

        public string Text
        {
            get => $"COLLISIONS: ${_source.Collisions}";
        }

        public CollisionsCount(Block source)
        {
            this._source = source;
        }
    }

    static class Utils
    {
        public static Segment GetSegment(double position, double size)
        {
            return new Segment(position, position + size);
        }
        public static double GetLength(Segment segment)
        {
            return Math.Abs(segment.Point2 - segment.Point1);
        }
        public static double GetCenter(Segment segment)
        {
            return (segment.Point2 + segment.Point1) / 2;
        }
        public static double DistanceBeetweenSegments(Segment segment1, Segment segment2)
        {
            double center1 = Utils.GetCenter(segment1);
            double center2 = Utils.GetCenter(segment2);
            double radius1 = Utils.GetLength(segment1) / 2;
            double radius2 = Utils.GetLength(segment2) / 2;

            return Utils.GetLength(new Segment(center2, center1)) - (radius1 + radius2);
        }
        public static double DistanceBeetweenSegmentAndPoint(Segment segment, double point)
        {
            return Utils.GetLength(new Segment(Utils.GetCenter(segment), point)) - Utils.GetLength(segment) / 2;
        }
    }

    class Block : PhysicalObject, ICloneable
    {
        private MaterialPoint _properties;
        private double _size;
        private int _collisions;

        public MaterialPointReadonly Properties
        {
            get => this._properties;
        }
        public double Size
        {
            get => this._size;
        }
        public int Collisions
        {
            get => this._collisions;
        }

        public override double GetPosition(double? timeDelta = null)
        {
            if (timeDelta.HasValue)
            {
                return _properties.Position + _properties.Velocity * timeDelta.Value;
            }
            else
            {
                return _properties.Position;
            }
        }
        public override double GetVelocity()
        {
            return _properties.Velocity;
        }
        public override double Distance(PhysicalObject obj, double? timeDelta = null)
        {
            return obj switch
            {
                Block block => Utils.DistanceBeetweenSegments(Utils.GetSegment(GetPosition(timeDelta), this._size), Utils.GetSegment(block.GetPosition(timeDelta), block.Size)),
                Wall wall => Utils.DistanceBeetweenSegmentAndPoint(Utils.GetSegment(GetPosition(timeDelta), this._size), wall.Position),
                _ => throw new ArgumentException("Cannot define obj type")
            };
        }
        public override double ProcessCollision(PhysicalObject obj)
        {
            this._collisions++;

            return obj switch
            {
                Block block => (this._properties.Impulse - block.Properties.Mass * this._properties.Velocity + 2 * block.Properties.Impulse) / (this._properties.Mass + block.Properties.Mass),
                Wall => -_properties.Velocity,
                _ => throw new ArgumentException("Cannot define obj type")
            };
        }

        public void SetPosition(double value)
        {
            _properties.Position = value;
        }
        public void SetVelocity(double value)
        {
            _properties.Velocity = value;
        }

        public object Clone()
        {
            return new Block(_size, _properties.Mass, _properties.Velocity, _properties.Position);
        }

        public Block(double size, double mass, double velocity, double position)
        {
            this._properties = new MaterialPoint(mass, velocity, position);
            this._size = size;
            this._collisions = 0;
        }
    }

    class Wall : PhysicalObject
    {
        private double _position;

        public double Position
        {
            get => _position;
        }
        public double Velocity
        {
            get => 0;
        }

        public override double GetPosition(double? timeDelta = null)
        {
            return this._position;
        }
        public override double GetVelocity()
        {
            return Velocity;
        }
        public override double Distance(PhysicalObject obj, double? timeDelta = null)
        {
            return obj switch
            {
                Block block => block.Distance(this, timeDelta),
                Wall wall => -wall.Position - _position,
                _ => throw new ArgumentException("Cannot define obj type")
            };
        }
        public override double ProcessCollision(PhysicalObject obj)
        {
            return double.NaN;
        }

        public Wall(double position)
        {
            this._position = position;
        }
    }

    class PhysicalEngine
    {
        private int _timeUnit
        {
            get => 1000;
        }
        private double _epsilon
        {
            get => 1e-15;
        }

        private List<PhysicalObject> _objects;
        private EventHandler? _updateEvent;
        private object _syncObj;

        public IReadOnlyList<PhysicalObject> Objects
        {
            get => _objects;
        }
        public event EventHandler OnUpdate
        {
            add => _updateEvent += value;
            remove => _updateEvent -= value;
        }
        public int Interval
        {
            get => 10;
        }

        private bool IsMovingTowards(PhysicalObject obj1, PhysicalObject obj2)
        {
            double distance = obj2.GetPosition() - obj1.GetPosition();
            double relativeVelocity = obj1.GetVelocity() - obj2.GetVelocity();
            int movementDiretion = Math.Abs(distance) > _epsilon ? Math.Sign(distance) : 0;
            double velocityDiretion = Math.Abs(relativeVelocity) > _epsilon ? Math.Sign(relativeVelocity) : 0;

            return movementDiretion == velocityDiretion;
        }
        private double ComputeCollisionTime(PhysicalObject obj1, PhysicalObject obj2)
        {
            double resultVelocity = Math.Abs(obj1.GetVelocity() - obj2.GetVelocity());
            double time = obj1.Distance(obj2) / resultVelocity;

            return time;
        }
        private Collision? GetNearestCollision(IReadOnlyList<PhysicalObject> objects, double timeDelta)
        {
            Collision? collision = null;

            for (int index1 = 0; index1 < objects.Count; index1++)
            {
                var object1 = objects[index1];

                for (int index2 = 0; index2 < objects.Count; index2++)
                {
                    var object2 = objects[index2];

                    var towards = IsMovingTowards(object1, object2);

                    if (index1 != index2 && towards)
                    {
                        var time = ComputeCollisionTime(object1, object2);

                        if (time < timeDelta && (!collision.HasValue || (time < collision.Value.Time)))
                        {
                            collision = new Collision(object1, object2, time);
                        }
                    }
                }
            }

            return collision;
        }
        private void UpdatePositions(IReadOnlyList<PhysicalObject> processedObjects, double timeDelta)
        {
            foreach (var obj in _objects)
            {
                if (obj is Block && !processedObjects.Contains(obj))
                {
                    var block = (Block)obj;

                    lock (_syncObj)
                    {
                        block.SetPosition(block.GetPosition(timeDelta));
                    }
                }
            }
        }
        private CollisionResponse ComputeCollision(Collision collision, CollisionTarget target)
        {
            return (int)target switch
            {
                0 => new CollisionResponse(collision.Object1.GetPosition(collision.Time), collision.Object1.ProcessCollision(collision.Object2)),
                1 => new CollisionResponse(collision.Object2.GetPosition(collision.Time), collision.Object2.ProcessCollision(collision.Object1)),
                _ => throw new NotImplementedException()
            };
        }
        private List<PhysicalObject> ProcessCollisions(double timeDelta)
        {
            List<PhysicalObject> processedObjects = new();
            bool computed = false;

            Action<PhysicalObject, CollisionResponse> Respond = (PhysicalObject obj, CollisionResponse response) =>
            {
                if (obj is Block)
                {
                    lock (_syncObj)
                    {
                        ((Block)obj).SetPosition(response.Position);
                        ((Block)obj).SetVelocity(response.Velocity);
                    }
                }
            };

            for (; !computed;)
            {
                var nearestCollision = this.GetNearestCollision(_objects, timeDelta);

                if (nearestCollision.HasValue)
                {
                    var properties1 = ComputeCollision(nearestCollision.Value, CollisionTarget.First);
                    var properties2 = ComputeCollision(nearestCollision.Value, CollisionTarget.Second);

                    Respond(nearestCollision.Value.Object1, properties1);
                    Respond(nearestCollision.Value.Object2, properties2);

                    processedObjects.AddRange(new[] { nearestCollision.Value.Object1, nearestCollision.Value.Object2 });
                }
                else
                {
                    computed = true;
                }
            }

            return processedObjects;
        }

        public void Update()
        {
            var timeDelta = (double)Interval / _timeUnit;

            var processedObjects = ProcessCollisions(timeDelta);

            UpdatePositions(processedObjects, timeDelta);

            _updateEvent?.Invoke(this, EventArgs.Empty);
        }

        public PhysicalEngine(object syncObj, params PhysicalObject[] objects)
        {
            _objects = new(objects);
            _syncObj = syncObj;
        }
    }

    class VisualObject
    {
        private object _value;

        public object Value
        {
            get => _value;
        }

        public VisualObject(PhysicalObject block)
        {
            _value = block;
        }
        public VisualObject(HorizontalAxis axis)
        {
            _value = axis;
        }
    }

    abstract class VisualEngine
    {
        public abstract void Zoom();
        public abstract void Move(Vector? value);
        public abstract void Refresh();
        public abstract void Set(VisualObject value);
    }

    class VisualEngine3D : VisualEngine
    {
        private object _syncObj;
        private Dictionary<VisualObject, ModelVisual3D> _scene;
        private Viewport3D _viewport3D;
        private CameraController _cameraController;
        private bool _scaled;

        private readonly Material _blockMaterial;
        private readonly Material _wallMaterial;
        private readonly Material _gridMaterial;
        private readonly Size3D _wallSize = new Size3D(1, 4, 4);

        private double _scalar
        {
            get => 1;
        }
        private double _defaultMultiplier
        {
            get => 1.1;
        }
        private double _thickness
        {
            get => 0.02;
        }
        private double _gridSize
        {
            get => 0.5;
        }

        public CameraController CameraController
        {
            get => _cameraController;
        }

        private ModelVisual3D GetModelVisual3D(MeshGeometry3D mesh, Material material)
        {
            return new ModelVisual3D() { Content = new GeometryModel3D(mesh, material) };
        }
        private ModelVisual3D GetModelVisual3D(Material material, params MeshGeometry3D[] meshes)
        {
            var collection = new Model3DGroup();

            foreach (var mesh in meshes)
            {
                collection.Children.Add(new GeometryModel3D(mesh, material));
            }

            return new ModelVisual3D() { Content = collection };
        }

        private MeshGeometry3D GetBox(Size3D size, Point3D position = new Point3D())
        {
            return new MeshGeometry3D()
            {
                Positions =
                {
                    new Point3D(position.X, position.Y, position.Z),
                    new Point3D(size.X + position.X, position.Y, position.Z),
                    new Point3D(position.X, size.Y + position.Y, position.Z),
                    new Point3D(size.X + position.X, size.Y + position.Y, position.Z),
                    new Point3D(position.X, position.Y, size.Z + position.Z),
                    new Point3D(size.X + position.X, position.Y, size.Z + position.Z),
                    new Point3D(position.X, size.Y + position.Y, size.Z + position.Z),
                    new Point3D(size.X + position.X, size.Y + position.Y, size.Z + position.Z),
                },
                TriangleIndices =
                {
                    0,2,1, 1,2,3, 0,4,2, 2,4,6,
                    0,1,4, 1,5,4, 1,7,5, 1,3,7,
                    4,5,6, 7,6,5, 2,6,3, 3,6,7
                }
            };
        }
        private IEnumerable<MeshGeometry3D> GetHorizontalPlane(double size, double yOffset)
        {
            for (double offset = -size / 2; offset < size / 2; offset += _gridSize)
            {
                yield return GetBox(new Size3D(_thickness, _thickness, size), new Point3D(offset - _thickness / 2, -_thickness + yOffset, -size / 2));
                yield return GetBox(new Size3D(size, _thickness, _thickness), new Point3D(-size / 2, -_thickness + yOffset, offset - _thickness / 2));
            }
        }

        private Transform3D GetBoxTransform(Block block)
        {
            var position = GetBlockPosition(block);

            return new TranslateTransform3D(position.X, position.Y, position.Z);
        }

        private Size3D GetBoxSize(Block block)
        {
            return new Size3D(block.Size * _scalar, block.Size * _scalar, block.Size * _scalar);
        }

        private MaterialGroup GetEmissiveMaterial(Brush brush)
        {
            return new MaterialGroup()
            {
                Children = {
                    new DiffuseMaterial(brush),
                    new EmissiveMaterial(brush),
                }
            };
        }

        private ModelVisual3D GetModel3D(VisualObject obj)
        {
            return obj.Value switch
            {
                Block block => GetModelVisual3D(GetBox(GetBoxSize(block)), _blockMaterial),
                Wall wall => GetModelVisual3D(GetBox(_wallSize, GetWallPosition(wall)), _wallMaterial),
                HorizontalAxis axis => GetModelVisual3D(_gridMaterial, GetHorizontalPlane(CameraController.Camera.FarPlaneDistance * 2, GetAxisPosition(axis).Y).ToArray()),
                _ => throw new NotImplementedException("Unknown visual element type")
            };
        }

        public override void Zoom()
        {
            Zoom(_scaled ? 1 / _defaultMultiplier : _defaultMultiplier);

            _scaled = !_scaled;
        }
        public override void Move(Vector? value)
        {
            throw new NotImplementedException();
        }

        public override void Refresh()
        {
            List<KeyValuePair<ModelVisual3D, Block>> _sceneCopy = new();
            lock (_syncObj)
            {
                foreach (var entry in _scene)
                {
                    var block = entry.Key.Value as Block;

                    if (block != null)
                    {
                        _sceneCopy.Add(new(entry.Value, block));
                    }
                }
            }

            foreach (var entry in _sceneCopy)
            {
                entry.Key.Transform = GetBoxTransform(entry.Value);
            }
        }
        public override void Set(VisualObject obj)
        {
            var model = GetModel3D(obj);

            if (_scene.ContainsKey(obj))
            {
                _scene[obj] = model;
            }
            else
            {
                _scene.Add(obj, model);
            }

            _viewport3D.Children.Add(model);
        }

        public void Zoom(double value)
        {
            _cameraController.Scale(value);
        }

        public Point3D GetBlockPosition(Block block)
        {
            return new Point3D(block.GetPosition() * _scalar, 0, -block.Size * _scalar / 2);
        }
        public Point3D GetWallPosition(Wall wall)
        {
            return new Point3D(wall.GetPosition() * _scalar - _wallSize.X, 0, -_wallSize.Z / 2);
        }
        public Point3D GetAxisPosition(HorizontalAxis axis)
        {
            return new Point3D(0, axis.Y * _scalar, 0);
        }

        public VisualEngine3D(object syncObj, Viewport3D viewport3D, CameraController cameraController)
        {
            _scene = new();
            _viewport3D = viewport3D;
            _blockMaterial = GetEmissiveMaterial(Brushes.DodgerBlue);
            _wallMaterial = new DiffuseMaterial(Brushes.OrangeRed);
            _gridMaterial = GetEmissiveMaterial(Brushes.White);
            _cameraController = cameraController;
            _syncObj = syncObj;
        }
    }

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

    public partial class MainWindow : Window
    {
        Point? lastMouseDown;
        CameraController cameraController;

        private void SetCameraProperties(CameraController controller, PerspectiveCamera camera)
        {
            camera.Transform = controller.Transform;
        }

        public MainWindow()
        {
            InitializeComponent();

            Block firstBlock = new Block(1, 1, 0, 2);

            cameraController = new CameraController(camera, new Vector(20, 200));

            cameraController.MinAngle = new Vector(0, 0);
            cameraController.MaxAngle = new Vector(90, 360);

            var sync = new object();

            VisualEngine3D visualEngine = new VisualEngine3D(sync, viewport, cameraController);
            PhysicalEngine physicalEngine = new PhysicalEngine(sync, firstBlock, new Block(1.5, 100.0 * 100 * 100 * 100 * 100, -2, 8), new Wall(0));
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);

            foreach (var obj in physicalEngine.Objects)
            {
                visualEngine.Set(new VisualObject(obj));
            }

            visualEngine.Set(new VisualObject(new HorizontalAxis(0)));

            timer.Interval = TimeSpan.FromMilliseconds(physicalEngine.Interval);

            var task = Task.Run(() =>
            {
                for (; ; )
                {
                    physicalEngine.Update();

                    Thread.Sleep(physicalEngine.Interval);
                }
            });

            timer.Tick += (object? sender, EventArgs args) =>
            {
                visualEngine.Refresh();

                collisions.Text = $"COLLISIONS: {firstBlock.Collisions}";

                SetCameraProperties(cameraController, camera);
            };

            timer.Start();

            SetCameraProperties(cameraController, camera);
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            if (lastMouseDown.HasValue && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var size = new Size(ActualWidth, ActualHeight);
                var movement = new Vector(position.X - lastMouseDown.Value.X, position.Y - lastMouseDown.Value.Y);

                cameraController.AddMovement(movement, size);

                SetCameraProperties(cameraController, camera);
            }

            lastMouseDown = position;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            lastMouseDown = e.GetPosition(this);
        }

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            cameraController.Scale(-0.1 * Math.Sign(e.Delta) + 1);
        }
    }
}
