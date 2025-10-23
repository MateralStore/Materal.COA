using Materal.Extensions;
using System.Runtime.InteropServices;
using System.Text;

namespace Materal.COA;

/// <summary>
/// COA主机
/// </summary>
public static class COAHost
{
    /// <summary>
    /// 验证授权
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public static bool VerifyAuthorization(string name, string password, out DateTimeOffset? endDate)
    {
        string baseDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('\\') :
            typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('/');
        string certificateFilePath = Path.Combine(baseDirectory, "MateralCertificate.cer");
        return VerifyAuthorization(name, password, certificateFilePath, out endDate);
    }
    /// <summary>
    /// 验证授权
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <param name="certificateFilePath"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static bool VerifyAuthorization(string name, string password, string certificateFilePath, out DateTimeOffset? endDate)
    {
        endDate = null;
        if (!File.Exists(certificateFilePath)) throw new FileNotFoundException("证书不存在");
        byte[] contentBuffer = File.ReadAllBytes(certificateFilePath);
        try
        {
            string encodeContent = Encoding.UTF8.GetString(contentBuffer);
            string content = encodeContent.RSADecode(password);
            string[] contents = content.Split('|');
            if (contents.Length != 2) return false;
            if (contents[0] != name) return false;
            endDate = DateTimeOffset.Parse(contents[1]);
            if (endDate < DateTimeOffset.Now.Date.ToDateTimeOffset()) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }
#if NET8_0_OR_GREATER
    /// <summary>
    /// 验证授权
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public static bool VerifyAuthorizationPEM(string name, string password, out DateTimeOffset? endDate)
    {
        string baseDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('\\') :
            typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('/');
        string certificateFilePath = Path.Combine(baseDirectory, "MateralCertificate.cer");
        return VerifyAuthorizationPEM(name, password, certificateFilePath, out endDate);
    }
    /// <summary>
    /// 验证授权
    /// </summary>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <param name="certificateFilePath"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static bool VerifyAuthorizationPEM(string name, string password, string certificateFilePath, out DateTimeOffset? endDate)
    {
        endDate = null;
        if (!File.Exists(certificateFilePath)) throw new FileNotFoundException("证书不存在");
        byte[] contentBuffer = File.ReadAllBytes(certificateFilePath);
        try
        {
            string encodeContent = Encoding.UTF8.GetString(contentBuffer);
            string content = encodeContent.RSADecodePEM(password);
            string[] contents = content.Split('|');
            if (contents.Length != 2) return false;
            if (contents[0] != name) return false;
            endDate = DateTimeOffset.Parse(contents[1]);
            if (endDate < DateTimeOffset.Now.Date.ToDateTimeOffset()) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }
#endif
    /// <summary>
    /// 写入授权证书
    /// </summary>
    /// <param name="name"></param>
    /// <param name="endDate"></param>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public static void WriteCertificateFile(string name, DateTimeOffset endDate, string? directoryPath = null)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            string baseDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('\\') :
            typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('/');
            directoryPath = Path.Combine(baseDirectory, "MateralCertificates", name);
        }
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        string content = $"{name}|{endDate}";
        string encodeContent = content.ToRSAEncode(out string publicKey, out string privateKey);
        byte[] contentBuffer = Encoding.UTF8.GetBytes(encodeContent);
        string certificateFilePath = Path.Combine(directoryPath, "MateralCertificate.cer");
        if (File.Exists(certificateFilePath))
        {
            File.Delete(certificateFilePath);
        }
        File.WriteAllBytes(certificateFilePath, contentBuffer);
        string publicKeyFilePath = Path.Combine(directoryPath, "public.key");
        if (File.Exists(publicKeyFilePath))
        {
            File.Delete(publicKeyFilePath);
        }
        File.WriteAllText(publicKeyFilePath, publicKey);
        string privateKeyFilePath = Path.Combine(directoryPath, "private.key");
        if (File.Exists(privateKeyFilePath))
        {
            File.Delete(privateKeyFilePath);
        }
        File.WriteAllText(privateKeyFilePath, privateKey);
#if NET8_0_OR_GREATER
        string publicKeyPEM = StringExtensions.GetXmlKeyFromPEM(publicKey);
        string privateKeyPEM = StringExtensions.GetXmlKeyFromPEM(privateKey);
        File.WriteAllText(Path.Combine(directoryPath, "public.pem"), publicKeyPEM);
        File.WriteAllText(Path.Combine(directoryPath, "private.pem"), privateKeyPEM);
#endif
    }
}
