using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;
using System.Windows.Controls;

namespace CSharpPICollision
{
    public partial class MainWindow : Window
    {
        private Point? lastMouseDown;
        private CancellationTokenSource cancelPhysicalEngineTask;
        private DispatcherTimer dispatcherTimer;
        private CameraController cameraController;

        private EventHandler UpdateView(IVisualEngine visualEngine3D, Block firstBlock, TextBlock collisions)
        {
            var closure = firstBlock;

            return (object? sender, EventArgs args) =>
            {
                visualEngine3D.Refresh();

                collisions.Text = $"COLLISIONS: {closure.Collisions}";
            };
        }

        private void SetCameraProperties(CameraController controller)
        {
            controller.Camera.Transform = controller.Transform;
        }

        private (decimal Mass, decimal Speed)? ShowPropertiesEditDialog()
        {
            var dialog = new PropertiesEditDialog();
            var result = dialog.ShowDialog();

            return (result.HasValue && result.Value) ? dialog.Properties : null;
        }

        private PerspectiveCamera InitializeCamera()
        {
            return new PerspectiveCamera() { Position = new Point3D(0, 0, -20), LookDirection = new Vector3D(0, 0, 1), FarPlaneDistance = 75 };
        }

        private CameraController InitializeCameraController(PerspectiveCamera camera)
        {
            var cameraController = new CameraController(camera, new Vector(20, 200));

            cameraController.MinAngle = new Vector(0, double.NegativeInfinity);
            cameraController.MaxAngle = new Vector(90, double.PositiveInfinity);

            return cameraController;
        }

        private VisualEngine3D InitializeVisualEngine3D(object sync, IPhysicalEngine physicalEngine, CameraController cameraController, Viewport3D viewport)
        {
            VisualEngine3D visualEngine = new VisualEngine3D(sync, viewport, cameraController);

            foreach (var obj in physicalEngine.Objects)
            {
                visualEngine.Set(new VisualObject(obj));
            }

            visualEngine.Set(new VisualObject(new HorizontalAxis(0)));
            visualEngine.Set(new VisualObject(new DirectionalLight(Colors.White, new Vector3D(-1, 0, 0))));
            visualEngine.Set(new VisualObject(new DirectionalLight(Colors.White, new Vector3D(0, -1, 0))));

            return visualEngine;
        }

        private DispatcherTimer InitializeTimer(IPhysicalEngine physicalEngine, CancellationToken cancellationToken, EventHandler handler)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);

            timer.Interval = TimeSpan.FromMilliseconds(physicalEngine.Interval);

            var task = Task.Run(async () =>
            {
                for (; !cancellationToken.IsCancellationRequested;)
                {
                    physicalEngine.Update();

                    await Task.Delay(physicalEngine.Interval);
                }
            });

            timer.Tick += handler;

            return timer;
        }

        private (Block, Block) InitializeBlocks(decimal mass, decimal speed)
        {
            return (new Block(1, 1, 0, 2), new Block(1.5m, mass, speed, 6));
        }

        private (IPhysicalEngine physicalEngine, IVisualEngine visualEngine) InitializeEngines(CameraController cameraController, Viewport3D viewport, (Block, Block) blocks)
        {
            var sync = new object();

            var physicalEngine = new PhysicalEngine(sync, blocks.Item1, blocks.Item2, new Wall(0));
            var visualEngine = InitializeVisualEngine3D(sync, physicalEngine, cameraController, viewport);

            return (physicalEngine, visualEngine);
        }

        private void Start(IPhysicalEngine engine, DispatcherTimer timer)
        {
            engine.ResetTime();
            timer.Start();
        }

        private void Stop()
        {
            cancelPhysicalEngineTask.Cancel();
            dispatcherTimer.Stop();
        }

        public MainWindow(decimal mass, decimal speed)
        {
            InitializeComponent();

            cancelPhysicalEngineTask = new CancellationTokenSource();

            var camera = InitializeCamera();

            cameraController = InitializeCameraController(camera);
            viewport.Camera = camera;
            SetCameraProperties(cameraController);

            var blocks = InitializeBlocks(mass, speed);
            var engines = InitializeEngines(cameraController, viewport, blocks);
            var updateView = UpdateView(engines.visualEngine, blocks.Item1, collisions);

            dispatcherTimer = InitializeTimer(engines.physicalEngine, cancelPhysicalEngineTask.Token, updateView);

            Start(engines.physicalEngine, dispatcherTimer);
        }

        public MainWindow() : this(1e6m, -2)
        {

        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            if (lastMouseDown.HasValue && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var size = new Size(ActualWidth, ActualHeight);
                var movement = new Vector(position.X - lastMouseDown.Value.X, position.Y - lastMouseDown.Value.Y);

                cameraController.AddMovement(movement, size);

                SetCameraProperties(cameraController);
            }

            lastMouseDown = position;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            lastMouseDown = e.GetPosition(this);
        }

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            cameraController.Scale(-0.1m * Math.Sign(e.Delta) + 1);

            SetCameraProperties(cameraController);
        }

        private void EditProperties_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var newProperties = ShowPropertiesEditDialog();

            if (newProperties.HasValue)
            {
                var newWindow = new MainWindow(newProperties.Value.Mass, newProperties.Value.Speed);

                newWindow.Show();

                Close();
            }
        }
    }
}
