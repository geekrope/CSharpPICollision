using System.Threading;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;

using CSharpPICollision;

namespace Graph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancelPhysicalEngineTask;
        private DispatcherTimer dispatcherTimer;
        private DispatcherTimer InitializeTimer(IPhysicalEngine physicalEngine, CancellationToken cancellationToken, EventHandler handler)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
            timer.Interval = TimeSpan.FromMilliseconds(physicalEngine.Interval);
            timer.Tick += handler;

            return timer;
        }

        private EventHandler UpdateView(IPhysicalEngine physicalEngine, Block block)
        {
            return (object? sender, EventArgs args) =>
            {
                physicalEngine.Update();
                SetCollisionsCount(block);
            };
        }
        private void SetCollisionsCount(Block block)
        {
            collisions.Text = "Collsions: " + block.Collisions;
        }
        private (Block, Block) InitializeBlocks(decimal mass, decimal speed)
        {
            return (new Block(1, 1, 0, 2), new Block(1.5m, mass, speed, 6));
        }
        private (IPhysicalEngine physicalEngine, GraphEngine visualEngine) InitializeEngines(Canvas canvas, (Block, Block) blocks)
        {
            var sync = new object();

            var physicalEngine = new PhysicalEngine(sync, blocks.Item1, blocks.Item2, new Wall(0));
            var visualEngine = new GraphEngine(canvas, (blocks.Item2, blocks.Item1), sync);

            physicalEngine.OnCollisionProcessed += (object? sender, EventArgs args) => { visualEngine.Refresh(); };

            return (physicalEngine, visualEngine);
        }
        private (decimal Mass, decimal Velocity) GetPropeties()
        {
            var propetiesEditDialog = new PropertiesEditDialog();
            var result = propetiesEditDialog.ShowDialog();

            if (result.HasValue && result.Value && propetiesEditDialog.Properties.HasValue)
            {
                return propetiesEditDialog.Properties.Value;
            }
            else
            {
                return (100, -2);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            var properties = GetPropeties();
            var blocks = InitializeBlocks(properties.Mass, properties.Velocity);
            var engines = InitializeEngines(cnvs, blocks);

            cancelPhysicalEngineTask = new CancellationTokenSource();
            dispatcherTimer = InitializeTimer(engines.physicalEngine, cancelPhysicalEngineTask.Token, UpdateView(engines.physicalEngine, blocks.Item1));

            dispatcherTimer.Start();

            Title = $"Graph calculator, ratio = {blocks.Item1.Properties.Mass}/{blocks.Item2.Properties.Mass}";
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.P:
                        var printDialog = new PrintDialog();

                        if (printDialog.ShowDialog() == true)
                        {
                            printDialog.PrintVisual(cnvs, "Printing velocities graph");
                        }

                        break;
                }
            }
        }
    }
}
