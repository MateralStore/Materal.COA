namespace Materal.COA;

/// <summary>
/// 证书数据内部类
/// </summary>
public class CertificateData
{
    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;
    /// <summary>
    /// 有效期
    /// </summary>
    public DateTimeOffset ExpirationTime { get; set; }
    /// <summary>
    /// 发行时间
    /// </summary>
    public DateTimeOffset IssuedTime { get; set; }
    /// <summary>
    /// 密钥位数
    /// </summary>
    public int KeySize { get; set; }
    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; } = string.Empty;
    /// <summary>
    /// 算法
    /// </summary>
    public string Algorithm { get; set; } = string.Empty;
}