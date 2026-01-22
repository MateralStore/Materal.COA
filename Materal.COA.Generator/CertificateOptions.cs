namespace Materal.COA.Generator;

/// <summary>
/// 证书生成选项
/// </summary>
public class CertificateOptions
{
    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset ExpirationTime { get; set; }

    /// <summary>
    /// 密钥大小，默认2048
    /// </summary>
    public int KeySize { get; set; } = 2048;

    /// <summary>
    /// 私钥密码（可选）
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 是否包含公钥，默认true
    /// </summary>
    public bool IncludePublicKey { get; set; } = true;
}
