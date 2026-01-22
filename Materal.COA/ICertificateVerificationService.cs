namespace Materal.COA;

/// <summary>
/// 证书验证服务
/// </summary>
public interface ICertificateVerificationService
{
    /// <summary>
    /// 验证证书
    /// </summary>
    /// <param name="certificate">证书流</param>
    /// <param name="privateKey">私钥字符串</param>
    /// <param name="projectName"></param>
    /// <param name="expirationTime">过期时间</param>
    /// <returns>验证结果</returns>
    bool Verify(Stream certificate, string privateKey, string projectName, out DateTimeOffset? expirationTime);

    /// <summary>
    /// 验证证书
    /// </summary>
    /// <param name="certificate">证书流</param>
    /// <param name="privateKey">私钥流</param>
    /// <param name="projectName"></param>
    /// <param name="expirationTime">过期时间</param>
    /// <returns>验证结果</returns>
    bool Verify(Stream certificate, Stream privateKey, string projectName, out DateTimeOffset? expirationTime);

    /// <summary>
    /// 验证证书文件
    /// </summary>
    /// <param name="certificateFile">证书文件</param>
    /// <param name="privateKeyFile">私钥文件</param>
    /// <param name="projectName"></param>
    /// <param name="expirationTime">过期时间</param>
    /// <returns>验证结果</returns>
    bool Verify(FileInfo certificateFile, FileInfo privateKeyFile, string projectName, out DateTimeOffset? expirationTime);

    /// <summary>
    /// 验证证书文件
    /// </summary>
    /// <param name="certificateFilePath">证书文件路径</param>
    /// <param name="privateKeyFilePath">私钥文件路径</param>
    /// <param name="projectName"></param>
    /// <param name="expirationTime">过期时间</param>
    /// <returns>验证结果</returns>
    bool Verify(string certificateFilePath, string privateKeyFilePath, string projectName, out DateTimeOffset? expirationTime);
}
