using System;
using System.Diagnostics;
using System.Windows.Forms;
using AhDung.WinForm;

namespace AhDung
{
    public partial class FmMDI : Form
    {
        public FmMDI()
        {
            InitializeComponent();

            FormDragger.Dragging += FormDragger_Dragging;
            FormDragger.EnabledChanged += FormDragger_EnabledChanged;

            //例外方式一：将控件添加进例外列表
            //FormDragger.ExcludeControls.Add(pictureBox1);
        }

        void FormDragger_Dragging(object sender, FormDraggingCancelEventArgs e)
        {
            //例外方式二：在FormDragging事件中令Cancel = true
            //if (e.Control == pictureBox1)
            //{
            //    e.Cancel = true;
            //}
            statusBarPanel1.Text = string.Format("{0}, {1}：{2}",
                e.Control.GetType().Name, e.IsClientArea ? "客户区坐标" : "屏幕坐标", e.MousePosition);
        }

        //图片点击事件。仅当该控件处于例外或关闭整个拖拽器时会触发
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            VisitAuthorHome();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //拖拽器开关
            FormDragger.Enabled = toolStripCheckBox1.Checked;
        }

        void FormDragger_EnabledChanged(object sender, EventArgs e)
        {
            //放心这里不会造成来回触发，因为Enabled内部有重入处理
            if (FormDragger.Enabled)
            {
                toolStripCheckBox1.Checked = true;
                toolStripCheckBox1.Text = "拖拽：开";
            }
            else
            {
                toolStripCheckBox1.Checked = false;
                toolStripCheckBox1.Text = "拖拽：关";
            }
        }

        private void FmMDI_Load(object sender, EventArgs e)
        {
            linkLabel1.Links.Add(2, 5, "AAA");
            linkLabel1.Links.Add(8, 6, "BBB").Enabled = false;

            treeView1.ExpandAll();

            this.ContextMenu = contextMenu1;
        }

        private void VisitAuthorHome()
        {
            Process.Start("http://www.cnblogs.com/ahdung/");
        }

        private void 作者主页ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VisitAuthorHome();
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            new FmTestDrag { MdiParent = this }.Show();
        }

        private void menuItem9_Click(object sender, EventArgs e)
        {
            new FmTestDrag().Show();
        }

        private void menuItem10_Click(object sender, EventArgs e)
        {
            new FmTestDrag().ShowDialog();
        }

    }
}
