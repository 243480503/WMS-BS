/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System.Web.Mvc;

namespace MST.Web.Areas.SystemManage
{
    public class PC_MsgManageRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PC_MsgManage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
              this.AreaName + "_Default",
              this.AreaName + "/{controller}/{action}/{id}",
              new { area = this.AreaName, controller = "Home", action = "Index", id = UrlParameter.Optional },
              new string[] { "MST.Web.Areas." + this.AreaName + ".Controllers" }
            );
        }
    }
}
