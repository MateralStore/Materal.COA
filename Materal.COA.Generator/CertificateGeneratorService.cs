using Materal.Utils.Crypto;
using System.Text;
using System.Text.Json;

namespace Materal.COA.Generator;

/// <summary>
/// 证书生成服务实现
/// </summary>
public class CertificateGeneratorService : ICertificateGeneratorService
{
    /// <summary>
    /// 生成证书到内存流
    /// </summary>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书生成结果</returns>
    public CertificateResult Generate(CertificateOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        (string publicKey, string privateKey) = RsaCrypto.GenerateKeyPair(options.KeySize);
        (string publicKeyPem, string privateKeyPem) = RsaCrypto.ConvertXmlToPem(publicKey, privateKey);
        return Generate(privateKey, publicKey, privateKeyPem, publicKeyPem, options);
    }

    /// <summary>
    /// 使用现有的公钥私钥生成证书到内存流
    /// </summary>
    /// <param name="privateKey">私钥字符串</param>
    /// <param name="publicKey">公钥字符串</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书生成结果</returns>
    public CertificateResult Generate(string privateKey, string publicKey, CertificateOptions options)
    {
        if (string.IsNullOrEmpty(privateKey)) throw new ArgumentException("私钥不能为空", nameof(privateKey));
        if (string.IsNullOrEmpty(publicKey)) throw new ArgumentException("公钥不能为空", nameof(publicKey));
        if (options == null) throw new ArgumentNullException(nameof(options));
        (string publicKeyPem, string privateKeyPem) = RsaCrypto.ConvertXmlToPem(publicKey, privateKey);
        return Generate(privateKey, publicKey, privateKeyPem, publicKeyPem, options);
    }

    /// <summary>
    /// 使用现有的公钥私钥生成证书到内存流
    /// </summary>
    /// <param name="privateKey">私钥字符串</param>
    /// <param name="publicKey">公钥字符串</param>
    /// <param name="privateKeyPem">私钥PEM字符串</param>
    /// <param name="publicKeyPem">公钥PEM字符串</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书生成结果</returns>
    private CertificateResult Generate(string privateKey, string publicKey, string privateKeyPem, string publicKeyPem, CertificateOptions options)
    {
        if (string.IsNullOrEmpty(privateKey)) throw new ArgumentException("私钥不能为空", nameof(privateKey));
        if (string.IsNullOrEmpty(publicKey)) throw new ArgumentException("公钥不能为空", nameof(publicKey));
        if (options == null) throw new ArgumentNullException(nameof(options));
        // 创建证书数据
        byte[] certificateData = CreateCertificateData(options);
        // 使用HybridCrypto加密证书数据
        byte[] encryptedCertificate = HybridCrypto.Encrypt(certificateData, publicKey);
        // 创建结果
        CertificateResult result = new()
        {
            Certificate = new MemoryStream(encryptedCertificate),
            PrivateKeyXML = new MemoryStream(Encoding.UTF8.GetBytes(privateKey)),
            PrivateKeyPem = new MemoryStream(Encoding.UTF8.GetBytes(privateKeyPem))
        };
        if (options.IncludePublicKey)
        {
            result.PublicKeyXML = new MemoryStream(Encoding.UTF8.GetBytes(publicKey));
            result.PublicKeyPem = new MemoryStream(Encoding.UTF8.GetBytes(publicKeyPem));
        }
        return result;
    }

    /// <summary>
    /// 使用现有的公钥私钥生成证书到内存流
    /// </summary>
    /// <param name="privateKey">私钥流</param>
    /// <param name="publicKey">公钥流</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书生成结果</returns>
    public CertificateResult Generate(Stream privateKey, Stream publicKey, CertificateOptions options)
    {
        if (privateKey == null) throw new ArgumentNullException(nameof(privateKey));
        if (publicKey == null) throw new ArgumentNullException(nameof(publicKey));
        if (options == null) throw new ArgumentNullException(nameof(options));
        using StreamReader privateKeyReader = new(privateKey);
        using StreamReader publicKeyReader = new(publicKey);
        string privateKeyStr = privateKeyReader.ReadToEnd();
        string publicKeyStr = publicKeyReader.ReadToEnd();
        (string publicKeyPem, string privateKeyPem) = RsaCrypto.ConvertXmlToPem(publicKeyStr, privateKeyStr);
        return Generate(privateKeyStr, publicKeyStr, privateKeyPem, publicKeyPem, options);
    }

