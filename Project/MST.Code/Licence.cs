/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System.Configuration;
using System.Web;

namespace MST.Code
{
    public sealed class Licence
    {
        public static bool IsLicence(string key)
        {
            string host = HttpContext.Current.Request.Url.Host.ToLower();
            if (host.Equals("localhost"))
                return true;
            string licence = ConfigurationManager.AppSettings["LicenceKey"];
            if (licence != null && licence == Md5.md5(key, 32))
                return true;

            return false;
        }
        public static string GetLicence()
        {
            var licence = Configs.GetValue("LicenceKey");
            if (string.IsNullOrEmpty(licence))
            {
                licence = Common.GuId();
                Configs.SetValue("LicenceKey", licence);
            }
            return Md5.md5(licence, 32);
        }
    }
}
