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
    /// Interaction logic for ObjectTreePage.xaml
    /// </summary>
    public partial class ObjectTreePage : Page
    {
        public ObjectTreePage()
        {
            InitializeComponent();

            ObjectTree.Source = TextDisplay;
        }

        private void ObjectTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ObjectTree.SelectedNode != null)
            {
                var node = ObjectTree.SelectedNode;
                TextDisplay.Text = string.Format("{0}\n{1}\n{2}\n{3}", node.Name,
                    node.Path, node.Category, node.Description);
            }
        }
    }
}
