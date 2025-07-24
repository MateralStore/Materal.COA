using Materal.Utils.Model;
using Microsoft.AspNetCore.Authorization;

namespace Materal.MergeBlock.COA.Controllers
{
    /// <summary>
    /// 枚举控制器
    /// </summary>
    public class COAController(ICOAService coaService) : COAControllerBase
    {
        /// <summary>
        /// 获取证书授权状态
        /// </summary>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        public ResultModel<COAResultDTO> GetCOAState()
        {
            COAResultDTO result = new()
            {
                IsValidity = coaService.VerifyCertificatesAuthorization(out DateTimeOffset? endDate)
            };
            if (endDate is not null)
            {
                result.EndDate = endDate.Value;
            }
            return ResultModel<COAResultDTO>.Success(result, "获取成功");
        }
    }
}
