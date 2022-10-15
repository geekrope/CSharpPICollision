using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace CSharpPICollision
{
    public partial class PropertiesEditDialog : Window
    {
        private double? _mass;
        private double? _speed;

        private double? ValidateInput(TextBox input)
        {
            double result = 0;

            if (double.TryParse(input.Text, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        private void SetTextBoxStyle(TextBox input, bool valid)
        {
            input.BorderBrush = valid ? Brushes.Gray : Brushes.OrangeRed;
        }
        private void ReadValue(TextBox input, out double? value)
        {
            value = ValidateInput(input);

            SetTextBoxStyle(input, value.HasValue);
        }

        public (double Mass, double Speed)? Properties
        {
            get => (_mass.HasValue && _speed.HasValue) ? (_mass.Value, _speed.Value) : null;
        }
        public PropertiesEditDialog()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (_mass.HasValue && _speed.HasValue)
            {
                DialogResult = true;

                Close();
            }
            else
            {
                MessageBox.Show("Fill in correct values", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }

        private void mass_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReadValue(mass, out _mass);
        }
        private void speed_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReadValue(speed, out _speed);
        }
    }
}
