/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace MST.Web.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public virtual ActionResult Index()
        {
            ViewBag.SoftName = Configs.GetValue("SoftName");
            ViewBag.SoftPubName = Configs.GetValue("SoftPubName");
            ViewBag.WebSite = Configs.GetValue("WebSite");
            ViewBag.GithubUrl = Configs.GetValue("GithubUrl");
            ViewBag.Version = Configs.GetValue("Version");
            ViewBag.CompanyName = Configs.GetValue("CompanyName");
            ViewBag.LogoPath = Configs.GetValue("LogoPath");
            ViewBag.LogoScale = Configs.GetValue("LogoScale").Split(' ')[0];
            ViewBag.IsForever = true;   //默认许可证永久有效
            ViewBag.DayNum = int.MaxValue; //默认许可证永久有效

            return View();
        }

        [HttpGet]
        public ActionResult GetAuthCode()
        {
            return File(new VerifyCode().GetVerifyCode(), @"image/Gif");
        }
        [HttpGet]
        public ActionResult OutLogin()
        {
            new LogApp().WriteDbLog(new LogEntity
            {
                F_ModuleName = "系统登录",
                F_Type = DbLogType.Exit.ToString(),
                F_Account = OperatorProvider.Provider.GetCurrent() == null ? "" : OperatorProvider.Provider.GetCurrent().UserCode,
                F_NickName = OperatorProvider.Provider.GetCurrent() == null ? "" : OperatorProvider.Provider.GetCurrent().UserName,
                F_Result = true,
                F_Description = "安全退出系统",
            });
            Session.Abandon();
            Session.Clear();
            OperatorProvider.Provider.RemoveCurrent();
            return RedirectToAction("Index", "Login");
        }
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult CheckLogin(string username, string password, string code)
        {
            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "系统登录";
            logEntity.F_Type = DbLogType.Login.ToString();
            try
            {
                if (code != "-0001") //不为-0001则需要验证码
                {
                    if (Session["MST_session_verifycode"].IsEmpty() || Md5.md5(code.ToLower(), 16) != Session["MST_session_verifycode"].ToString())
                    {
                        throw new Exception("验证码错误，请重新输入");
                    }
                }

                bool isIPPass = CheckIP(Net.Ip);
                if (!isIPPass)
                {
                    throw new Exception("IP地址限制，无法登录");
                }
                UserEntity userEntity = new UserApp().CheckLogin(username, password);
                if (userEntity != null)
                {
                    OperatorModel operatorModel = new OperatorModel();
                    operatorModel.UserId = userEntity.F_Id;
                    operatorModel.UserCode = userEntity.F_Account;
                    operatorModel.UserName = userEntity.F_RealName;
                    operatorModel.CompanyId = userEntity.F_OrganizeId;
                    operatorModel.DepartmentId = userEntity.F_DepartmentId;
                    operatorModel.RoleId = userEntity.F_RoleId;
                    operatorModel.LoginIPAddress = Net.Ip;
                    operatorModel.LoginIPAddressName = Net.GetLocation(operatorModel.LoginIPAddress);
                    operatorModel.LoginTime = DateTime.Now;
                    operatorModel.LoginToken = DESEncrypt.Encrypt(Guid.NewGuid().ToString());
                    if (userEntity.F_Account == "admin")
                    {
                        operatorModel.IsSystem = true;
                    }
                    else
                    {
                        operatorModel.IsSystem = false;
                    }
                    OperatorProvider.Provider.AddCurrent(operatorModel);
                    logEntity.F_Account = userEntity.F_Account;
                    logEntity.F_NickName = userEntity.F_RealName;
                    logEntity.F_Result = true;
                    logEntity.F_Description = "登录成功";
                    new LogApp().WriteDbLog(logEntity);
                }
                return Content(new AjaxResult { state = ResultType.success.ToString(), message = "登录成功。" }.ToJson());
            }
            catch (Exception ex)
            {
                logEntity.F_Account = username;
                logEntity.F_NickName = username;
                logEntity.F_Result = false;
                logEntity.F_Description = "登录失败，" + ex.Message;
                new LogApp().WriteDbLog(logEntity);
                return Content(new AjaxResult { state = ResultType.error.ToString(), message = ex.Message }.ToJson());
            }
        }

        private bool CheckIP(string ip)
        {
            string[] ipArray = ip.Split('.');
            long ipNum = ((long)ipArray[0].ToInt()) * 256 * 256 * 256 + ((long)ipArray[1].ToInt()) * 256 * 256 + ((long)ipArray[2].ToInt()) * 256 + (long)ipArray[3].ToInt();

            IList<FilterIPEntity> filterIPList_Wirte = new FilterIPApp().FindList(o => o.F_Type == true).ToList(); //白名单列表
            IList<FilterIPEntity> filterIPList_Black = new FilterIPApp().FindList(o => o.F_Type == false).ToList(); //黑名单列表

            if (filterIPList_Wirte.Count > 0)  //如果设置了白名单，则黑名单失效
            {
                foreach (FilterIPEntity cell in filterIPList_Wirte)
                {
                    string[] beginArray = cell.F_StartIP.Split('.');
                    long beginIP = ((long)beginArray[0].ToInt()) * 256 * 256 * 256 + ((long)beginArray[1].ToInt()) * 256 * 256 + ((long)beginArray[2].ToInt()) * 256 + (long)beginArray[3].ToInt();

                    string[] endArray = cell.F_EndIP.Split('.');
                    long endIP = ((long)endArray[0].ToInt()) * 256 * 256 * 256 + ((long)endArray[1].ToInt()) * 256 * 256 + ((long)endArray[2].ToInt()) * 256 + (long)endArray[3].ToInt();

                    if (beginIP <= ipNum && endIP >= ipNum) //白名单地址段内
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                foreach (FilterIPEntity cell in filterIPList_Black)
                {
                    string[] beginArray = cell.F_StartIP.Split('.');
                    long beginIP = ((long)beginArray[0].ToInt()) * 256 * 256 * 256 + ((long)beginArray[1].ToInt()) * 256 * 256 + ((long)beginArray[2].ToInt()) * 256 + (long)beginArray[3].ToInt();

                    string[] endArray = cell.F_EndIP.Split('.');
                    long endIP = ((long)endArray[0].ToInt()) * 256 * 256 * 256 + ((long)endArray[1].ToInt()) * 256 * 256 + ((long)endArray[2].ToInt()) * 256 + (long)endArray[3].ToInt();

                    if (beginIP <= ipNum && endIP >= ipNum) //黑名单地址段内
                    {
                        return false;
                    }
                }
                return true;
            }

        }
    }
}
