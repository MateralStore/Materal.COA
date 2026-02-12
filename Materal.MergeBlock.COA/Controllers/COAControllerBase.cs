using Materal.MergeBlock.Web.Abstractions.Controllers;

namespace Materal.MergeBlock.COA.Controllers;

/// <summary>
/// 证书授权控制器基类
/// </summary>
[ApiController, Route("/COAAPI/[controller]/[action]")]
public abstract class COAControllerBase : MergeBlockController
{
}
