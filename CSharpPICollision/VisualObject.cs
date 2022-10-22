using System.Windows.Media.Media3D;

namespace CSharpPICollision
{
    public class VisualObject
    {
        /// <summary>
        /// Represents an object that is contained in current element
        /// </summary>
        private object _value;

        /// <summary>
        /// Represents an object that is contained in current element
        /// </summary>
        public object Value
        {
            get => _value;
        }

        /// <summary>
        /// Initializes new instance with light source
        /// </summary>
        /// <param name="light"></param>
        public VisualObject(Light light)
        {
            _value = light;
        }
        /// <summary>
        /// Initializes new instance with PhysicalObject
        /// </summary>
        /// <param name="block"></param>
        public VisualObject(PhysicalObject block)
        {
            _value = block;
        }
        /// <summary>
        /// Initializes new instance with HorizontalAxis
        /// </summary>
        /// <param name="axis"></param>
        public VisualObject(HorizontalAxis axis)
        {
            _value = axis;
        }
    }
}
