namespace Materal.COA.Generator;

/// <summary>
/// 证书生成结果
/// </summary>
public class CertificateResult
{
    /// <summary>
    /// 证书流
    /// </summary>
    public Stream Certificate { get; set; } = null!;

    /// <summary>
    /// 私钥流
    /// </summary>
    public Stream PrivateKeyXML { get; set; } = null!;

    /// <summary>
    /// 私钥流
    /// </summary>
    public Stream PrivateKeyPem { get; set; } = null!;

    /// <summary>
    /// 公钥流
    /// </summary>
    public Stream? PublicKeyXML { get; set; }

    /// <summary>
    /// 公钥流
    /// </summary>
    public Stream? PublicKeyPem { get; set; }
}
