namespace Materal.MergeBlock.COA;

/// <summary>
/// 证书授权服务
/// </summary>
public interface ICOAService
{
    /// <summary>
    /// 验证证书授权
    /// </summary>
    /// <returns></returns>
    COAResultDTO VerifyCertificatesAuthorization();
}
