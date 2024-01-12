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
using MST.Domain;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_QAManage.Controllers
{
    public class QAGetController : ControllerBase
    {
        private static object lockObj = new object();
        private T_QAApp qAApp = new T_QAApp();
        private T_QADetailApp qADetailApp = new T_QADetailApp();
        private T_QAResultApp qAResultApp = new T_QAResultApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_InBoundApp inBoundApp = new T_InBoundApp();

        #region 获取质检取样单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            string qAOrderType = "GetSample";
            List<T_QAEntity> data = qAApp.GetList(pagination, qAOrderType, queryJson);

            List<QAModel> qaModelList = new List<QAModel>();
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_QAEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumOrderTypeList = itemsDetailApp.FindEnum<T_QAEntity>(o => o.QAOrderType).ToList();
            IList<ItemsDetailEntity> enumGenTypeList = itemsDetailApp.FindEnum<T_QAEntity>(o => o.GenType).ToList();

            foreach (T_QAEntity item in data)
            {
                QAModel model = item.ToObject<QAModel>();
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.QAOrderTypeName = enumOrderTypeList.FirstOrDefault(o => o.F_ItemCode == item.QAOrderType).F_ItemName;
                model.GenTypeName = enumGenTypeList.FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;
                qaModelList.Add(model);
            }

            var resultList = new
            {
                rows = qaModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 质检取样单详情
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = qAApp.GetForm(keyValue);
            QAModel model = data.ToObject<QAModel>();
            model.StateName = itemsDetailApp.FindEnum<T_QAEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
            return Content(model.ToJson());
        }
        #endregion

        #region 获取待检入库单列表（仅限新建质检单据）
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetRefInBoundCodeList()
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                IQueryable<T_InBoundEntity> inboundQuery = db.IQueryable<T_InBoundEntity>(o => true && o.State == "Over");
                IQueryable<T_ContainerDetailEntity> cdQuery = db.IQueryable<T_ContainerDetailEntity>(o => o.CheckState == "WaitCheck");
                var data = inboundQuery.Join(cdQuery, m => m.F_Id, n => n.InBoundID, (m, n) => new { RefInBoundCode = m.RefOrderCode }).Distinct().OrderByDescending(o => o.RefInBoundCode).ToList();
                List<T_QAEntity> qaQuery = db.IQueryable<T_QAEntity>(o => !string.IsNullOrEmpty(o.RefInBoundCode)).ToList(); //RefInBoundCode 不为空表示入库单已存在质检单
                var noQAOrder = data.Where(o => !qaQuery.Any(x => x.RefInBoundCode == o.RefInBoundCode)).Select(o => new { RefInBoundCode = o.RefInBoundCode }).Distinct().OrderByDescending(o => o.RefInBoundCode).ToList();
                return Content(noQAOrder.ToJson());
            }
        }
        #endregion

        #region 获取待检入库单列表（全部）
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetRefInBoundCodeListAll()
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                IQueryable<T_InBoundEntity> inboundQuery = db.IQueryable<T_InBoundEntity>(o => true);
                var data = inboundQuery.Join(inboundQuery, m => m.F_Id, n => n.F_Id, (m, n) => new { RefInBoundCode = m.RefOrderCode }).OrderByDescending(o => o.RefInBoundCode).ToList();
                return Content(data.ToJson());
            }
        }
        #endregion

        #region 新建/修改质检单
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_QAEntity qAEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QAGetController.SubmitForm";
                logObj.Parms = new { qAEntity = qAEntity, keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "提交新建/修改质检取样单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/

                    if (qAEntity.State != "New") return Error("单据不是新建状态", "");
                    T_QAEntity qaBefore = qAApp.FindEntity(o => o.F_Id == keyValue);

                    T_QAEntity qAEntityOther = qAApp.FindEntity(o => o.RefInBoundCode == qAEntity.RefInBoundCode);
                    if (qAEntityOther != null) return Error("入库单已绑定质检", "");

                    if (string.IsNullOrEmpty(qAEntity.RefOrderCode)) qAEntity.RefOrderCode = qAEntity.QACode;
                    qAEntity.QAOrderType = "GetSample";
                    qAEntity.TransState = "New";
                    qAEntity.GenType = "MAN";
                    qAEntity.F_DeleteMark = false;
                    if (!string.IsNullOrEmpty(qAEntity.Remark)) qAEntity.Remark = qAEntity.Remark.Replace("\n", " ");

                    if (string.IsNullOrEmpty(keyValue))
                    {
                        qAEntity.F_Id = Guid.NewGuid().ToString();
                        db.Insert<T_QAEntity>(qAEntity);
                    }
                    else
                    {
                        qAEntity.F_Id = keyValue;
                        /// 已添加质检明细，禁止修改绑定入库单
                        var detail = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id);
                        if (detail.Count != 0)
                        {
                            if (qAEntity.RefInBoundCode != qaBefore.RefInBoundCode)
                            {
                                db.RollBack();
                                return Error("已添加质检明细，禁止修改对应入库单", "操作失败");
                            }
                        }
                        db.Update<T_QAEntity>(qAEntity);
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

        #region 开始质检取样
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult BeginOut(string QAID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QAGetController.BeginOut"; //按实际情况修改
                logObj.Parms = new { QAID = QAID }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始质检取样"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    lock (lockObj)
                    {
                        /*************************************************/
                        AjaxResult rst = new AjaxResult();
                        rst = WaveGen_QA_All(db, QAID);
                        if ((ResultType)rst.state != ResultType.success)
                        {
                            return Error(rst.message, "");
                        }

                        /// 执行并发送任务
                        IList<T_QADetailEntity> qADetailList = db.FindList<T_QADetailEntity>(o => o.QAID == QAID);
                        if (qADetailList.Count == 0) return Error("质检明细列表为空", "");

                        IList<string> outBoundDetailIDList = qADetailList.Select(o => o.F_Id).ToList();
                        rst = new T_QARecordApp().QADetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                        if ((ResultType)rst.state != ResultType.success)
                        {
                            db.RollBack();
                            return Error("操作失败。", rst.message);
                        }

                        db.SaveChanges();

                        /*************************************************/

                        db.CommitWithOutRollBack();
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

        public AjaxResult WaveGen_QA_All(IRepositoryBase db, string QAID)
        {
            AjaxResult rst = new AjaxResult();
            T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.F_Id == QAID);
            if (qAEntity.State == "Over")
            {
                rst.state = ResultType.error;
                rst.message = "单据已结束";
                return rst;
            }
            if (qAEntity.State == "Picked" || qAEntity.State == "WaitReturn" || qAEntity.State == "WaitResult")
            {
                rst.state = ResultType.error;
                rst.message = "单据已取样完成";
                return rst;
            }
            if (qAEntity.State == "Outing" || qAEntity.State == "Picking")
            {
                rst.state = ResultType.error;
                rst.message = "单据已开始取样";
                return rst;
            }

            IList<T_QADetailEntity> qADetailList = db.FindList<T_QADetailEntity>(o => o.QAID == QAID);
            if (qADetailList.Count == 0)
            {
                rst.state = ResultType.error;
                rst.message = "质检明细列表为空";
                return rst;
            }

            IList<T_QADetailEntity> emptyStationList = qADetailList.Where(o => string.IsNullOrEmpty(o.StationID)).ToList();
            if (emptyStationList.Count > 0)
            {
                string err = $"项次[{string.Join(",", emptyStationList.Select(o => o.SEQ).OrderBy(o => o.Value).ToList().ToArray())}]未指定出库站台";

                rst.state = ResultType.error;
                rst.message = err;
                return rst;
            }

            IList<string> stationArray = qADetailList.Select(o => o.StationID).Distinct().ToArray();
            IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => stationArray.Contains(o.F_Id));

            stationList = stationList.Where(o => string.IsNullOrEmpty(o.CurOrderID) || o.CurOrderID == QAID).ToList();
            if (stationList.Count != stationArray.Count)
            {
                rst.state = ResultType.error;
                rst.message = "站台已被占用";
                return rst;
            }


            IList<T_QADetailEntity> needWaveByNumList = new List<T_QADetailEntity>();
            IList<T_QADetailEntity> needWaveByCntList = new List<T_QADetailEntity>();
            IList<T_QADetailEntity> needOutList = new List<T_QADetailEntity>();
            foreach (T_QADetailEntity qADetail in qADetailList)
            {
                if (qADetail.State == "New" && qADetail.ActionType == "Init")
                {
                    if (qADetail.IsAppearQA == "true")
                    {
                        needWaveByCntList.Add(qADetail);
                    }
                    else
                    {
                        needWaveByNumList.Add(qADetail);
                    }
                }
                else if (qADetail.State == "Waved")
                {
                    needOutList.Add(qADetail);
                }
            }

            T_QARecordApp qARecordApp = new T_QARecordApp();
            /// 产生波次单（外观质检）
            if (needWaveByCntList.Count > 0)
            {
                string[] needOutArray = needWaveByCntList.Select(o => o.F_Id).ToArray();
                rst = qARecordApp.WaveGen_QA_CntBar(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                if ((ResultType)rst.state != ResultType.success)
                {
                    db.RollBack();
                    return rst;
                }
                db.SaveChanges();
            }

            /// 产生波次单（取样质检）
            if (needWaveByNumList.Count > 0)
            {
                string[] needOutArray = needWaveByNumList.Select(o => o.F_Id).ToArray();
                rst = qARecordApp.WaveGen_QA(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                if ((ResultType)rst.state != ResultType.success)
                {
                    db.RollBack();
                    return rst;
                }
                db.SaveChanges();
            }



            rst.state = ResultType.success;
            return rst;
        }

        #endregion

        #region 删除质检取样单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QAGetController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除质检取样单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.F_Id == keyValue);
                    if (qAEntity.State != "New")
                    {
                        return Error("非新建状态不可删除", "");
                    }

                    List<T_QADetailEntity> detailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id);
                    foreach (T_QADetailEntity detail in detailList)
                    {
                        db.Delete<T_QADetailEntity>(detail);
                    }

                    db.Delete<T_QAEntity>(qAEntity);
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
                logObj.Path = "QAGetController.TransByHand";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "手动过账";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.F_Id == keyValue);
                    if (!(qAEntity.State == "WaitReturn" || qAEntity.State == "Over"))
                    {
                        return Error("单据状态必须为待还样或结束", "");
                    }
                    if (qAEntity.TransState == "OverTrans" || qAEntity.TransState == "UnNeedTrans")
                    {
                        return Error("当前单据已过账或免过账", "");
                    }

                    string qaOrderType = qAEntity.QAOrderType;
                    string orderType;

                    switch (qaOrderType)
                    {
                        case "BackSample":
                            {
                                orderType = "BackSample";
                            }
                            break;
                        case "GetSample":
                            {
                                orderType = "GetSample";
                            }
                            break;
                        default:
                            {
                                return Error("单据类型未知。", "");
                            }
                    }

                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, qAEntity.F_Id, orderType);
                    if ((ResultType)rst.state == ResultType.success)
                    {
                        qAEntity.TransState = "OverTrans";
                        db.Update<T_QAEntity>(qAEntity);

                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                        ERPPost post = new ERPPost();
                        ERPResult erpRst = post.PostFactInOutQty(db, orderType, trans.F_Id);
                        db.CommitWithOutRollBack();
                        if (!erpRst.IsSuccess)
                        {
                            return Error(erpRst.FailMsg, "");
                        }
                    }
                    else return Error(rst.message, "");

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

        #region 展开质检明细
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult QADetails()
        {
            return View();
        }
        #endregion

        #region 获取质检取样单明细
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJsonDetails(Pagination pagination, string QAID, string keyword)
        {
            IList<T_QADetailEntity> detailList = qADetailApp.GetList(pagination, QAID, keyword);

            T_QAEntity qAEntity = qAApp.FindEntity(o => o.F_Id == QAID);
            T_InBoundEntity inBoundEntity = inBoundApp.FindEntity(o => o.RefOrderCode == qAEntity.RefInBoundCode);
            if (inBoundEntity == null) return Error("未找到对应入库单", "");

            IList<QADetailModel> qaDetailModelList = detailList.ToObject<IList<QADetailModel>>();
            IList<ItemsDetailEntity> items = itemsDetailApp.FindEnum<T_QADetailEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumResultStateList = itemsDetailApp.FindEnum<T_QADetailEntity>(o => o.ResultState).ToList();
            IList<ItemsDetailEntity> enumQAResultList = itemsDetailApp.FindEnum<T_QADetailEntity>(o => o.QAResult).ToList();

            foreach (QADetailModel model in qaDetailModelList)
            {
                T_QADetailEntity entity = detailList.FirstOrDefault(o => o.F_Id == model.F_Id);
                model.SupplierCode = entity.SupplierCode;
                model.SupplierName = entity.SupplierUserName;
                model.SupplierID = entity.SupplierUserID;

                T_StationEntity t_stationEntity = stationApp.FindEntity(o => o.F_Id == model.StationID);
                model.StationName = t_stationEntity.StationName;
                model.StationCode = t_stationEntity.StationCode;

                model.StateName = items.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                if (!string.IsNullOrEmpty(model.ResultState)) model.ResultStateName = enumResultStateList.FirstOrDefault(o => o.F_ItemCode == model.ResultState).F_ItemName;
                else model.ResultState = "";
                if (!string.IsNullOrEmpty(model.QAResult)) model.QAResultName = enumQAResultList.FirstOrDefault(o => o.F_ItemCode == model.QAResult).F_ItemName;
                else model.QAResult = "";
                model.QtySum = containerDetailApp.FindList(o => o.ItemID == model.ItemID && o.Lot == model.Lot && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false"
                                && o.CheckState == "WaitCheck" && o.State != "Freeze"
                                && o.InBoundID == inBoundEntity.F_Id).Sum(o => o.Qty);

                if ((model.State == "Picked"
                    || model.State == "WaitReturn"
                    || model.State == "Returning"
                    || model.State == "WaitResult")
                    && (model.ResultState == "New" || string.IsNullOrEmpty(model.ResultState)))
                {
                    model.IsCanSetResult = "true";
                }
                else model.IsCanSetResult = "false";

                model.IsAppearQAStr = model.IsAppearQA;
            }

            var resultList = new
            {
                rows = qaDetailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 打开维护明细窗口
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult TransferForm()
        {
            return View();
        }
        #endregion

        #region 打开质检结果录入窗口
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult QAResultSet()
        {
            return View();
        }
        #endregion

        #region 维护明细列表左侧
        private class QALeftModel
        {
            public string F_Id { get; set; }
            public string ItemID { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Factory { get; set; } /// 生产厂家
            public string Spec { get; set; }
            public string Lot { get; set; }
            public string ItemUnitText { get; set; }
            public DateTime? OverdueDate { get; set; }
            public int? SEQ { get; set; }
            public string SupplierName { get; set; }
            public string SupplierID { get; set; }
            public string SupplierCode { get; set; }
            public decimal? QtySum { get; set; }
            public decimal? SampleSumNum { get; set; }  /// 应抽检数量(根据基础数据计算而来)
        }
        [HttpGet]
        [HandlerAjaxOnly]

        public ActionResult QALeftList(Pagination pagination, string QAID,string kindID, string keyword)
        {
            var data = containerDetailApp.GetList(pagination, kindID, keyword);

            T_QAEntity qAEntity = qAApp.FindEntity(o => o.F_Id == QAID);
            T_InBoundEntity inBoundEntity = inBoundApp.FindEntity(o => o.RefOrderCode == qAEntity.RefInBoundCode);

            /// 过滤系统数据 过滤冻结物料
            IList<string> itemSys = new T_ItemApp().FindList(o => o.IsBase == "true").Select(o => o.F_Id).Distinct().ToArray();

            /// 仅需质检【不合格】【待质检】物料，不需质检【免检】【合格】物料，以及【质检冻结】物料
            /// 新增：20211109175200 不质检【不合格】物料
            List<QALeftModel> modelList = data.Where(o => !itemSys.Contains(o.ItemID) && o.State != "Freeze"
                    && o.CheckState == "WaitCheck" && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false"
                    && o.InBoundID == inBoundEntity.F_Id)   /// 筛选对应入库单物料
                .GroupBy(o => new { o.ItemID, o.ItemCode, o.ItemName, o.Lot, o.SEQ, o.Spec, o.ItemUnitText, o.OverdueDate })
                .Select(o => new QALeftModel
                {
                    F_Id = o.Key.ItemID + o.Key.Lot,
                    ItemID = o.Key.ItemID,
                    ItemCode = o.Key.ItemCode,
                    ItemName = o.Key.ItemName,
                    Factory = o.FirstOrDefault().Factory,
                    Lot = o.Key.Lot,
                    Spec = o.Key.Spec,
                    ItemUnitText = o.Key.ItemUnitText,
                    OverdueDate = o.Key.OverdueDate,
                    SEQ = o.Key.SEQ,
                    QtySum = o.Sum(i => (i.Qty ?? 0)),
                    SupplierID = o.FirstOrDefault().SupplierID,
                    SupplierCode = o.FirstOrDefault().SupplierCode,
                    SupplierName = o.FirstOrDefault().SupplierName
                })
                .ToList();

            foreach (QALeftModel cell in modelList)
            {
                //T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == cell.ItemID);
                ///// 根据抽检比例计算最小出库数量
                ////decimal? minOutNum = ((cell.QtySum ?? 0) * (item.CheckPerc ?? 0) / 100).NearInt(item.UnitQty, true);
                //cell.SampleSumNum = (int)(((cell.QtySum ?? 0) * (item.CheckPerc ?? 0) / 100) + 0.5M);
                //if (cell.SampleSumNum < 1 && cell.SampleSumNum >= 0)
                //{
                //    cell.SampleSumNum = 1;
                //}

                cell.SampleSumNum = 1;
            }
            return Content(modelList.ToJson());
        }
        #endregion

        #region 新增/修改质检明细
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFormList(string qADetailEntityListStr, string QAID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QAGetController.SubmitFormList";
                logObj.Parms = new { qADetailEntityList = qADetailEntityListStr, QAID = QAID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "新增/修改质检取样单明细";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_QAEntity QAHead = qAApp.FindEntity(o => o.F_Id == QAID);
                    if (QAHead.State != "New") return Error("单据不是新建状态。", "");

                    IList<QADetailModel> QADetailList = qADetailEntityListStr.ToObject<IList<QADetailModel>>();
                    string[] arrayInPage = QADetailList.Select(o => o.ItemID + o.Lot).ToArray();

                    IList<T_QADetailEntity> QADetailInDB = db.FindList<T_QADetailEntity>(o => o.QAID == QAID);
                    string[] arrayInDB = QADetailInDB.Select(o => o.ItemID + o.Lot).ToArray();

                    IList<T_QADetailEntity> needDelList = db.FindList<T_QADetailEntity>(o => (!arrayInPage.Contains(o.ItemID + o.Lot)) && o.QAID == QAID);
                    IList<T_QADetailEntity> needUpdateList = db.FindList<T_QADetailEntity>(o => arrayInPage.Contains(o.ItemID + o.Lot) && o.QAID == QAID);
                    IList<QADetailModel> needInsertList = QADetailList.Where(o => !arrayInDB.Contains(o.ItemID + o.Lot)).ToList();

                    foreach (QADetailModel entity in needInsertList)
                    {
                        T_QADetailEntity qaDetail = new T_QADetailEntity();
                        qaDetail.QAID = QAID;
                        T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == entity.ItemID);
                        qaDetail.ItemCode = item.ItemCode;
                        qaDetail.ItemName = item.ItemName;
                        qaDetail.ItemID = item.F_Id;
                        qaDetail.Factory = item.Factory;
                        qaDetail.Lot = entity.Lot;
                        qaDetail.Spec = entity.Spec;
                        qaDetail.ItemUnitText = entity.ItemUnitText;
                        qaDetail.OverdueDate = entity.OverdueDate;
                        qaDetail.SEQ = entity.SEQ;
                        qaDetail.SupplierCode = entity.SupplierCode;
                        qaDetail.SupplierUserID = entity.SupplierID;
                        qaDetail.SupplierUserName = entity.SupplierName;
                        qaDetail.SampleSumNum = entity.SampleSumNum;
                        qaDetail.SampleSumCnt = entity.SampleSumCnt;
                        qaDetail.F_DeleteMark = false;
                        T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
                        string containerKind = containerType.ContainerKind;
                        if (containerKind == "Rack")
                        {
                            T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString() && o.UseCode.Contains("CheckPickOut"));
                            qaDetail.StationID = t_Station.F_Id;
                        }
                        else if (containerKind == "Plastic")
                        {
                            T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString() && o.UseCode.Contains("CheckPickOut"));
                            qaDetail.StationID = t_Station.F_Id;
                        }
                        else if (containerKind == "Box")
                        {
                            T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString() && o.UseCode.Contains("CheckPickOut"));
                            qaDetail.StationID = t_Station.F_Id;
                        }
                        else
                        {
                            return Error("物料的容器类型未知:" + item.ItemCode, "");
                        }

                        qaDetail.F_Id = Guid.NewGuid().ToString();
                        qaDetail.State = "New";
                        qaDetail.ResultState = "New";
                        qaDetail.QAResult = "WaitCheck";
                        qaDetail.ActionType = "Init";
                        if (entity.IsAppearQA == "true")
                        {
                            qaDetail.IsAppearQA = "true";
                            qaDetail.IsBroken = "false";
                        }
                        else if (entity.IsAppearQA == "false")
                        {
                            qaDetail.IsAppearQA = "false";
                            qaDetail.IsBroken = entity.IsBroken;
                        }
                        else return Error("未知的质检类型", "");

                        db.Insert<T_QADetailEntity>(qaDetail);
                    }

                    foreach (T_QADetailEntity upcell in needUpdateList)
                    {
                        upcell.SampleSumNum = QADetailList.FirstOrDefault(o => o.ItemID == upcell.ItemID && o.Lot == upcell.Lot).SampleSumNum;
                        upcell.SampleSumCnt = QADetailList.FirstOrDefault(o => o.ItemID == upcell.ItemID && o.Lot == upcell.Lot).SampleSumCnt;
                        upcell.IsAppearQA = QADetailList.FirstOrDefault(o => o.ItemID == upcell.ItemID && o.Lot == upcell.Lot).IsAppearQA;
                        upcell.IsBroken = QADetailList.FirstOrDefault(o => o.ItemID == upcell.ItemID && o.Lot == upcell.Lot).IsBroken;

                        if (upcell.IsAppearQA == "true") upcell.IsBroken = "false";
                        else if (upcell.IsAppearQA == "false") upcell.IsBroken = upcell.IsBroken;
                        else return Error("未知的质检类型", "");

                        db.Update<T_QADetailEntity>(upcell);
                    }

                    foreach (T_QADetailEntity delcell in needDelList)
                    {
                        db.Delete<T_QADetailEntity>(delcell);
                    }

                    db.CommitWithOutRollBack();

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                catch (Exception ex)
                {
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

        #region 删除质检单明细
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFormDetail(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "QAGetController.DeleteFormDetail";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "质检取样单";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除质检取样单明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_QADetailEntity data = qADetailApp.GetForm(keyValue);
                if (data.State != "New") return Error("非新建状态不可删除", "");

                qADetailApp.DeleteForm(keyValue);

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

        #region 开始还样，生成质检还样单
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ReturnOrder(string QAID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QAGetController.BeginReturn";
                logObj.Parms = new { QAID = QAID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "生成还样单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_QAEntity qAGet = qAApp.FindEntity(o => o.F_Id == QAID);
                    List<T_QADetailEntity> qADetailList = qADetailApp.FindList(o => o.QAID == qAGet.F_Id).ToList();
                    if (qADetailList.Any(o => o.State != "Picked"
                                            && o.State != "Over"
                                            && o.State != "WaitResult"
                                            && o.State != "WaitApply"
                                            && o.State != "WaitReturn"
                                            ))
                    {
                        return Error("单据明细必须全部取样或结束", "");
                    }

                    List<T_QARecordEntity> recordList = db.FindList<T_QARecordEntity>(o => o.QAID == QAID && o.IsNeedBack == "true" && o.IsReturnOver != "true" && o.IsAppearQA == "false");
                    if (recordList.Count() == 0)
                    {
                        return Error("没有需要还样的记录", "");
                    }

                    /// 更新明细当前状态：【结束】/【待录入结果】
                    List<T_QADetailEntity> detailGetList = db.FindList<T_QADetailEntity>(o => o.QAID == QAID);
                    foreach (T_QADetailEntity detail in detailGetList)
                    {
                        if (detail.ResultState == "New" || string.IsNullOrEmpty(detail.ResultState))
                        {
                            if (detail.State != "WaitApply") 
                                detail.State = "WaitResult";
                        }
                        else
                        {
                            detail.State = "Over";
                        }
                        db.Update<T_QADetailEntity>(detail);
                    }

                    /// 判断取样入库任务完成
                    List<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == qAGet.F_Id && o.TaskType == "TaskType_CheckPickIn").ToList();
                    if (taskList.Count != 0) return Error("取样入库任务未执行完成", "");

                    /// 生成还样单
                    T_QAEntity qABack = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == qAGet.QACode && o.QAOrderType == "BackSample");
                    if (qABack == null) /// 没有创建质检还样单
                    {
                        qABack = new T_QAEntity();
                        qABack.F_Id = Guid.NewGuid().ToString();
                        qABack.QACode = T_CodeGenApp.GenNum("QARule");
                        qABack.QAOrderType = "BackSample";
                        qABack.State = "New";
                        qABack.TransState = "New";
                        qABack.RefOrderCode = qAGet.QACode;
                        qABack.StationID = qAGet.StationID;
                        qABack.GenType = "MAN";
                        qABack.Remark = qAGet.Remark;
                        qABack.F_DeleteMark = false;
                        db.Insert<T_QAEntity>(qABack);
                    }
                    else
                    {
                        return Error("单据已生成还样单", "");
                    }

                    db.SaveChanges();

                    /// 生成还样单明细
                    List<T_QADetailEntity> detailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qABack.F_Id);
                    if (detailBackList.Count == 0)   /// 明细不存在
                    {
                        foreach (var detailOld in detailGetList)
                        {
                            if (detailOld.IsAppearQA == "true" || (detailOld.IsAppearQA == "false" && detailOld.IsBroken == "true")) continue; /// 外观质检 || 破坏性质检

                            T_QADetailEntity detailNew = new T_QADetailEntity();
                            detailNew.F_Id = Guid.NewGuid().ToString();
                            detailNew.SEQ = detailOld.SEQ;
                            detailNew.QAID = qABack.F_Id;
                            detailNew.ItemID = detailOld.ItemID;
                            detailNew.ItemName = detailOld.ItemName;
                            detailNew.ItemCode = detailOld.ItemCode;
                            detailNew.Factory = detailOld.Factory;
                            detailNew.SupplierUserID = detailOld.SupplierUserID;
                            detailNew.SupplierCode = detailOld.SupplierCode;
                            detailNew.SupplierUserName = detailOld.SupplierUserName;
                            detailNew.State = "New";
                            detailNew.Lot = detailOld.Lot;
                            detailNew.Spec = detailOld.Spec;
                            detailNew.ItemUnitText = detailOld.ItemUnitText;
                            detailNew.OverdueDate = detailOld.OverdueDate;
                            detailNew.StationID = detailOld.StationID;
                            detailNew.ResultState = detailOld.ResultState;
                            detailNew.QAResult = detailOld.QAResult;
                            detailNew.IsBroken = detailOld.IsBroken;
                            detailNew.SampleSumNum = detailOld.SampleSumNum;
                            detailNew.OutQty = detailOld.OutQty;
                            detailNew.ResultSendTime = detailOld.ResultSendTime;
                            detailNew.SampleType = detailOld.SampleType;
                            detailNew.ActionType = "Init";
                            detailNew.F_DeleteMark = false;
                            db.Insert<T_QADetailEntity>(detailNew);
                        }
                    }
                    else
                    {
                        return Error("已存在还样记录", "");
                    }

                    /// 更新单据当前状态：完成/待录入结果/待还样
                    if (detailGetList.All(o => o.State == "Over"))
                    {
                        qAGet.State = "Over";  ///完成
                    }
                    else if (detailGetList.All(o => o.State == "Over" || o.State == "WaitApply"))
                    {
                        qAGet.State = "WaitApply";    /// 待应用结果
                    }
                    else if (detailGetList.All(o => o.State == "Over" || o.State == "WaitResult"))
                    {
                        qAGet.State = "WaitResult";    /// 待录入结果
                    }
                    db.Update<T_QAEntity>(qAGet);

                    db.SaveChanges();
                    db.CommitWithOutRollBack();

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

        #region 获取结果录入界面初始化信息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetQAResult(string QADetailID)
        {
            T_QADetailEntity qaDetail = qADetailApp.FindEntity(o => o.F_Id == QADetailID);
            T_QAResultEntity resultQA = qAResultApp.FindEntity(o => o.QADetailID == QADetailID);
            if (resultQA == null)
            {
                resultQA = new T_QAResultEntity();
                resultQA.ItemCode = qaDetail.ItemCode;
                resultQA.ItemName = qaDetail.ItemName;
                resultQA.Lot = qaDetail.Lot;
            }
            return Content(resultQA.ToJson());
        }
        #endregion

        #region 提交手动录入质检结果
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult QAResultSubmit(T_QAResultEntity qaResultEntity)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QAGetController.QAResultSubmit";
                logObj.Parms = new { qaResultEntity = qaResultEntity };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "质检取样录入结果";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_QADetailEntity qADetail = qADetailApp.FindEntity(o => o.F_Id == qaResultEntity.QADetailID);
                    T_QAEntity qAEntity = qAApp.FindEntity(o => o.F_Id == qADetail.QAID);
                    T_QAResultEntity resultQA = qAResultApp.FindEntity(o => o.QADetailID == qaResultEntity.QADetailID);

                    T_InBoundEntity inBoundEntity = inBoundApp.FindEntity(o => o.RefOrderCode == qAEntity.RefInBoundCode);
                    if (inBoundEntity == null) return Error("未找到对应入库来源单", "");

                    if (resultQA == null) /// 没有质检结果
                    {
                        T_ItemEntity itemEntity = itemApp.FindEntity(o => o.ItemCode == qADetail.ItemCode && o.F_DeleteMark == false);

                        qaResultEntity.F_Id = Guid.NewGuid().ToString();
                        qaResultEntity.QAID = qADetail.QAID;
                        qaResultEntity.QADetailID = qADetail.F_Id;
                        qaResultEntity.RefOrderCode = qAEntity.RefOrderCode;
                        qaResultEntity.SEQ = Convert.ToInt32(qADetail.SEQ);
                        qaResultEntity.ItemID = itemEntity.F_Id;
                        qaResultEntity.ItemCode = itemEntity.ItemCode;
                        qaResultEntity.ItemName = itemEntity.ItemName;

                        if (itemEntity.IsMustLot == "true" && string.IsNullOrEmpty(qADetail.Lot))
                        {
                            return Error("物料批号必填", "");
                        }
                        else
                        {
                            qaResultEntity.Lot = qADetail.Lot;
                        }

                        qaResultEntity.IsUsed = "true";
                        qaResultEntity.F_CreatorTime = DateTime.Now;
                        qaResultEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                        qaResultEntity.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                        qaResultEntity.AccessTime = DateTime.Now;
                        qaResultEntity.F_DeleteMark = false;
                        db.Insert<T_QAResultEntity>(qaResultEntity);

                        /// 更新库存质检状态
                        List<T_ContainerDetailEntity> detailList = new List<T_ContainerDetailEntity>();
                        if (string.IsNullOrEmpty(qADetail.Lot)) detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && o.InBoundID == inBoundEntity.F_Id);
                        else detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && o.Lot == qADetail.Lot && o.InBoundID == inBoundEntity.F_Id);

                        /// 结合质检取样记录，解冻库存
                        var recordList = db.FindList<T_QARecordEntity>(o => o.QADetailID == qADetail.F_Id);
                        foreach (T_ContainerDetailEntity cd in detailList)
                        {
                            if (qaResultEntity.QAResult == "Qua") cd.CheckState = "Qua";   /// 合格
                            else if (qaResultEntity.QAResult == "UnQua") cd.CheckState = "UnQua";  /// 不合格
                            else return Error("未知的质检结果参数", "");

                            T_QARecordEntity res = recordList.Find(o => o.ContainerDetailID == cd.F_Id);
                            if (res != null && res.IsReturnOver == "false" && res.IsAppearQA == "false") { }    /// 结果录入时，只有一种库存不能解冻：1.已取样 2.未还样 3.取样质检（不是外观质检）
                            else cd.IsCheckFreeze = "false";

                            db.Update<T_ContainerDetailEntity>(cd);
                        }

                        /// 修改明细质检状态
                        qADetail.ResultState = "MAN";
                        qADetail.QAResult = qaResultEntity.QAResult;
                        qADetail.ResultSendTime = qaResultEntity.AccessTime;
                        qADetail.SampleType = "Hand";
                        db.SaveChanges();

                        /// 同步修改还样单质检结果
                        T_QAEntity qaReturnOrder = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == qAEntity.QACode && o.QAOrderType == "BackSample");
                        if (qaReturnOrder != null) /// 存在质检还样单
                        {
                            if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false") /// 非破坏性质检，且需要还样明细
                            {
                                T_QADetailEntity qaDetailReturn = db.FindEntity<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id && o.SEQ == qADetail.SEQ);
                                if (qaDetailReturn.State == "WaitResult") qaDetailReturn.State = "Over";
                                qaDetailReturn.ResultState = "MAN";
                                qaDetailReturn.QAResult = qaResultEntity.QAResult;
                                qaDetailReturn.ResultSendTime = qaResultEntity.AccessTime;
                                qaDetailReturn.SampleType = "Hand";
                                db.Update<T_QADetailEntity>(qaDetailReturn);
                            }

                            /// 还样单据
                            List<T_QADetailEntity> qADetailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id).ToList();
                            if (qADetailBackList.All(o => o.State == "Over"))
                            {
                                qaReturnOrder.State = "Over";  /// 完成
                            }
                            else if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitResult"))
                            {
                                qaReturnOrder.State = "WaitResult";    /// 待录入结果
                            }
                            db.Update<T_QAEntity>(qaReturnOrder);
                        }

                        if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false" && qaReturnOrder == null) qADetail.State = "WaitReturn";
                        else qADetail.State = "Over";
                        db.Update<T_QADetailEntity>(qADetail);
                        db.SaveChanges();

                        /// 判断单据关闭
                        List<T_QADetailEntity> qaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id).ToList();
                        if (qaDetailList.All(o => o.State == "Over"))
                        {
                            qAEntity.State = "Over";  ///完成
                        }
                        else if (qaDetailList.All(o => o.State == "Over" || o.State == "WaitResult"))
                        {
                            qAEntity.State = "WaitResult";    /// 待录入结果
                        }
                        else if (qaDetailList.All(o => o.State == "Over" || o.State == "WaitReturn"))
                        {
                            qAEntity.State = "WaitReturn"; /// 待还样
                        }
                        db.Update<T_QAEntity>(qAEntity);
                        db.SaveChanges();

                        /// 生成验退单：取样单结束 && 还样单结束或未生成
                        if (qAEntity.State == "Over" && (qaReturnOrder == null || qaReturnOrder.State == "Over"))
                        {
                            /// 不合格明细
                            List<T_QADetailEntity> unQuaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id && o.QAResult == "UnQua");
                            if (unQuaDetailList.Count != 0)
                            {
                                /// 验退出库单
                                T_OutBoundEntity outBound = new T_OutBoundEntity();
                                outBound.F_Id = Guid.NewGuid().ToString();

                                outBound.ReceiveDepartment = "";
                                outBound.ReceiveDepartmentId = "";
                                outBound.ReceiveUserName = "";
                                outBound.ReceiveUserId = "";

                                outBound.OutBoundCode = T_CodeGenApp.GenNum("OutBoundRule");
                                outBound.RefOrderCode = outBound.OutBoundCode;
                                outBound.QAID = qAEntity.F_Id;
                                outBound.PointInBoundID = inBoundEntity.F_Id;

                                outBound.OutBoundType = "VerBackOut";
                                outBound.State = "New";
                                outBound.GenType = "MAN";
                                outBound.IsUrgent = "false";
                                outBound.TransState = "New";
                                outBound.Remark = $"质检单：{qAEntity.RefOrderCode} 有物料未通过质检，手动反馈不合格执行验退单";
                                outBound.F_DeleteMark = false;

                                db.Insert<T_OutBoundEntity>(outBound);
                                db.SaveChanges();

                                List<T_OutBoundDetailEntity> outDetailList = new List<T_OutBoundDetailEntity>();
                                foreach (T_QADetailEntity unQua in unQuaDetailList)
                                {
                                    T_OutBoundDetailEntity detail = new T_OutBoundDetailEntity();
                                    detail.F_Id = Guid.NewGuid().ToString();
                                    detail.OutBoundID = outBound.F_Id;
                                    detail.SEQ = Convert.ToInt32(unQua.SEQ);
                                    detail.ItemID = unQua.ItemID;
                                    detail.ItemName = unQua.ItemName;
                                    detail.ItemCode = unQua.ItemCode;
                                    detail.Factory = unQua.Factory;
                                    detail.SupplierUserID = unQua.SupplierUserID;
                                    detail.SupplierUserName = unQua.SupplierUserName;
                                    detail.SupplierCode = unQua.SupplierCode;
                                    detail.ActionType = "Init";
                                    detail.SourceInOrderCode = inBoundEntity.InBoundCode;

                                    /// 出库库存数量
                                    List<T_ContainerDetailEntity> cdOutList = new List<T_ContainerDetailEntity>();
                                    if (string.IsNullOrEmpty(unQua.Lot)) cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && string.IsNullOrEmpty(o.Lot));
                                    else cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && o.Lot == unQua.Lot);

                                    detail.Qty = cdOutList.Sum(o => o.Qty);
                                    detail.OutQty = 0;
                                    detail.WaveQty = 0;
                                    detail.Lot = unQua.Lot;
                                    detail.Spec = unQua.Spec;
                                    detail.ItemUnitText = unQua.ItemUnitText;
                                    detail.OverdueDate = unQua.OverdueDate;
                                    detail.State = "New";

                                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == detail.ItemID);
                                    /// 物料出库站台
                                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                    string containerKind = containerType.ContainerKind;
                                    if (containerKind == "Rack")
                                    {
                                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                        detail.StationID = station.F_Id;
                                        detail.StationCode = station.StationCode;
                                    }
                                    else if (containerKind == "Plastic" || containerKind == "Box")
                                    {
                                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                        detail.StationID = station.F_Id;
                                        detail.StationCode = station.StationCode;
                                    }
                                    else return Error("物料的容器类型未知:" + item.ItemCode, "");

                                    outDetailList.Add(detail);
                                    db.Insert<T_OutBoundDetailEntity>(detail);
                                }
                                db.SaveChanges();

                                #region 验退单自动出库（已注释）
                                /*
                                if (RuleConfig.OutConfig.ERPPost.AutoOutInterface)
                                {
                                    lock (lockObj)
                                    {
                                        /// 波次运算
                                        string[] needOutArray = outDetailList.Select(o => o.F_Id).ToArray();
                                        T_OutRecordApp outRecApp = new T_OutRecordApp();
                                        AjaxResult rst = outRecApp.WaveGen(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                                        if ((ResultType)rst.state != ResultType.success) return Error(rst.message, "");

                                        /// 发送任务给WCS
                                        IList<string> outBoundDetailIDList = detailList.Select(o => o.F_Id).ToList();
                                        rst = outRecApp.OutDetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                                        if ((ResultType)rst.state != ResultType.success) return Error(rst.message, "");
                                        db.SaveChanges();

                                        //发送消息到UI
                                        IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => o.CurOrderID == qAEntity.F_Id);
                                        foreach (T_StationEntity station in stationList)
                                        {
                                            RunnerOrder order = T_EquApp.GetRunnerOrderInfo(station.StationCode);
                                            T_EquApp.UIManage.RunnerList.First(o => o.Station.StationCode == station.StationCode).Order = order;
                                            UIManage uiManage = new UIManage();
                                            uiManage.RunnerList = new List<Runner>();
                                            uiManage.RunnerList.Add(new Runner() { Station = station.ToObject<StationModel>(), Order = order });
                                            SendMsg msg = new SendMsg();
                                            msg.WebUIPoint = FixType.WebUIPoint.Runner;
                                            msg.Data = new RunnerMsgModel() { Data = uiManage };
                                            WebSocketPost.SendToAll(msg);
                                        }
                                    }
                                }
                                 * */
                                #endregion
                            }
                        }

                        db.CommitWithOutRollBack();
                    }
                    else
                    {
                        db.RollBack();
                        return Error("质检结果已存在", "");
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

        #region 确认应用质检结果
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmQAResult(string QAID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QAGetController.ConfirmQAResult";
                logObj.Parms = new { QAID = QAID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "确认应用质检反馈结果";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************************/
                    T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(QAID);
                    if (qAEntity == null || qAEntity.QAOrderType != "GetSample") return Error("取样质检单据不存在", "");
                    if (qAEntity.State == "Over") return Error("质检单据已结束", "");
                    if (!(qAEntity.State == "Picked" || qAEntity.State == "WaitReturn" 
                        || qAEntity.State == "WaitApply" || qAEntity.State == "WaitApply")) 
                        return Error("质检单据未完成取样", "");

                    List<T_QAResultEntity> resultListAll = db.FindList<T_QAResultEntity>(o => o.RefOrderCode == qAEntity.RefOrderCode);
                    if (resultListAll.Count == 0) return Error("单据未收到反馈结果", "");
                    if (resultListAll.All(o => o.IsUsed == "true")) return Error("单据已应用反馈结果", "");

                    IList<T_QADetailEntity> detailListAll = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id).ToList();
                    IList<T_QARecordEntity> recordListAll = db.FindList<T_QARecordEntity>(o => o.QAID == qAEntity.F_Id).ToList();

                    /// 对应还样单
                    T_QAEntity qaReturnOrder = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == qAEntity.QACode && o.QAOrderType == "BackSample");
                    /// 质检单对应入库单
                    T_InBoundEntity inBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == qAEntity.RefInBoundCode);
                    if (inBoundEntity == null) return Error("未找到对应入库单", "");

                    /// 应用质检反馈结果
                    foreach (T_QAResultEntity qaResultEntity in resultListAll)
                    {
                        qaResultEntity.IsUsed = "true";

                        T_QADetailEntity qADetail = detailListAll.FirstOrDefault(o => o.SEQ == qaResultEntity.SEQ);
                        if (qADetail == null) return Error("不存在质检明细", "");

                        /// 更新库存质检状态
                        List<T_ContainerDetailEntity> detailList = new List<T_ContainerDetailEntity>();
                        if (string.IsNullOrEmpty(qADetail.Lot)) detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == inBoundEntity.F_Id);
                        else detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && o.Lot == qADetail.Lot && o.InBoundID == inBoundEntity.F_Id);

                        /// 结合质检取样记录，解冻库存
                        var recordList = db.FindList<T_QARecordEntity>(o => o.QADetailID == qADetail.F_Id);
                        foreach (T_ContainerDetailEntity cd in detailList)
                        {
                            if (qaResultEntity.QAResult == "Qua") cd.CheckState = "Qua";   /// 合格
                            else if (qaResultEntity.QAResult == "UnQua") cd.CheckState = "UnQua";  /// 不合格
                            else return Error("未知的质检结果参数", "");

                            T_QARecordEntity res = recordList.Find(o => o.ContainerDetailID == cd.F_Id);
                            if (res != null && res.IsReturnOver == "false" && res.IsAppearQA == "false") { }    /// 结果录入时，只有一种库存不能解冻：1.已取样 2.未还样 3.取样质检（不是外观质检）
                            else cd.IsCheckFreeze = "false";

                            db.Update<T_ContainerDetailEntity>(cd);
                        }

                        /// 修改明细质检状态
                        qADetail.ResultState = "ERP";
                        qADetail.QAResult = qaResultEntity.QAResult;
                        qADetail.ResultSendTime = qaResultEntity.AccessTime;
                        qADetail.SampleType = "Auto";
                        db.SaveChanges();

                        /// 同步修改还样单质检结果
                        if (qaReturnOrder != null) /// 存在质检还样单
                        {
                            if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false") /// 非破坏性质检，且需要还样明细
                            {
                                T_QADetailEntity qaDetailReturn = db.FindEntity<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id && o.SEQ == qADetail.SEQ);
                                if (qaDetailReturn.State == "WaitApply") qaDetailReturn.State = "Over";
                                qaDetailReturn.ResultState = "ERP";
                                qaDetailReturn.QAResult = qaResultEntity.QAResult;
                                qaDetailReturn.ResultSendTime = qaResultEntity.AccessTime;
                                qaDetailReturn.SampleType = "Auto";
                                db.Update<T_QADetailEntity>(qaDetailReturn);
                            }

                            /// 还样单据
                            List<T_QADetailEntity> qADetailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id).ToList();
                            if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitApply"))
                            {
                                qaReturnOrder.State = "Over";  /// 部分结束/待应用结果 ---> 全部结束
                            }
                            db.Update<T_QAEntity>(qaReturnOrder);
                        }

                        if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false" && qaReturnOrder == null) qADetail.State = "WaitReturn";
                        else qADetail.State = "Over";
                        db.Update<T_QADetailEntity>(qADetail);
                        db.SaveChanges();

                        /// 判断单据关闭
                        if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply"))
                        {
                            qAEntity.State = "Over";  /// 部分结束/待应用结果 ---> 全部结束
                        }
                        else if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply" || o.State == "WaitReturn"))
                        {
                            qAEntity.State = "WaitReturn"; /// 部分结束/待应用结果/待还样 ---> 待还样
                        }
                        db.Update<T_QAEntity>(qAEntity);
                        db.SaveChanges();

                        /// 生成验退单：取样单结束 && 还样单结束或未生成
                        if (qAEntity.State == "Over" && (qaReturnOrder == null || qaReturnOrder.State == "Over") && inBoundEntity.State == "Over")
                        {
                            /// 不合格明细
                            List<T_QADetailEntity> unQuaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id && o.QAResult == "UnQua");
                            if (unQuaDetailList.Count != 0)
                            {
                                /// 验退出库单
                                T_OutBoundEntity outBound = new T_OutBoundEntity();
                                outBound.F_Id = Guid.NewGuid().ToString();

                                outBound.ReceiveDepartment = "";
                                outBound.ReceiveDepartmentId = "";
                                outBound.ReceiveUserName = "";
                                outBound.ReceiveUserId = "";

                                outBound.OutBoundCode = T_CodeGenApp.GenNum("OutBoundRule");
                                outBound.RefOrderCode = outBound.OutBoundCode;
                                outBound.QAID = qAEntity.F_Id;
                                outBound.PointInBoundID = inBoundEntity.F_Id;

                                outBound.OutBoundType = "VerBackOut";
                                outBound.State = "New";
                                outBound.GenType = "ERP";
                                outBound.IsUrgent = "false";
                                outBound.TransState = "New";
                                outBound.Remark = $"质检单：{qAEntity.RefOrderCode} 有物料未通过质检，ERP结果反馈执行验退单";
                                outBound.F_DeleteMark = false;

                                db.Insert<T_OutBoundEntity>(outBound);
                                db.SaveChanges();

                                List<T_OutBoundDetailEntity> outDetailList = new List<T_OutBoundDetailEntity>();
                                foreach (T_QADetailEntity unQua in unQuaDetailList)
                                {
                                    T_OutBoundDetailEntity detail = new T_OutBoundDetailEntity();
                                    detail.F_Id = Guid.NewGuid().ToString();
                                    detail.OutBoundID = outBound.F_Id;
                                    detail.SEQ = Convert.ToInt32(unQua.SEQ);
                                    detail.ItemID = unQua.ItemID;
                                    detail.ItemName = unQua.ItemName;
                                    detail.ItemCode = unQua.ItemCode;
                                    detail.Factory = unQua.Factory;
                                    detail.SupplierUserID = unQua.SupplierUserID;
                                    detail.SupplierUserName = unQua.SupplierUserName;
                                    detail.SupplierCode = unQua.SupplierCode;

                                    /// 出库库存数量
                                    List<T_ContainerDetailEntity> cdOutList = new List<T_ContainerDetailEntity>();
                                    if (string.IsNullOrEmpty(unQua.Lot)) cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && string.IsNullOrEmpty(o.Lot));
                                    else cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && o.Lot == unQua.Lot);

                                    detail.Qty = cdOutList.Sum(o => o.Qty);
                                    detail.OutQty = 0;
                                    detail.WaveQty = 0;
                                    detail.Lot = unQua.Lot;
                                    detail.Spec = unQua.Spec;
                                    detail.ItemUnitText = unQua.ItemUnitText;
                                    detail.OverdueDate = unQua.OverdueDate;
                                    detail.State = "New";

                                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == detail.ItemID);
                                    /// 物料出库站台
                                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                    string containerKind = containerType.ContainerKind;
                                    if (containerKind == "Rack")
                                    {
                                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                        detail.StationID = station.F_Id;
                                        detail.StationCode = station.StationCode;
                                    }
                                    else if (containerKind == "Plastic" || containerKind == "Box")
                                    {
                                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                        detail.StationID = station.F_Id;
                                        detail.StationCode = station.StationCode;
                                    }
                                    else return Error($"物料的容器类型未知: {item.ItemCode}", "");

                                    outDetailList.Add(detail);
                                    db.Insert<T_OutBoundDetailEntity>(detail);
                                }
                                db.SaveChanges();

                                #region 验退单自动出库（已注释）
                                /*
                                if (RuleConfig.OutConfig.ERPPost.AutoOutInterface)
                                {
                                    lock (lockObj)
                                    {
                                        /// 波次运算
                                        string[] needOutArray = outDetailList.Select(o => o.F_Id).ToArray();
                                        T_OutRecordApp outRecApp = new T_OutRecordApp();
                                        AjaxResult rst = outRecApp.WaveGen(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                                        if ((ResultType)rst.state != ResultType.success)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = rst.message;
                                            return result;
                                        }

                                        /// 发送任务给WCS
                                        IList<string> outBoundDetailIDList = detailList.Select(o => o.F_Id).ToList();
                                        rst = outRecApp.OutDetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                                        if ((ResultType)rst.state != ResultType.success)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = rst.message;
                                            return result;
                                        }
                                        db.SaveChanges();

                                        //发送消息到UI
                                        IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => o.CurOrderID == qAEntity.F_Id);
                                        foreach (T_StationEntity station in stationList)
                                        {
                                            RunnerOrder order = T_EquApp.GetRunnerOrderInfo(station.StationCode);
                                            T_EquApp.UIManage.RunnerList.First(o => o.Station.StationCode == station.StationCode).Order = order;
                                            UIManage uiManage = new UIManage();
                                            uiManage.RunnerList = new List<Runner>();
                                            uiManage.RunnerList.Add(new Runner() { Station = station.ToObject<StationModel>(), Order = order });
                                            SendMsg msg = new SendMsg();
                                            msg.WebUIPoint = FixType.WebUIPoint.Runner;
                                            msg.Data = new RunnerMsgModel() { Data = uiManage };
                                            WebSocketPost.SendToAll(msg);
                                        }
                                    }
                                }
                                 * */
                                #endregion
                            }
                        }
                        db.Update<T_QAResultEntity>(qaResultEntity);
                    }

                    db.CommitWithOutRollBack();
                    /*************************************************************/

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
