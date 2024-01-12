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
using MST.Application.WebMsg;
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

namespace MST.Web.Areas.PC_OutBoundManage.Controllers
{
    public class OutBoundController : ControllerBase
    {
        private static object lockObj = new object();
        private T_OutBoundApp outBoundApp = new T_OutBoundApp();
        private T_OutBoundDetailApp outBoundDetailApp = new T_OutBoundDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_InBoundApp inboundApp = new T_InBoundApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_QAApp qaApp = new T_QAApp();

        #region 出库单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            var data = outBoundApp.GetList(pagination, queryJson);
            IList<OutBoundModel> OutBoundList = new List<OutBoundModel>();
            foreach (T_OutBoundEntity item in data)
            {
                OutBoundModel model = item.ToObject<OutBoundModel>();
                model.StateName = itemsDetailApp.FindEnum<T_OutBoundEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.GenTypeName = itemsDetailApp.FindEnum<T_OutBoundEntity>(o => o.GenType).FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;
                model.OutBoundTypeName = itemsDetailApp.FindEnum<T_OutBoundEntity>(o => o.OutBoundType).FirstOrDefault(o => o.F_ItemCode == item.OutBoundType).F_ItemName;
                OutBoundList.Add(model);
            }

            var resultList = new
            {
                rows = OutBoundList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 出库单详情
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = outBoundApp.GetForm(keyValue);
            OutBoundModel model = data.ToObject<OutBoundModel>();
            model.StateName = itemsDetailApp.FindEnum<T_OutBoundEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == data.State).F_ItemName;
            model.GenTypeName = itemsDetailApp.FindEnum<T_OutBoundEntity>(o => o.GenType).FirstOrDefault(o => o.F_ItemCode == data.GenType).F_ItemName;
            model.OutBoundTypeName = itemsDetailApp.FindEnum<T_OutBoundEntity>(o => o.OutBoundType).FirstOrDefault(o => o.F_ItemCode == data.OutBoundType).F_ItemName;
            if (model.OutBoundType == "VerBackOut" && (!string.IsNullOrEmpty(model.PointInBoundID)))
            {
                model.PointInBoundCode = inboundApp.FindEntity(o => o.F_Id == model.PointInBoundID).InBoundCode;
            }
            return Content(model.ToJson());
        }
        #endregion

        #region 质检单下拉联动入库单编码
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetQAInBoundJson(string qaId)
        {
            T_QAEntity qa = qaApp.FindEntity(o => o.F_Id == qaId);
            var data = inboundApp.FindEntity(o => o.RefOrderCode == qa.RefInBoundCode);
            return Content(data.ToJson());
        }
        #endregion

