using System.Windows;

namespace CSharpPICollision
{
    abstract class VisualEngine
    {
        public abstract void Zoom();
        public abstract void Move(Vector? value);
        public abstract void Refresh();
        public abstract void Set(VisualObject value);
    }
}
