using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace wpfDialogs
{
    public class ColorDialogNative : CommonDialog
    {
        #region Variables
        private int options;
        private int[] customColors;

        private Color color;
        #endregion

        #region Constructor
        public ColorDialogNative()
        {
            customColors = new int[16];
        }
        #endregion

        #region Properties
        public virtual bool AllowFullOpen
        {
            get => !GetOption(CC_PREVENTFULLOPEN);
            set { SetOption(CC_PREVENTFULLOPEN, !value); }
        }

        public virtual bool AnyColor
        {
            get => GetOption(CC_ANYCOLOR);
            set { SetOption(CC_ANYCOLOR, value); }
        }

        public Color Color
        {
            get => color;
            set
            {
                if (!value.IsEmpty())
                {
                    color = value;
                }
                else
                {
                    color = Colors.Black;
                }
            }
        }

        public int[] CustomColors
        {
            get => (int[]) customColors.Clone();
            set
            {
                int length = value == null ? 0 : Math.Min(value.Length, 16);
                if (length > 0) Array.Copy(value, 0, customColors, 0, length);
                for (int i = length; i < 16; i++) customColors[i] = 0x00FFFFFF;
            }
        }

        public virtual bool FullOpen
        {
            get => GetOption(CC_FULLOPEN);
            set { SetOption(CC_FULLOPEN, value); }
        }

        public virtual bool ShowHelp
        {
            get => GetOption(CC_SHOWHELP);
            set { SetOption(CC_SHOWHELP, value); }
        }

        public virtual bool SolidColorOnly
        {
            get => GetOption(CC_SOLIDCOLOR);
            set { SetOption(CC_SOLIDCOLOR, value); }
        }

        protected virtual IntPtr Instance
        {
            get
            {
                IntPtr instance = GetModuleHandle(null);
                return instance;
            }
        }

        protected virtual int Options
        {
            get => options;
        }
        #endregion

        #region Methods
        private bool GetOption(int option)
        {
            return (options & option) != 0;
        }

        private void SetOption(int option, bool value)
        {
            if (value)
            {
                options |= option;
            }
            else
            {
                options &= ~option;
            }
        }

        public override void Reset()
        {
            options = 0;
            color = Colors.Black;
            CustomColors = null;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            WndProc hookProcPtr = new WndProc(HookProc);
            CHOOSECOLOR cc = new CHOOSECOLOR();
            IntPtr custColorPtr = Marshal.AllocCoTaskMem(64);

            bool result = false;
            try
            {
                Marshal.Copy(customColors, 0, custColorPtr, 16);

                cc.hwndOwner = hwndOwner;
                cc.hInstance = Instance;
                cc.rgbResult = ColorExtensions.ToWin32(color);
                cc.lpCustColors = custColorPtr;

                int flags = Options | CC_RGBINIT | CC_ENABLEHOOK;
                // Our docs say AllowFullOpen takes precedence over FullOpen; ChooseColor implements the opposite
                if (!AllowFullOpen)
                    flags &= ~CC_FULLOPEN;
                cc.Flags = flags;

                cc.lpfnHook = hookProcPtr;

                result = ChooseColor(cc);
                if (result == true)
                {
                    var win32color = Color.ToWin32();
                    if (cc.rgbResult != win32color)
                    {
                        color = ColorExtensions.FromWin32(win32color);
                    }

                    Marshal.Copy(custColorPtr, customColors, 0, 16);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(custColorPtr);
            }

            return result;
        }

        public override string ToString()
        {
            string s = base.ToString();
            return s + ",  Color: " + Color.ToString();
        }
        #endregion

        #region Native Methods
        protected const int CC_ANYCOLOR = 0x100;
        protected const int CC_ENABLEHOOK = 0x10;
        protected const int CC_FULLOPEN = 2;
        protected const int CC_PREVENTFULLOPEN = 4;
        protected const int CC_RGBINIT = 1;
        protected const int CC_SHOWHELP = 8;
        protected const int CC_SOLIDCOLOR = 0x80;

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern bool ChooseColor([In, Out] CHOOSECOLOR cc);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        protected static extern IntPtr GetModuleHandle(string modName);
        #endregion

        #region Nested Types
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        protected class CHOOSECOLOR
        {
            public int lStructSize = Marshal.SizeOf(typeof(CHOOSECOLOR));
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public int rgbResult;
            public IntPtr lpCustColors;
            public int Flags;
            public IntPtr lCustData = IntPtr.Zero;
            public WndProc lpfnHook;
            public string lpTemplateName;
        }
        #endregion
    }
}
