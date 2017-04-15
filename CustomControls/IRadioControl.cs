namespace AhDung.WinForm
{
    /// <summary>
    /// 单选互斥类控件
    /// </summary>
    interface IRadioControl
    {
        /// <summary>
        /// 获取或设置选中状态
        /// </summary>
        bool Checked { get; set; }
    }
}
