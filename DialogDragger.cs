using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace AhDung.WinForm
{
    //todo 颜色对话框有问题，禁止使用
    /// <summary>
    /// 对话框拖拽器
    /// </summary>
    [Obsolete("尚有严重问题，禁用", true)]
    internal static class DialogDragger
    {
        private static bool CanDrag(IntPtr hWnd)//, POINT pt)
        {
            var name = GetClassName(hWnd);
            switch (name)
            {
                case "SysTabControl32":
                case "msctls_progress32":
                case "msctls_statusbar32":
                    return true;

                case "Static":
                    return !HasStyle(hWnd, 0xB/*SS_SIMPLE*/);

                case "Button":
                    return HasStyle(hWnd, 7/*BS_GROUPBOX*/);
            }
            return false;
        }

        /// <summary>
        /// 钩子消息处理过程
        /// </summary>
        private static IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == 0/*MSGF_DIALOGBOX*/)
            {
                var m = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
                switch (m.Msg)
                {
                    case 0x201:
                        //todo 颜色对话框中的色块点击时得到的句柄是窗体本身，暂时没办法处理
                        if ((int)m.wParam != 1) { break; }
                        if (GetClassName(m.hWnd) == "ComboLBox") { break; }

                        var hForm = GetAncestor(m.hWnd, 2/*GA_ROOT*/);
                        if (m.hWnd == hForm || CanDrag(m.hWnd))
                        {
                            SendMessage(hForm, 0xA1, (IntPtr)2, IntPtr.Zero);
                            return (IntPtr)1;
                        }
                        break;
                }
            }
            return CallNextHookEx(_hHook, nCode, wParam, lParam);
        }


        static IntPtr _hHook;
        static HookProcDelegate _proc = HookProc;

        /// <summary>
        /// 安装钩子。不能重复安装
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="Win32Exception"/>
        private static void Install()
        {
            if (_hHook != IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }
            // ReSharper disable once CSharpWarnings::CS0618
            _hHook = SetWindowsHookEx(-1/*WH_MSGFILTER*/, _proc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
            if (_hHook == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// 卸载钩子
        /// </summary>
        /// <exception cref="Win32Exception"/>
        private static void UnInstall()
        {
            if (_hHook == IntPtr.Zero)
            {
                return;
            }
            if (UnhookWindowsHookEx(_hHook))
            {
                _hHook = IntPtr.Zero;
            }
            else
            {
                throw new Win32Exception();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hWnd;
            public int Msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public POINT pt;
        }

        delegate IntPtr HookProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wparam, IntPtr lparam);


        /// <summary>
        /// 检查窗口是否含有指定样式
        /// </summary>
        private static bool HasStyle(IntPtr hWnd, int style, bool isExStyle = false)
        {
            var s = unchecked((int)(long)GetWindowLong(hWnd, isExStyle
                ? -20/*GWL_EXSTYLE*/
                : -16/*GWL_STYLE*/));
            return (s & style) == style;
        }

        private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 获取窗口类名
        /// </summary>
        private static string GetClassName(IntPtr hWnd)
        {
            var sb = new StringBuilder(255);
            if (GetClassName(hWnd, sb, sb.Capacity) == 0)
            {
                throw new Win32Exception();
            }
            return sb.ToString();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetAncestor(IntPtr hWnd, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static explicit operator POINT(Point pt)
            {
                return new POINT { X = pt.X, Y = pt.Y };
            }

            public static explicit operator Point(POINT pt)
            {
                return new Point(pt.X, pt.Y);
            }

            public override string ToString()
            {
                return X + ", " + Y;
            }
        }
    }
}
