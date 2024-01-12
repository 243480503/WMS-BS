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

namespace MST.Web.Areas.PC_MoveManage.Controllers
{
    public class MoveController : ControllerBase
    {
        private static object lockObj = new object();
        private T_MoveApp moveApp = new T_MoveApp();
        private T_MoveRecordApp moveRecordApp = new T_MoveRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_TaskApp taskApp = new T_TaskApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private T_ItemApp itemApp = new T_ItemApp();

        #region 移库单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            var data = moveApp.GetList(pagination, queryJson);
            IList<MoveModel> moveList = new List<MoveModel>();
            foreach (T_MoveEntity item in data)
            {
                MoveModel model = item.ToObject<MoveModel>();
                model.StateName = itemsDetailApp.FindEnum<T_MoveEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.GenTypeName = itemsDetailApp.FindEnum<T_MoveEntity>(o => o.GenType).FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;
                model.AreaCodeName = new T_AreaApp().FindEntity(o => o.AreaCode == item.AreaCode).AreaName;
                moveList.Add(model);
            }

            var resultList = new
            {
                rows = moveList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 移库单详情
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = moveApp.GetForm(keyValue);
            MoveModel model = data.ToObject<MoveModel>();
            model.StateName = itemsDetailApp.FindEnum<T_MoveEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == data.State).F_ItemName;
            model.IsHaveDetail = moveRecordApp.FindList(o => o.MoveID == keyValue).Count() > 0 ? "true" : "false";
            return Content(model.ToJson());
        }
        #endregion

        #region 区域下拉框列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetAreaDicList()
        {
            List<T_AreaEntity> areaList = new List<T_AreaEntity>();
            areaList = areaApp.FindList(o => (o.AreaType == "Tunnel" || o.AreaType == "Concentrate" || o.AreaType == "AGV") && o.AreaCode != FixType.Area.EmptyArea.ToString()).ToList();
            return Content(areaList.ToJson());
        }
        #endregion

        #region 新建/编辑移库单
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_MoveEntity MoveEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "MoveController.SubmitForm";
                logObj.Parms = new { MoveEntity = MoveEntity, keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "移库单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存移库单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (MoveEntity.State != "New") return Error("单据不是新建状态", "");

                    T_AreaEntity area = areaApp.FindEntity(o => o.AreaCode == MoveEntity.AreaCode);
                    MoveEntity.AreaID = area.F_Id;
                    MoveEntity.GenType = "MAN";
                    MoveEntity.TaskCellNum = 0;
                    if (!string.IsNullOrEmpty(MoveEntity.Remark)) MoveEntity.Remark = MoveEntity.Remark.Replace("\n", " ");

                    if (string.IsNullOrEmpty(keyValue))
                    {
                        MoveEntity.F_Id = Guid.NewGuid().ToString();
                        db.Insert<T_MoveEntity>(MoveEntity);
                    }
                    else
                    {
                        MoveEntity.F_Id = keyValue;
                        db.Update<T_MoveEntity>(MoveEntity);
                    }

                    db.CommitWithOutRollBack();

                    /*************************************************/

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

        #region 获取供应商列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetSupplierList()
        {
            var data = supplierApp.FindList(o => o.F_DeleteMark == false);
            return Content(data.ToJson());
        }
        #endregion

        #region 删除移库单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "MoveController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "移库单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除移库单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_MoveEntity moveEntity = db.FindEntity<T_MoveEntity>(o => o.F_Id == keyValue);
                    if (moveEntity.State != "New") return Error("非新建状态不可删除", "");

                    List<T_MoveRecordEntity> recordList = db.FindList<T_MoveRecordEntity>(o => o.MoveID == moveEntity.F_Id);
                    foreach (T_MoveRecordEntity detail in recordList)
                    {
                        db.Delete<T_MoveRecordEntity>(detail);
                    }
                    db.SaveChanges();
                    db.Delete<T_MoveEntity>(moveEntity);
                    db.SaveChanges();

                    db.CommitWithOutRollBack();

                    /**************************************************/

                    logObj.Message = "删除成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("删除成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("删除失败。", ex.ToJson());
                }
            }
        }
        #endregion

        #region 开始移库
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult BeginMove(string moveID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "MoveController.BeginMove";
                logObj.Parms = new { moveID = moveID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "移库单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始移库";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/

                    T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();
                    bool IsHaveOffLine = offLineDownApp.IsHaveOff(db);
                    if (IsHaveOffLine)
                    {
                        return Error("存在未处理的离线数据", "");
                    }

                    IList<T_MoveEntity> otherMove = db.FindList<T_MoveEntity>(o => o.F_Id != moveID && o.State != "New" && o.State != "Over");
                    if(otherMove.Count>0)
                    {
                        return Error("存在未结束的移库单", "");
                    }

                    WCSResult rst = new WCSResult();
                    T_MoveEntity moveEntity = db.FindEntity<T_MoveEntity>(o => o.F_Id == moveID);
                    if (moveEntity.State != "New")
                    {
                        return Error("单据不是新建状态", "");
                    }

                    moveEntity.State = "Moving";
                    db.Update<T_MoveEntity>(moveEntity);
                    db.SaveChanges();

                    IList<string> taskNoList = new List<string>();
                    int MaxTaskNum = RuleConfig.MoveRule.MoveTaskRule.MaxTaskNum ?? Int32.MaxValue;
                    AjaxResult ajaxRes = new T_MoveApp().GenMoveRecord(db, moveID, MaxTaskNum, ref taskNoList);
                    if ((ResultType)ajaxRes.state != ResultType.success)
                    {
                        return Error(ajaxRes.message, "");
                    }

                    if (taskNoList.Count < 1) //一条任务也没产生，直接结束
                    {
                        moveEntity.State = "Over";
                        db.Update<T_MoveEntity>(moveEntity);
                        db.SaveChanges();
                    }

                    //执行并发送任务
                    rst = new WCSPost().SendTaskBulk(db, taskNoList);
                    if (!((WCSResult)rst).IsSuccess)
                    {
                        db.RollBack();
                        return Error(rst.FailMsg, "");
                    }
                    db.CommitWithOutRollBack();

                    /*************************************************/

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

        #region 结束移库
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult StopMove(string moveID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "MoveController.StopMove";
                logObj.Parms = new { moveID = moveID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "移库单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "结束移库";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    WCSResult rst = new WCSResult();
                    T_MoveEntity moveEntity = db.FindEntity<T_MoveEntity>(o => o.F_Id == moveID);

                    if (moveEntity.State == "Overing")
                    {
                        return Error("单据已处于结束中。", "");
                    }
                    if (moveEntity.State == "Over")
                    {
                        return Error("单据已结束。", "");
                    }

                    if (moveEntity.IsAuto != "true")
                    {
                        return Error("非自动移库将自动结束", "");
                    }

                    List<T_MoveRecordEntity> recordList = db.FindList<T_MoveRecordEntity>(o => o.MoveID == moveID);
                    if (recordList.Count < 1)
                    {
                        return Error("移库没有任何明细", "");
                    }

                    IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderCode == moveEntity.MoveCode).ToList();
                    if (taskList.Count < 1) //任务表中不存在该移库单的任务
                    {
                        moveEntity.State = "Over";
                    }
                    else
                    {
                        moveEntity.State = "Overing"; //此处标记为结束中，真正结束在任务完成后
                    }
                    db.Update<T_MoveEntity>(moveEntity);
                    db.SaveChanges();
                    db.CommitWithOutRollBack();

                    /*************************************************/

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

    }
}
