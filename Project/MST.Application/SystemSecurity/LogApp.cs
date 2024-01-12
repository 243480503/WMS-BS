/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.IRepository.SystemSecurity;
using MST.Repository.SystemSecurity;
using System;
using System.Collections.Generic;

namespace MST.Application.SystemSecurity
{
    public class LogApp
    {
        private ILogRepository service = new LogRepository();

        public List<LogEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<LogEntity>();
            var queryParam = queryJson.ToJObject();
            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.F_Account.Contains(keyword));
            }
            if (!queryParam["timeType"].IsEmpty())
            {
                string timeType = queryParam["timeType"].ToString();
                DateTime startTime = DateTime.Now.ToString("yyyy-MM-dd").ToDate();
                DateTime endTime = DateTime.Now.ToString("yyyy-MM-dd").ToDate().AddDays(1);
                switch (timeType)
                {
                    case "1":
                        break;
                    case "2":
                        startTime = DateTime.Now.AddDays(-7);
                        break;
                    case "3":
                        startTime = DateTime.Now.AddMonths(-1);
                        break;
                    case "4":
                        startTime = DateTime.Now.AddMonths(-3);
                        break;
                    default:
                        break;
                }
                expression = expression.And(t => t.F_Date >= startTime && t.F_Date <= endTime);
            }

            if (!queryParam["resultType"].IsEmpty())
            {
                string resultType = queryParam["resultType"].ToString();
                switch (resultType)
                {
                    case "1":
                        break;
                    case "2":
                        expression = expression.And(t => t.F_Result == false);

                        break;
                    case "3":
                        expression = expression.And(t => t.F_Result == true);
                        break;
                    default:
                        break;
                }
            }

            if (!queryParam["roleType"].IsEmpty())
            {

                string resultType = queryParam["roleType"].ToString();
                switch (resultType)
                {
                    case "1"://全部
                        break;
                    case "2"://用户
                        expression = expression.And(t => t.F_Account != "WCS" && t.F_Account != "ERP" && t.F_Account != "Sys");

                        break;
                    case "3"://ERP
                        expression = expression.And(t => t.F_Account == "ERP");
                        break;
                    case "4"://WCS
                        expression = expression.And(t => t.F_Account == "WCS");
                        break;
                    case "5"://系统
                        expression = expression.And(t => t.F_Account == "Sys");
                        break;
                    default:
                        break;
                }
            }
            return service.FindList(expression, pagination);
        }
        public LogEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void RemoveLog(string keepTime)
        {
            DateTime operateTime = DateTime.Now;
            if (keepTime == "7")            //保留近一周
            {
                operateTime = DateTime.Now.AddDays(-7);
            }
            else if (keepTime == "1")       //保留近一个月
            {
                operateTime = DateTime.Now.AddMonths(-1);
            }
            else if (keepTime == "3")       //保留近三个月
            {
                operateTime = DateTime.Now.AddMonths(-3);
            }
            var expression = ExtLinq.True<LogEntity>();
            expression = expression.And(t => t.F_Date <= operateTime);
            service.Delete(expression);
        }
        public void WriteDbLog(bool result, string resultLog)
        {
            LogEntity logEntity = new LogEntity();
            logEntity.F_Id = Common.GuId();
            logEntity.F_Date = DateTime.Now;
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_IPAddress = Net.Ip;
            logEntity.F_IPAddressName = Net.GetLocation(logEntity.F_IPAddress);
            logEntity.F_Result = result;
            logEntity.F_Description = resultLog;
            logEntity.Create();
            service.Insert(logEntity);
        }
        public void WriteDbLog(LogEntity logEntity)
        {
            logEntity.F_Id = Common.GuId();
            logEntity.F_Date = DateTime.Now;
            logEntity.F_IPAddress = Net.Ip;
            logEntity.F_IPAddressName = Net.GetLocation(logEntity.F_IPAddress);
            logEntity.Create();
            service.Insert(logEntity);
        }

        public void WriteDbLogThread(LogEntity logEntity, OperatorModel user)
        {
            logEntity.F_Id = Common.GuId();
            logEntity.F_Date = DateTime.Now;
            logEntity.F_IPAddress = Net.Ip;
            logEntity.F_IPAddressName = Net.GetLocation(logEntity.F_IPAddress);
            logEntity.CreateForThread(user);
            service.Insert(logEntity);
        }
    }
}
