namespace Materal.MergeBlock.COA
{
    /// <summary>
    /// 证书验证帮助类
    /// </summary>
    public class COAService : ICOAService
    {
        /// <summary>
        /// 验证证书授权
        /// </summary>
        /// <param name="endDate"></param>
        /// <returns></returns>
        /// <exception cref="MergeBlockException"></exception>
        public bool VerifyCertificatesAuthorization(out DateTimeOffset? endDate)
        {
            IConfiguration config = MateralServices.ServiceProvider.GetRequiredService<IConfiguration>();
            string? applicationName = config["MergeBlock:ApplicationName"];
            if (string.IsNullOrWhiteSpace(applicationName)) throw new MergeBlockException("获取应用程序名称失败[MergeBlock:ApplicationName]");
            string baseDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('\\') :
                typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('/');
#if DEBUG
            COAHost.WriteCertificateFile(applicationName, DateTimeOffset.Now.AddDays(1), baseDirectory);
            string publicKeyFilePath = Path.Combine(baseDirectory, "public.key");
            if (File.Exists(publicKeyFilePath))
            {
                File.Delete(publicKeyFilePath);
            }
#endif
            string privateKeyPath = Path.Combine(baseDirectory, "private.key");
            string privateKey = File.ReadAllText(privateKeyPath);
            return COAHost.VerifyAuthorization(applicationName, privateKey, out endDate);
        }
    }
}
