namespace Materal.MergeBlock.COA
{
    /// <summary>
    /// 证书验证帮助类
    /// </summary>
    public class COAService(ICertificateVerificationService certificateVerificationService) : ICOAService
    {
        /// <summary>
        /// 验证证书授权
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MergeBlockException"></exception>
        public COAResultDTO VerifyCertificatesAuthorization()
        {
            IConfiguration config = MateralServices.ServiceProvider.GetRequiredService<IConfiguration>();
            string? applicationName = config["MergeBlock:ApplicationName"];
            if (string.IsNullOrWhiteSpace(applicationName)) throw new MergeBlockException("获取应用程序名称失败[MergeBlock:ApplicationName]");
            string baseDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                typeof(COAModule).Assembly.GetDirectoryPath().TrimEnd('\\') :
                typeof(COAModule).Assembly.GetDirectoryPath().TrimEnd('/');
            string certificatePath = Path.Combine(baseDirectory, "MateralCertificate.cer");
            string privateKeyPath = Path.Combine(baseDirectory, "private.key");
            bool result = certificateVerificationService.Verify(certificatePath, privateKeyPath, applicationName, out DateTimeOffset? endTime);
            COAResultDTO dto = new(result, endTime);
            return dto;
        }
    }
}
