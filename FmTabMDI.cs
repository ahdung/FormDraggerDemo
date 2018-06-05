using System;
using System.Windows.Forms;
using AhDung.WinForm;

namespace AhDung
{
    public partial class FmTabMDI : Form
    {
        public FmTabMDI()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var form = tabControl1.SelectedTab.Controls[0];
            MessageBoxEx.Show(form.GetType().ToString(), "Custom message box", "attach message");
        }

        private void FmTabMDI_Load(object sender, EventArgs e)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                page.Controls.Add(new FmTabSub { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None, Visible = true });
            }
        }
    }
}
