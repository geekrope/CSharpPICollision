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
        private Brush DeadZone
        {
            get => Brushes.Red;
        }
        private Brush DeadZoneFill
        {
            get => new SolidColorBrush(Color.FromArgb(127, 255, 0, 0));
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
        private double DeadZoneThickness
        {
            get => CircleThickness;
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
        private Shape GetDeadZone(double mass1, double mass2, double size, Point center)
        {
            var slope = Math.Sqrt(mass1 / mass2);
            var radius = size / 2;
            var intersectionAngle = Math.Atan(slope);
            var intersectionPoint = center + new Vector(Math.Cos(intersectionAngle), -Math.Sin(intersectionAngle)) * radius;
            var endPoint = center + new Vector(radius, 0);

            var segment1 = new LineSegment(center, true);

            var segment2 = new LineSegment(intersectionPoint, true);

            var arcEnd = new ArcSegment(endPoint, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true);
            var sector = new PathFigure(intersectionPoint, new PathSegment[] { arcEnd, segment1, segment2 }, false);

            return new Path() { Stroke = DeadZone, StrokeThickness = DeadZoneThickness, Fill = DeadZoneFill, Data = new PathGeometry(new PathFigure[] { sector }) };
        }

        private void InitializeView(Canvas canvas, (Block x, Block y) blocks, Point center, double padding, double size)
        {
            canvas.Children.Add(GetGraphCircle(center, size));
            canvas.Children.Add(GetAxises(center, padding, size));
            canvas.Children.Add(GetDeadZone((double)blocks.y.Properties.Mass, (double)blocks.x.Properties.Mass, size, center));
        }
        private double GetKineticEnergy(params Block[] blocks)
        {
            var energy = 0.0;

            foreach (var block in blocks)
            {
                energy += (double)block.Properties.Mass * Math.Pow((double)block.GetVelocity(), 2) / 2;
            }

            return energy;
        }
        private double NormalizeBlockVelocity(Block block)
        {
            var maxVelocity = Math.Sqrt(KineticEnergy / (double)block.Properties.Mass * 2);

            return (double)block.GetVelocity() / maxVelocity * (Size / 2);
        }
        private Point GetCurrentPoint(Point center, (Block x, Block y) blocks)
        {
            return new Point(NormalizeBlockVelocity(blocks.x), -NormalizeBlockVelocity(blocks.y)) + ((Vector)center);
        }
        public void Refresh()
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

            InitializeView(_canvas, _blocks, Center, Padding, Size);
        }
    }
}
