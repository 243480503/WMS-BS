/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.APIPost;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
/**********精确到主容器的波次运算***************/
namespace MST.Web.Areas.PC_MsgManage.Controllers
{
    public class MsgController : ControllerBase
    {
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SendMsgApp msgApp = new T_SendMsgApp();


        #region 获取列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            IList<SendMsgModel> sendMsgModelList = new List<SendMsgModel>();
            IList<ItemsDetailEntity> itemsDetailType = itemsDetailApp.FindEnum<T_SendMsgEntity>(o => o.MsgType);
            //IList<T_SendMsgEntity> msgList = msgApp.FindList(o => o.F_DeleteMark == false && o.ReceiveID == user.UserId).OrderByDescending(o => o.F_CreatorTime).ToList();
            IList<T_SendMsgEntity> msgList = msgApp.GetList(pagination, queryJson);
            foreach (T_SendMsgEntity msg in msgList)
            {
                SendMsgModel model = msg.ToObject<SendMsgModel>();
                model.MsgTypeName = itemsDetailType.FirstOrDefault(o => o.F_ItemCode == msg.MsgType).F_ItemName;
                sendMsgModelList.Add(model);
            }

            var resultList = new
            {
                rows = sendMsgModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        #endregion

        #region 查看消息详情
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = msgApp.GetForm(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region 单条标记为已读
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SetRead(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "MsgController.SetRead"; //按实际情况修改
                logObj.Parms = new { keyValue = keyValue }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "消息管理"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "单条标记已读"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_SendMsgEntity msgEntity = db.FindEntity<T_SendMsgEntity>(o => o.F_Id == keyValue);
                    if (msgEntity.IsReadOver == "true")
                    {
                        return Error("消息已经为已读", "");
                    }

                    msgEntity.IsReadOver = "true";
                    db.Update<T_SendMsgEntity>(msgEntity);
                    db.SaveChanges();
                    db.CommitWithOutRollBack();

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作失败。", ex.ToJson());
                }
            }
        }
        #endregion

        #region 批量标记已读
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SetAllRead()
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "MsgController.SetAllRead"; //按实际情况修改
                logObj.Parms = new { }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "消息管理"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "批量标记已读"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    OperatorModel user = OperatorProvider.Provider.GetCurrent();
                    IList<T_SendMsgEntity> msgEntityList = db.FindList<T_SendMsgEntity>(o => o.ReceiveID == user.UserId && o.IsReadOver == "false");
                    foreach (T_SendMsgEntity cell in msgEntityList)
                    {
                        cell.IsReadOver = "true";
                        db.Update<T_SendMsgEntity>(cell);
                        db.SaveChanges();
                    }
                    db.CommitWithOutRollBack();

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作失败。", ex.ToJson());
                }
            }
        }
        #endregion

        #region 删除消息
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "MsgController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "消息管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除消息";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                msgApp.Delete(o => o.F_Id == keyValue);
                logObj.Message = "删除成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("删除成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("删除失败。", ex.ToJson());
            }
        }
        #endregion

        #region 获取推送给用户的消息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetUIMsg(string keyValue)
        {
            var data = msgApp.GetForm(keyValue);
            return Content(data.ToJson());
        }
        #endregion
    }
}
