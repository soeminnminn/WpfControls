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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfControlDemo.Pages
{
    /// <summary>
    /// Interaction logic for DialogsPage.xaml
    /// </summary>
    public partial class DialogsPage : Page
    {
        public DialogsPage()
        {
            InitializeComponent();
        }

        private Window MainWindow
        {
            get => Application.Current.MainWindow;
        }

        private void ColorDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new wpfDialogs.ColorDialog();
            dialog.Color = SystemColors.ControlColor;

            if (dialog.ShowDialog(MainWindow) == true)
            {
                if (sender is Button btn)
                {
                    btn.Background = new SolidColorBrush(dialog.Color);
                }
            }
        }

        private void ColorDialogNative_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new wpfDialogs.ColorDialogNative();
            if (dialog.ShowDialog(MainWindow) == true)
            { }
        }

        private void FontDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new wpfDialogs.FontDialog();
            if (dialog.ShowDialog(MainWindow) == true)
            { }
        }

        private void FontDialogNative_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new wpfDialogs.FontDialogNative();
            dialog.ShowColor = true;

            if (dialog.ShowDialog(MainWindow) == true)
            { }
        }
    }
}
