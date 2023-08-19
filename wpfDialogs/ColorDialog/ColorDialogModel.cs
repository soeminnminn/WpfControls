using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace wpfDialogs
{
    internal class ColorDialogModel : INotifyPropertyChanged
    {
        #region Variables
        private static readonly RoutedCommand defineCustomCommand = new RoutedCommand("DefineCustom", typeof(ColorDialogWindow));
        private static readonly RoutedCommand addToCustomCommand = new RoutedCommand("AddToCustom", typeof(ColorDialogWindow));
        private static readonly RoutedCommand acceptCommand = new RoutedCommand("Accept", typeof(ColorDialogWindow));

        private static readonly CommandBinding defineCustomCommandBinding = new CommandBinding(defineCustomCommand,
                new ExecutedRoutedEventHandler(OnDefineCustomExecuted),
                new CanExecuteRoutedEventHandler(OnDefineCustomCanExecute));

        private static readonly CommandBinding addToCustomCommandBinding = new CommandBinding(addToCustomCommand,
                new ExecutedRoutedEventHandler(OnAddToCustomExecuted));

        private static readonly CommandBinding acceptCommandBinding = new CommandBinding(acceptCommand,
                new ExecutedRoutedEventHandler(OnAcceptExecuted));

        private static List<Color> _basicColors = null;

        private ColorDialogWindow _owner = null;

        private int _red = 0;
        private int _green = 0;
        private int _blue = 0;

        private double _hue = 0;
        private double _saturation = 0;
        private double _luminance = 0;

        private int _selectedBasicIndex = 0;
        private int _selectedCustomIndex = 0;

        private Color _paletteColor = Colors.Transparent;
        private Color _selectedColor = Colors.Transparent;

        private bool _showCustoms = false;

        private bool _isChanging = false;
        #endregion

        #region Events
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        private event PropertyChangedEventHandler _propertyChanged;
        #endregion

        #region Constructor
        static ColorDialogModel()
        {
            CustomColors = new ObservableCollection<Color>(Enumerable.Repeat(Colors.Transparent, 16));
        }

        internal ColorDialogModel()
        { }

        public ColorDialogModel(ColorDialogWindow owner)
            : this()
        {
            _owner = owner;

            if (!_owner.CommandBindings.Contains(defineCustomCommandBinding))
            {
                _owner.CommandBindings.Add(defineCustomCommandBinding);
            }
            if (!_owner.CommandBindings.Contains(addToCustomCommandBinding))
            {
                _owner.CommandBindings.Add(addToCustomCommandBinding);
            }
            if (!_owner.CommandBindings.Contains(acceptCommandBinding))
            {
                _owner.CommandBindings.Add(acceptCommandBinding);
            }
        }
        #endregion

        #region Properties
        public static List<Color> BasicColors
        {
            get
            {
                if (_basicColors == null)
                {
                    _basicColors = new List<Color>(ColorExtensions.BasicColors);
                }
                return _basicColors;
            }
        }

        public static ObservableCollection<Color> CustomColors { get; private set; }

        public int Red
        {
            get => _red;
            set { SetProperty(ref _red, value); }
        }

        public int Green
        {
            get => _green;
            set { SetProperty(ref _green, value); }
        }

        public int Blue
        {
            get => _blue;
            set { SetProperty(ref _blue, value); }
        }

        public double Hue
        {
            get => _hue;
            set { SetProperty(ref _hue, value); }
        }

        public double Saturation
        {
            get => _saturation;
            set { SetProperty(ref _saturation, value); }
        }

        public double Luminance
        {
            get => _luminance;
            set { SetProperty(ref _luminance, value); }
        }

        public int SelectedBasicIndex
        {
            get => _selectedBasicIndex;
            set { SetProperty(ref _selectedBasicIndex, value); }
        }

        public int SelectedCustomIndex
        {
            get => _selectedCustomIndex;
            set { SetProperty(ref _selectedCustomIndex, value); }
        }

        public Color PaletteColor
        {
            get => _paletteColor;
            set { SetProperty(ref _paletteColor, value); }
        }

        public Color SelectedColor
        {
            get => _selectedColor;
            set { SetProperty(ref _selectedColor, value); }
        }

        public bool ShowCustoms
        {
            get => _showCustoms;
            set { SetProperty(ref _showCustoms, value); }
        }

        public ICommand DefineCustomCommand
        {
            get => defineCustomCommand;
        }

        public ICommand AddToCustomCommand
        {
            get => addToCustomCommand;
        }

        public ICommand AcceptCommand
        {
            get => acceptCommand;
        }
        #endregion

        #region Methods
        private static void OnDefineCustomCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is ColorDialogWindow win)
            {
                win.OnDefineCustomCanExecute(e);
            }
        }

        private static void OnDefineCustomExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ColorDialogWindow win)
            {
                win.OnDefineCustomExecuted(e);
            }
        }

        private static void OnAddToCustomExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ColorDialogWindow win)
            {
                win.OnAddToCustomExecuted(e);
            }
        }

        private static void OnAcceptExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ColorDialogWindow win)
            {
                win.OnAcceptExecuted(e);
            }
        }

        public void SetCurrentColor(Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
        }

        private void SetProperty<T>(ref T backingStore, T value, [CallerMemberName]string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;
            backingStore = value;
            RaisePropertyChanged(propertyName);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            if (_propertyChanged != null)
            {
                _propertyChanged(this, args);
            }
            OnPropertyChanged(args);
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Red) || e.PropertyName == nameof(Green) || e.PropertyName == nameof(Blue))
            {
                OnRgbColorChanged();
            }
            else if (e.PropertyName == nameof(Hue) || e.PropertyName == nameof(Saturation) || e.PropertyName == nameof(Luminance))
            {
                OnHslColorChanged();
            }
            else if (e.PropertyName == nameof(SelectedBasicIndex))
            {
                OnSelectedBasicIndexChanged();
            }
            else if (e.PropertyName == nameof(SelectedCustomIndex))
            {
                OnSelectedCustomIndexChanged();
            }
        }

        private void OnRgbColorChanged()
        {
            if (!_isChanging)
            {
                _isChanging = true;

                var color = Color.FromRgb((byte)(_red & 0xFF), (byte)(_green & 0xFF), (byte)(_blue & 0xFF));

                Hue = color.GetHue();
                Saturation = color.GetSaturation() * 100.0;
                Luminance = color.GetBrightness() * 100.0;

                SelectedColor = color;

                _isChanging = false;
            }
        }

        private void OnHslColorChanged()
        {
            if (!_isChanging)
            {
                _isChanging = true;

                var color = ColorExtensions.FromHsb((float)_hue, (float)(_saturation / 100.0f), (float)(_luminance / 100.0f));

                Red = color.R;
                Green = color.G;
                Blue = color.B;

                SelectedColor = color;

                _isChanging = false;
            }
        }

        private void OnSelectedBasicIndexChanged()
        {
            if (_isChanging) return;

            int idx = SelectedBasicIndex;
            var color = BasicColors[idx];

            SelectedColor = color;
            SetCurrentColor(color);
        }

        private void OnSelectedCustomIndexChanged()
        {
            if (_isChanging) return;

            int idx = SelectedCustomIndex;
            var color = CustomColors[idx];
            if (color != Colors.Transparent)
            {
                SelectedColor = color;
                SetCurrentColor(color);
            }
        }

        public void OnAddToCustomExecuted()
        {
            int idx = SelectedCustomIndex;
            if (idx == -1)
            {
                for (int i = 0; i < CustomColors.Count; i++)
                {
                    if (CustomColors[i] == Colors.Transparent)
                    {
                        idx = i;
                        break;
                    }
                }
            }

            _isChanging = true;
            CustomColors[idx] = SelectedColor;
            _isChanging = false;
        }
        #endregion
    }
}
