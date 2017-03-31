using System;
using System.Windows.Forms;
using AhDung.WinForm;

namespace AhDung
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormDragger.Enabled = true;//启用拖拽器
            Application.Run(new FmMDI());
        }
    }
}
