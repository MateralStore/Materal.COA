namespace Materal.MergeBlock.COA
{
    /// <summary>
    /// COA返回数据传输模型
    /// </summary>
    public sealed class COAResultDTO
    {
        /// <summary>
        /// 有效标识
        /// </summary>
        public bool IsValidity { get; set; } = false;
        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTimeOffset EndDate { get; set; } = DateTimeOffset.Now;
        /// <summary>
        /// 过期天数
        /// </summary>
        public int ExpirationDays => EndDate > DateTimeOffset.Now.Date ? 0 : Convert.ToInt32((DateTimeOffset.Now.Date - EndDate).TotalDays);
    }
}
