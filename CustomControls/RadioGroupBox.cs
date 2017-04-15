using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AhDung.WinForm.Controls
{
    /// <summary>
    /// 单选组面板
    /// </summary>
    [ToolboxItem(true)]
    public class RadioGroupBox : CheckableGroupBox, IRadioControl
    {
        Control _lastParent;

        /// <summary>
        /// 初始化本类
        /// </summary>
        public RadioGroupBox()
            : base(false)
        {
            this.ParentChanged += RadioGroupBox_ParentChanged;
        }

        void RadioGroupBox_ParentChanged(object sender, EventArgs e)
        {
            if (_lastParent != null)
            {
                foreach (Control c in _lastParent.Controls)
                {
                    if (c is RadioButton)
                    {
                        ((RadioButton)c).CheckedChanged -= RadioGroupBox_CheckedChanged;
                    }
                }
                _lastParent.ControlAdded -= Parent_ControlAdded;
                _lastParent.ControlRemoved -= Parent_ControlRemoved;
            }
            _lastParent = Parent;
            if (_lastParent != null)
            {
                foreach (Control c in _lastParent.Controls)
                {
                    if (c is RadioButton)
                    {
                        ((RadioButton)c).CheckedChanged += RadioGroupBox_CheckedChanged;
                    }
                }
                _lastParent.ControlAdded += Parent_ControlAdded;
                _lastParent.ControlRemoved += Parent_ControlRemoved;
            }
        }

        void RadioGroupBox_CheckedChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb.Checked)
            {
                this.Checked = false;
            }
        }

        void Parent_ControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control is RadioButton)
            {
                ((RadioButton)e.Control).CheckedChanged += RadioGroupBox_CheckedChanged;
            }
        }

        void Parent_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (e.Control is RadioButton)
            {
                ((RadioButton)e.Control).CheckedChanged -= RadioGroupBox_CheckedChanged;
            }
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            //互斥。
            if (this.Checked && this.Parent != null)
            {
                foreach (Control c in this.Parent.Controls)
                {
                    if (c == this) { continue; }

                    if (c is RadioButton)
                    {
                        ((RadioButton)c).Checked = false;
                    }
                    else if (c is IRadioControl)
                    {
                        ((IRadioControl)c).Checked = false;
                    }
                }
            }
            base.OnCheckedChanged(e);
        }
    }
}
