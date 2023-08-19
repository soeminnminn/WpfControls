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
    /// Interaction logic for GridControlPage.xaml
    /// </summary>
    public partial class GridControlPage : Page
    {
        #region Variables
        private List<GridDataRow> mGridData;
        #endregion

        public GridControlPage()
        {
            InitializeComponent();

            this.BuildData();
            this.gridControl.GridSource = new GridStorage(this.mGridData);
        }

        private void BuildData()
        {
            this.mGridData = new List<GridDataRow>();

            for (int i = 0; i < 300; i++)
            {
                this.mGridData.Add(new GridDataRow()
                {
                    Image = null, // Properties.Resources.favicon,
                    Text = string.Format("Text: {0}", i),
                    DropDownText = string.Format("DropDown: {0}", i),
                    DropDownListText = string.Format("DropDownList: {0}", i),
                    SpinValue = i + 1,
                    HyperLinkText = string.Format("HyperLink: {0}", i),
                    ButtonText = string.Format("Button: {0}", i)
                });
            }
        }
    }
}
