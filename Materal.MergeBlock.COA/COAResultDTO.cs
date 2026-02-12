namespace Materal.MergeBlock.COA;

/// <summary>
/// COA返回数据传输模型
/// </summary>
public sealed class COAResultDTO(bool result, DateTimeOffset? expirationTime)
{
    /// <summary>
    /// 有效标识
    /// </summary>
    public bool IsValidity { get; set; } = result;
    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTimeOffset? EndDate { get; set; } = expirationTime;
    /// <summary>
    /// 过期天数
    /// </summary>
    public int ExpirationDays => (EndDate is null || EndDate.Value > DateTimeOffset.Now.Date) ? 0 : Convert.ToInt32((DateTimeOffset.Now.Date - EndDate.Value).TotalDays);
}
