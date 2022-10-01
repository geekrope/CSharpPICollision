using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;

namespace CSharpPICollision
{
    public partial class MainWindow : Window
    {
        private Point? lastMouseDown;
        private (Block, Block) blocks;
        private PhysicalEngine physicalEngine;
        private VisualEngine3D visualEngine;
        private DispatcherTimer dispatcherTimer;
        private CameraController cameraController;
        
        private Thickness playButtonMargin { get => new Thickness(0, 10, 0, 10); }
        private Brush playButtonBrush { get => Brushes.Black; }

        private Polygon startButton
        {
            get => new Polygon()
            {
                Points = new PointCollection(new Point[] { new Point(0, 0), new Point(start.Width, start.Height / 2), new Point(0, start.Height) }),
                Fill = playButtonBrush,
                Stroke = playButtonBrush,
                StrokeThickness = 5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Margin = playButtonMargin
            };
        }

        private Path stopButton
        {
            get => new Path()
            {
                Data = Geometry.Parse($"M 5,0 V {start.Height} M {start.Width - 5},0 V {start.Height}"),
                Stroke = playButtonBrush,
                StrokeThickness = 5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Margin = playButtonMargin
            };
        }

        private Path refreshButton
        {
            get => new Path()
            {
                Data = Geometry.Parse("M20,8 C18.5974037,5.04031171 15.536972,3 12,3 C7.02943725,3 3,7.02943725 3,12 C3,16.9705627 7.02943725,21 12,21 L12,21 C16.9705627,21 21,16.9705627 21,12 M21,3 L21,9 L15,9"),
                Fill = Brushes.Transparent,
                Stroke = playButtonBrush,
                StrokeThickness = 2,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Margin = playButtonMargin,
                RenderTransform = new ScaleTransform(start.Width / 24.0, start.Height / 24.0)
            };
        }

        private bool running
        {
            get => dispatcherTimer.IsEnabled;
        }

        private void Start()
        {
            physicalEngine.ResetTime();
            dispatcherTimer.Start();

            properties.IsEnabled = false;

            SetPlayButton();
        }
        private void Stop()
        {
            dispatcherTimer.Stop();

            properties.IsEnabled = true;

            SetPlayButton();
        }

        private void SetPlayButton()
        {
            start.Children.Clear();
            start.Children.Add(running ? stopButton : startButton);
        }

        private EventHandler UpdateView(Block firstBlock)
        {
            var closure = firstBlock;

            return (object? sender, EventArgs args) =>
            {
                visualEngine.Refresh();

                collisions.Text = $"COLLISIONS: {closure.Collisions}";
            };
        }

        private void SetCameraProperties(CameraController controller, PerspectiveCamera camera)
        {
            camera.Transform = controller.Transform;
        }

        private CameraController InitializeCameraController()
        {
            var cameraController = new CameraController(camera, new Vector(20, 200));

            cameraController.MinAngle = new Vector(0, 0);
            cameraController.MaxAngle = new Vector(90, 360);

            return cameraController;
        }

        private VisualEngine3D InitializeVisualEngine3D(object sync, PhysicalEngine physicalEngine)
        {
            VisualEngine3D visualEngine = new VisualEngine3D(sync, viewport, cameraController);

            foreach (var obj in physicalEngine.Objects)
            {
                visualEngine.Set(new VisualObject(obj));
            }

            visualEngine.Set(new VisualObject(new HorizontalAxis(0)));

            return visualEngine;
        }

        private DispatcherTimer InitializeTimer(VisualEngine visualEngine, PhysicalEngine physicalEngine, EventHandler handler)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);

            timer.Interval = TimeSpan.FromMilliseconds(physicalEngine.Interval);

            var task = Task.Run(async () =>
            {
                for (; ; )
                {
                    if (running)
                    {
                        physicalEngine.Update();

                        await Task.Delay(physicalEngine.Interval);
                    }
                }
            });

            timer.Tick += handler;

            return timer;
        }

        public MainWindow()
        {
            InitializeComponent();

            blocks.Item1 = new Block(1, 1, 0, 2);
            blocks.Item2 = new Block(1.5, Math.Pow(10, 6), -2, 6);

            cameraController = InitializeCameraController();

            var sync = new object();
            var updateView = UpdateView(blocks.Item1);

            physicalEngine = new PhysicalEngine(sync, blocks.Item1, blocks.Item2, new Wall(0));
            visualEngine = InitializeVisualEngine3D(sync, physicalEngine);
            dispatcherTimer = InitializeTimer(visualEngine, physicalEngine, updateView);

            Start();

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

            SetCameraProperties(cameraController, camera);
        }

        private void start_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (running)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }
    }
}