    /// <summary>
    /// 生成证书到指定目录
    /// </summary>
    /// <param name="directoryInfo">目标目录</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    public CertificateFileResult GenerateToFile(DirectoryInfo directoryInfo, CertificateOptions options)
    {
        if (directoryInfo == null) throw new ArgumentNullException(nameof(directoryInfo));
        if (options == null) throw new ArgumentNullException(nameof(options));
        // 确保目录存在
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
        // 生成证书
        CertificateResult certificateResult = Generate(options);
        // 保存到文件
        FileInfo certificateFile = new(Path.Combine(directoryInfo.FullName, "MateralCertificate.cer"));
        FileInfo privateKeyFile = new(Path.Combine(directoryInfo.FullName, "private.key"));
        FileInfo privateKeyPemFile = new(Path.Combine(directoryInfo.FullName, "private.pem"));
        FileInfo? publicKeyFile = null;
        FileInfo? publicKeyPemFile = null;
        // 保存证书文件
        using (FileStream certificateStream = File.Create(certificateFile.FullName))
        {
            certificateResult.Certificate.CopyTo(certificateStream);
        }
        // 保存私钥文件
        using (FileStream privateKeyStream = File.Create(privateKeyFile.FullName))
        {
            certificateResult.PrivateKeyXML.CopyTo(privateKeyStream);
        }
        // 保存私钥PEM文件
        using (FileStream privateKeyPemStream = File.Create(privateKeyPemFile.FullName))
        {
            certificateResult.PrivateKeyPem.CopyTo(privateKeyPemStream);
        }
        // 保存公钥文件（如果需要）
        if (options.IncludePublicKey && certificateResult.PublicKeyXML != null)
        {
            publicKeyFile = new FileInfo(Path.Combine(directoryInfo.FullName, "public.key"));
            using FileStream publicKeyStream = File.Create(publicKeyFile.FullName);
            certificateResult.PublicKeyXML.CopyTo(publicKeyStream);
        }
        // 保存公钥PEM文件（如果需要）
        if (options.IncludePublicKey && certificateResult.PublicKeyPem != null)
        {
            publicKeyPemFile = new FileInfo(Path.Combine(directoryInfo.FullName, "public.pem"));
            using FileStream publicKeyPemStream = File.Create(publicKeyPemFile.FullName);
            certificateResult.PublicKeyPem.CopyTo(publicKeyPemStream);
        }
        return new CertificateFileResult
        {
            Certificate = certificateFile,
            PrivateKeyXML = privateKeyFile,
            PrivateKeyPem = privateKeyPemFile,
            PublicKeyXML = publicKeyFile,
            PublicKeyPem = publicKeyPemFile
        };
    }

