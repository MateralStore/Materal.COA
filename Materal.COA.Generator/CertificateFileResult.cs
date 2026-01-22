namespace Materal.COA.Generator;

/// <summary>
/// 证书文件生成结果
/// </summary>
public class CertificateFileResult
{
    /// <summary>
    /// 证书文件
    /// </summary>
    public FileInfo Certificate { get; set; } = null!;

    /// <summary>
    /// 私钥文件XML
    /// </summary>
    public FileInfo PrivateKeyXML { get; set; } = null!;

    /// <summary>
    /// 私钥文件Pem
    /// </summary>
    public FileInfo PrivateKeyPem { get; set; } = null!;

    /// <summary>
    /// 公钥文件XML
    /// </summary>
    public FileInfo? PublicKeyXML { get; set; }

    /// <summary>
    /// 公钥文件Pem
    /// </summary>
    public FileInfo? PublicKeyPem { get; set; }
}
