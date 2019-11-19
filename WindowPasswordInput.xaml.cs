using System;
using System.Collections.Generic;
using System.Linq;
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

namespace fftWavEncryption
{
    /// <summary>
    /// Interaction logic for WindowPasswordInput.xaml
    /// </summary>
    public partial class WindowPasswordInput : Window
    {
        public enum ProcessType
        {
            ENCRYPTION = 0,
            DECRYPTION
        }

        private bool okPressed;

        public WindowPasswordInput(ProcessType pt, String originalName)
        {
            InitializeComponent();

            okPressed = false;
            textBoxName.Text = originalName + (pt == ProcessType.ENCRYPTION ? " (encrypted)" : " (decrypted)");
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxName.Text))
            {
                MessageBox.Show("Name cannot be empty!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (String.IsNullOrEmpty(passwordBoxPassword.Password))
            {
                MessageBox.Show("Password cannot be empty!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (passwordBoxPassword.Password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.okPressed = true;
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.okPressed == false)
            {
                this.DialogResult = false;
            }
        }

        public String GetName()
        {
            return this.textBoxName.Text;
        }

        public String GetPassword()
        {
            return this.passwordBoxPassword.Password;
        }
    }
}