    /// <summary>
    /// 使用现有的公钥私钥生成证书到指定目录
    /// </summary>
    /// <param name="privateKey">私钥字符串</param>
    /// <param name="publicKey">公钥字符串</param>
    /// <param name="directoryInfo">目标目录</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    public CertificateFileResult GenerateToFile(string privateKey, string publicKey, DirectoryInfo directoryInfo, CertificateOptions options)
    {
        if (directoryInfo == null)
            throw new ArgumentNullException(nameof(directoryInfo));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        // 确保目录存在
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        // 生成证书
        (string publicKeyPem, string privateKeyPem) = RsaCrypto.ConvertXmlToPem(publicKey, privateKey);
        CertificateResult certificateResult = Generate(privateKey, publicKey, privateKeyPem, publicKeyPem, options);

        // 保存到文件
        FileInfo certificateFile = new(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.certificate"));
        FileInfo privateKeyFile = new(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.private.key"));
        FileInfo privateKeyPemFile = new(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.private.pem"));
        FileInfo? publicKeyFile = null;
        FileInfo? publicKeyPemFile = null;

        // 保存证书文件
        using (FileStream certificateStream = File.Create(certificateFile.FullName))
        {
            certificateResult.Certificate.CopyTo(certificateStream);
        }

        // 保存私钥文件
        using (FileStream privateKeyStream = File.Create(privateKeyFile.FullName))
        {
            certificateResult.PrivateKeyXML.CopyTo(privateKeyStream);
        }

        // 保存私钥PEM文件
        using (FileStream privateKeyPemStream = File.Create(privateKeyPemFile.FullName))
        {
            certificateResult.PrivateKeyPem.CopyTo(privateKeyPemStream);
        }

        // 保存公钥文件（如果需要）
        if (options.IncludePublicKey && certificateResult.PublicKeyXML != null)
        {
            publicKeyFile = new FileInfo(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.public.key"));
            using FileStream publicKeyStream = File.Create(publicKeyFile.FullName);
            certificateResult.PublicKeyXML.CopyTo(publicKeyStream);
        }

        // 保存公钥PEM文件（如果需要）
        if (options.IncludePublicKey && certificateResult.PublicKeyPem != null)
        {
            publicKeyPemFile = new FileInfo(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.public.pem"));
            using FileStream publicKeyPemStream = File.Create(publicKeyPemFile.FullName);
            certificateResult.PublicKeyPem.CopyTo(publicKeyPemStream);
        }

        return new CertificateFileResult
        {
            Certificate = certificateFile,
            PrivateKeyXML = privateKeyFile,
            PrivateKeyPem = privateKeyPemFile,
            PublicKeyXML = publicKeyFile,
            PublicKeyPem = publicKeyPemFile
        };
    }

    /// <summary>
    /// 使用现有的公钥私钥生成证书到指定目录
    /// </summary>
    /// <param name="privateKey">私钥字符串</param>
    /// <param name="publicKey">公钥字符串</param>
    /// <param name="privateKeyPem">私钥PEM字符串</param>
    /// <param name="publicKeyPem">公钥PEM字符串</param>
    /// <param name="directoryInfo">目标目录</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    private CertificateFileResult GenerateToFile(string privateKey, string publicKey, string privateKeyPem, string publicKeyPem, DirectoryInfo directoryInfo, CertificateOptions options)
    {
        if (directoryInfo == null)
            throw new ArgumentNullException(nameof(directoryInfo));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        // 确保目录存在
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        // 生成证书
        CertificateResult certificateResult = Generate(privateKey, publicKey, privateKeyPem, publicKeyPem, options);

        // 保存到文件
        FileInfo certificateFile = new(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.certificate"));
        FileInfo privateKeyFile = new(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.private.key"));
        FileInfo privateKeyPemFile = new(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.private.pem"));
        FileInfo? publicKeyFile = null;
        FileInfo? publicKeyPemFile = null;

        // 保存证书文件
        using (FileStream certificateStream = File.Create(certificateFile.FullName))
        {
            certificateResult.Certificate.CopyTo(certificateStream);
        }

        // 保存私钥文件
        using (FileStream privateKeyStream = File.Create(privateKeyFile.FullName))
        {
            certificateResult.PrivateKeyXML.CopyTo(privateKeyStream);
        }

        // 保存私钥PEM文件
        using (FileStream privateKeyPemStream = File.Create(privateKeyPemFile.FullName))
        {
            certificateResult.PrivateKeyPem.CopyTo(privateKeyPemStream);
        }

        // 保存公钥文件（如果需要）
        if (options.IncludePublicKey && certificateResult.PublicKeyXML != null)
        {
            publicKeyFile = new FileInfo(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.public.key"));
            using FileStream publicKeyStream = File.Create(publicKeyFile.FullName);
            certificateResult.PublicKeyXML.CopyTo(publicKeyStream);
        }

        // 保存公钥PEM文件（如果需要）
        if (options.IncludePublicKey && certificateResult.PublicKeyPem != null)
        {
            publicKeyPemFile = new FileInfo(Path.Combine(directoryInfo.FullName, $"{options.ProjectName}.public.pem"));
            using FileStream publicKeyPemStream = File.Create(publicKeyPemFile.FullName);
            certificateResult.PublicKeyPem.CopyTo(publicKeyPemStream);
        }

        return new CertificateFileResult
        {
            Certificate = certificateFile,
            PrivateKeyXML = privateKeyFile,
            PrivateKeyPem = privateKeyPemFile,
            PublicKeyXML = publicKeyFile,
            PublicKeyPem = publicKeyPemFile
        };
    }

    /// <summary>
    /// 生成证书到指定目录路径
    /// </summary>
    /// <param name="directoryPath">目标目录路径</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    public CertificateFileResult GenerateToFile(string directoryPath, CertificateOptions options)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentException("目录路径不能为空", nameof(directoryPath));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        DirectoryInfo directoryInfo = new(directoryPath);
        return GenerateToFile(directoryInfo, options);
    }

    /// <summary>
    /// 使用现有的公钥私钥生成证书到指定目录路径
    /// </summary>
    /// <param name="privateKey">私钥流</param>
    /// <param name="publicKey">公钥流</param>
    /// <param name="directoryPath">目标目录路径</param>
    /// <param name="options">证书生成选项</param>
    /// <returns>证书文件生成结果</returns>
    public CertificateFileResult GenerateToFile(Stream privateKey, Stream publicKey, string directoryPath, CertificateOptions options)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentException("目录路径不能为空", nameof(directoryPath));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        DirectoryInfo directoryInfo = new(directoryPath);

        // 读取密钥
        using StreamReader privateKeyReader = new(privateKey);
        using StreamReader publicKeyReader = new(publicKey);

        string privateKeyStr = privateKeyReader.ReadToEnd();
        string publicKeyStr = publicKeyReader.ReadToEnd();
        (string publicKeyPem, string privateKeyPem) = RsaCrypto.ConvertXmlToPem(publicKeyStr, privateKeyStr);

        return GenerateToFile(privateKeyStr, publicKeyStr, privateKeyPem, publicKeyPem, directoryInfo, options);
    }

    /// <summary>
    /// 创建证书数据
    /// </summary>
    /// <param name="privateKey">私钥</param>
    /// <param name="publicKey">公钥</param>
    /// <param name="options">证书选项</param>
    /// <returns>证书数据字节数组</returns>
    private static byte[] CreateCertificateData(CertificateOptions options)
    {
        CertificateData certificateData = new()
        {
            ProjectName = options.ProjectName,
            ExpirationTime = options.ExpirationTime,
            IssuedTime = DateTimeOffset.UtcNow,
            KeySize = options.KeySize,
            Version = "1.0",
            Algorithm = "RSA+AES"
        };
        return JsonSerializer.SerializeToUtf8Bytes(certificateData);
    }
}
