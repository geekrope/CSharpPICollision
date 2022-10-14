using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        private void OpenSimulation()
        {
            MainWindow window;

            if (_mass.HasValue && _speed.HasValue)
            {
                window = new MainWindow(_mass.Value, _speed.Value);
            }
            else
            {
                window = new MainWindow();
            }

            window.Show();
        }
        public PropertiesEditDialog()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (_mass.HasValue && _speed.HasValue)
            {


                Close();
            }
            else
            {
                MessageBox.Show("Fill in correct values", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OpenSimulation();
        }
    }
}
