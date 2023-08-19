using System;
using System.ComponentModel;
using System.Windows.Media;

namespace WpfControlDemo.Models
{
    internal class ColorModel: INotifyPropertyChanged
    {
        /// <summary>
        /// The color 1.
        /// </summary>
        private Color color1;

        /// <summary>
        /// The color 2.
        /// </summary>
        private Color color2;

        /// <summary>
        /// The color 3.
        /// </summary>
        private Color color3;

        /// <summary>
        /// The color 4.
        /// </summary>
        private Color? color4;

        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the color 1.
        /// </summary>
        public Color Color1
        {
            get
            {
                return this.color1;
            }

            set
            {
                this.color1 = value;
                this.RaisePropertyChanged("Color1");
            }
        }

        /// <summary>
        /// Gets or sets the color 2.
        /// </summary>
        public Color Color2
        {
            get
            {
                return this.color2;
            }

            set
            {
                this.color2 = value;
                this.RaisePropertyChanged("Color2");
            }
        }

        /// <summary>
        /// Gets or sets the color 3.
        /// </summary>
        public Color Color3
        {
            get
            {
                return this.color3;
            }

            set
            {
                this.color3 = value;
                this.RaisePropertyChanged("Color3");
            }
        }

        /// <summary>
        /// Gets or sets the color 4.
        /// </summary>
        public Color? Color4
        {
            get
            {
                return this.color4;
            }

            set
            {
                this.color4 = value;
                this.RaisePropertyChanged("Color4");
            }
        }

        /// <summary>
        /// Gets or sets the brush.
        /// </summary>
        public SolidColorBrush Brush { get; set; }

        public ColorModel()
        {
            this.Color1 = Color.FromArgb(80, 255, 0, 0);
            this.Color2 = ColorPicker.ColorHelper.UndefinedColor;
            this.Color3 = Colors.Aqua;
            this.Color4 = null;
            this.Brush = Brushes.Blue;
        }

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="property">The property.</param>
        protected void RaisePropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
