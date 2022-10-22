using System.Threading.Tasks;
using System.Threading;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
        private (Block, Block) InitializeBlocks(double mass, double speed)
        {
            return (new Block(1, 1, 0, 2), new Block(1.5, mass, speed, 6));
        }
        private (IPhysicalEngine physicalEngine, GraphEngine visualEngine) InitializeEngines(Canvas canvas, (Block, Block) blocks)
        {
            var sync = new object();

            var physicalEngine = new PhysicalEngine(sync, blocks.Item1, blocks.Item2, new Wall(0));
            var visualEngine = new GraphEngine(canvas, (blocks.Item2, blocks.Item1), sync);

            return (physicalEngine, visualEngine);
        }

        public MainWindow()
        {
            InitializeComponent();

            var blocks = InitializeBlocks(100, -2);
            var engines = InitializeEngines(cnvs, blocks);

            cancelPhysicalEngineTask = new CancellationTokenSource();
            dispatcherTimer = InitializeTimer(engines.physicalEngine, cancelPhysicalEngineTask.Token, UpdateView(engines.physicalEngine,blocks.Item1));

            dispatcherTimer.Start();
        }
    }
}
