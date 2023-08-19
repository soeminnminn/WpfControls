using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfControlDemo.Models
{
    public class GridDataRow
    {
        #region Constructor
        public GridDataRow()
        {
            IsChecked = false;
            Image = null;
            TreeText = string.Empty;
            Text = string.Empty;
            DropDownText = string.Empty;
            DropDownListText = string.Empty;
            SpinValue = 0;
            HyperLinkText = string.Empty;
            ButtonText = string.Empty;
        }
        #endregion

        #region Properties
        public bool IsChecked { get; set; }

        public ImageSource Image { get; set; }

        public string TreeText { get; set; }

        public string Text { get; set; }

        public string DropDownText { get; set; }

        public string DropDownListText { get; set; }

        public int SpinValue { get; set; }

        public string HyperLinkText { get; set; }

        public string ButtonText { get; set; }
        #endregion
    }
}
