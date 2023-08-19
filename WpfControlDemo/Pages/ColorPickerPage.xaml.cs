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
using WpfControlDemo.Models;

namespace WpfControlDemo.Pages
{
    /// <summary>
    /// Interaction logic for ColorPickerPage.xaml
    /// </summary>
    public partial class ColorPickerPage : Page
    {
        private ColorModel model = new ColorModel();

        public ColorPickerPage()
        {
            InitializeComponent();

            this.DataContext = model;
        }
    }
}
