using Materal.COA;
using Materal.COA.Generator;

namespace Materal.COA.Test;

[TestClass]
public class CertificateGeneratorAndVerificationTests
{
    private CertificateGeneratorService _generatorService = null!;
    private CertificateVerificationService _verificationService = null!;
    private string _testDirectory = null!;

    [TestInitialize]
    public void Setup()
    {
        _generatorService = new CertificateGeneratorService();
        _verificationService = new CertificateVerificationService();
        _testDirectory = Path.Combine(Path.GetTempPath(), "CertificateTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    /// <summary>
    /// 测试使用有效数据生成和验证证书是否成功
    /// 验证证书生成器能够正确生成证书，并且验证器能够验证生成的证书
    /// </summary>
    [TestMethod]
    public void GenerateAndVerifyCertificate_ShouldSucceed_WithValidData()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "TestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048,
            IncludePublicKey = true
        };

        // Act - 生成证书
        CertificateResult result = _generatorService.Generate(options);

        // Assert - 验证生成结果
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Certificate);
        Assert.IsNotNull(result.PrivateKeyXML);
        Assert.IsNotNull(result.PublicKeyXML);

        // 重置流位置
        result.Certificate.Position = 0;
        result.PrivateKeyXML.Position = 0;

        // 读取私钥
        using StreamReader privateKeyReader = new(result.PrivateKeyXML);
        string privateKey = privateKeyReader.ReadToEnd();

        // Act - 验证证书
        result.Certificate.Position = 0;
        bool verifyResult = _verificationService.Verify(result.Certificate, privateKey, "TestProject", out DateTimeOffset? expirationTime);

