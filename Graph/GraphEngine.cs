using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using CSharpPICollision;
using System.Windows.Markup;

namespace Graph
{
    internal class GraphEngine
    {
        private Canvas _canvas;
        private (Block x, Block y) _blocks;
        private Point _lastPoint;
        private object _sync;

        private Brush CircleColor
        {
            get => Brushes.Green;
        }
        private Brush AxesColor
        {
            get => Brushes.Green;
        }
        private Brush GraphColor
        {
            get => Brushes.Black;
        }

        private double Size
        {
            get => Math.Min(_canvas.Width - Padding * 2, _canvas.Height - Padding * 2);
        }
        private double KineticEnergy
        {
            get => GetKineticEnergy(_blocks.x, _blocks.y);
        }
        private double CircleThickness
        {
            get => 1;
        }
        private double AxesThickness
        {
            get => 1;
        }
        private double GraphThickness
        {
            get => 1;
        }

        private double Padding
        {
            get => 10;
        }
        private Point Center
        {
            get => new Point(_canvas.Width / 2, _canvas.Height / 2);
        }

        private Shape GetGraphCircle(Point center, double size)
        {
            var radiusX = size / 2;
            var radiusY = size / 2;
            var circle = new EllipseGeometry(center, radiusX, radiusY);

            return new Path() { Stroke = CircleColor, StrokeThickness = CircleThickness, Data = circle };
        }
        private Shape GetAxises(Point center, double padding, double size)
        {
            var xAxes = new LineSegment(new Point(size + padding, center.Y), true);
            var yAxes = new LineSegment(new Point(center.X, size + padding), true);
            var xFigure = new PathFigure(new Point(padding, center.Y), new PathSegment[] { xAxes }, false);
            var yFigure = new PathFigure(new Point(center.X, padding), new PathSegment[] { yAxes }, false);
            var axises = new PathFigureCollection(new PathFigure[] { xFigure, yFigure });

            return new Path() { Data = new PathGeometry(axises), Stroke = AxesColor, StrokeThickness = AxesThickness };
        }
        private Shape GetVelocityDifference(Point lastPoint, Point newPoint)
        {
            var lineEnd = new LineSegment(newPoint, true);
            var line = new PathFigure(lastPoint, new PathSegment[] { lineEnd }, false);

            return new Path() { Data = new PathGeometry(new PathFigure[] { line }), Stroke = GraphColor, StrokeThickness = GraphThickness };
        }

        private void InitializeView(Canvas canvas, Point center, double padding, double size)
        {
            canvas.Children.Add(GetGraphCircle(center, size));
            canvas.Children.Add(GetAxises(center, padding, size));
        }
        private double GetKineticEnergy(params Block[] blocks)
        {
            var energy = 0.0;

            foreach (var block in blocks)
            {
                energy += block.Properties.Mass * Math.Pow(block.GetVelocity(), 2) / 2;
            }

            return energy;
        }
        private double NormalizeBlockVelocity(Block block)
        {
            var maxVelocity = Math.Sqrt(KineticEnergy / block.Properties.Mass * 2);

            return block.GetVelocity() / maxVelocity * (Size / 2);
        }
        private Point GetCurrentPoint(Point center, (Block x, Block y) blocks)
        {
            return new Point(NormalizeBlockVelocity(blocks.x), -NormalizeBlockVelocity(blocks.y)) + ((Vector)center);
        }
        private void OnChanged(Block sender)
        {
            lock (_sync)
            {
                var newPoint = GetCurrentPoint(Center, _blocks);

                _canvas.Children.Add(GetVelocityDifference(_lastPoint, newPoint));

                _lastPoint = newPoint;
            }
        }

        public GraphEngine(Canvas canvas, (Block x, Block y) blocks, object sync)
        {
            _canvas = canvas;
            _blocks = blocks;
            _sync = sync;
            _lastPoint = GetCurrentPoint(Center, _blocks);

            _blocks.x.OnCollide += OnChanged;
            _blocks.y.OnCollide += OnChanged;

            InitializeView(_canvas, Center, Padding, Size);
        }
    }
}
