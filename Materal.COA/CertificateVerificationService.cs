using Materal.Utils.Crypto;
using System.Text.Json;

namespace Materal.COA;

/// <summary>
/// 证书验证服务实现
/// </summary>
public class CertificateVerificationService : ICertificateVerificationService
{
    /// <inheritdoc/>
    public bool Verify(Stream certificate, string privateKey, string projectName, out DateTimeOffset? expirationTime)
    {
        if (certificate is null) throw new ArgumentNullException(nameof(certificate));
        if (string.IsNullOrEmpty(privateKey)) throw new ArgumentException("私钥不能为空", nameof(privateKey));
        try
        {
            // 读取证书数据
            byte[] certificateBytes = ReadStreamToBytes(certificate);
            // 解密证书数据
            byte[] decryptedData = HybridCrypto.Decrypt(certificateBytes, privateKey);
            // 解析证书内容
            CertificateData certificateInfo = ParseCertificateData(decryptedData);
            // 验证证书是否过期
            expirationTime = certificateInfo.ExpirationTime;
            if (certificateInfo.ProjectName != projectName) return false;
            return DateTime.UtcNow <= certificateInfo.ExpirationTime;
        }
        catch
        {
            expirationTime = null;
            return false;
        }
    }

    /// <inheritdoc/>
    public bool Verify(Stream certificate, Stream privateKey, string projectName, out DateTimeOffset? expirationTime)
    {
        if (certificate is null) throw new ArgumentNullException(nameof(certificate));
        if (privateKey is null) throw new ArgumentNullException(nameof(privateKey));
        using StreamReader privateKeyReader = new(privateKey);
        string privateKeyStr = privateKeyReader.ReadToEnd();
        return Verify(certificate, privateKeyStr, projectName, out expirationTime);
    }

    /// <inheritdoc/>
    public bool Verify(FileInfo certificateFile, FileInfo privateKeyFile, string projectName, out DateTimeOffset? expirationTime)
    {
        if (certificateFile is null) throw new ArgumentNullException(nameof(certificateFile));
        if (privateKeyFile is null) throw new ArgumentNullException(nameof(privateKeyFile));
        using FileStream certificateStream = certificateFile.OpenRead();
        using FileStream privateKeyStream = privateKeyFile.OpenRead();
        return Verify(certificateStream, privateKeyStream, projectName, out expirationTime);
    }

    /// <inheritdoc/>
    public bool Verify(string certificateFilePath, string privateKeyFilePath, string projectName, out DateTimeOffset? expirationTime)
    {
        if (string.IsNullOrEmpty(certificateFilePath)) throw new ArgumentException("证书文件路径不能为空", nameof(certificateFilePath));
        if (string.IsNullOrEmpty(privateKeyFilePath)) throw new ArgumentException("私钥文件路径不能为空", nameof(privateKeyFilePath));
        FileInfo certificateFile = new(certificateFilePath);
        FileInfo privateKeyFile = new(privateKeyFilePath);
        return Verify(certificateFile, privateKeyFile, projectName, out expirationTime);
    }

    /// <summary>
    /// 读取流到字节数组
    /// </summary>
    /// <param name="stream">输入流</param>
    /// <returns>字节数组</returns>
    private static byte[] ReadStreamToBytes(Stream stream)
    {
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 解析证书数据
    /// </summary>
    /// <param name="certificateData">证书数据字节数组</param>
    /// <returns>证书信息对象</returns>
    private static CertificateData ParseCertificateData(byte[] certificateData)
        => JsonSerializer.Deserialize<CertificateData>(certificateData) ?? throw new ArgumentException("数据错误", nameof(certificateData));
}
