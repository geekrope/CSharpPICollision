namespace CSharpPICollision
{
    class VisualObject
    {
        private object _value;

        public object Value
        {
            get => _value;
        }

        public VisualObject(PhysicalObject block)
        {
            _value = block;
        }
        public VisualObject(HorizontalAxis axis)
        {
            _value = axis;
        }
    }
}
