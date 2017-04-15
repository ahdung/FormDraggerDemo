using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AhDung.WinForm.Controls
{
    /// <summary>
    /// 复选组面板
    /// </summary>
    [ToolboxItem(true)]
    public class CheckGroupBox : CheckableGroupBox
    {
        CheckBox CheckBoxControl
        {
            get { return (CheckBox)CheckControl; }
        }

        /// <summary>
        /// 获取或设置勾选状态
        /// </summary>
        [Description("勾选状态")]
        [DefaultValue(CheckState.Unchecked)]
        public CheckState CheckState
        {
            get { return CheckBoxControl.CheckState; }
            set { CheckBoxControl.CheckState = value; }
        }

        /// <summary>
        /// 获取或设置是否允许三态
        /// </summary>
        [Description("是否允许三态")]
        [DefaultValue(false)]
        public bool ThreeState
        {
            get { return CheckBoxControl.ThreeState; }
            set { CheckBoxControl.ThreeState = value; }
        }

        /// <summary>
        /// 选中状态改变后
        /// </summary>
        public event EventHandler CheckStateChanged;

        /// <summary>
        /// 初始化本类
        /// </summary>
        public CheckGroupBox()
            : base(true)
        {
            CheckBoxControl.CheckStateChanged += checkControl_CheckStateChanged;
        }

        protected virtual void OnCheckStateChanged(EventArgs e)
        {
            var handler = CheckStateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void checkControl_CheckStateChanged(object sender, EventArgs e)
        {
            OnCheckStateChanged(EventArgs.Empty);
        }
    }
}
