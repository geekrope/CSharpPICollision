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
        private bool running
        {
            get => dispatcherTimer.IsEnabled;
        }

        private void Start()
        {
            physicalEngine.ResetTime();
            dispatcherTimer.Start();
        }
        private void Stop()
        {
            dispatcherTimer.Stop();
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

        private VisualEngine3D InitializeVisualEngine3D(object sync, PhysicalEngine physicalEngine, CameraController cameraController)
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

        private (PhysicalEngine physicalEngine, VisualEngine3D visualEngine) InitScene(CameraController cameraController, double mass, double speed)
        {
            blocks.Item1 = new Block(1, 1, 0, 2);
            blocks.Item2 = new Block(1.5, mass, speed, 6);

            var sync = new object();

            physicalEngine = new PhysicalEngine(sync, blocks.Item1, blocks.Item2, new Wall(0));
            visualEngine = InitializeVisualEngine3D(sync, physicalEngine, cameraController);

            return (physicalEngine, visualEngine);
        }

        public MainWindow(double mass = 1e12, double speed = -2)
        {
            InitializeComponent();

            cameraController = InitializeCameraController();

            var scene = InitScene(cameraController, mass, speed);

            var updateView = UpdateView(blocks.Item1);

            physicalEngine = scene.physicalEngine;
            visualEngine = scene.visualEngine;

            dispatcherTimer = InitializeTimer(visualEngine, physicalEngine, updateView);

            SetCameraProperties(cameraController, camera);

            Start();
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
    }
}
