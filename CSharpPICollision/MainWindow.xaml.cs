using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace CSharpPICollision
{
    public partial class MainWindow : Window
    {
        Point? lastMouseDown;
        CameraController cameraController;

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

        private DispatcherTimer InitializeTimer(VisualEngine visualEngine, PhysicalEngine physicalEngine, Block firstBlock)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);

            timer.Interval = TimeSpan.FromMilliseconds(physicalEngine.Interval);

            var task = Task.Run(async () =>
            {
                for (; ; )
                {
                    physicalEngine.Update();

                    await Task.Delay(physicalEngine.Interval);
                }
            });

            timer.Tick += (object? sender, EventArgs args) =>
            {
                visualEngine.Refresh();

                collisions.Text = $"COLLISIONS: {firstBlock.Collisions}";

                SetCameraProperties(cameraController, camera);
            };

            return timer;
        }

        public MainWindow()
        {
            InitializeComponent();

            Block firstBlock = new Block(1, 1, 0, 2);

            cameraController = InitializeCameraController();

            var sync = new object();

            PhysicalEngine physicalEngine = new PhysicalEngine(sync, firstBlock, new Block(1.5, 100.0 * 100 * 100 * 100 * 100, -2, 8), new Wall(0));
            VisualEngine3D visualEngine = InitializeVisualEngine3D(sync, physicalEngine);
            DispatcherTimer timer = InitializeTimer(visualEngine, physicalEngine, firstBlock);

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
