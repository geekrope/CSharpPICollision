using System.Windows;

namespace CSharpPICollision
{
    interface IVisualEngine
    {
        /// <summary>
        /// Adjust zoom of scene
        /// </summary>
        public abstract void Zoom();
        /// <summary>
        /// Adjust movment of scene
        /// </summary>
        public abstract void Move(Vector? value);
        /// <summary>
        /// Refreshes the view
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Sets as the key and 3d model as the value
        /// </summary>
        /// <param name="value">VisualObject to be added or replaced</param>
        public abstract void Set(VisualObject value);
    }
}
