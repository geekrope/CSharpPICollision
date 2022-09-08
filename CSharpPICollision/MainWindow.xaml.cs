﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Documents;

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

    class Block : PhysicalObject
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
        private long _timeOffset;

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

                    block.SetPosition(block.GetPosition(timeDelta));
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
                    ((Block)obj).SetPosition(response.Position);
                    ((Block)obj).SetVelocity(response.Velocity);
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
            var timeDelta = (double)(Environment.TickCount64 - _timeOffset) / _timeUnit;

            var processedObjects = ProcessCollisions(timeDelta);

            UpdatePositions(processedObjects, timeDelta);

            ResetTime();

            _updateEvent?.Invoke(this, EventArgs.Empty);
        }
        public void ResetTime()
        {
            _timeOffset = Environment.TickCount64;
        }

        public PhysicalEngine(params PhysicalObject[] objects)
        {
            _objects = new(objects);

            ResetTime();
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
        public abstract void Zoom(Point? relativePoint);
        public abstract void Move(Point? value);
        public abstract void Refresh();
        public abstract void Set(VisualObject value);
    }

    class VisualEngine3D : VisualEngine
    {
        private Dictionary<VisualObject, ModelVisual3D> _scene;
        private Viewport3D _viewport3D;
        private CameraController _cameraController;
        private readonly DiffuseMaterial _blockMaterial;
        private readonly Size3D _wallSize = new Size3D(1, 10, 10);

        private double _scalar
        {
            get => 1;
        }

        public CameraController CameraController
        {
            get => _cameraController;
        }

        private ModelVisual3D GetModelVisual3D(MeshGeometry3D mesh, Material material)
        {
            return new ModelVisual3D() { Content = new GeometryModel3D(mesh, material) { BackMaterial = material } };
        }

        private MeshGeometry3D GetBox(Size3D size)
        {
            return new MeshGeometry3D()
            {
                Positions =
                {
                    new Point3D(0,0,0),
                    new Point3D(size.X, 0, 0),
                    new Point3D(0, size.Y, 0),
                    new Point3D(size.X, size.Y, 0),
                    new Point3D(0, 0, size.Z),
                    new Point3D(size.X, 0, size.Z),
                    new Point3D(0, size.Y, size.Z),
                    new Point3D(size.X, size.Y, size.Z),
                },
                TriangleIndices =
                {
                    0,2,1, 1,2,3, 0,4,2, 2,4,6,
                    0,1,4, 1,5,4, 1,7,5, 1,3,7,
                    4,5,6, 7,6,5, 2,6,3, 3,6,7
                }
            };
        }
        private MeshGeometry3D GetHorizontalPlane(double size, double yOffset)
        {
            return new MeshGeometry3D()
            {
                Positions =
                {
                    new Point3D(-size / 2, yOffset, -size / 2),
                    new Point3D(size / 2, yOffset, -size / 2),
                    new Point3D(size / 2, yOffset, size / 2),
                    new Point3D(-size / 2, yOffset, size / 2)
                },
                TriangleIndices =
                {
                    0, 1, 2, 2, 3, 0
                }
            };
        }

        private Transform3D GetBoxTransform(Block block)
        {
            var position = GetBlockPosition(block);

            return new TranslateTransform3D(position.X, position.Y, position.Z);
        }
        private Transform3D GetWallTransform(Wall wall)
        {
            var position = GetWallPosition(wall);

            return new TranslateTransform3D(position.X - _wallSize.X, position.Y - _wallSize.Y / 2, position.Z - _wallSize.Z/2);
        }

        private Size3D GetBoxSize(Block block)
        {
            return new Size3D(block.Size * _scalar, block.Size * _scalar, block.Size * _scalar);
        }

        private ModelVisual3D GetModel3D(VisualObject obj)
        {
            return obj.Value switch
            {
                Block block => GetModelVisual3D(GetBox(GetBoxSize(block)), _blockMaterial),
                Wall wall => GetModelVisual3D(GetBox(_wallSize), _blockMaterial),
                HorizontalAxis axis => GetModelVisual3D(GetHorizontalPlane(CameraController.Camera.FarPlaneDistance, GetAxisPosition(axis).Y), _blockMaterial),
                _ => throw new NotImplementedException("Unknown visual element type")
            };
        }

        public override void Zoom(Point? relativePoint)
        {
            throw new NotImplementedException();
        }
        public override void Move(Point? value)
        {
            throw new NotImplementedException();
        }

        public override void Refresh()
        {
            foreach (var entry in _scene)
            {
                switch (entry.Key.Value)
                {
                    case Block block:
                        entry.Value.Transform = GetBoxTransform(block);
                        break;
                    case Wall wall:
                        entry.Value.Transform = GetWallTransform(wall);
                        break;
                    case HorizontalAxis axis:
                    case CollisionsCount block:
                        break;
                }
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

        public Point3D GetBlockPosition(Block block)
        {
            return new Point3D(block.GetPosition() * _scalar, 0, -block.Size * _scalar / 2);
        }
        public Point3D GetWallPosition(Wall wall)
        {
            return new Point3D(wall.GetPosition() * _scalar, 0, 0);
        }
        public Point3D GetAxisPosition(HorizontalAxis axis)
        {
            return new Point3D(0, axis.Y * _scalar, 0);
        }

        public VisualEngine3D(Viewport3D viewport3D, CameraController cameraController)
        {
            _scene = new();
            _viewport3D = viewport3D;
            _blockMaterial = new(new SolidColorBrush(Colors.DodgerBlue));
            _cameraController = cameraController;
        }
    }

    class CameraController
    {
        private Vector _rotation;
        private PerspectiveCamera _camera;

        public Vector Rotation
        {
            get => _rotation;
        }
        public Transform3D Transform
        {
            get => GetRotationTransform(_rotation);
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

            return new Vector(angleX, angleY);
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

        public CameraController(PerspectiveCamera camera)
        {
            _camera = camera;
            _rotation = new Vector();
        }
        public CameraController(PerspectiveCamera camera, Vector initialRotation)
        {
            _camera = camera;
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

            cameraController.MinAngle = new Vector(0, 180);
            cameraController.MaxAngle = new Vector(90, 360);

            VisualEngine3D visualEngine = new VisualEngine3D(viewport, cameraController);
            PhysicalEngine physicalEngine = new PhysicalEngine(firstBlock, new Block(1.5, 100.0 * 100 * 100 * 100 * 100, -1, 8), new Wall(0));
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);

            foreach (var obj in physicalEngine.Objects)
            {
                visualEngine.Set(new VisualObject(obj));
            }

            timer.Interval = TimeSpan.FromMilliseconds(physicalEngine.Interval);

            timer.Tick += (object? sender, EventArgs args) =>
            {
                physicalEngine.Update();
                visualEngine.Refresh();

                SetCameraProperties(cameraController, camera);
            };

            timer.Start();

            SetCameraProperties(cameraController, camera);
        }

        private void viewport_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
    }
}