using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace AhDung.WinForm
{
    /// <summary>
    /// 窗体拖拽器
    /// <para>- 使窗体客户区可拖动</para>
    /// <para>- 若多次调用Application.Run，建议在每次调用之前都启用</para>
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class FormDragger
    {
        static bool _enabled;
        static MouseLeftDownMessageFilter _filter;

        /// <summary>
        /// 拖拽发生时
        /// <para>- 令Cancel = true可取消拖拽</para>
        /// <para>- 注意sender为null</para>
        /// </summary>
        public static event EventHandler<FormDraggingCancelEventArgs> Dragging;

        private static void OnDragging(FormDraggingCancelEventArgs e) => Dragging?.Invoke(null, e);

        /// <summary>
        /// 拖拽器开关状态改变后
        /// </summary>
        /// <para>- 注意sender为null</para>
        public static event EventHandler EnabledChanged;

        private static void OnEnabledChanged(EventArgs e) => EnabledChanged?.Invoke(null, e);

        /// <summary>
        /// 获取或设置是否启用拖拽器
        /// </summary>
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                //先移除再加入。这样做是允许重复Enable，
                //因为存在多次Application.Run的情况下，每次Run都有可能会启动新
                //线程上下文（ThreadContext），从而导致过滤器因未加入新context
                //而失效，所以需要在每次Run之前都启用

                Application.RemoveMessageFilter(_filter);

                if (value)
                {
                    if (_filter == null)
                    {
                        _filter = new MouseLeftDownMessageFilter();
                    }
                    Application.AddMessageFilter(_filter);
                }

                if (_enabled != value)
                {
                    _enabled = value;
                    OnEnabledChanged(EventArgs.Empty);
                }
            }
        }

        ///// <summary>
        ///// 检测本消息过滤器是否还在工作
        ///// </summary>
        ///// <exception cref="Exception"/>
        //private static bool FilterExists()
        //{
        //    if (_filter == null)
        //    {
        //        return false;
        //    }

        //    string formsFullName = null;
        //    foreach (var a in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
        //    {
        //        byte[] token;
        //        if (a.Name == "System.Windows.Forms" && (token = a.GetPublicKeyToken()) != null && token.Length != 0)
        //        {
        //            formsFullName = a.FullName;
        //            break;
        //        }
        //    }

        //    var type = Type.GetType("System.Windows.Forms.Application+ThreadContext, " + formsFullName, true);
        //    var crrThreadContext = type.GetMethod("FromCurrent", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        //    if (crrThreadContext != null)
        //    {
        //        var filterArr = type.GetField("messageFilters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(crrThreadContext);
        //        if (filterArr != null)
        //        {
        //            return ((ArrayList)filterArr).Contains(_filter);
        //        }
        //    }

        //    return false;
        //}

        static List<Control> _excludeControls;

        /// <summary>
        /// 例外控件
        /// </summary>
        public static List<Control> ExcludeControls => _excludeControls ?? (_excludeControls = new List<Control>());

        /// <summary>
        /// 左键单击消息过滤器
        /// </summary>
        private class MouseLeftDownMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                Point point;
                switch (m.Msg)
                {
                    case 0x201://WM_LBUTTONDOWN
                        //Debug.WriteLine(m.HWnd.ToString("X8") + "\t" + m.WParam + "\t" + GetPoint(m.LParam));

                        if ((int)m.WParam != 1) { break; }//仅处理单纯的左键单击

                        var c = Control.FromHandle(m.HWnd);
                        if (c != null && (
                               CanDrag(c, point = GetPoint(m.LParam))
                            || CanDrag(c.GetChildAtPoint(point), point))//支持自定义容器控件
                            )
                        {
                            return DoDrag(c, point, true);
                        }
                        break;

                    case 0xA1://WM_NCLBUTTONDOWN
                        var ht = (int)m.WParam;
                        if (ht == 5/*HTMENU*/)//点到MainMenu时
                        {
                            //判断是否点到菜单项。若不是或菜单项不可用则拖拽
                            //点击菜单时m.HWnd为窗体句柄而非菜单句柄
                            point = GetPoint(m.LParam);
                            var menu = GetMenu(m.HWnd);
                            var item = MenuItemFromPoint(m.HWnd, menu, (POINT)point);
                            if (item == -1
                                || (GetMenuState(menu, item, 0x400/*MF_BYPOSITION*/) & 2/*MF_DISABLED*/) == 2)
                            {
                                return DoDrag(Control.FromHandle(m.HWnd), point, false);
                            }
                        }
                        else if (ht == 4/*HTSIZE*/ || ht == 18/*HTBORDER*/)//点到两个滚动条右下空白、控件边框时
                        {
                            return DoDrag(Control.FromHandle(m.HWnd), GetPoint(m.LParam), false);
                        }
                        else if (ht == 6/*HTHSCROLL*/ || ht == 7/*HTVSCROLL*/)//点到滚动条时
                        {
                            //获取滚动条状态，若处于无效则拖拽
                            //OBJID_HSCROLL=0xFFFFFFFA
                            //OBJID_VSCROLL=0xFFFFFFFB
                            var idObject = unchecked((int)(ht + 0xFFFFFFF4));
                            SCROLLBARINFO sbi = new SCROLLBARINFO { cbSize = SizeOfSCROLLBARINFO };
                            if (GetScrollBarInfo(m.HWnd, idObject, ref sbi)
                                && sbi.scrollBarInfo == 1/*STATE_SYSTEM_UNAVAILABLE*/)
                            {
                                return DoDrag(Control.FromHandle(m.HWnd), GetPoint(m.LParam), false);
                            }
                        }
                        break;
                }
                return false;
            }
        }


        /// <summary>
        /// 执行拖拽
        /// </summary>
        /// <returns>指示是否拦截消息</returns>
        private static bool DoDrag(Control c, Point point, bool isClientArea)
        {
            try
            {
                Form form;
                if (c == null || (form = c.FindForm()) == null)
                {
                    return false;
                }
                var e = new FormDraggingCancelEventArgs(c, point, isClientArea);
                OnDragging(e);
                if (e.Cancel)
                {
                    return false;
                }
                //当作为MDI子窗体且最大化时，拖MDI主窗体
                if (form.WindowState == FormWindowState.Maximized || !form.TopLevel && form.Dock == DockStyle.Fill)
                {
                    form = form.MdiParent ?? form.ParentForm ?? form;
                }
                SendMessage(form.Handle, 0xA1/*WM_NCLBUTTONDOWN*/, (IntPtr)2, IntPtr.Zero);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static PropertyInfo _linkState;

        /// <summary>
        /// 判断是否处于可拖拽情形
        /// </summary>
        private static bool CanDrag(Control c, Point pt)
        {
            if (c == null || _excludeControls != null && _excludeControls.Contains(c))
            {
                return false;
            }

            if (c is Form
               || c is Label && !(c is LinkLabel)
               || c is Panel
               || c is GroupBox
               || c is PictureBox
               || c is ProgressBar
               || c is StatusBar
               || c is MdiClient
                )
            {
                return true;
            }

            if (c is LinkLabel linkLabel)
            {
                //若点到链接部分且链接可用则不拦截
                //取State优于调用PointInLink
                foreach (LinkLabel.Link link in linkLabel.Links)
                {
                    if (_linkState == null)
                    {
                        _linkState = typeof(LinkLabel.Link).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic);
                    }
                    var state = (LinkState)_linkState.GetValue(link, null);
                    if ((state & LinkState.Hover) == LinkState.Hover && link.Enabled)
                    {
                        return false;
                    }
                }
                return true;
            }

            if (c is ToolStrip strip)
            {
                var item = strip.GetItemAt(pt);
                if (item == null
                    || !item.Enabled
                    || item is ToolStripSeparator
                    || (item is ToolStripLabel label && !label.IsLink))
                {
                    return true;
                }
                return false;
            }

            if (c is ToolBar bar)
            {
                //判断鼠标是否点击在按钮上
                var ptPoint = GCHandle.Alloc((POINT)(pt), GCHandleType.Pinned);
                var index = (int)SendMessage(bar.Handle, (0x400 + 69)/*TB_HITTEST */, IntPtr.Zero, ptPoint.AddrOfPinnedObject());
                ptPoint.Free();

                //若点击在按钮上且当按钮可用时则不拦截
                if (index >= 0 && bar.Buttons[index].Enabled)
                {
                    return false;
                }
                return true;
            }

            if (c is DataGridView dgv)
            {
                return dgv.HitTest(pt.X, pt.Y).Type == DataGridViewHitTestType.None;
            }

            if (c is DataGrid grid)
            {
                return grid.HitTest(pt).Type == DataGrid.HitTestType.None;
            }

            return !c.CanSelect;
        }

        #region Win32 API

        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private static Point GetPoint(IntPtr lParam)
        {
            return new Point(unchecked((int)(long)lParam));
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetScrollBarInfo(IntPtr hwnd, int idObject, ref SCROLLBARINFO psbi);

        static readonly int SizeOfSCROLLBARINFO = Marshal.SizeOf(typeof(SCROLLBARINFO));

        [StructLayout(LayoutKind.Sequential)]
        private struct SCROLLBARINFO
        {
            public int cbSize;
            public int rcLeft;
            public int rcTop;
            public int rcRight;
            public int rcBottom;
            public int dxyLineButton;
            public int xyThumbTop;
            public int xyThumbBottom;
            public int reserved;
            public int scrollBarInfo;
            public int upArrowInfo;
            public int largeDecrementInfo;
            public int thumbnfo;
            public int largeIncrementInfo;
            public int downArrowInfo;
        }

        [DllImport("user32.dll")]
        private static extern int GetMenuState(IntPtr hMenu, int uId, int uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int MenuItemFromPoint(IntPtr hWnd, IntPtr hMenu, POINT ptScreen);

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
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore MemberCanBePrivate.Local

        #endregion
    }

    /// <inheritdoc />
    /// <summary>
    /// 窗体拖拽取消事件参数
    /// </summary>
    public class FormDraggingCancelEventArgs : CancelEventArgs
    {
        /// <summary>
        /// 获取点击控件
        /// </summary>
        public Control Control { get; private set; }

        /// <summary>
        /// 获取鼠标位置
        /// <para>- 取自点击消息产生时的lParam</para>
        /// <para>- 当IsClientArea = true时，相对Control的客户区坐标，否则相对屏幕坐标</para>
        /// </summary>
        public Point MousePosition { get; private set; }

        /// <summary>
        /// 是否控件客户区
        /// </summary>
        public bool IsClientArea { get; private set; }

        public FormDraggingCancelEventArgs(Control c, Point point, bool isClientArea)
        {
            this.Control = c;
            this.MousePosition = point;
            this.IsClientArea = isClientArea;
        }
    }
}
