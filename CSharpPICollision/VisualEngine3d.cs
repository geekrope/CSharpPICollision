using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;

namespace CSharpPICollision
{
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
                Light light => new ModelVisual3D()
                {
                    Content = light
                },
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
        /// <summary>
        /// Initilizes new instance of VisualEngine3D
        /// </summary>
        /// <param name="syncObj">Object that sync drawing thread</param>
        /// <param name="viewport3D">Wpf control to which 3d models belong to</param>
        /// <param name="cameraController">Controler of scene camera</param>
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
}
