using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Materal.MergeBlock.COA
{
    /// <summary>
    /// 证书授权模块
    /// </summary>
    public class COAModule() : MergeBlockModule("证书授权模块")
    {
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="context"></param>
        public override void OnConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.TryAddSingleton<ICOAService, COAService>();
            context.Services.TryAddSingleton<ICertificateVerificationService, CertificateVerificationService>();
            base.OnConfigureServices(context);
        }
        /// <summary>
        /// 应用程序初始化
        /// </summary>
        /// <param name="context"></param>
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            ICOAService coaService = context.ServiceProvider.GetRequiredService<ICOAService>();
            COAResultDTO dto = coaService.VerifyCertificatesAuthorization();
            if (dto.IsValidity) return;
            ILogger? logger = context.ServiceProvider.GetService<ILogger<COAModule>>();
            if (dto.EndDate is null)
            {
                logger?.LogWarning("证书授权已过期");
            }
            else
            {
                logger?.LogWarning($"证书授权已过期{dto.ExpirationDays}天");
            }
        }
    }
}
