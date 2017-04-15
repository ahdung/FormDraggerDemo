using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace AhDung.WinForm.Controls
{
    /// <summary>
    /// 可Check的分组框
    /// </summary>
    [Designer(typeof(CheckableGroupBoxDesigner))]
    [ToolboxItem(false)]
    [DefaultEvent("CheckedChanged")]
    [DefaultProperty("Checked")]
    public class CheckableGroupBox : UserControl
    {
        readonly bool _isCheckBox;
        protected ButtonBase CheckControl;
        GroupBoxInner _grpBox;
        bool _autoDisableContent;
        Color _titleColor;

        /// <summary>
        /// 获取或设置文本
        /// </summary>
        [Description("抬头文字")]
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]            //不是蛋疼
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] //不是蛋疼
        public override string Text
        {
            get { return CheckControl.Text; }
            set { CheckControl.Text = value; }
        }

        /// <summary>
        /// 获取或设置标题颜色
        /// </summary>
        [Description("标题颜色")]
        [DefaultValue(typeof(Color), "0, 70, 213")]
        public Color TitleColor
        {
            get { return _titleColor; }
            set
            {
                if (_titleColor == value) { return; }
                _titleColor = value;
                CheckControl.ForeColor = _titleColor;
            }
        }

        /// <summary>
        /// 获取或设置是否勾选
        /// </summary>
        [Description("是否勾选")]
        [DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return _isCheckBox
                    ? ((CheckBox)CheckControl).Checked
                    : ((RadioButton)CheckControl).Checked;
            }
            set
            {
                if (_isCheckBox)
                {
                    ((CheckBox)CheckControl).Checked = value;
                }
                else
                {
                    ((RadioButton)CheckControl).Checked = value;
                }
            }
        }

        [Description("取消Checked时是否屏蔽内容区")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DefaultValue(true)]
        public bool AutoDisableContent
        {
            get { return _autoDisableContent; }
            set
            {
                if (_autoDisableContent == value) { return; }
                _autoDisableContent = value;
                if (!_autoDisableContent)//当开闭状态不联动时，强制开启
                {
                    _grpBox.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 是否选中改变后
        /// </summary>
        public event EventHandler CheckedChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GroupBoxInner GroupBoxControl
        {
            get { return _grpBox; }
        }

        /// <summary>
        /// 初始化本类
        /// </summary>
        /// <param name="isCheckBox">true为复选框，false为单选框</param>
        protected CheckableGroupBox(bool isCheckBox)
        {
            _isCheckBox = isCheckBox;
            _titleColor = Color.FromArgb(0, 70, 213);
            InitializeComponent();
            AutoDisableContent = true;
            _grpBox.Enabled = false;
        }

        private void InitializeComponent()
        {
            this._grpBox = new GroupBoxInner();
            this.SuspendLayout();
            this._grpBox.SuspendLayout();
            //
            // CheckControl
            //
            if (_isCheckBox)
            {
                CheckControl = new CheckBox();
                ((CheckBox)CheckControl).CheckedChanged += checkControl_CheckedChanged;
            }
            else
            {
                CheckControl = new RadioButton();
                ((RadioButton)CheckControl).CheckedChanged += checkControl_CheckedChanged;
            }
            this.CheckControl.AutoSize = true;
            this.CheckControl.ForeColor = _titleColor;
            this.CheckControl.Location = new System.Drawing.Point(8, -1);
            this.CheckControl.Name = "CheckControl";
            this.CheckControl.TabIndex = 0;
            // 
            // _grpBox
            // 
            this._grpBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grpBox.Name = "_grpBox";
            this._grpBox.TabIndex = 1;
            // 
            // CheckedGroupBox
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(CheckControl);
            this.Controls.Add(this._grpBox);
            this._grpBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            var handler = CheckedChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void checkControl_CheckedChanged(object sender, EventArgs e)
        {
            if (AutoDisableContent)
            {
                _grpBox.Enabled = this.Checked;
            }
            OnCheckedChanged(EventArgs.Empty);
        }

        [ToolboxItem(false)]
        public class GroupBoxInner : GroupBox
        {
            // ReSharper disable UnusedMember.Global
            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public override DockStyle Dock
            {
                get { return base.Dock; }
                set { base.Dock = value; }
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public new bool Enabled
            {
                get { return base.Enabled; }
                set { base.Enabled = value; }
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public new Point Location
            {
                get { return base.Location; }
                set { base.Location = value; }
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public new string Name
            {
                get { return base.Name; }
                set { base.Name = value; }
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public new int TabIndex
            {
                get { return base.TabIndex; }
                set { base.TabIndex = value; }
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public new bool TabStop
            {
                get { return base.TabStop; }
                set { base.TabStop = value; }
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public override string Text
            {
                get { return base.Text; }
                set { base.Text = value; }
            }
            // ReSharper restore UnusedMember.Global
        }
    }

    public class CheckableGroupBoxDesigner : ParentControlDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            if (this.Control is CheckableGroupBox)
            {
                EnableDesignMode(((CheckableGroupBox)this.Control).GroupBoxControl, "GroupBoxControl");
            }
        }
    }
}