        // Assert - 验证结果
        Assert.IsTrue(verifyResult);
        Assert.IsNotNull(expirationTime);
        Assert.IsLessThan(1, Math.Abs((options.ExpirationTime - expirationTime.Value).TotalSeconds));
    }

    /// <summary>
    /// 测试使用错误的项目名验证证书是否失败
    /// 验证当使用与生成时不匹配的项目名时，证书验证应该失败
    /// </summary>
    [TestMethod]
    public void GenerateAndVerifyCertificate_ShouldFail_WithWrongProjectName()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "TestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048
        };

        // Act
        CertificateResult result = _generatorService.Generate(options);
        
        // 重置流位置并读取私钥
        result.Certificate.Position = 0;
        result.PrivateKeyXML.Position = 0;
        using StreamReader privateKeyReader = new(result.PrivateKeyXML);
        string privateKey = privateKeyReader.ReadToEnd();

        // Act - 使用错误的项目名验证
        result.Certificate.Position = 0;
        bool verifyResult = _verificationService.Verify(result.Certificate, privateKey, "WrongProject", out DateTimeOffset? expirationTime);

        // Assert
        Assert.IsFalse(verifyResult);
        Assert.IsNotNull(expirationTime);
    }

    /// <summary>
    /// 测试验证过期证书是否失败
    /// 验证当证书已过期时，证书验证应该失败
    /// </summary>
    [TestMethod]
    public void GenerateAndVerifyCertificate_ShouldFail_WithExpiredCertificate()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "TestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(-1), // 已过期
            KeySize = 2048
        };

        // Act
        CertificateResult result = _generatorService.Generate(options);
        
        // 重置流位置并读取私钥
        result.Certificate.Position = 0;
        result.PrivateKeyXML.Position = 0;
        using StreamReader privateKeyReader = new(result.PrivateKeyXML);
        string privateKey = privateKeyReader.ReadToEnd();

        // Act - 验证过期证书
        result.Certificate.Position = 0;
        bool verifyResult = _verificationService.Verify(result.Certificate, privateKey, "TestProject", out DateTimeOffset? expirationTime);

        // Assert
        Assert.IsFalse(verifyResult);
        Assert.IsNotNull(expirationTime);
        Assert.IsTrue(expirationTime.Value < DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 测试生成证书到文件并验证是否成功
    /// 验证证书生成器能够正确生成证书文件，并且验证器能够验证生成的证书文件
    /// </summary>
    [TestMethod]
    public void GenerateToFileAndVerifyCertificate_ShouldSucceed()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "FileTestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(60),
            KeySize = 4096,
            IncludePublicKey = true
        };

        // Act - 生成证书到文件
        CertificateFileResult fileResult = _generatorService.GenerateToFile(_testDirectory, options);

        // Assert - 验证文件生成结果
        Assert.IsNotNull(fileResult);
        Assert.IsTrue(fileResult.Certificate.Exists);
        Assert.IsTrue(fileResult.PrivateKeyXML.Exists);
        Assert.IsNotNull(fileResult.PublicKeyXML);
        Assert.IsTrue(fileResult.PublicKeyXML!.Exists);

        // Act - 验证证书文件
        bool verifyResult = _verificationService.Verify(
            fileResult.Certificate.FullName, 
            fileResult.PrivateKeyXML.FullName, 
            "FileTestProject", 
            out DateTimeOffset? expirationTime);

        // Assert
        Assert.IsTrue(verifyResult);
        Assert.IsNotNull(expirationTime);
        Assert.IsLessThan(1, Math.Abs((options.ExpirationTime - expirationTime.Value).TotalSeconds));
    }

    /// <summary>
    /// 测试使用现有密钥生成证书并验证是否成功
    /// 验证证书生成器能够使用现有的密钥对生成新的证书
    /// </summary>
    [TestMethod]
    public void GenerateWithExistingKeysAndVerify_ShouldSucceed()
    {
        // Arrange - 生成密钥对
        CertificateOptions options = new()
        {
            ProjectName = "ExistingKeysTest",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(90),
            KeySize = 2048,
            IncludePublicKey = true
        };

        // 先生成一次获取密钥对
        CertificateResult firstResult = _generatorService.Generate(options);
        
        // 读取密钥
        firstResult.PrivateKeyXML.Position = 0;
        firstResult.PublicKeyXML!.Position = 0;
        using StreamReader privateKeyReader = new(firstResult.PrivateKeyXML);
        using StreamReader publicKeyReader = new(firstResult.PublicKeyXML);
        string privateKey = privateKeyReader.ReadToEnd();
        string publicKey = publicKeyReader.ReadToEnd();

        // Act - 使用现有密钥生成新证书
        CertificateOptions newOptions = new()
        {
            ProjectName = "ExistingKeysTest",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(45),
            KeySize = 2048,
            IncludePublicKey = false
        };
        CertificateResult secondResult = _generatorService.Generate(privateKey, publicKey, newOptions);

        // Assert - 验证生成结果
        Assert.IsNotNull(secondResult);
        Assert.IsNotNull(secondResult.Certificate);
        Assert.IsNotNull(secondResult.PrivateKeyXML);
        Assert.IsNull(secondResult.PublicKeyXML);

        // Act - 验证新证书
        secondResult.Certificate.Position = 0;
        bool verifyResult = _verificationService.Verify(secondResult.Certificate, privateKey, "ExistingKeysTest", out DateTimeOffset? expirationTime);

        // Assert
        Assert.IsTrue(verifyResult);
        Assert.IsNotNull(expirationTime);
        Assert.IsLessThan(1, Math.Abs((newOptions.ExpirationTime - expirationTime.Value).TotalSeconds));
    }

    /// <summary>
    /// 测试使用流验证证书是否成功
    /// 验证证书验证器能够正确处理流格式的证书和私钥
    /// </summary>
    [TestMethod]
    public void VerifyCertificateWithStream_ShouldSucceed()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "StreamTestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048
        };

        CertificateResult result = _generatorService.Generate(options);

        // 创建内存流用于测试
        MemoryStream certificateStream = new();
        MemoryStream privateKeyStream = new();
        
        result.Certificate.CopyTo(certificateStream);
        result.PrivateKeyXML.CopyTo(privateKeyStream);

        // Act
        certificateStream.Position = 0;
        privateKeyStream.Position = 0;
        bool verifyResult = _verificationService.Verify(certificateStream, privateKeyStream, "StreamTestProject", out DateTimeOffset? expirationTime);

        // Assert
        Assert.IsTrue(verifyResult);
        Assert.IsNotNull(expirationTime);
    }

    /// <summary>
    /// 测试使用FileInfo验证证书是否成功
    /// 验证证书验证器能够正确处理FileInfo格式的证书文件
    /// </summary>
    [TestMethod]
    public void VerifyCertificateWithFileInfo_ShouldSucceed()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "FileInfoTestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048
        };

        CertificateFileResult fileResult = _generatorService.GenerateToFile(_testDirectory, options);

        // Act
        bool verifyResult = _verificationService.Verify(fileResult.Certificate, fileResult.PrivateKeyXML, "FileInfoTestProject", out DateTimeOffset? expirationTime);

        // Assert
        Assert.IsTrue(verifyResult);
        Assert.IsNotNull(expirationTime);
    }

    /// <summary>
    /// 测试使用null证书验证时是否抛出异常
    /// 验证证书验证器在接收到null证书时能够正确抛出ArgumentNullException
    /// </summary>
    [TestMethod]
    public void VerifyCertificate_ShouldThrowException_WithNullCertificate()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            _verificationService.Verify((Stream)null!, "test", "test", out _));
    }

    /// <summary>
    /// 测试使用空私钥验证时是否抛出异常
    /// 验证证书验证器在接收到空私钥时能够正确抛出ArgumentException
    /// </summary>
    [TestMethod]
    public void VerifyCertificate_ShouldThrowException_WithEmptyPrivateKey()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "TestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30)
        };
        CertificateResult result = _generatorService.Generate(options);

        // Act & Assert
        result.Certificate.Position = 0;
        Assert.ThrowsExactly<ArgumentException>(() => 
            _verificationService.Verify(result.Certificate, "", "TestProject", out _));
    }

    /// <summary>
    /// 测试使用null选项生成证书时是否抛出异常
    /// 验证证书生成器在接收到null选项时能够正确抛出ArgumentNullException
    /// </summary>
    [TestMethod]
    public void GenerateCertificate_ShouldThrowException_WithNullOptions()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            _generatorService.Generate(null!));
    }

    /// <summary>
    /// 测试验证被篡改的证书是否失败
    /// 验证当证书数据被篡改时，证书验证应该失败，确保安全性
    /// </summary>
    [TestMethod]
    public void VerifyTamperedCertificate_ShouldFail()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "TamperTestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048
        };

        CertificateResult result = _generatorService.Generate(options);
        
        // 读取私钥
        result.PrivateKeyXML.Position = 0;
        using StreamReader privateKeyReader = new(result.PrivateKeyXML);
        string privateKey = privateKeyReader.ReadToEnd();

        // 篡改证书数据
        result.Certificate.Position = 0;
        using MemoryStream memoryStream = new();
        result.Certificate.CopyTo(memoryStream);
        byte[] certificateBytes = memoryStream.ToArray();
        
        // 修改一些字节来模拟篡改
        if (certificateBytes.Length > 10)
        {
            certificateBytes[5] ^= 0xFF; // 翻转一个字节
        }

        // Act - 验证被篡改的证书
        using MemoryStream tamperedStream = new(certificateBytes);
        bool verifyResult = _verificationService.Verify(tamperedStream, privateKey, "TamperTestProject", out DateTimeOffset? expirationTime);

        // Assert
        Assert.IsFalse(verifyResult);
        Assert.IsNull(expirationTime);
    }

    /// <summary>
    /// 测试生成证书时包含PEM格式密钥文件
    /// 验证当IncludePublicKey为true时，生成的证书结果包含PEM格式的公钥和私钥文件
    /// </summary>
    [TestMethod]
    public void GenerateCertificate_ShouldIncludePemFiles_WhenIncludePublicKeyIsTrue()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "PemTestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048,
            IncludePublicKey = true
        };

        // Act
        CertificateResult result = _generatorService.Generate(options);

        // Assert - 验证内存流结果包含PEM文件
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Certificate);
        Assert.IsNotNull(result.PrivateKeyXML);
        Assert.IsNotNull(result.PrivateKeyPem);
        Assert.IsNotNull(result.PublicKeyXML);
        Assert.IsNotNull(result.PublicKeyPem);

        // 验证PEM格式内容
        result.PrivateKeyPem.Position = 0;
        result.PublicKeyPem!.Position = 0;
        
        using StreamReader privateKeyPemReader = new(result.PrivateKeyPem);
        using StreamReader publicKeyPemReader = new(result.PublicKeyPem);
        
        string privateKeyPem = privateKeyPemReader.ReadToEnd();
        string publicKeyPem = publicKeyPemReader.ReadToEnd();
        
        Assert.Contains("-----BEGIN PRIVATE KEY-----", privateKeyPem);
        Assert.Contains("-----END PRIVATE KEY-----", privateKeyPem);
        Assert.Contains("-----BEGIN PUBLIC KEY-----", publicKeyPem);
        Assert.Contains("-----END PUBLIC KEY-----", publicKeyPem);
    }

    /// <summary>
    /// 测试生成证书到文件时创建PEM格式密钥文件
    /// 验证当IncludePublicKey为true时，生成的证书文件包含PEM格式的公钥和私钥文件
    /// </summary>
    [TestMethod]
    public void GenerateCertificateToFile_ShouldCreatePemFiles_WhenIncludePublicKeyIsTrue()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "PemFileTestProject",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048,
            IncludePublicKey = true
        };

        // Act
        CertificateFileResult fileResult = _generatorService.GenerateToFile(_testDirectory, options);

        // Assert - 验证文件生成结果包含PEM文件
        Assert.IsNotNull(fileResult);
        Assert.IsTrue(fileResult.Certificate.Exists);
        Assert.IsTrue(fileResult.PrivateKeyXML.Exists);
        Assert.IsTrue(fileResult.PrivateKeyPem.Exists);
        Assert.IsNotNull(fileResult.PublicKeyXML);
        Assert.IsTrue(fileResult.PublicKeyXML!.Exists);
        Assert.IsNotNull(fileResult.PublicKeyPem);
        Assert.IsTrue(fileResult.PublicKeyPem!.Exists);

        // 验证PEM文件内容
        string privateKeyPem = File.ReadAllText(fileResult.PrivateKeyPem.FullName);
        string publicKeyPem = File.ReadAllText(fileResult.PublicKeyPem.FullName);
        
        Assert.Contains("-----BEGIN PRIVATE KEY-----", privateKeyPem);
        Assert.Contains("-----END PRIVATE KEY-----", privateKeyPem);
        Assert.Contains("-----BEGIN PUBLIC KEY-----", publicKeyPem);
        Assert.Contains("-----END PUBLIC KEY-----", publicKeyPem);
    }

    /// <summary>
    /// 测试生成证书到文件时只创建私钥PEM文件
    /// 验证当IncludePublicKey为false时，生成的证书文件只包含私钥PEM文件，不包含公钥文件
    /// </summary>
    [TestMethod]
    public void GenerateCertificateToFile_ShouldCreateOnlyPrivateKeyPem_WhenIncludePublicKeyIsFalse()
    {
        // Arrange
        CertificateOptions options = new()
        {
            ProjectName = "PrivateKeyOnlyPemTest",
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(30),
            KeySize = 2048,
            IncludePublicKey = false
        };

        // Act
        CertificateFileResult fileResult = _generatorService.GenerateToFile(_testDirectory, options);

        // Assert - 验证只生成私钥PEM文件
        Assert.IsNotNull(fileResult);
        Assert.IsTrue(fileResult.Certificate.Exists);
        Assert.IsTrue(fileResult.PrivateKeyXML.Exists);
        Assert.IsTrue(fileResult.PrivateKeyPem.Exists);
        Assert.IsNull(fileResult.PublicKeyXML);
        Assert.IsNull(fileResult.PublicKeyPem);

        // 验证私钥PEM文件内容
        string privateKeyPem = File.ReadAllText(fileResult.PrivateKeyPem.FullName);
        Assert.Contains("-----BEGIN PRIVATE KEY-----", privateKeyPem);
        Assert.Contains("-----END PRIVATE KEY-----", privateKeyPem);
    }
}
