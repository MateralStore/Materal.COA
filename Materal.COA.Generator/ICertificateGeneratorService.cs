namespace Materal.COA.Generator;

/// <summary>
/// 证书签发服务
/// </summary>
public interface ICertificateGeneratorService
{
    /// <summary>
    /// 生成证书到内存流
    /// </summary>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书生成结果</returns>
    CertificateResult Generate(CertificateOptions options);

    /// <summary>
    /// 使用现有的公钥私钥生成证书到内存流
    /// </summary>
    /// <param name="privateKey">私钥字符串</param>
    /// <param name="publicKey">公钥字符串</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书生成结果</returns>
    CertificateResult Generate(string privateKey, string publicKey, CertificateOptions options);

    /// <summary>
    /// 使用现有的公钥私钥生成证书到内存流
    /// </summary>
    /// <param name="privateKey">私钥流</param>
    /// <param name="publicKey">公钥流</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书生成结果</returns>
    CertificateResult Generate(Stream privateKey, Stream publicKey, CertificateOptions options);

    /// <summary>
    /// 生成证书到指定目录
    /// </summary>
    /// <param name="directoryInfo">目标目录</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    CertificateFileResult GenerateToFile(DirectoryInfo directoryInfo, CertificateOptions options);

    /// <summary>
    /// 使用现有的公钥私钥生成证书到指定目录
    /// </summary>
    /// <param name="privateKey">私钥字符串</param>
    /// <param name="publicKey">公钥字符串</param>
    /// <param name="directoryInfo">目标目录</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    CertificateFileResult GenerateToFile(string privateKey, string publicKey, DirectoryInfo directoryInfo, CertificateOptions options);

    /// <summary>
    /// 生成证书到指定目录路径
    /// </summary>
    /// <param name="directoryPath">目标目录路径</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    CertificateFileResult GenerateToFile(string directoryPath, CertificateOptions options);

    /// <summary>
    /// 使用现有的公钥私钥生成证书到指定目录路径
    /// </summary>
    /// <param name="privateKey">私钥流</param>
    /// <param name="publicKey">公钥流</param>
    /// <param name="directoryPath">目标目录路径</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    CertificateFileResult GenerateToFile(Stream privateKey, Stream publicKey, string directoryPath, CertificateOptions options);
}