        #region 入库单列表（新建验退单时需要）
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetQAGridJson()
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                IQueryable<T_QAEntity> qaQuery = db.IQueryable<T_QAEntity>(o => true);
                IQueryable<T_ContainerDetailEntity> containerDetailQuery = db.IQueryable<T_ContainerDetailEntity>(o => o.CheckState == "UnQua");
                var data = qaQuery.Join(containerDetailQuery, m => m.F_Id, n => n.CheckID, (m, n) => new { F_Id = m.F_Id, RefOrderCode = m.RefOrderCode }).Distinct().OrderByDescending(o => o.RefOrderCode).ToList();
                return Content(data.ToJson());
            }
        }
        #endregion


        #region 打开明细维护窗口
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult TransferForm()
        {
            return View();
        }
        #endregion

        #region 修建/修改出库单
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_OutBoundEntity OutBoundEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutBoundController.SubmitForm"; //按实际情况修改
                logObj.Parms = new { OutBoundEntity = OutBoundEntity, keyValue = keyValue }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存出库单"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (!string.IsNullOrEmpty(OutBoundEntity.ReceiveDepartmentId))
                    {
                        OrganizeEntity organize = new OrganizeApp().FindEntity(o => o.F_Id == OutBoundEntity.ReceiveDepartmentId);
                        OutBoundEntity.ReceiveDepartmentId = organize.F_Id;
                        OutBoundEntity.ReceiveDepartment = organize.F_FullName;
                    }

                    if (!string.IsNullOrEmpty(OutBoundEntity.ReceiveUserId))
                    {
                        UserEntity user = new UserApp().FindEntity(o => o.F_Id == OutBoundEntity.ReceiveUserId);
                        OutBoundEntity.ReceiveUserId = user.F_Id;
                        OutBoundEntity.ReceiveUserName = user.F_RealName;
                    }


                    T_OutBoundEntity outBoundBefore = outBoundApp.FindEntity(o => o.F_Id == keyValue);
                    OutBoundEntity.GenType = "MAN";
                    if (!string.IsNullOrEmpty(OutBoundEntity.Remark))
                    {
                        OutBoundEntity.Remark = OutBoundEntity.Remark.Replace("\n", " ");
                    }

                    if (string.IsNullOrEmpty(OutBoundEntity.RefOrderCode))
                    {
                        OutBoundEntity.RefOrderCode = OutBoundEntity.OutBoundCode;
                    }

                    if (OutBoundEntity.OutBoundType == "VerBackOut") //验退必须选定质检单
                    {
                        if (string.IsNullOrEmpty(OutBoundEntity.QAID))
                        {
                            return Error("验退必须选定质检单", "操作失败");
                        }
                        T_QAEntity qa = db.FindEntity<T_QAEntity>(o => o.F_Id == OutBoundEntity.QAID);
                        T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == qa.RefInBoundCode);
                        OutBoundEntity.PointInBoundID = inbound.F_Id;
                    }
                    else
                    {
                        OutBoundEntity.PointInBoundID = "";
                        OutBoundEntity.QAID = "";
                    }

                    if (string.IsNullOrEmpty(keyValue)) //新增
                    {
                        OutBoundEntity.F_Id = Guid.NewGuid().ToString();
                        db.Insert<T_OutBoundEntity>(OutBoundEntity);
                    }
                    else
                    {
                        OutBoundEntity.F_Id = keyValue;
                        /// 已添加出库明细，禁止修改出库单出库方式
                        var detail = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == OutBoundEntity.F_Id);
                        if (detail.Count != 0)
                        {
                            if (OutBoundEntity.OutBoundType != outBoundBefore.OutBoundType)
                            {
                                db.RollBack();
                                return Error("已添加出库明细，禁止修改出库单类型", "操作失败");
                            }
                        }
                        db.Update<T_OutBoundEntity>(OutBoundEntity);
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

        #region 开始出库
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult BeginOut(string outBoundID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutBoundController.BeginOut"; //按实际情况修改
                logObj.Parms = new { outBoundID = outBoundID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始出库"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    lock (lockObj)
                    {
                        /*************************************************/
                        AjaxResult rst = new AjaxResult();
                        T_OutBoundEntity outEntity = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == outBoundID);
                        if (outEntity.State == "Outing" || outEntity.State == "Over")
                        {
                            return Error("单据状态不正确", "");
                        }

                        IList<T_OutBoundDetailEntity> outBoundDetailList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == outBoundID);
                        if (outBoundDetailList.Count < 1)
                        {
                            return Error("单据没有明细", "");
                        }
                        IList<T_OutBoundDetailEntity> emptyStationList = outBoundDetailList.Where(o => string.IsNullOrEmpty(o.StationID)).ToList();
                        if (emptyStationList.Count > 0)
                        {
                            string err = "项次[" + string.Join(",", emptyStationList.Select(o => o.SEQ).OrderBy(o => o.Value).ToList().ToArray()) + "]未指定出库站台";
                            return Error(err, "");
                        }

                        IList<string> stationArray = outBoundDetailList.Select(o => o.StationID).Distinct().ToArray();
                        IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => stationArray.Contains(o.F_Id));

                        stationList = stationList.Where(o => string.IsNullOrEmpty(o.CurOrderID)).ToList();
                        if (stationList.Count != stationArray.Count)
                        {
                            return Error("站台已被占用", "");
                        }

                        IList<T_OutBoundDetailEntity> needWaveList = new List<T_OutBoundDetailEntity>();
                        IList<T_OutBoundDetailEntity> needOutList = new List<T_OutBoundDetailEntity>();
                        foreach (T_OutBoundDetailEntity outBoundDtail in outBoundDetailList)
                        {
                            if (outBoundDtail.State == "New" && (outBoundDtail.ActionType == "Init"))
                            {
                                needWaveList.Add(outBoundDtail);
                            }
                            else if (outBoundDtail.State == "Waved")
                            {
                                needOutList.Add(outBoundDtail);
                            }
                        }

                        T_OutRecordApp outRecApp = new T_OutRecordApp();
                        //产生波次单
                        if (needWaveList.Count > 0)
                        {
                            string[] needOutArray = needWaveList.Select(o => o.F_Id).ToArray();
                            rst = outRecApp.WaveGen(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                            if ((ResultType)rst.state != ResultType.success)
                            {
                                db.RollBack();
                                return Error(rst.message, "");
                            }
                            db.SaveChanges();
                        }
                        //执行并发送任务
                        IList<string> outBoundDetailIDList = outBoundDetailList.Select(o => o.F_Id).ToList();
                        rst = outRecApp.OutDetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                        if ((ResultType)rst.state != ResultType.success)
                        {
                            db.RollBack();
                            return Error(rst.message, "");
                        }
                        db.CommitWithOutRollBack();
                        /**************************************************/

                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLog(logEntity);

                        return Success("操作成功。");
                    }
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

        #region 部门列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDepartmentList()
        {
            var data = new OrganizeApp().GetList();
            //data = data.Where(o => o.F_CategoryId == "Department").ToList();
            return Content(data.ToJson());
        }
        #endregion

        #region 用户列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetUserList()
        {
            var data = new UserApp().GetList("");
            return Content(data.ToJson());
        }
        #endregion

        #region 删除出库单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutBoundController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除出库单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    lock (lockObj)
                    {
                        /*************************************************/
                        T_OutBoundEntity outBound = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == keyValue);
                        if (outBound.State != "New") return Error("非新建状态不可删除", "");

                        List<T_OutBoundDetailEntity> detailList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == outBound.F_Id);
                        foreach (T_OutBoundDetailEntity detail in detailList)
                        {
                            db.Delete<T_OutBoundDetailEntity>(detail);
                        }

                        db.Delete<T_OutBoundEntity>(outBound);
                        db.SaveChanges();

                        db.CommitWithOutRollBack();

                        /**************************************************/

                        logObj.Message = "删除成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLog(logEntity);

                        return Success("删除成功。");
                    }
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

        #region 手动过账
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult TransByHand(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutBoundController.TransByHand";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "手动过账";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_OutBoundEntity outBound = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == keyValue);
                    if (outBound.State != "Over")
                    {
                        return Error("单据不是完成状态。", "");
                    }

                    if (outBound.TransState == "OverTrans" || outBound.TransState == "UnNeedTrans")
                    {
                        return Error("该单据已过账或免过账。", "");
                    }

                    string outBoundType = outBound.OutBoundType;
                    string orderType;
                    switch (outBoundType)
                    {
                        case "GetItemOut":
                            {
                                orderType = "GetItemOut";
                            }
                            break;
                        case "VerBackOut":
                            {
                                orderType = "VerBackOut";
                            }
                            break;
                        case "WarehouseBackOut":
                            {
                                orderType = "WarehouseBackOut";
                            }
                            break;
                        default:
                            {
                                return Error("单据类型未知。", "");
                            }
                    }

                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, outBound.F_Id, orderType);
                    if ((ResultType)rst.state == ResultType.success)
                    {
                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                        ERPPost post = new ERPPost();
                        ERPResult erpRst = post.PostFactInOutQty(db, orderType, trans.F_Id);
                        db.CommitWithOutRollBack();
                        if (!erpRst.IsSuccess)
                        {
                            return Error(erpRst.FailMsg, "");
                        }
                    }
                    else
                    {
                        return Error(rst.message, "");
                    }

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
