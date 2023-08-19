using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace wpfDialogs
{
    public abstract class CommonDialog
    {
        #region Variables
        protected const int WM_INITDIALOG = 0x110;
        protected const int WM_USER = 0x400;
        protected const int WM_SETFOCUS = 7;

        protected const int CDM_SETDEFAULTFOCUS = WM_USER + 0x51;

        protected const int SWP_NOACTIVATE = 0x10;
        protected const int SWP_NOMOVE = 2;
        protected const int SWP_NOOWNERZORDER = 0x200;
        protected const int SWP_NOSIZE = 1;
        protected const int SWP_NOZORDER = 4;

        protected const int GWL_WNDPROC = -4;

        protected static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private IntPtr defOwnerWndProc;

        private IntPtr hookedWndProc;

        private IntPtr defaultControlHwnd;
        #endregion

        #region Native Methods
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        protected static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        protected static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        protected static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        protected static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        protected static extern IntPtr SetFocus(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        protected static extern IntPtr PostMessage(HandleRef hwnd, int msg, int wparam, int lparam);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        protected static extern IntPtr GetWindowLong32(HandleRef hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        protected static extern IntPtr GetWindowLongPtr64(HandleRef hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        protected static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
        
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        protected static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, WndProc wndproc);
        
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        protected static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
        
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        protected static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, WndProc wndproc);
        #endregion

        #region Constructor
        public CommonDialog()
        {
        }
        #endregion

        #region Methods
        protected static IntPtr GetWindowLong(HandleRef hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        protected static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, WndProc wndproc)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, wndproc);
            }
            return SetWindowLongPtr64(hWnd, nIndex, wndproc);
        }

        protected static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, HandleRef dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        protected static void MoveToScreenCenter(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(new HandleRef(null, hWnd), ref rect);

            Rect workingArea = SystemParameters.WorkArea;

            int x = (int)(workingArea.X + (workingArea.Width - rect.right + rect.left) / 2);
            int y = (int)(workingArea.Y + (workingArea.Height - rect.bottom + rect.top) / 3);
            SetWindowPos(new HandleRef(null, hWnd), NullHandleRef, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        }

        protected virtual IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (msg == WM_INITDIALOG)
            {
                MoveToScreenCenter(hWnd);
                this.defaultControlHwnd = wparam;
                SetFocus(new HandleRef(null, wparam));
            }
            else if (msg == WM_SETFOCUS)
            {
                PostMessage(new HandleRef(null, hWnd), CDM_SETDEFAULTFOCUS, 0, 0);
            }
            else if (msg == CDM_SETDEFAULTFOCUS)
            {
                SetFocus(new HandleRef(this, defaultControlHwnd));
            }
            return IntPtr.Zero;
        }

        protected virtual IntPtr OwnerWndProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            return CallWindowProc(defOwnerWndProc, hWnd, msg, wparam, lparam);
        }

        public abstract void Reset();

        protected abstract bool RunDialog(IntPtr hwndOwner);

        public bool? ShowDialog()
        {
            return ShowDialog(Application.Current.MainWindow);
        }

        public bool? ShowDialog(Window owner)
        {
            var handle = new WindowInteropHelper(owner).Handle;
            return ShowDialog(handle);
        }

        public bool? ShowDialog(IntPtr hwndOwner)
        {
            bool? result = null;

            if (hwndOwner == IntPtr.Zero)
            {
                hwndOwner = Process.GetCurrentProcess().MainWindowHandle;
            }

            if (hwndOwner == IntPtr.Zero)
            {
                hwndOwner = GetActiveWindow();
            }

            if (hwndOwner == IntPtr.Zero)
            {
                throw new ArgumentException();
            }

            WndProc ownerProc = new WndProc(OwnerWndProc);
            hookedWndProc = Marshal.GetFunctionPointerForDelegate(ownerProc);

            try
            {
                //UnsafeNativeMethods.[Get|Set]WindowLong is smart enough to call SetWindowLongPtr on 64-bit OS
                defOwnerWndProc = SetWindowLong(new HandleRef(this, hwndOwner), GWL_WNDPROC, ownerProc);

                try
                {
                    result = RunDialog(hwndOwner);
                }
                catch (Exception)
                { }
            }
            finally
            {
                IntPtr currentSubClass = GetWindowLong(new HandleRef(this, hwndOwner), GWL_WNDPROC);
                if (IntPtr.Zero != defOwnerWndProc || currentSubClass != hookedWndProc)
                {
                    SetWindowLong(new HandleRef(this, hwndOwner), GWL_WNDPROC, new HandleRef(this, defOwnerWndProc));
                }

                defOwnerWndProc = IntPtr.Zero;
                hookedWndProc = IntPtr.Zero;
                //Ensure that the subclass delegate will not be GC collected until after it has been subclassed
                GC.KeepAlive(ownerProc);
            }

            return result;
        }
        #endregion

        #region Nested Types
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(Rect r)
            {
                this.left = (int)r.Left;
                this.top = (int)r.Top;
                this.right = (int)r.Right;
                this.bottom = (int)r.Bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }

            public Size Size
            {
                get
                {
                    return new Size(this.right - this.left, this.bottom - this.top);
                }
            }
        }
        #endregion
    }
}
