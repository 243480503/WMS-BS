/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.APIPost;
using MST.Application.SystemSecurity;
using MST.Application.WebMsg;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Domain.ViewModel;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MST.Application.WMSLogic
{
    public class T_TaskApp
    {
        private static object syncObj = new object(); /// 单线程
        private static object lockObj = new object();   /// 验退单波次锁
        private static object taskOverLock = new object(); //任务完成线程 & 扫码申请线程 共用

        private IT_TaskRepository service = new T_TaskRepository();
        public IQueryable<T_TaskEntity> FindList(Expression<Func<T_TaskEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_TaskEntity FindEntity(Expression<Func<T_TaskEntity, bool>> predicate)
        {
            T_TaskEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_TaskEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_TaskEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_TaskEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_TaskEntity>();
            /// 过滤不可执行的任务
            expression = expression.And(t => t.IsCanExec == "true");
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.TaskNo.Contains(keyword) || t.BarCode.Contains(keyword) || t.SrcLocationCode.Contains(keyword) || t.TagLocationCode.Contains(keyword) || t.OrderCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }
        public T_TaskEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_TaskEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.Create();
                service.Insert(itemsEntity);
            }
        }

        public void Insert(T_TaskEntity entity)
        {
            service.Insert(entity);
        }


        #region 移动任务到历史任务
        public void MoveToHis(IRepositoryBase db, string taskNo)
        {
            T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskNo);
            T_TaskHisEntity taskHis = new T_TaskHisEntity();
            taskHis.F_Id = task.F_Id;
            taskHis.TaskNo = task.TaskNo;
            taskHis.TaskInOutType = task.TaskInOutType;
            taskHis.TaskType = task.TaskType;
            taskHis.ContainerID = task.ContainerID;
            taskHis.BarCode = task.BarCode;
            taskHis.ContainerType = task.ContainerType;
            taskHis.SrcLocationID = task.SrcLocationID;
            taskHis.SrcLocationCode = task.SrcLocationCode;
            taskHis.SrcWCSLocCode = task.SrcWCSLocCode;
            taskHis.TagAreaID = task.TagAreaID;
            taskHis.TagLocationID = task.TagLocationID;
            taskHis.TagLocationCode = task.TagLocationCode;
            taskHis.TagWCSLocCode = task.TagWCSLocCode;
            taskHis.ApplyStationID = task.ApplyStationID;
            taskHis.WaveID = task.WaveID;
            taskHis.WaveCode = task.WaveCode;
            taskHis.CurEquID3D = task.CurEquID3D;
            taskHis.WaveDetailID = task.WaveDetailID;
            taskHis.SEQ = task.SEQ;
            taskHis.PointExecRobotCode = task.PointExecRobotCode;
            taskHis.Level = task.Level;
            taskHis.IsWcsTask = task.IsWcsTask;
            taskHis.ExecEquID = task.ExecEquID;
            taskHis.State = task.State;
            taskHis.IsCanExec = task.IsCanExec;
            taskHis.SendWCSTime = task.SendWCSTime;
            taskHis.OrderType = task.OrderType;
            taskHis.OrderID = task.OrderID;
            taskHis.OrderDetailID = task.OrderDetailID;
            taskHis.OrderCode = task.OrderCode;
            taskHis.OverTime = task.OverTime;
            taskHis.MoveDate = DateTime.Now;
            taskHis.IsHandPointLoc = task.IsHandPointLoc;
            taskHis.F_DeleteMark = false;
            taskHis.F_CreatorTime = task.F_CreatorTime;
            taskHis.F_CreatorUserId = task.F_CreatorUserId;
            taskHis.CreatorUserName = task.CreatorUserName;
            taskHis.F_DeleteUserId = task.F_DeleteUserId;
            taskHis.F_DeleteTime = task.F_DeleteTime;
            taskHis.DeleteUserName = task.DeleteUserName;
            taskHis.F_LastModifyTime = task.F_LastModifyTime;
            taskHis.F_LastModifyUserId = task.F_LastModifyUserId;
            taskHis.ModifyUserName = task.ModifyUserName;
            taskHis.SendMsg = task.SendMsg;

            db.Insert<T_TaskHisEntity>(taskHis);
            db.Delete<T_TaskEntity>(task);
        }
        #endregion

        #region 手动下发出库任务
        public WCSResult TaskDownToWCS_Hand(IList<string> taskCodeList)
        {
            WCSResult result = new WCSResult();

            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    result = new WCSPost().SendTask(db, taskCodeList);
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    else
                    {
                        db.RollBack();
                    }
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        /**********************任务通用方法***********************/

        #region 扫码申请入库（正常入库站台，正常出库站台），分配货位并返回任务给WCS（WCS调用）
        public class ApplyInTaskModel
        {
            public string ApplyStationCode { get; set; }    /// 申请入库站台编码
            public string BarCode { get; set; } /// 申请的容器编码

            public bool IsRF { get; set; } //是否RF扫码触发(输送线扫码器触发用不到)

            public string PointLocCode { get; set; } //RF扫码触发的货位(输送线扫码器触发用不到)

            public bool IsSendToAGV { get; set; } //RF扫码触发是否需要发送到WCS(输送线扫码器触发用不到)

        }

        #region 扫码器、手持RF 触发申请入库(不包含大件流道)
        public WCSResult ApplyInTask(ApplyInTaskModel inModel)
        {
            lock (taskOverLock) //为解决采购单入库数量累加问题，需保证扫码事件和任务完成事件必须为单线程，避免不可重复读
            {
                lock (syncObj)
                {
                    using (var db = new RepositoryBase().BeginTrans())
                    {
                        try
                        {
                            WCSResult result = new WCSResult();
                            T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.StationCode == inModel.ApplyStationCode);
                            if (stationEntity == null)
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "申请站台不存在";
                                return result;
                            }
                            else
                            {

                                if (string.IsNullOrEmpty(inModel.BarCode))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "容器编码不能为空";
                                    return result;
                                }
                                T_TaskEntity taskOld = db.FindEntity<T_TaskEntity>(o => o.BarCode == inModel.BarCode);
                                if (taskOld != null && (!string.IsNullOrEmpty(taskOld.SendMsg)) && false) //之前扫码器已经申请过，但任务没有完成，此为判断是否为重复申请
                                {
                                    WCSApplayResult obj = taskOld.SendMsg.ToObject<WCSApplayResult>();
                                    result.IsSuccess = true;
                                    result.FailCode = "0000";
                                    result.Data = obj;
                                }
                                else
                                {
                                    /// 入库申请需考虑站台单据明细，出库不需考虑
                                    if ((stationEntity.StationCode == FixType.Station.StationIn_BigItem.ToString() || stationEntity.StationCode == FixType.Station.StationIn_Normal.ToString())
                                        && string.IsNullOrEmpty(stationEntity.CurOrderDetailID) && stationEntity.CurModel != "Empty")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "申请站台未绑定单据";
                                        return result;
                                    }

                                    /// 正常入库站台扫码入库
                                    if (stationEntity.StationCode == FixType.Station.StationIn_Normal.ToString())
                                    {
                                        if (stationEntity.CurModel == "Empty") // 空容器模式
                                        {
                                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                            T_ContainerTypeEntity containerTypeEntity = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                            result = ApplyIn_NormalStationIn_ForEmptyPlastic(db, stationEntity, inModel, item, containerTypeEntity); // 创建任务
                                        }
                                        else
                                        {
                                            T_InBoundDetailEntity detail = db.FindEntity<T_InBoundDetailEntity>(o => o.F_Id == stationEntity.CurOrderDetailID);
                                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == detail.ItemID);
                                            T_ContainerTypeEntity containerTypeEntity = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                            if (containerTypeEntity.ContainerKind == "Box") // (纸箱) 当前入库单据为纸箱，需先创建收货记录，然创建任务
                                            {
                                                // 创建收货记录，创建任务,分配货位,返回任务（纸箱自动入库）
                                                result = ApplyIn_NormalStationIn_ForBox(db, stationEntity, inModel, detail, item, containerTypeEntity); // 创建任务
                                            }
                                            else if (containerTypeEntity.ContainerKind == "Plastic") // (料箱) 正常站台的 
                                            {
                                                result = ApplyIn_NormalStationIn_ForPlastic(db, stationEntity, inModel, detail);
                                            }
                                            else
                                            {
                                                result.IsSuccess = false;
                                                result.FailCode = "0001";
                                                result.FailMsg = "容器大类未知";
                                                return result;
                                            }
                                        }

                                    }
                                    else if (stationEntity.StationCode == FixType.Station.StationOut_Normal.ToString()) /// 正常出库站台扫码入库（非空纸箱 与 料箱 的回库）
                                    {
                                        result = ApplyIn_NormalStationOut_ForBoxAndPlastic(db, stationEntity, inModel);
                                    }
                                    else
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "未知的申请站台编码";
                                        return result;
                                    }
                                }

                            }
                            if (result.IsSuccess)
                            {
                                if (inModel.IsRF) //使用RF客户端请求
                                {
                                    if (inModel.IsSendToAGV) //需要发送到AGV
                                    {
                                        IList<string> taskCodeList = new List<string>();
                                        T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.BarCode == inModel.BarCode);
                                        if (task != null)
                                        {
                                            taskCodeList.Add(task.TaskNo);
                                            result = new WCSPost().SendTask(db, taskCodeList);
                                        }
                                        else
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "任务未产生";
                                            return result;
                                        }

                                        if (!result.IsSuccess)
                                        {
                                            db.RollBack();
                                            return result;
                                        }
                                    }
                                }

                                db.CommitWithOutRollBack();
                            }
                            else
                            {
                                db.RollBack();
                            }
                            return result;
                        }
                        catch (Exception ex)
                        {
                            db.RollBack();
                            LogFactory.GetLogger().Error(new LogObj() { Path = "T_TaskApp.ApplyInTask", Parms = inModel.ToJson(), Message = ex.Message });
                            throw;
                        }
                    }
                }
            }

        }
        #endregion

        private class WCSApplayResult
        {
            public string TaskNo { get; set; }
            public string TaskInOutType { get; set; }
            public string TaskType { get; set; }
            public string SrcLocationCode { get; set; }
            public string TagLocationCode { get; set; }

            public string SrcWCSLocCode { get; set; }
            public string TagWCSLocCode { get; set; }
            public int? Level { get; set; }
            public string OrderType { get; set; }
            public string OrderCode { get; set; }

            public string StationCode { get; set; }

            /// <summary>
            /// 容器宽度
            /// </summary>
            public decimal? ConWidth { get; set; }

            /// <summary>
            /// 容器高度
            /// </summary>
            public decimal? ConHeight { get; set; }

            /// <summary>
            /// 容器深度
            /// </summary>
            public decimal? ConLength { get; set; }

            /// <summary>
            /// 容器大类
            /// </summary>
            public string ContainerKind { get; set; }

            /// <summary>
            /// 容器类型
            /// </summary>
            public string ContainerType { get; set; }
            /// <summary>
            /// AGV对应的箱型编码
            /// </summary>
            public string AGVContainerTypeCode { get; set; }
            public string ContainerBarCode { get; set; }
            public IList<string> ItemCodes { get; set; }
        }


        #region 出库后再申请入库，非空纸箱 与 料箱 的回库
        /// <summary>
        /// 非空纸箱 与 料箱  的回库
        /// </summary>
        /// <param name="db"></param>
        /// <param name="stationEntity"></param>
        /// <param name="inModel"></param>
        /// <returns></returns>
        public WCSResult ApplyIn_NormalStationOut_ForBoxAndPlastic(IRepositoryBase db, T_StationEntity stationEntity, ApplyInTaskModel inModel)
        {
            WCSResult result = new WCSResult();
            T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();

            T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.BarCode == inModel.BarCode && o.IsWcsTask == "true");
            if (taskEntity == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "容器任务不存在";
                return result;
            }

            if (taskEntity.IsCanExec == "false")
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "该任务不允许执行";
                return result;
            }

            if (taskEntity.State != "New")
            {
                result.IsSuccess = true;
                result.FailCode = "0000";
                result.FailMsg = "该容器已申请过";
                return result;
            }

            taskEntity.SendWCSTime = DateTime.Now;

            string errMsg = "";
            T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskEntity.ContainerType);
            T_LocationApp locationApp = new T_LocationApp();

            T_LocationEntity loc;
            if (inModel.IsRF && (!string.IsNullOrEmpty(inModel.PointLocCode))) //是RF扫码入库，并且指定了货位
            {
                if (taskEntity.TaskType == "TaskType_EmptyIn")
                {
                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                    loc = locationApp.CheckLocIn(ref errMsg, db, containerType, taskEntity.TagAreaID, true, null, item.F_Id, null, inModel.PointLocCode, true);
                }
                else
                {
                    T_ContainerDetailEntity detail = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == taskEntity.BarCode);
                    loc = locationApp.CheckLocIn(ref errMsg, db, containerType, taskEntity.TagAreaID, false, detail.ERPWarehouseCode, detail.ItemID, detail.CheckState, inModel.PointLocCode, true);
                }
            }
            else
            {
                LogObj log = null;
                if (taskEntity.TaskType == "TaskType_EmptyIn")
                {
                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                    loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, true, null, null, true, null, item);
                }
                else
                {
                    T_ContainerDetailEntity detail = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == taskEntity.BarCode);
                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == detail.ItemID);
                    loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, false, detail.ERPWarehouseCode, detail.CheckState, true, null, item);
                }

            }
            if (loc == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"入库货位分配失败：{ errMsg }";
                return result;
            }

            if (inModel.IsRF && (!string.IsNullOrEmpty(inModel.PointLocCode))) //是RF扫码入库，并且指定了货位
            {
                taskEntity.IsHandPointLoc = "true";
            }
            else
            {
                taskEntity.IsHandPointLoc = "false";
            }
            taskEntity.TagLocationCode = loc.LocationCode;
            taskEntity.TagLocationID = loc.F_Id;
            taskEntity.TagWCSLocCode = loc.WCSLocCode;
            if (inModel.IsRF)
            {
                taskEntity.State = "New"; //RF需推送任务
            }
            else
            {
                taskEntity.State = "Execing";  //扫码器不需推送任务
            }

            IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == taskEntity.BarCode).ToList();
            IList<string> detailItemBarCodeList = new List<string>();
            if (detailList.Count > 0)
            {
                detailItemBarCodeList = detailList.Select(o => o.ItemBarCode).ToList();
            }


            WCSApplayResult obj = new WCSApplayResult()
            {
                TaskNo = taskEntity.TaskNo,
                TaskInOutType = taskEntity.TaskInOutType,
                TaskType = taskEntity.TaskType,
                SrcLocationCode = taskEntity.SrcLocationCode,
                TagLocationCode = taskEntity.TagLocationCode,
                SrcWCSLocCode = taskEntity.SrcWCSLocCode,
                TagWCSLocCode = taskEntity.TagWCSLocCode,
                Level = taskEntity.Level,
                OrderType = taskEntity.OrderType,
                OrderCode = taskEntity.OrderCode,
                ItemCodes = detailItemBarCodeList,
                ConLength = containerType.BorderLong,
                ConHeight = containerType.BorderHeight,
                ConWidth = containerType.BorderWidth,
                ContainerKind = containerType.ContainerKind,
                ContainerType = containerType.ContainerTypeCode,
                StationCode = inModel.ApplyStationCode,
                ContainerBarCode = taskEntity.BarCode,
                AGVContainerTypeCode = containerType.AGVContainerTypeCode
            };
            taskEntity.SendMsg = obj.ToJson();
            db.Update<T_TaskEntity>(taskEntity);
            db.SaveChanges();

            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == taskEntity.ApplyStationID); /// 解绑站台操作，关联的是任务的申请站台
            if (station == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"未找到容器回库任务申请站台：{ taskEntity.BarCode }";
                return result;
            }

            bool isStationOver = false;
            string taskType = taskEntity.TaskType;
            switch (taskType)
            {
                case "TaskType_GetItemBack": /// 领料回库
                    {
                        IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完
                        if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                        {
                            isStationOver = true;
                        }

                        IList<T_OutRecordEntity> list = db.FindList<T_OutRecordEntity>(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_OutRecordEntity rec in list)
                        {
                            rec.IsScanBack = "true";
                            db.Update<T_OutRecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "GetItemOut", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_CheckReturnIn": /// 质检还样入库
                    {
                        /// 还样出库需要找到：质检取样单对应的质检记录
                        T_QAEntity qABack = db.FindEntity<T_QAEntity>(o => o.F_Id == station.CurOrderID);
                        T_QAEntity qAGet = db.FindEntity<T_QAEntity>(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");

                        List<T_QARecordEntity> recList = db.FindList<T_QARecordEntity>(o => o.TagLocationID == station.F_Id && o.QAID == qAGet.F_Id && o.IsNeedBack == "true");
                        if (recList.All(o => o.State == "Over" && o.IsReturnOver == "true"))
                        {
                            isStationOver = true;
                        }

                        List<T_QARecordEntity> list = recList.Where(o => o.QAID == qAGet.F_Id && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_QARecordEntity rec in list)
                        {
                            rec.IsScanBack_Back = "true";
                            db.Update<T_QARecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "BackSample", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_CheckPickIn":/// 质检取样入库
                    {
                        List<T_QARecordEntity> recList = db.FindList<T_QARecordEntity>(o => o.TagLocationID == station.F_Id && o.QAID == station.CurOrderID);
                        if (recList.All(o => o.State == "Picked") || recList.Count == 0)
                        {
                            isStationOver = true;
                        }

                        List<T_QARecordEntity> list = recList.Where(o => o.QAID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_QARecordEntity rec in list)
                        {
                            rec.IsScanBack_Get = "true";
                            db.Update<T_QARecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "GetSample", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_CountIn": /// 盘点入库
                    {
                        /// 盘点记录已完成 && 所有任务入库已分配货位
                        List<T_CountRecordEntity> recList = db.FindList<T_CountRecordEntity>(o => o.StationID == station.F_Id && o.CountID == station.CurOrderID);
                        if (recList.All(o => o.IsOutCount == "true" && o.CountState == "Over") || recList.Count == 0)
                        {
                            isStationOver = true;
                        }

                        List<T_CountRecordEntity> list = recList.Where(o => o.CountID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_CountRecordEntity rec in list)
                        {
                            rec.IsScanBack = "true";
                            db.Update<T_CountRecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "Count", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_VerBackIn": /// 验退回库
                    {
                        IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完
                        if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                        {
                            isStationOver = true;
                        }

                        List<T_OutRecordEntity> list = recList.Where(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_OutRecordEntity rec in list)
                        {
                            rec.IsScanBack = "true";
                            db.Update<T_OutRecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "VerBackOut", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_WarehouseBackIn": /// 仓退回库
                    {
                        IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完
                        if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                        {
                            isStationOver = true;
                        }

                        List<T_OutRecordEntity> list = recList.Where(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_OutRecordEntity rec in list)
                        {
                            rec.IsScanBack = "true";
                            db.Update<T_OutRecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "WarehouseBackOut", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_OtherIn": /// 其它回库
                    {
                        IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完
                        if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                        {
                            isStationOver = true;
                        }

                        List<T_OutRecordEntity> list = recList.Where(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_OutRecordEntity rec in list)
                        {
                            rec.IsScanBack = "true";
                            db.Update<T_OutRecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "OtherOut", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_OffRack": //下架（下架没有回库申请）
                    break;
                case "TaskType_EmptyIn": //空容器回库
                    {
                        string stationOrderType = station.OrderType;
                        switch (stationOrderType)
                        {
                            case "GetItemOut": //领料出库单
                                {
                                    IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完（包含正在出库的任务、应出库但未出库的任务）
                                    if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                                    {
                                        isStationOver = true;
                                    }

                                    List<T_OutRecordEntity> list = recList.Where(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                                    foreach (T_OutRecordEntity rec in list)
                                    {
                                        rec.IsScanBack = "true";
                                        db.Update<T_OutRecordEntity>(rec);
                                    }
                                }
                                break;
                            case "WarehouseBackOut":// 仓退出库单
                                {
                                    IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完（包含正在出库的任务、应出库但未出库的任务）
                                    if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                                    {
                                        isStationOver = true;
                                    }

                                    List<T_OutRecordEntity> list = recList.Where(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                                    foreach (T_OutRecordEntity rec in list)
                                    {
                                        rec.IsScanBack = "true";
                                        db.Update<T_OutRecordEntity>(rec);
                                    }
                                }
                                break;
                            case "VerBackOut"://验退出库
                                {
                                    IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完（包含正在出库的任务、应出库但未出库的任务）
                                    if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                                    {
                                        isStationOver = true;
                                    }

                                    List<T_OutRecordEntity> list = recList.Where(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                                    foreach (T_OutRecordEntity rec in list)
                                    {
                                        rec.IsScanBack = "true";
                                        db.Update<T_OutRecordEntity>(rec);
                                    }
                                }
                                break;
                            case "OtherOut": //其它出库
                                {
                                    IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完（包含正在出库的任务、应出库但未出库的任务）
                                    if (recList.Count < 1) //该站台已没有当前单据任务和拣选任务
                                    {
                                        isStationOver = true;
                                    }

                                    List<T_OutRecordEntity> list = recList.Where(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                                    foreach (T_OutRecordEntity rec in list)
                                    {
                                        rec.IsScanBack = "true";
                                        db.Update<T_OutRecordEntity>(rec);
                                    }
                                }
                                break;
                            case "GetSample"://质检取样出库
                                {
                                    List<T_QARecordEntity> recList = db.FindList<T_QARecordEntity>(o => o.TagLocationID == station.F_Id && o.QAID == station.CurOrderID);
                                    if (recList.All(o => o.State == "Picked") || recList.Count == 0)
                                    {
                                        isStationOver = true;
                                    }

                                    List<T_QARecordEntity> list = recList.Where(o => o.BarCode == taskEntity.BarCode).ToList();
                                    foreach (T_QARecordEntity rec in list)
                                    {
                                        rec.IsScanBack_Get = "true";
                                        db.Update<T_QARecordEntity>(rec);
                                    }
                                }
                                break;
                            case "OffRack": //下架单
                                {
                                    List<T_OffRackRecordEntity> recList = db.FindList<T_OffRackRecordEntity>(o => o.StationID == station.F_Id && o.OffRackID == station.CurOrderID);
                                    if (recList.All(o => o.State == "OverPick") || recList.Count == 0)
                                    {
                                        isStationOver = true;
                                    }

                                    List<T_OffRackRecordEntity> list = recList.Where(o => o.BarCode == taskEntity.BarCode).ToList();
                                    foreach (T_OffRackRecordEntity rec in list)
                                    {
                                        rec.IsScanBack = "true";
                                        db.Update<T_OffRackRecordEntity>(rec);
                                    }
                                }
                                break;
                            default:
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = $"站台单据类型未知";
                                    return result;
                                }
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "EmptyIn", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                case "TaskType_LocCountErrIn": /// 异常盘点入库
                    {
                        /// 盘点记录已完成 && 所有任务入库已分配货位
                        List<T_LocCountRecordEntity> recList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == station.CurOrderID);
                        if (recList.All(o => o.CountState == "Over") || recList.Count == 0)
                        {
                            isStationOver = true;
                        }

                        List<T_LocCountRecordEntity> list = recList.Where(o => o.LocCountID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                        foreach (T_LocCountRecordEntity rec in list)
                        {
                            rec.IsScanBack = "true";
                            db.Update<T_LocCountRecordEntity>(rec);
                        }

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, loc, "InType", "LocCount", "Empty", "In", taskEntity.TaskNo);
                    }
                    break;
                default:
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"单据类型未知";
                        return result;
                    }
            }

            //当前回库容器就是站台绑定的容器，则清除绑定容器
            if (stationEntity.BarCode == inModel.BarCode)
            {
                stationEntity.BarCode = "";
            }

            if (isStationOver) /// 单据已完成
            {
                stationEntity.CurOrderID = "";
                stationEntity.WaveID = "";
                stationEntity.OrderType = "";
                stationEntity.CurOrderDetailID = "";
            }

            db.Update<T_StationEntity>(stationEntity);
            db.SaveChanges();

            result.IsSuccess = true;
            result.FailCode = "0000";
            result.Data = obj;

            return result;
        }
        #endregion

        #region 分配货位，返回任务（料箱入库,非空箱模式）
        public WCSResult ApplyIn_NormalStationIn_ForPlastic(IRepositoryBase db, T_StationEntity stationEntity, ApplyInTaskModel inModel, T_InBoundDetailEntity detail)
        {
            WCSResult result = new WCSResult();
            T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.BarCode == inModel.BarCode);
            if (taskEntity == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "容器任务不存在";
                return result;
            }

            if (taskEntity.IsCanExec == "false")
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "该任务不允许执行";
                return result;
            }


            IList<T_ReceiveRecordEntity> receiveList = db.FindList<T_ReceiveRecordEntity>(o => o.BarCode == inModel.BarCode && o.State == "LockOver" && o.InBoundDetailID == taskEntity.OrderDetailID);
            T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskEntity.ContainerType);


            if (taskEntity.State != "New") //已申请,RF组盘点击入库，新创建时为New，申请后为执行中
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "该容器已申请";
                return result;
            }

            taskEntity.SendWCSTime = DateTime.Now;

            string errMsg = "";
            T_LocationApp locationApp = new T_LocationApp();
            T_LocationEntity loc = null;
            if (taskEntity.IsHandPointLoc == "true")
            {
                loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == taskEntity.TagLocationID);
            }
            else
            {
                T_ReceiveRecordEntity oneRec = db.FindEntity<T_ReceiveRecordEntity>(o => o.BarCode == inModel.BarCode && o.State == "LockOver" && o.InBoundDetailID == taskEntity.OrderDetailID);
                LogObj log = null;
                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == oneRec.ItemID);
                loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, false, oneRec.ERPWarehouseCode, oneRec.CheckState, true, null, item);
                if (loc == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = $"入库货位分配失败：{ errMsg }";
                    return result;
                }
            }

            taskEntity.TagLocationCode = loc.LocationCode;
            taskEntity.TagLocationID = loc.F_Id;
            taskEntity.TagWCSLocCode = loc.WCSLocCode;
            if (inModel.IsRF)
            {
                taskEntity.State = "New"; //RF需推送任务
            }
            else
            {
                taskEntity.State = "Execing";  //扫码器不需推送任务
            }

            IList<string> detailItemBarCodeList = new List<string>();
            if (receiveList.Count > 0)
            {
                detailItemBarCodeList = receiveList.Select(o => o.ItemBarCode).ToList();
            }

            WCSApplayResult obj = new WCSApplayResult()
            {
                TaskNo = taskEntity.TaskNo,
                TaskInOutType = taskEntity.TaskInOutType,
                TaskType = taskEntity.TaskType,
                SrcLocationCode = taskEntity.SrcLocationCode,
                TagLocationCode = taskEntity.TagLocationCode,
                SrcWCSLocCode = taskEntity.SrcWCSLocCode,
                TagWCSLocCode = taskEntity.TagWCSLocCode,
                Level = taskEntity.Level,
                OrderType = taskEntity.OrderType,
                OrderCode = taskEntity.OrderCode,
                ItemCodes = detailItemBarCodeList,
                ConLength = containerType.BorderLong,
                ConHeight = containerType.BorderHeight,
                ConWidth = containerType.BorderWidth,
                ContainerKind = containerType.ContainerKind,
                ContainerType = containerType.ContainerTypeCode,
                StationCode = inModel.ApplyStationCode,
                ContainerBarCode = taskEntity.BarCode,
                AGVContainerTypeCode = containerType.AGVContainerTypeCode
            };

            taskEntity.SendMsg = obj.ToJson();

            db.Update<T_TaskEntity>(taskEntity);
            db.SaveChanges();

            /// 货位状态变更记录
            new T_LocationStateDetailApp().SyncLocState(db, loc, "InType", "PurchaseIn", "Empty", "In", taskEntity.TaskNo);

            foreach (T_ReceiveRecordEntity item in receiveList)
            {
                item.LocationCode = loc.LocationCode;
                item.LocationID = loc.F_Id;
                item.IsScanOver = "true";
                db.Update<T_ReceiveRecordEntity>(item);
            }
            db.SaveChanges();



            IList<T_ReceiveRecordEntity> notAskReceiveList = db.FindList<T_ReceiveRecordEntity>(o => string.IsNullOrEmpty(o.LocationID) && o.ReceiveStaionID == stationEntity.F_Id && (o.State == "LockOver" || o.State == "NewGroup") && o.InBoundDetailID == stationEntity.CurOrderDetailID);
            if (detail.CurQty >= detail.Qty && notAskReceiveList.Count < 1 && stationEntity.CurOrderDetailID == detail.F_Id) //单据明细已完成,且没有当前明细需要入库的其它待入库容器
            {
                stationEntity.BarCode = "";
                stationEntity.CurOrderDetailID = "";
                stationEntity.CurOrderID = "";
                stationEntity.WaveID = "";
                stationEntity.OrderType = "";
                db.Update<T_StationEntity>(stationEntity);
            }
            db.SaveChanges();



            result.IsSuccess = true;
            result.FailCode = "0000";
            result.Data = obj;

            return result;
        }
        #endregion

        #region 创建收货记录，创建任务,分配货位,返回任务（纸箱自动入库）
        public WCSResult ApplyIn_NormalStationIn_ForBox(IRepositoryBase db, T_StationEntity stationEntity, ApplyInTaskModel inModel, T_InBoundDetailEntity detail, T_ItemEntity item, T_ContainerTypeEntity containerTypeEntity)
        {
            WCSResult result = new WCSResult();

            T_TaskEntity inDBTask = db.FindEntity<T_TaskEntity>(o => o.BarCode == inModel.BarCode && o.ApplyStationID == stationEntity.F_Id);
            if (inDBTask != null) //新提交
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"该条码任务已存在：{ inModel.BarCode }";
                return result;
            }
            T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.F_Id == detail.StoreAreaID);
            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == inModel.BarCode && o.F_DeleteMark == false);
            if (containerEntity == null) /// 如果是新容器
            {
                containerEntity = new T_ContainerEntity();
                containerEntity.F_Id = Guid.NewGuid().ToString();
                containerEntity.BarCode = inModel.BarCode;
                containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                containerEntity.IsContainerVir = "0";

                containerEntity.AreaID = areaEntnty.F_Id;
                containerEntity.AreaCode = areaEntnty.AreaCode;
                containerEntity.AreaName = areaEntnty.AreaName;
                containerEntity.F_DeleteMark = false;
                db.Insert<T_ContainerEntity>(containerEntity);
                db.SaveChanges();
            }
            else
            {
                if (containerEntity.F_DeleteMark == true)
                {
                    containerEntity.BarCode = inModel.BarCode;
                    containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                    containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                    containerEntity.IsContainerVir = "0";
                    containerEntity.AreaID = areaEntnty.F_Id;
                    containerEntity.AreaCode = areaEntnty.AreaCode;
                    containerEntity.AreaName = areaEntnty.AreaName;
                    containerEntity.F_DeleteMark = false;

                    db.Update<T_ContainerEntity>(containerEntity);
                    db.SaveChanges();
                }
                else
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = $"该容器条码已存在：{ inModel.BarCode }";
                    return result;
                }
            }


            T_MarkRuleEntity rule = db.FindEntity<T_MarkRuleEntity>(o => o.InBoundDetailID == detail.F_Id);
            if (rule == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "没有申请标签";
                return result;
            }

            T_MarkRecordEntity markRecord = db.FindEntity<T_MarkRecordEntity>(o => o.MarkRuleID == rule.F_Id && o.BarCode == inModel.BarCode);
            if (markRecord == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "不存在该条码";
                return result;
            }
            markRecord.IsUsed = "true";
            db.Update<T_MarkRecordEntity>(markRecord);

            decimal SumQty = db.FindList<T_ReceiveRecordEntity>(o => o.InBoundDetailID == detail.F_Id).Sum(o => o.Qty ?? 0) + markRecord.Qty ?? 0;
            if (SumQty > detail.Qty && detail.IsMustQtySame == "true")
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"应收数量已满";
                return result;
            }

            db.SaveChanges();

            string errMsg = "";
            T_LocationApp locationApp = new T_LocationApp();

            T_LocationEntity loc;
            if (inModel.IsRF && (!string.IsNullOrEmpty(inModel.PointLocCode))) //是RF扫码入库，并且指定了货位
            {
                loc = locationApp.CheckLocIn(ref errMsg, db, containerTypeEntity, detail.StoreAreaID, false, detail.ERPWarehouseCode, detail.ItemID, detail.CheckState, inModel.PointLocCode, true);
            }
            else
            {
                LogObj log = null;
                loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerTypeEntity, detail.StoreAreaID, false, detail.ERPWarehouseCode, detail.CheckState, true, null, item);
            }
            if (loc == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"入库货位分配失败：{ errMsg }";
                return result;
            }

            T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == detail.InBoundID);
            T_ReceiveRecordEntity receiveRecordEntity = new T_ReceiveRecordEntity();
            receiveRecordEntity.Create();
            receiveRecordEntity.InBoundID = inbound.F_Id;
            receiveRecordEntity.InBoundDetailID = detail.F_Id;
            receiveRecordEntity.ReceiveStaionID = stationEntity.F_Id;
            receiveRecordEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
            receiveRecordEntity.BarCode = inModel.BarCode;
            receiveRecordEntity.ItemBarCode = inModel.BarCode;
            receiveRecordEntity.ItemID = item.F_Id;
            receiveRecordEntity.IsScanOver = "true";
            receiveRecordEntity.ItemCode = item.ItemCode;
            receiveRecordEntity.Qty = markRecord.Qty;
            receiveRecordEntity.ProductDate = detail.ProductDate;
            receiveRecordEntity.ERPWarehouseCode = detail.ERPWarehouseCode;
            receiveRecordEntity.SEQ = detail.SEQ;
            receiveRecordEntity.AreaID = detail.StoreAreaID;
            receiveRecordEntity.Lot = detail.Lot;
            receiveRecordEntity.Spec = item.Spec;
            receiveRecordEntity.ItemUnitText = item.ItemUnitText;
            receiveRecordEntity.CheckState = detail.CheckState;
            receiveRecordEntity.SupplierUserID = inbound.SupplierUserID;
            receiveRecordEntity.DoneUserID = stationEntity.F_LastModifyUserId;
            receiveRecordEntity.DoneUserName = stationEntity.ModifyUserName;
            receiveRecordEntity.LocationID = loc.F_Id;
            receiveRecordEntity.LocationCode = loc.LocationCode;
            receiveRecordEntity.State = "LockOver";
            receiveRecordEntity.TransState = "WaittingTrans";
            receiveRecordEntity.ContainerKind = "Box";
            receiveRecordEntity.FailDesc = null;
            receiveRecordEntity.F_DeleteMark = false;
            receiveRecordEntity.IsItemMark = item.IsItemMark;
            receiveRecordEntity.Factory = item.Factory;
            receiveRecordEntity.ValidityDayNum = detail.ValidityDayNum;
            receiveRecordEntity.OverdueDate = detail.OverdueDate;
            db.Insert<T_ReceiveRecordEntity>(receiveRecordEntity);

            detail.CurQty = (detail.CurQty ?? 0) + receiveRecordEntity.Qty;
            db.Update<T_InBoundDetailEntity>(detail);

            inDBTask = new T_TaskEntity();
            inDBTask.Create();
            inDBTask.TaskNo = T_CodeGenApp.GenNum("TaskRule");
            inDBTask.TaskInOutType = "InType";
            string inBoundType = inbound.InBoundType;
            if (inBoundType == "PurchaseInType")
            {
                inDBTask.TaskType = "TaskType_PurchaseIn";
                inDBTask.OrderType = "PurchaseIn";
            }
            else
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"未知的入库单单据类型：{ inbound.InBoundType }";
                return result;
            }

            inDBTask.ContainerID = containerEntity.F_Id;
            inDBTask.BarCode = receiveRecordEntity.BarCode;
            inDBTask.ContainerType = containerTypeEntity.ContainerTypeCode;
            inDBTask.SrcLocationID = stationEntity.F_Id;
            inDBTask.SrcLocationCode = stationEntity.LeaveAddress;
            inDBTask.SrcWCSLocCode = stationEntity.LeaveAddress;
            inDBTask.TagAreaID = detail.StoreAreaID;
            inDBTask.ApplyStationID = stationEntity.F_Id;
            inDBTask.SEQ = detail.SEQ;
            inDBTask.Level = 10;
            if (inModel.IsRF)
            {
                inDBTask.State = "New"; //RF需推送任务
            }
            else
            {
                inDBTask.State = "Execing";  //扫码器不需推送任务
            }
            inDBTask.IsWcsTask = "true";
            inDBTask.ExecEquID = null;
            inDBTask.IsCanExec = "true";
            inDBTask.SendWCSTime = DateTime.Now;
            inDBTask.OverTime = null;
            inDBTask.F_DeleteMark = false;
            inDBTask.OrderCode = inbound.InBoundCode;
            inDBTask.OrderDetailID = detail.F_Id;
            inDBTask.OrderID = inbound.F_Id;
            if (inModel.IsRF && (!string.IsNullOrEmpty(inModel.PointLocCode))) //是RF扫码入库，并且指定了货位
            {
                inDBTask.IsHandPointLoc = "true";
            }
            else
            {
                inDBTask.IsHandPointLoc = "false";
            }
            inDBTask.TagLocationCode = loc.LocationCode;
            inDBTask.TagLocationID = loc.F_Id;
            inDBTask.TagWCSLocCode = loc.WCSLocCode;

            IList<T_ReceiveRecordEntity> receiveList = db.FindList<T_ReceiveRecordEntity>(o => o.BarCode == inDBTask.BarCode && o.State == "LockOver" && o.InBoundDetailID == stationEntity.CurOrderDetailID).ToList();
            IList<string> detailItemBarCodeList = new List<string>();
            if (receiveList.Count > 0)
            {
                detailItemBarCodeList = receiveList.Select(o => o.ItemCode).ToList();
            }

            WCSApplayResult obj = new WCSApplayResult()
            {
                TaskNo = inDBTask.TaskNo,
                TaskInOutType = inDBTask.TaskInOutType,
                TaskType = inDBTask.TaskType,
                SrcLocationCode = inDBTask.SrcLocationCode,
                TagLocationCode = inDBTask.TagLocationCode,
                SrcWCSLocCode = inDBTask.SrcWCSLocCode,
                TagWCSLocCode = inDBTask.TagWCSLocCode,
                Level = inDBTask.Level,
                OrderType = inDBTask.OrderType,
                OrderCode = inDBTask.OrderCode,
                ItemCodes = detailItemBarCodeList,
                ConLength = containerTypeEntity.BorderLong,
                ConHeight = containerTypeEntity.BorderHeight,
                ConWidth = containerTypeEntity.BorderWidth,
                ContainerKind = containerTypeEntity.ContainerKind,
                ContainerType = containerTypeEntity.ContainerTypeCode,
                StationCode = inModel.ApplyStationCode,
                ContainerBarCode = inDBTask.BarCode,
                AGVContainerTypeCode = containerTypeEntity.AGVContainerTypeCode
            };

            inDBTask.SendMsg = obj.ToJson();
            db.Insert<T_TaskEntity>(inDBTask);

            db.SaveChanges();

            /// 货位状态变更记录
            new T_LocationStateDetailApp().SyncLocState(db, loc, "InType", "PurchaseIn", "Empty", "In", inDBTask.TaskNo);

            //清理站台
            if (detail.CurQty == detail.Qty && stationEntity.CurOrderDetailID == detail.F_Id && detail.IsMustQtySame == "true") //单据明细已完成
            {
                stationEntity.BarCode = "";
                stationEntity.CurOrderDetailID = "";
                stationEntity.CurOrderID = "";
                stationEntity.WaveID = "";
                stationEntity.OrderType = "";
                db.Update<T_StationEntity>(stationEntity);
                db.SaveChanges();
            }




            result.IsSuccess = true;
            result.FailCode = "0000";
            result.Data = obj;

            return result;
        }
        #endregion

        #region 创建收货记录，创建任务,分配货位,料箱空容器模式
        public WCSResult ApplyIn_NormalStationIn_ForEmptyPlastic(IRepositoryBase db, T_StationEntity stationEntity, ApplyInTaskModel inModel, T_ItemEntity item, T_ContainerTypeEntity containerTypeEntity)
        {
            WCSResult result = new WCSResult();

            T_TaskEntity inDBTask = db.FindEntity<T_TaskEntity>(o => o.BarCode == inModel.BarCode && o.ApplyStationID == stationEntity.F_Id);
            if (inDBTask != null) //已申请
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"该容器已申请：{ inModel.BarCode }";
                return result;
            }

            T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.NormalArea.ToString());
            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == inModel.BarCode && o.F_DeleteMark == false);
            if (containerEntity == null) /// 如果是新容器
            {
                containerEntity = new T_ContainerEntity();
                containerEntity.F_Id = Guid.NewGuid().ToString();
                containerEntity.BarCode = inModel.BarCode;
                containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                containerEntity.IsContainerVir = "0";
                containerEntity.AreaID = areaEntnty.F_Id;
                containerEntity.AreaCode = areaEntnty.AreaCode;
                containerEntity.AreaName = areaEntnty.AreaName;
                containerEntity.F_DeleteMark = false;
                db.Insert<T_ContainerEntity>(containerEntity);
                db.SaveChanges();
            }
            else
            {
                if (containerEntity.F_DeleteMark == true)
                {
                    containerEntity.BarCode = inModel.BarCode;
                    containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                    containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                    containerEntity.IsContainerVir = "0";
                    containerEntity.AreaID = areaEntnty.F_Id;
                    containerEntity.AreaCode = areaEntnty.AreaCode;
                    containerEntity.AreaName = areaEntnty.AreaName;
                    containerEntity.F_DeleteMark = false;
                    db.Update<T_ContainerEntity>(containerEntity);
                    db.SaveChanges();
                }
                else
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = $"该容器条码已存在：{ inModel.BarCode }";
                    return result;
                }
            }

            string errMsg = "";
            T_LocationApp locationApp = new T_LocationApp();
            T_LocationEntity loc;

            if (inModel.IsRF && (!string.IsNullOrEmpty(inModel.PointLocCode))) //是RF扫码入库，并且指定了货位
            {
                loc = locationApp.CheckLocIn(ref errMsg, db, containerTypeEntity, areaEntnty.F_Id, true, null, item.F_Id, null, inModel.PointLocCode, true);
            }
            else
            {
                LogObj log = null;
                loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerTypeEntity, areaEntnty.F_Id, true, null, null, true, null, item);
            }
            if (loc == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = $"入库货位分配失败：{ errMsg }";
                return result;
            }

            /// 生成入库任务
            inDBTask = new T_TaskEntity();
            inDBTask.Create();
            inDBTask.TaskNo = T_CodeGenApp.GenNum("TaskRule");
            inDBTask.TaskInOutType = "InType";

            inDBTask.TaskType = "TaskType_EmptyIn";
            inDBTask.OrderType = "EmptyIn";

            inDBTask.ContainerID = containerEntity.F_Id;
            inDBTask.BarCode = inModel.BarCode;
            inDBTask.ContainerType = containerTypeEntity.ContainerTypeCode;
            inDBTask.SrcLocationID = stationEntity.F_Id;
            inDBTask.SrcLocationCode = stationEntity.LeaveAddress;
            inDBTask.SrcWCSLocCode = stationEntity.LeaveAddress;
            inDBTask.TagAreaID = areaEntnty.F_Id;
            inDBTask.ApplyStationID = stationEntity.F_Id;
            inDBTask.SEQ = 1;
            inDBTask.Level = 10;
            if (inModel.IsRF)
            {
                inDBTask.State = "New"; //RF需推送任务
            }
            else
            {
                inDBTask.State = "Execing";  //扫码器不需推送任务
            }
            inDBTask.IsWcsTask = "true";
            inDBTask.ExecEquID = null;
            inDBTask.IsCanExec = "true";
            inDBTask.SendWCSTime = DateTime.Now;
            inDBTask.OverTime = null;
            inDBTask.F_DeleteMark = false;
            inDBTask.OrderCode = "";
            inDBTask.OrderDetailID = "";
            inDBTask.OrderID = "";
            inDBTask.TagLocationCode = loc.LocationCode;
            inDBTask.TagWCSLocCode = loc.WCSLocCode;
            if (inModel.IsRF && (!string.IsNullOrEmpty(inModel.PointLocCode))) //是RF扫码入库，并且指定了货位
            {
                inDBTask.IsHandPointLoc = "true";
            }
            else
            {
                inDBTask.IsHandPointLoc = "false";
            }
            inDBTask.TagLocationID = loc.F_Id;

            WCSApplayResult obj = new WCSApplayResult()
            {
                TaskNo = inDBTask.TaskNo,
                TaskInOutType = inDBTask.TaskInOutType,
                TaskType = inDBTask.TaskType,
                SrcLocationCode = inDBTask.SrcLocationCode,
                TagLocationCode = inDBTask.TagLocationCode,
                SrcWCSLocCode = inDBTask.SrcWCSLocCode,
                TagWCSLocCode = inDBTask.TagWCSLocCode,
                Level = inDBTask.Level,
                OrderType = inDBTask.OrderType,
                OrderCode = inDBTask.OrderCode,
                ConLength = containerTypeEntity.BorderLong,
                ConHeight = containerTypeEntity.BorderHeight,
                ConWidth = containerTypeEntity.BorderWidth,
                ContainerKind = containerTypeEntity.ContainerKind,
                ContainerType = containerTypeEntity.ContainerTypeCode,
                StationCode = inModel.ApplyStationCode,
                ContainerBarCode = inDBTask.BarCode,
                AGVContainerTypeCode = containerTypeEntity.AGVContainerTypeCode
            };

            inDBTask.SendMsg = obj.ToJson();
            db.Insert<T_TaskEntity>(inDBTask);
            db.SaveChanges();

            /// 货位状态变更记录
            new T_LocationStateDetailApp().SyncLocState(db, loc, "InType", "EmptyIn", "Empty", "In", inDBTask.TaskNo);

            result.IsSuccess = true;
            result.FailCode = "0000";
            result.Data = obj;

            return result;
        }
        #endregion


        #endregion

        #region 任务状态反馈（WCS调用）
        public class StateChangeTaskModel
        {
            public string TaskCode { get; set; }    /// 任务编号
            public string State { get; set; }   /// 任务状态(该WCS任务状态，可能与WMS任务状态不同，暂先使用WMS任务状态)
        }

        public WCSResult StateChangeTask(StateChangeTaskModel stateChangeTaskModel)
        {
            WCSResult result = new WCSResult();
            if (string.IsNullOrEmpty(stateChangeTaskModel.TaskCode))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "TaskCode不能为空";
                return result;
            }
            if (string.IsNullOrEmpty(stateChangeTaskModel.State))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "状态State不能为空";
                return result;
            }

            lock (taskOverLock)
            {
                using (var db = new RepositoryBase().BeginTrans())
                {
                    try
                    {
                        if (stateChangeTaskModel.State == "Over")
                        {
                            T_TaskApp taskApp = new T_TaskApp();
                            T_TaskEntity taskTemp = db.FindEntity<T_TaskEntity>(o => o.TaskNo == stateChangeTaskModel.TaskCode);
                            /// 任务完成
                            result = taskApp.TaskOver(db, stateChangeTaskModel.TaskCode);
                            if (result.IsSuccess)
                            {
                                taskApp.MoveToHis(db, stateChangeTaskModel.TaskCode);

                                db.SaveChanges();
                                db.CommitWithOutRollBack();
                            }
                            else
                            {
                                db.RollBack();
                            }
                            return result;
                        }
                        else if (stateChangeTaskModel.State == "HungUp")
                        {
                            /// 挂起
                            T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.TaskNo == stateChangeTaskModel.TaskCode);
                            if (task == null)
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "无此条码对应的任务";
                                return result;
                            }

                            if (task.State == "HungUp")
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "该任务已处于挂起状态";
                                return result;
                            }

                            task.State = "HungUp";
                            db.Update<T_TaskEntity>(task);

                            db.CommitWithOutRollBack();

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "WMS无此对应的状态";
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        db.RollBack();
                        LogFactory.GetLogger().Info(new LogObj() { Path = "T_TaskApp.StateChangeTask", Parms = stateChangeTaskModel.ToJson(), Message = ex.Message });
                        throw;
                    }
                }
            }

        }
        #endregion

        /*********************************************************/

        #region 所有任务类型的完成操作
        public WCSResult TaskOver(IRepositoryBase db, string taskCode)
        {
            try
            {
                WCSResult result = new WCSResult();
                T_LocationStateDetailApp locState = new T_LocationStateDetailApp();

                T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskCode);
                if (task == null)
                {
                    T_TaskHisEntity taskHis = db.FindEntity<T_TaskHisEntity>(o => o.TaskNo == taskCode);
                    if (taskHis != null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"该任务已完成：{ taskCode }";
                        return result;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"任务不存在：{ taskCode }";
                        return result;
                    }
                }

                if (task.State != "Execing")
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = $"任务状态必须为执行中：{ task.TaskNo }";
                    return result;
                }

                task.State = "Over";
                task.OverTime = DateTime.Now;
                db.Update<T_TaskEntity>(task);

                string curType = task.TaskType;
                switch (curType)
                {
                    case "TaskType_CheckPickOut":/// 质检取样出库
                        {
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            tagStation.BarCode = task.BarCode;
                            //没有绑定单据明细，是因为可能一个容器对应多个单据明细
                            tagStation.CurOrderDetailID = task.OrderDetailID;
                            db.Update<T_StationEntity>(tagStation);

                            T_LocationEntity tagLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == tagStation.StationCode); //货位与站台是特例： LocationCode = StationCode
                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID && o.F_DeleteMark == false);
                            containerEntity.LocationID = tagLoc.F_Id;
                            containerEntity.LocationNo = tagLoc.LocationCode;
                            containerEntity.AreaID = tagLoc.AreaID;
                            containerEntity.AreaCode = tagLoc.AreaCode;
                            containerEntity.AreaName = tagLoc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLoc.F_Id;
                                cdEntity.LocationNo = tagLoc.LocationCode;
                                cdEntity.AreaID = tagLoc.AreaID;
                                cdEntity.AreaCode = tagLoc.AreaCode;
                                cdEntity.AreaName = tagLoc.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            IList<T_QARecordEntity> curRecordList = db.FindList<T_QARecordEntity>(o => o.QADetailID == task.OrderDetailID && o.BarCode == task.BarCode && o.TagLocationCode == task.TagLocationCode && o.State == "WaitPick").ToList();
                            foreach (T_QARecordEntity qacell in curRecordList)
                            {
                                qacell.State = "Picking";
                                qacell.IsArrive_Get = "true";
                                db.Update<T_QARecordEntity>(qacell);
                            }

                            T_QADetailEntity qADetail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == task.OrderDetailID);
                            qADetail.State = "Picking";
                            db.Update<T_QADetailEntity>(qADetail);
                            db.SaveChanges();

                            T_QAEntity qaEntity = db.FindEntity<T_QAEntity>(o => o.F_Id == task.OrderID);
                            IList<T_QADetailEntity> qADetailList = db.FindList<T_QADetailEntity>(o => o.QAID == task.OrderID && o.State == "Outing");
                            if (qADetailList.Count == 0) qaEntity.State = "Picking";
                            db.Update<T_QAEntity>(qaEntity);
                            db.SaveChanges();

                            IList<T_TaskDetailEntity> curTaskDetailList = db.FindList<T_TaskDetailEntity>(o => o.BarCode == task.BarCode && o.OrderDetailID == task.OrderDetailID && o.IsCurTask == "true");
                            foreach (T_TaskDetailEntity cell in curTaskDetailList)
                            {
                                cell.IsCurTask = "false";
                                cell.IsOver = "true";
                                cell.OverTime = DateTime.Now;
                                db.Update<T_TaskDetailEntity>(cell);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "GetSample", "Out", "Empty", task.TaskNo);

                            //质检出库不用自动产生下一个任务

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_CheckPickIn":/// 质检取样入库
                        {
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID && o.F_DeleteMark == false);
                            containerEntity.LocationID = loc.F_Id;
                            containerEntity.LocationNo = loc.LocationCode;
                            containerEntity.AreaID = loc.AreaID;
                            containerEntity.AreaCode = loc.AreaCode;
                            containerEntity.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                            foreach (T_ContainerDetailEntity detail in detailList)
                            {
                                detail.LocationID = loc.F_Id;
                                detail.LocationNo = loc.LocationCode;
                                detail.AreaID = loc.AreaID;
                                detail.AreaCode = loc.AreaCode;
                                detail.AreaName = loc.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "GetSample", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_CheckReturnOut":/// 质检还样出库
                        {
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            tagStation.BarCode = task.BarCode;
                            //没有绑定单据明细，是因为可能一个容器对应多个单据明细
                            tagStation.CurOrderDetailID = task.OrderDetailID;
                            db.Update<T_StationEntity>(tagStation);

                            T_LocationEntity tagLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == tagStation.StationCode); //货位与站台是特例： LocationCode = StationCode
                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID && o.F_DeleteMark == false);
                            containerEntity.LocationID = tagLoc.F_Id;
                            containerEntity.LocationNo = tagLoc.LocationCode;
                            containerEntity.AreaID = tagLoc.AreaID;
                            containerEntity.AreaCode = tagLoc.AreaCode;
                            containerEntity.AreaName = tagLoc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLoc.F_Id;
                                cdEntity.LocationNo = tagLoc.LocationCode;
                                cdEntity.AreaID = tagLoc.AreaID;
                                cdEntity.AreaCode = tagLoc.AreaCode;
                                cdEntity.AreaName = tagLoc.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            T_QAEntity qABack = db.FindEntity<T_QAEntity>(o => o.F_Id == task.OrderID);
                            T_QAEntity qAGet = db.FindEntity<T_QAEntity>(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");
                            IList<T_QARecordEntity> curRecordList = db.FindList<T_QARecordEntity>(o => o.QAID == qAGet.F_Id && o.BarCode == task.BarCode && o.State == "WaitReturn").ToList();
                            foreach (T_QARecordEntity cell in curRecordList)
                            {
                                cell.State = "Returning";
                                cell.IsArrive_Back = "true";
                                db.Update<T_QARecordEntity>(cell);
                            }

                            T_QADetailEntity qADetail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == task.OrderDetailID);
                            qADetail.State = "Returning";
                            db.Update<T_QADetailEntity>(qADetail);

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "BackSample", "Out", "Empty", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_CheckReturnIn":/// 质检还样入库
                        {
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID && o.F_DeleteMark == false);
                            containerEntity.LocationID = loc.F_Id;
                            containerEntity.LocationNo = loc.LocationCode;
                            containerEntity.AreaID = loc.AreaID;
                            containerEntity.AreaCode = loc.AreaCode;
                            containerEntity.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                            foreach (T_ContainerDetailEntity detail in detailList)
                            {
                                detail.LocationID = loc.F_Id;
                                detail.LocationNo = loc.LocationCode;
                                detail.AreaID = loc.AreaID;
                                detail.AreaCode = loc.AreaCode;
                                detail.AreaName = loc.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "BackSample", "In", "Stored", task.TaskNo);

                            /// 已生成还样单的质检单，还样回库任务完成时，产生验退单
                            T_QAEntity qaBack = db.FindEntity<T_QAEntity>(o => o.F_Id == task.OrderID);
                            T_QAEntity qaGet = db.FindEntity<T_QAEntity>(o => o.QACode == qaBack.RefOrderCode);
                            T_InBoundEntity inBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == qaGet.RefInBoundCode);

                            if (qaGet.State == "Over" && qaBack.State == "Over")
                            {
                                /// 不合格明细
                                List<T_QADetailEntity> unQuaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qaGet.F_Id && o.QAResult == "UnQua");
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
                                    outBound.QAID = qaGet.F_Id;
                                    outBound.PointInBoundID = inBoundEntity.F_Id;

                                    outBound.OutBoundType = "VerBackOut";
                                    outBound.State = "New";
                                    outBound.GenType = "ERP";
                                    outBound.IsUrgent = "false";
                                    outBound.TransState = "New";
                                    outBound.Remark = $"质检单：{qaGet.RefOrderCode} 有物料未通过质检，还样完成执行验退单";
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
                                        if (string.IsNullOrEmpty(unQua.Lot)) cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode);
                                        else cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && o.Lot == unQua.Lot);

                                        detail.Qty = cdOutList.Sum(o => o.Qty);
                                        detail.OutQty = 0;
                                        detail.WaveQty = 0;
                                        detail.Lot = unQua.Lot;
                                        detail.Spec = unQua.Spec;
                                        detail.ItemUnitText = unQua.ItemUnitText;
                                        detail.OverdueDate = unQua.OverdueDate;
                                        detail.State = "New";
                                        detail.SourceInOrderCode = inBoundEntity.ERPInDocCode;

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
                                        else
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料的容器类型未知:" + item.ItemCode;
                                            return result;
                                        }

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
                                            IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => o.CurOrderID == qaGet.F_Id);
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

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_CountOut":/// 盘点出库
                        {
                            /// 更新货位状态
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.SrcLocationID);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            /// 站台绑定到达容器
                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            //station.CurOrderDetailID = task.OrderDetailID;    /// 盘点绑定站台，仅需绑定盘点单和容器
                            station.BarCode = task.BarCode;
                            station.CurOrderDetailID = task.OrderDetailID;
                            db.Update<T_StationEntity>(station);

                            /// 更新容器货位
                            T_LocationEntity tagLocation = db.FindEntity<T_LocationEntity>(o => o.LocationCode == station.StationCode); /// 货位=站台
                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID && o.F_DeleteMark == false);
                            container.LocationID = tagLocation.F_Id;
                            container.LocationNo = tagLocation.LocationCode;
                            container.AreaID = tagLocation.AreaID;
                            container.AreaCode = tagLocation.AreaCode;
                            container.AreaName = tagLocation.AreaName;
                            db.Update<T_ContainerEntity>(container);

                            /// 更新库存明细货位
                            List<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == container.F_Id);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLocation.F_Id;
                                cdEntity.LocationNo = tagLocation.LocationCode;
                                cdEntity.AreaID = tagLocation.AreaID;
                                cdEntity.AreaCode = tagLocation.AreaCode;
                                cdEntity.AreaName = tagLocation.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            /// 更新盘点记录状态
                            IList<T_CountRecordEntity> curRecordList = db.FindList<T_CountRecordEntity>(o => o.CountID == task.OrderID && o.BarCode == task.BarCode && o.StationID == task.TagLocationID && o.CountState == "Outing").ToList();
                            foreach (T_CountRecordEntity record in curRecordList)
                            {
                                if (record.CountState != "NoNeed")
                                {
                                    record.CountState = "Counting";
                                }

                                record.IsArrive = "true";
                                db.Update<T_CountRecordEntity>(record);
                            }

                            /// 更新盘点明细状态
                            T_CountDetailEntity detail = db.FindEntity<T_CountDetailEntity>(o => o.F_Id == task.OrderDetailID);
                            detail.CountState = "Counting";
                            db.Update<T_CountDetailEntity>(detail);
                            db.SaveChanges();

                            /// 更新盘点单状态
                            IList<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == task.OrderID && o.CountState == "Outing");
                            if (detailList.Count == 0)
                            {
                                T_CountEntity countEntity = db.FindEntity<T_CountEntity>(o => o.F_Id == task.OrderID);
                                countEntity.State = "Counting";
                                db.Update<T_CountEntity>(countEntity);
                            }
                            db.SaveChanges();

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "Count", "Out", "Empty", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_CountIn":/// 盘点入库
                        {
                            /// 更新货位状态
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            /// 更新容器货位
                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID && o.F_DeleteMark == false);
                            container.LocationID = loc.F_Id;
                            container.LocationNo = loc.LocationCode;
                            container.AreaID = loc.AreaID;
                            container.AreaCode = loc.AreaCode;
                            container.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(container);

                            /// 更新库存明细货位
                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                            foreach (T_ContainerDetailEntity detail in containerDetailList)
                            {
                                detail.LocationID = loc.F_Id;
                                detail.LocationNo = loc.LocationCode;
                                detail.AreaID = loc.AreaID;
                                detail.AreaCode = loc.AreaCode;
                                detail.AreaName = loc.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "Count", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_LocCountErrIn": /// 货位盘点异常回库
                        {
                            /// 更新货位状态
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            /// 更新容器货位
                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID && o.F_DeleteMark == false);
                            if (container != null)
                            {
                                container.LocationID = loc.F_Id;
                                container.LocationNo = loc.LocationCode;
                                container.AreaID = loc.AreaID;
                                container.AreaCode = loc.AreaCode;
                                container.AreaName = loc.AreaName;
                                db.Update<T_ContainerEntity>(container);

                                /// 更新库存明细货位
                                IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                                foreach (T_ContainerDetailEntity detail in containerDetailList)
                                {
                                    detail.LocationID = loc.F_Id;
                                    detail.LocationNo = loc.LocationCode;
                                    detail.AreaID = loc.AreaID;
                                    detail.AreaCode = loc.AreaCode;
                                    detail.AreaName = loc.AreaName;
                                    db.Update<T_ContainerDetailEntity>(detail);
                                }
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "LocCount", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_CountAGV": /// 在库盘点
                        {
                            /// TODO　更新货位，完成任务
                            /// 不回传这个接口，直接反馈盘点AGV数据反馈 CountData
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = $"在库盘点调用[反馈盘点数据接口：CountData]：{ task.TaskNo }";
                            return result;
                        }
                    case "TaskType_PurchaseIn":/// 采购入库
                        {
                            IList<T_ReceiveRecordEntity> receList = db.FindList<T_ReceiveRecordEntity>(o => o.BarCode == task.BarCode && o.State == "LockOver").ToList();
                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            T_LocationEntity locationEntity = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            T_AreaEntity areaEntity = db.FindEntity<T_AreaEntity>(o => o.F_Id == locationEntity.AreaID);
                            foreach (T_ReceiveRecordEntity rece in receList)
                            {
                                T_ItemEntity itemEntity = db.FindEntity<T_ItemEntity>(o => o.F_Id == rece.ItemID);
                                T_ItemKindEntity itemKindEntity = db.FindEntity<T_ItemKindEntity>(o => o.F_Id == itemEntity.ItemKindID);
                                T_SupplierEntity supplierEntity = db.FindEntity<T_SupplierEntity>(o => o.F_Id == rece.SupplierUserID);

                                T_ContainerDetailEntity containerDetailEntity = new T_ContainerDetailEntity();
                                containerDetailEntity.F_Id = Guid.NewGuid().ToString();
                                containerDetailEntity.ItemID = rece.ItemID;
                                containerDetailEntity.ContainerID = containerEntity.F_Id;
                                containerDetailEntity.ContainerType = rece.ContainerType;
                                containerDetailEntity.LocationID = locationEntity.F_Id;
                                containerDetailEntity.LocationNo = locationEntity.LocationCode;
                                containerDetailEntity.AreaID = locationEntity.AreaID;
                                containerDetailEntity.AreaCode = areaEntity.AreaCode;
                                containerDetailEntity.AreaName = areaEntity.AreaName;
                                containerDetailEntity.KindCode = itemKindEntity.KindCode;
                                containerDetailEntity.KindName = itemKindEntity.KindName;
                                containerDetailEntity.ItemName = itemEntity.ItemName;
                                containerDetailEntity.ItemCode = itemEntity.ItemCode;
                                containerDetailEntity.ItemBarCode = rece.ItemBarCode;
                                containerDetailEntity.Qty = rece.Qty;
                                containerDetailEntity.OutQty = 0;
                                containerDetailEntity.ItemUnitText = rece.ItemUnitText;
                                containerDetailEntity.State = "Normal";
                                containerDetailEntity.IsCountFreeze = "false";
                                containerDetailEntity.Lot = rece.Lot;
                                containerDetailEntity.Spec = rece.Spec;
                                containerDetailEntity.ProductDate = rece.ProductDate;
                                containerDetailEntity.ERPWarehouseCode = rece.ERPWarehouseCode;
                                containerDetailEntity.BarCode = rece.BarCode;
                                containerDetailEntity.ContainerKind = rece.ContainerKind;
                                containerDetailEntity.IsItemMark = rece.IsItemMark;
                                containerDetailEntity.Factory = rece.Factory;
                                containerDetailEntity.OverdueDate = rece.OverdueDate;
                                containerDetailEntity.ValidityDayNum = rece.ValidityDayNum;
                                containerDetailEntity.SupplierID = supplierEntity.F_Id;
                                containerDetailEntity.SupplierCode = supplierEntity.SupplierCode;
                                containerDetailEntity.SupplierName = supplierEntity.SupplierName;
                                containerDetailEntity.ReceiveRecordID = rece.F_Id;
                                //containerDetailEntity.IsSpecial = itemEntity.IsSpecial;
                                containerDetailEntity.InBoundID = rece.InBoundID;
                                containerDetailEntity.InBoundDetailID = rece.InBoundDetailID;
                                containerDetailEntity.SEQ = rece.SEQ;
                                containerDetailEntity.F_DeleteMark = false;
                                containerDetailEntity.CheckQty = 0;

                                T_InBoundEntity inBoundTemp = db.FindEntityAsNoTracking<T_InBoundEntity>(o => o.F_Id == rece.InBoundID);
                                containerDetailEntity.RefInBoundCode = inBoundTemp.RefOrderCode;
                                containerDetailEntity.InBoundCode = inBoundTemp.InBoundCode;
                                containerDetailEntity.ERPInDocCode = inBoundTemp.ERPInDocCode;

                                #region 后续入库质检状态
                                /// 获取已抛送的质检结果，更新入库新库存质检信息
                                bool checkStateInit = true; /// 是否初始化质检信息
                                T_InBoundEntity inBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.F_Id == rece.InBoundID);
                                T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.RefInBoundCode == inBoundEntity.RefOrderCode);
                                if (qAEntity != null) /// 存在质检单
                                {
                                    /// 获取已有库存状态
                                    T_ContainerDetailEntity cdPre = db.FindEntity<T_ContainerDetailEntity>(o => o.InBoundDetailID == rece.InBoundDetailID);
                                    if (cdPre != null)
                                    {
                                        checkStateInit = false;
                                        containerDetailEntity.CheckState = cdPre.CheckState;
                                        containerDetailEntity.CheckDetailID = cdPre.CheckDetailID;
                                        containerDetailEntity.CheckID = cdPre.CheckID;
                                        if ((cdPre.CheckState == "Qua" || cdPre.CheckState == "UnQua") && cdPre.CheckQty != 0)
                                            containerDetailEntity.IsCheckFreeze = "false";  /// 已抛送结果，待还样时候，不应用质检解冻状态
                                        else containerDetailEntity.IsCheckFreeze = cdPre.IsCheckFreeze;
                                    }
                                    else /// 不存在库存，判断是否破坏性质检 取空
                                    {
                                        T_QADetailEntity qaDetail = db.FindEntity<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id && o.ItemID == rece.ItemID && (o.Lot == rece.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(rece.Lot))));
                                        if (qaDetail != null && qaDetail.IsBroken == "true")
                                        {
                                            checkStateInit = false;
                                            containerDetailEntity.CheckID = qaDetail.QAID;
                                            containerDetailEntity.CheckDetailID = qaDetail.F_Id;

                                            if (qaDetail.QAResult == "Qua" || qaDetail.QAResult == "UnQua")
                                            {
                                                containerDetailEntity.CheckState = qaDetail.QAResult;
                                                containerDetailEntity.IsCheckFreeze = "false";
                                            }
                                            else //取组盘时的质检状态
                                            {
                                                containerDetailEntity.CheckState = rece.CheckState;
                                                containerDetailEntity.IsCheckFreeze = "false";
                                            }
                                        }
                                    }
                                }

                                if (checkStateInit)
                                {
                                    containerDetailEntity.CheckState = rece.CheckState;
                                    containerDetailEntity.CheckDetailID = null;
                                    containerDetailEntity.CheckID = null;
                                    containerDetailEntity.IsCheckFreeze = "false";
                                }
                                #endregion

                                db.Insert<T_ContainerDetailEntity>(containerDetailEntity);

                                /// 采购入库流水
                                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                inOutDetailApp.SyncInOutDetail(db, containerDetailEntity, "InType", "PurchaseIn", 0, containerDetailEntity.Qty, task.TaskNo);

                                rece.State = "PutawayOver";
                                db.Update<T_ReceiveRecordEntity>(rece);
                            }

                            containerEntity.LocationID = locationEntity.F_Id;
                            containerEntity.LocationNo = locationEntity.LocationCode;
                            containerEntity.AreaID = areaEntity.F_Id;
                            containerEntity.AreaCode = areaEntity.AreaCode;
                            containerEntity.AreaName = areaEntity.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);


                            locationEntity.State = "Stored";
                            db.Update<T_LocationEntity>(locationEntity);

                            T_InBoundDetailEntity inBoundDetail = db.FindEntity<T_InBoundDetailEntity>(o => o.F_Id == task.OrderDetailID);
                            decimal? curQty = receList.Sum(o => o.Qty);
                            LogFactory.GetLogger().Info(new LogObj() { Message = "采购任务完成：任务编码：" + task.TaskNo + ",入库单据：" + task.OrderCode + ",当前已上架" + (inBoundDetail.OverInQty ?? 0) + ",当前需上架：" + (curQty ?? 0) });//异常点日志
                            inBoundDetail.OverInQty = (inBoundDetail.OverInQty ?? 0) + (curQty ?? 0);
                            db.Update<T_InBoundDetailEntity>(inBoundDetail);
                            db.SaveChanges();

                            if (inBoundDetail.Qty == inBoundDetail.OverInQty) //当前入库明细入库完毕
                            {
                                if (inBoundDetail.IsMustQtySame == "true")
                                {
                                    inBoundDetail.State = "Over";

                                    db.Update<T_InBoundDetailEntity>(inBoundDetail);
                                    db.SaveChanges();

                                    IList<T_InBoundDetailEntity> inBoundDetailAllList = db.FindList<T_InBoundDetailEntity>(o => o.InBoundID == task.OrderID).Where(o => o.State != "Over").ToList();
                                    if (inBoundDetailAllList.Count < 1) //所有明细已完成
                                    {
                                        //T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == inBoundDetail.StationID);
                                        //station.OrderType = "";
                                        //station.CurOrderDetailID = "";
                                        //station.CurOrderID = "";
                                        //station.BarCode = "";
                                        //station.WaveID = "";
                                        //db.Update<T_StationEntity>(station);

                                        T_InBoundEntity inBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.F_Id == task.OrderID);
                                        inBoundEntity.State = "Over";
                                        db.Update<T_InBoundEntity>(inBoundEntity);

                                        #region 生成验退单
                                        /// 生成验退单：取样单结束 && 还样单结束或未生成 && 入库单完成（仅针对未完成入库单执行质检结果）
                                        T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.RefInBoundCode == inBoundEntity.RefOrderCode && o.QAOrderType == "GetSample");
                                        if (qAEntity != null && qAEntity.State == "Over") /// 存在质检单，且执行完成
                                        {
                                            T_OutBoundEntity outBound = db.FindEntity<T_OutBoundEntity>(o => o.QAID == qAEntity.F_Id && o.OutBoundType == "VerBackOut");
                                            if (outBound == null) /// 未生成验退单
                                            {
                                                T_QAEntity qaReturnOrder = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == qAEntity.QACode && o.QAOrderType == "BackSample");
                                                if (qaReturnOrder == null || qaReturnOrder.State == "Over")
                                                {
                                                    /// 不合格明细
                                                    List<T_QADetailEntity> unQuaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id && o.QAResult == "UnQua");
                                                    if (unQuaDetailList.Count != 0)
                                                    {
                                                        /// 验退出库单
                                                        outBound = new T_OutBoundEntity();
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
                                                                T_StationEntity stationOut = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                                                detail.StationID = stationOut.F_Id;
                                                                detail.StationCode = stationOut.StationCode;
                                                            }
                                                            else if (containerKind == "Plastic" || containerKind == "Box")
                                                            {
                                                                T_StationEntity stationOut = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                                                detail.StationID = stationOut.F_Id;
                                                                detail.StationCode = stationOut.StationCode;
                                                            }
                                                            else
                                                            {
                                                                result.IsSuccess = false;
                                                                result.FailCode = "0001";
                                                                result.FailMsg = $"物料的容器类型未知：{ item.ItemCode }";
                                                                return result;
                                                            }

                                                            outDetailList.Add(detail);
                                                            db.Insert<T_OutBoundDetailEntity>(detail);
                                                        }
                                                        db.SaveChanges();
                                                    }
                                                }
                                            }
                                        }
                                        #endregion

                                        /// 产生过账信息，并发送过账信息
                                        if (RuleConfig.OrderTransRule.InBoundTransRule.InBoundTrans)
                                        {
                                            if (inBoundEntity.GenType == "ERP")
                                            {
                                                T_TransRecordEntity transInDB = db.FindEntity<T_TransRecordEntity>(o => o.OrderID == inBoundEntity.F_Id);
                                                if (transInDB == null) //强制完成的时候也会产生过账信息，此处做个去重
                                                {
                                                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, inBoundEntity.F_Id, "PurchaseIn");
                                                    if ((ResultType)rst.state == ResultType.success)
                                                    {
                                                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                                        ERPPost post = new ERPPost();
                                                        ERPResult erpRst = post.PostFactInOutQty(db, "PurchaseIn", trans.F_Id);
                                                    }
                                                    else
                                                    {
                                                        result.IsSuccess = false;
                                                        result.FailCode = "0001";
                                                        result.FailMsg = rst.message;
                                                        return result;
                                                    }
                                                }
                                            }


                                        }
                                    }
                                }

                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, locationEntity, "InType", "PurchaseIn", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_GetItemOut":/// 领料出库
                        {
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            tagStation.BarCode = task.BarCode;
                            //没有绑定单据明细，是因为可能一个容器对应多个单据明细
                            tagStation.CurOrderDetailID = task.OrderDetailID;
                            db.Update<T_StationEntity>(tagStation);

                            T_LocationEntity tagLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == tagStation.StationCode); //货位与站台是特例： LocationCode = StationCode
                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = tagLoc.F_Id;
                            containerEntity.LocationNo = tagLoc.LocationCode;
                            containerEntity.AreaID = tagLoc.F_Id;
                            containerEntity.AreaCode = tagLoc.AreaCode;
                            containerEntity.AreaName = tagLoc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLoc.F_Id;
                                cdEntity.LocationNo = tagLoc.LocationCode;
                                cdEntity.AreaID = tagLoc.F_Id;
                                cdEntity.AreaCode = tagLoc.AreaCode;
                                cdEntity.AreaName = tagLoc.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            IList<T_OutRecordEntity> curRecordList = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == task.OrderDetailID && o.BarCode == task.BarCode && o.TagLocationCode == task.TagLocationCode && o.State == "WaitPick").ToList();
                            foreach (T_OutRecordEntity outcell in curRecordList)
                            {
                                outcell.State = "Picking";
                                outcell.IsArrive = "true";
                                db.Update<T_OutRecordEntity>(outcell);
                            }

                            IList<T_TaskDetailEntity> curTaskDetailList = db.FindList<T_TaskDetailEntity>(o => o.BarCode == task.BarCode && o.OrderDetailID == task.OrderDetailID && o.IsCurTask == "true");
                            foreach (T_TaskDetailEntity curTaskDetail in curTaskDetailList)
                            {
                                curTaskDetail.IsCurTask = "false";
                                curTaskDetail.IsOver = "true";
                                curTaskDetail.OverTime = DateTime.Now;
                                db.Update<T_TaskDetailEntity>(curTaskDetail);
                            }
                            //自动产生下一个任务

                            T_TaskEntity newTask = StartNextTask(db, task.BarCode, task.F_Id);
                            if (newTask != null)
                            {
                                //推送任务到WCS
                                List<string> newTaskNo = new List<string>();
                                newTaskNo.Add(newTask.TaskNo);
                                result = new WCSPost().SendTask(db, newTaskNo);
                                if (!result.IsSuccess)
                                {
                                    return result;
                                }
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "GetItemOut", "Out", "Empty", task.TaskNo);

                            //纸箱领料出库为整箱出库
                            if (task.TaskType == "TaskType_GetItemOut" && containerEntity.ContainerKind == "Box")
                            {
                                //应拣数量等于整个库存数量，且等于待出库数量，则整箱拣选
                                if (curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.Qty) && curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.OutQty))
                                {
                                    db.SaveChanges();

                                    int NoPickTimes = 0;    // 剩余次数
                                    decimal? OverPickQty = 0;
                                    decimal? AllNeedQty = 0;

                                    T_OutRecordApp outRecordApp = new T_OutRecordApp();
                                    foreach (T_OutRecordEntity rec in curRecordList) //自动拣选所有需拣选内容，纸箱，正常情况只有一条记录
                                    {
                                        AjaxResult tempRes = outRecordApp.PickRecord(db, tagStation.F_Id, rec.BarCode, rec.F_Id, rec.ItemBarCode, rec.NeedQty, ref NoPickTimes, ref OverPickQty, ref AllNeedQty);
                                        if (tempRes.state.ToString() != ResultType.success.ToString())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = tempRes.message;
                                            return result;
                                        }
                                    }

                                    //containerEntity.F_DeleteMark = true;
                                    //db.Update<T_ContainerEntity>(containerEntity);
                                }
                            }

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_VerBackIn":/// 验退回库 
                        {
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = loc.F_Id;
                            containerEntity.LocationNo = loc.LocationCode;
                            containerEntity.AreaID = loc.F_Id;
                            containerEntity.AreaCode = loc.AreaCode;
                            containerEntity.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                            foreach (T_ContainerDetailEntity detail in detailList)
                            {
                                detail.LocationID = loc.F_Id;
                                detail.LocationNo = loc.LocationCode;
                                detail.AreaID = loc.F_Id;
                                detail.AreaCode = loc.AreaCode;
                                detail.AreaName = loc.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "VerBackOut", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_WarehouseBackIn":///仓退回库 
                        {
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = loc.F_Id;
                            containerEntity.LocationNo = loc.LocationCode;
                            containerEntity.AreaID = loc.F_Id;
                            containerEntity.AreaCode = loc.AreaCode;
                            containerEntity.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                            foreach (T_ContainerDetailEntity detail in detailList)
                            {
                                detail.LocationID = loc.F_Id;
                                detail.LocationNo = loc.LocationCode;
                                detail.AreaID = loc.F_Id;
                                detail.AreaCode = loc.AreaCode;
                                detail.AreaName = loc.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "WarehouseBackOut", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_OtherOut":/// 其它出库
                        {
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            tagStation.BarCode = task.BarCode;
                            tagStation.CurOrderDetailID = task.OrderDetailID;
                            //没有绑定单据明细，是因为可能一个容器对应多个单据明细
                            db.Update<T_StationEntity>(tagStation);

                            T_LocationEntity tagLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == tagStation.StationCode); //货位与站台是特例： LocationCode = StationCode
                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = tagLoc.F_Id;
                            containerEntity.LocationNo = tagLoc.LocationCode;
                            containerEntity.AreaID = tagLoc.F_Id;
                            containerEntity.AreaCode = tagLoc.AreaCode;
                            containerEntity.AreaName = tagLoc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLoc.F_Id;
                                cdEntity.LocationNo = tagLoc.LocationCode;
                                cdEntity.AreaID = tagLoc.F_Id;
                                cdEntity.AreaCode = tagLoc.AreaCode;
                                cdEntity.AreaName = tagLoc.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            IList<T_OutRecordEntity> curRecordList = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == task.OrderDetailID && o.BarCode == task.BarCode && o.TagLocationCode == task.TagLocationCode && o.State == "WaitPick").ToList();
                            foreach (T_OutRecordEntity outcell in curRecordList)
                            {
                                outcell.IsArrive = "true";
                                outcell.State = "Picking";
                                db.Update<T_OutRecordEntity>(outcell);
                            }

                            IList<T_TaskDetailEntity> curTaskDetailList = db.FindList<T_TaskDetailEntity>(o => o.BarCode == task.BarCode && o.OrderDetailID == task.OrderDetailID && o.IsCurTask == "true");
                            foreach (T_TaskDetailEntity curTaskDetail in curTaskDetailList)
                            {
                                curTaskDetail.IsCurTask = "false";
                                curTaskDetail.IsOver = "true";
                                curTaskDetail.OverTime = DateTime.Now;
                                db.Update<T_TaskDetailEntity>(curTaskDetail);
                            }

                            db.SaveChanges();


                            //自动产生下一个任务

                            T_TaskEntity newTask = StartNextTask(db, task.BarCode, task.F_Id);
                            if (newTask != null)
                            {
                                //推送任务到WCS
                                List<string> newTaskNo = new List<string>();
                                newTaskNo.Add(newTask.TaskNo);
                                result = new WCSPost().SendTask(db, newTaskNo);
                                if (!result.IsSuccess)
                                {
                                    return result;
                                }
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "OtherOut", "Out", "Empty", task.TaskNo);

                            //纸箱其它出库为整箱出库
                            if (task.TaskType == "TaskType_OtherOut" && containerEntity.ContainerKind == "Box")
                            {
                                //应拣数量等于整个库存数量，且等于待出库数量，则整箱拣选
                                if (curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.Qty) && curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.OutQty))
                                {
                                    db.SaveChanges();

                                    int NoPickTimes = 0;    // 剩余次数
                                    decimal? OverPickQty = 0;
                                    decimal? AllNeedQty = 0;

                                    T_OutRecordApp outRecordApp = new T_OutRecordApp();
                                    foreach (T_OutRecordEntity rec in curRecordList) //自动拣选所有需拣选内容，纸箱，正常情况只有一条记录
                                    {
                                        AjaxResult tempRes = outRecordApp.PickRecord(db, tagStation.F_Id, rec.BarCode, rec.F_Id, rec.ItemBarCode, rec.NeedQty, ref NoPickTimes, ref OverPickQty, ref AllNeedQty);
                                        if (tempRes.state.ToString() != ResultType.success.ToString())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = tempRes.message;
                                            return result;
                                        }
                                    }
                                }
                            }

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_GetItemBack":/// 领料回库(针对领料出库之后的回库)
                        {
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = loc.F_Id;
                            containerEntity.LocationNo = loc.LocationCode;
                            containerEntity.AreaID = loc.AreaID;
                            containerEntity.AreaCode = loc.AreaCode;
                            containerEntity.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                            foreach (T_ContainerDetailEntity detail in detailList)
                            {
                                detail.LocationID = loc.F_Id;
                                detail.LocationNo = loc.LocationCode;
                                detail.AreaID = loc.AreaID;
                                detail.AreaCode = loc.AreaCode;
                                detail.AreaName = loc.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "GetItemOut", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_OtherIn":/// 其它回库(针对其它出库之后的回库)
                        {
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = loc.F_Id;
                            containerEntity.LocationNo = loc.LocationCode;
                            containerEntity.AreaID = loc.AreaID;
                            containerEntity.AreaCode = loc.AreaCode;
                            containerEntity.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> detailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == task.BarCode);
                            foreach (T_ContainerDetailEntity detail in detailList)
                            {
                                detail.LocationID = loc.F_Id;
                                detail.LocationNo = loc.LocationCode;
                                detail.AreaID = loc.AreaID;
                                detail.AreaCode = loc.AreaCode;
                                detail.AreaName = loc.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "OtherOut", "In", "Stored", task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_WarehouseBackOut":/// 仓退出库
                        {
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            tagStation.BarCode = task.BarCode;
                            //没有绑定单据明细，是因为可能一个容器对应多个单据明细
                            tagStation.CurOrderDetailID = task.OrderDetailID;
                            db.Update<T_StationEntity>(tagStation);

                            T_LocationEntity tagLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == tagStation.StationCode); //货位与站台是特例： LocationCode = StationCode
                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = tagLoc.F_Id;
                            containerEntity.LocationNo = tagLoc.LocationCode;
                            containerEntity.AreaID = tagLoc.AreaID;
                            containerEntity.AreaCode = tagLoc.AreaCode;
                            containerEntity.AreaName = tagLoc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLoc.F_Id;
                                cdEntity.LocationNo = tagLoc.LocationCode;
                                cdEntity.AreaID = tagLoc.AreaID;
                                cdEntity.AreaCode = tagLoc.AreaCode;
                                cdEntity.AreaName = tagLoc.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            IList<T_OutRecordEntity> curRecordList = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == task.OrderDetailID && o.BarCode == task.BarCode && o.TagLocationCode == task.TagLocationCode && o.State == "WaitPick").ToList();
                            foreach (T_OutRecordEntity outcell in curRecordList)
                            {
                                outcell.IsArrive = "true";
                                outcell.State = "Picking";
                                db.Update<T_OutRecordEntity>(outcell);
                            }

                            IList<T_TaskDetailEntity> curTaskDetailList = db.FindList<T_TaskDetailEntity>(o => o.BarCode == task.BarCode && o.OrderDetailID == task.OrderDetailID && o.IsCurTask == "true");
                            foreach (T_TaskDetailEntity curTaskDetail in curTaskDetailList)
                            {
                                curTaskDetail.IsCurTask = "false";
                                curTaskDetail.IsOver = "true";
                                curTaskDetail.OverTime = DateTime.Now;
                                db.Update<T_TaskDetailEntity>(curTaskDetail);
                            }

                            db.SaveChanges();
                            //自动产生下一个任务

                            T_TaskEntity newTask = StartNextTask(db, task.BarCode, task.F_Id);
                            if (newTask != null)
                            {
                                //推送任务到WCS
                                List<string> newTaskNo = new List<string>();
                                newTaskNo.Add(newTask.TaskNo);
                                result = new WCSPost().SendTask(db, newTaskNo);
                                if (!result.IsSuccess)
                                {
                                    return result;
                                }
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "WarehouseBackOut", "Out", "Empty", task.TaskNo);

                            //纸箱仓退出库为整箱出库
                            if (task.TaskType == "TaskType_WarehouseBackOut" && containerEntity.ContainerKind == "Box")
                            {
                                //应拣数量等于整个库存数量，且等于待出库数量，则整箱拣选
                                if (curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.Qty) && curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.OutQty))
                                {
                                    db.SaveChanges();

                                    int NoPickTimes = 0;    // 剩余次数
                                    decimal? OverPickQty = 0;
                                    decimal? AllNeedQty = 0;

                                    T_OutRecordApp outRecordApp = new T_OutRecordApp();
                                    foreach (T_OutRecordEntity rec in curRecordList) //自动拣选所有需拣选内容，纸箱，正常情况只有一条记录
                                    {
                                        AjaxResult tempRes = outRecordApp.PickRecord(db, tagStation.F_Id, rec.BarCode, rec.F_Id, rec.ItemBarCode, rec.NeedQty, ref NoPickTimes, ref OverPickQty, ref AllNeedQty);
                                        if (tempRes.state.ToString() != ResultType.success.ToString())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = tempRes.message;
                                            return result;
                                        }
                                    }
                                }
                            }

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_VerBackOut":/// 验退出库
                        {
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            tagStation.BarCode = task.BarCode;
                            tagStation.CurOrderDetailID = task.OrderDetailID;
                            db.Update<T_StationEntity>(tagStation);
                            T_LocationEntity tagLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == tagStation.StationCode); //货位与站台是特例： LocationCode = StationCode

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = tagLoc.F_Id;
                            containerEntity.LocationNo = tagLoc.LocationCode;
                            containerEntity.AreaID = tagLoc.AreaID;
                            containerEntity.AreaCode = tagLoc.AreaCode;
                            containerEntity.AreaName = tagLoc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLoc.F_Id;
                                cdEntity.LocationNo = tagLoc.LocationCode;
                                cdEntity.AreaID = tagLoc.AreaID;
                                cdEntity.AreaCode = tagLoc.AreaCode;
                                cdEntity.AreaName = tagLoc.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            IList<T_OutRecordEntity> curRecordList = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == task.OrderDetailID && o.BarCode == task.BarCode && o.TagLocationCode == task.TagLocationCode && o.State == "WaitPick").ToList();
                            foreach (T_OutRecordEntity outcell in curRecordList)
                            {
                                outcell.IsArrive = "true";
                                outcell.State = "Picking";
                                db.Update<T_OutRecordEntity>(outcell);
                            }

                            IList<T_TaskDetailEntity> curTaskDetailList = db.FindList<T_TaskDetailEntity>(o => o.BarCode == task.BarCode && o.OrderDetailID == task.OrderDetailID && o.IsCurTask == "true");
                            foreach (T_TaskDetailEntity curTaskDetail in curTaskDetailList)
                            {
                                curTaskDetail.IsCurTask = "false";
                                curTaskDetail.IsOver = "true";
                                curTaskDetail.OverTime = DateTime.Now;
                                db.Update<T_TaskDetailEntity>(curTaskDetail);
                            }

                            db.SaveChanges();
                            //自动产生下一个任务

                            T_TaskEntity newTask = StartNextTask(db, task.BarCode, task.F_Id);
                            if (newTask != null)
                            {
                                //推送任务到WCS
                                List<string> newTaskNo = new List<string>();
                                newTaskNo.Add(newTask.TaskNo);
                                result = new WCSPost().SendTask(db, newTaskNo);
                                if (!result.IsSuccess)
                                {
                                    return result;
                                }
                            }

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "VerBackOut", "Out", "Empty", task.TaskNo);

                            //纸箱验退出库为整箱出库
                            if (task.TaskType == "TaskType_VerBackOut" && containerEntity.ContainerKind == "Box")
                            {
                                //应拣数量等于整个库存数量，且等于待出库数量，则整箱拣选
                                if (curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.Qty) && curRecordList.Sum(o => o.NeedQty) == containerDetailList.Sum(o => o.OutQty))
                                {
                                    db.SaveChanges();

                                    int NoPickTimes = 0;    // 剩余次数
                                    decimal? OverPickQty = 0;
                                    decimal? AllNeedQty = 0;

                                    T_OutRecordApp outRecordApp = new T_OutRecordApp();
                                    foreach (T_OutRecordEntity rec in curRecordList) //自动拣选所有需拣选内容，纸箱，正常情况只有一条记录
                                    {
                                        AjaxResult tempRes = outRecordApp.PickRecord(db, tagStation.F_Id, rec.BarCode, rec.F_Id, rec.ItemBarCode, rec.NeedQty, ref NoPickTimes, ref OverPickQty, ref AllNeedQty);
                                        if (tempRes.state.ToString() != ResultType.success.ToString())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = tempRes.message;
                                            return result;
                                        }
                                    }
                                }
                            }

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_Move":/// 移库-出库和入库
                        {

                            if (!string.IsNullOrEmpty(task.OrderID)) //不为空则表示是单据产生的移库，否则为空表示系统内部呼叫空容器产生（仅空料架缓存位）
                            {
                                T_MoveEntity move = db.FindEntity<T_MoveEntity>(o => o.F_Id == task.OrderID);
                                T_MoveRecordEntity rec = db.FindEntity<T_MoveRecordEntity>(o => o.MoveID == move.F_Id && o.F_Id == task.OrderDetailID); //此处不可用BarCode做条件，因为可能一个移库单里面，一个容器多次移动
                                rec.State = "Over";
                                db.Update<T_MoveRecordEntity>(rec);
                                db.SaveChanges();

                                //结束一组移库后，自动产生下一组移库任务
                                if (move.IsAuto == "true")
                                {
                                    if (move.State != "Overing") //不处于结束中
                                    {
                                        int MaxTaskNum = RuleConfig.MoveRule.MoveTaskRule.MaxTaskNum ?? Int32.MaxValue;
                                        IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == move.F_Id && o.F_Id != task.F_Id && o.TaskType == "TaskType_Move");

                                        if (taskList.Count == 0)//只剩1半任务，重新产生新任务
                                        {
                                            IList<string> taskNoList = new List<string>();
                                            AjaxResult ajaxRes = new T_MoveApp().GenMoveRecord(db, task.OrderID, MaxTaskNum, ref taskNoList);

                                            if ((ResultType)ajaxRes.state == ResultType.error)
                                            {
                                                result.IsSuccess = false;
                                                result.FailCode = "0001";
                                                return result;
                                            }

                                            if (taskNoList.Count == 0) //表示已经处于最佳状态，无需移库
                                            {
                                                move.State = "Overing";
                                                db.Update(move);
                                                db.SaveChanges();
                                            }

                                            //执行并发送任务
                                            WCSResult rst = new WCSPost().SendTaskBulk(db, taskNoList);
                                            if (!rst.IsSuccess)
                                            {
                                                //db.RollBack();
                                                result.IsSuccess = false;
                                                result.FailCode = "0001";
                                                return result;
                                            }
                                        }
                                    }
                                }

                                IList<T_MoveRecordEntity> notOverList = db.FindList<T_MoveRecordEntity>(o => o.MoveID == move.F_Id && o.State != "Over");
                                if (notOverList.Count() < 1) //当前单据的移库已全部完成
                                {
                                    move.State = "Over";
                                    db.Update(move);
                                    db.SaveChanges();
                                }
                            }

                            //更新货位
                            T_LocationEntity locationSrc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                            locationSrc.State = "Empty";
                            db.Update<T_LocationEntity>(locationSrc);
                            locState.SyncLocState(db, locationSrc, "MoveType", "MoveType", "Out", "Empty", task.TaskNo);


                            T_LocationEntity tagLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.TagLocationCode);
                            tagLoc.State = "Stored";
                            db.Update<T_LocationEntity>(tagLoc);
                            locState.SyncLocState(db, tagLoc, "MoveType", "MoveType", "In", "Stored", task.TaskNo);


                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = tagLoc.F_Id;
                            containerEntity.LocationNo = tagLoc.LocationCode;
                            containerEntity.AreaID = tagLoc.AreaID;
                            containerEntity.AreaCode = tagLoc.AreaCode;
                            containerEntity.AreaName = tagLoc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID);
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLoc.F_Id;
                                cdEntity.LocationNo = tagLoc.LocationCode;
                                cdEntity.AreaID = tagLoc.AreaID;
                                cdEntity.AreaCode = tagLoc.AreaCode;
                                cdEntity.AreaName = tagLoc.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);
                            }

                            db.SaveChanges();

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_LocCount": /// 货位盘点
                        {
                            /// 不回传这个接口，直接反馈盘点AGV数据反馈 CountData

                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = $"货位盘点调用[反馈盘点数据接口：CountData]：{ task.TaskNo }";
                            return result;
                        }
                    case "TaskType_EmptyIn":/// 空托盘入库
                        {
                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                            loc.State = "Stored";
                            db.Update<T_LocationEntity>(loc);

                            /// 货位状态变更记录
                            locState.SyncLocState(db, loc, "InType", "EmptyIn", "In", "Stored", task.TaskNo);

                            T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            containerEntity.LocationID = loc.F_Id;
                            containerEntity.LocationNo = loc.LocationCode;
                            containerEntity.AreaID = loc.AreaID;
                            containerEntity.AreaCode = loc.AreaCode;
                            containerEntity.AreaName = loc.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            T_ItemEntity item = null;
                            T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == task.ContainerType);
                            if (containerType.ContainerKind == "Plastic")
                            {
                                item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                            }
                            else if (containerType.ContainerKind == "Rack")
                            {
                                item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "即不是空料箱也不是空料架";
                                return result;
                            }

                            if (item == null)
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未定义空容器物料";
                                return result;
                            }

                            T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == task.TagAreaID);

                            T_ContainerDetailEntity detail = new T_ContainerDetailEntity();
                            detail.F_Id = Guid.NewGuid().ToString();
                            detail.ItemID = item.F_Id;
                            detail.ContainerID = task.ContainerID;
                            detail.ContainerType = task.ContainerType;
                            detail.ContainerKind = containerType.ContainerKind;
                            detail.LocationID = task.TagLocationID;
                            detail.LocationNo = task.TagLocationCode;
                            detail.AreaID = task.TagAreaID;
                            detail.AreaCode = area.AreaCode;
                            detail.AreaName = area.AreaName;
                            detail.KindCode = item.KindCode;
                            detail.KindName = item.KindName;
                            detail.ItemName = item.ItemName;
                            detail.ItemCode = item.ItemCode;
                            detail.BarCode = task.BarCode;
                            detail.ItemBarCode = "";
                            detail.Qty = 1;
                            detail.OutQty = 0;
                            detail.CheckQty = 0;
                            detail.ItemUnitText = item.ItemUnitText;
                            detail.CheckState = "UnNeed";
                            detail.CheckDetailID = "";
                            detail.CheckID = "";
                            detail.State = "Normal";
                            detail.IsCheckFreeze = "false";
                            detail.IsCountFreeze = "false";
                            detail.Lot = "";
                            detail.Spec = "";
                            detail.ERPWarehouseCode = "";
                            detail.ProductDate = null;
                            detail.OverdueDate = null;
                            detail.SupplierID = "";
                            detail.SupplierCode = "";
                            detail.SupplierName = "";
                            detail.ReceiveRecordID = "";
                            //detail.IsSpecial = "false";
                            detail.IsItemMark = "";
                            detail.F_DeleteMark = false;

                            db.Insert<T_ContainerDetailEntity>(detail);

                            /// 库存流水
                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                            inOutDetailApp.SyncInOutDetail(db, detail, "InType", "EmptyIn", 0, detail.Qty, task.TaskNo);

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_EmptyOut":/// 空托盘出库
                        {
                            T_AreaEntity areaEmpty = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.EmptyArea.ToString());
                            if (task.TagAreaID == areaEmpty.F_Id) //目标为空料箱存储区，需更新目标货位状态(该逻辑不会进去，因为空料箱由存储位去空料箱区域的任务为移库任务，且空料箱抽屉出库接口清空库存)
                            {
                                T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.TagLocationID);
                                loc.State = "Stored";
                                db.Update<T_LocationEntity>(loc);

                                /// 货位状态变更记录
                                locState.SyncLocState(db, loc, "OutType", "EmptyOut", "In", "Stored", task.TaskNo);

                                T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                                containerEntity.LocationID = loc.F_Id;
                                containerEntity.LocationNo = loc.LocationCode;
                                containerEntity.AreaID = loc.AreaID;
                                containerEntity.AreaCode = loc.AreaCode;
                                containerEntity.AreaName = loc.AreaName;
                                containerEntity.F_DeleteMark = false;
                                db.Update<T_ContainerEntity>(containerEntity);

                                T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                                location.State = "Empty";
                                db.Update<T_LocationEntity>(location);

                                /// 货位状态变更记录
                                locState.SyncLocState(db, location, "InType", "EmptyOut", "Out", "Empty", task.TaskNo);

                                T_ContainerDetailEntity containerDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID); //空托盘只有1个明细
                                containerDetail.LocationID = loc.F_Id;
                                containerDetail.LocationNo = loc.LocationCode;
                                containerDetail.AreaID = areaEmpty.F_Id;
                                containerDetail.AreaCode = areaEmpty.AreaCode;
                                containerDetail.AreaName = areaEmpty.AreaName;
                                db.Update<T_ContainerDetailEntity>(containerDetail);
                            }
                            else  //大件站台
                            {
                                T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                                containerEntity.LocationID = "";
                                containerEntity.LocationNo = "";
                                containerEntity.AreaID = ""; //大件空容器出库后，无站台，因此AreaID为NULL
                                containerEntity.AreaCode = "";
                                containerEntity.AreaName = "";
                                containerEntity.F_DeleteMark = true;
                                db.Update<T_ContainerEntity>(containerEntity);

                                T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == task.SrcLocationCode);
                                location.State = "Empty";
                                db.Update<T_LocationEntity>(location);

                                /// 货位状态变更记录
                                locState.SyncLocState(db, location, "OutType", "EmptyOut", "Out", "Empty", task.TaskNo);

                                T_ContainerDetailEntity containerDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.ContainerID == task.ContainerID); //空托盘只有1个明细
                                /// 库存流水
                                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                inOutDetailApp.SyncInOutDetail(db, containerDetail, "OutType", "EmptyOut", containerDetail.Qty, containerDetail.Qty, task.TaskNo);
                                db.Delete<T_ContainerDetailEntity>(containerDetail);
                            }

                            db.SaveChanges();

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    case "TaskType_OffRack": /// 下架出库
                        {
                            /// 更新货位状态
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.F_Id == task.SrcLocationID);
                            location.State = "Empty";
                            db.Update<T_LocationEntity>(location);

                            /// 货位状态变更记录
                            locState.SyncLocState(db, location, "OutType", "OffRack", "Out", "Empty", task.TaskNo);

                            /// 站台绑定到达容器
                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == task.TagLocationID);
                            station.BarCode = task.BarCode;
                            station.CurOrderDetailID = task.OrderDetailID;
                            //station.CurOrderID = task.OrderID;
                            //station.OrderType = "OffRack";
                            db.Update<T_StationEntity>(station);

                            /// 更新容器货位
                            T_LocationEntity tagLocation = db.FindEntity<T_LocationEntity>(o => o.LocationCode == station.StationCode); /// 货位=站台
                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == task.ContainerID);
                            container.LocationID = tagLocation.F_Id;
                            container.LocationNo = tagLocation.LocationCode;
                            container.AreaID = tagLocation.AreaID;
                            container.AreaCode = tagLocation.AreaCode;
                            container.AreaName = tagLocation.AreaName;
                            //container.F_DeleteMark = true; //改为在PDA上确认后删除
                            db.Update<T_ContainerEntity>(container);

                            /// 删除库存明细货位
                            List<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == container.F_Id);

                            //T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                            foreach (T_ContainerDetailEntity cdEntity in containerDetailList)
                            {
                                cdEntity.LocationID = tagLocation.F_Id;
                                cdEntity.LocationNo = tagLocation.LocationCode;
                                cdEntity.AreaID = tagLocation.AreaID;
                                cdEntity.AreaCode = tagLocation.AreaCode;
                                cdEntity.AreaName = tagLocation.AreaName;
                                db.Update<T_ContainerDetailEntity>(cdEntity);

                                //删除库存改为在RF确认后删除
                                //inOutDetailApp.SyncInOutDetail(db, cdEntity, "OutType", "EmptyOut", cdEntity.Qty, cdEntity.Qty, task.TaskNo);
                                //db.Delete<T_ContainerDetailEntity>(cdEntity);
                            }

                            /// 更新下架记录状态
                            List<T_OffRackRecordEntity> curRecordList = db.FindList<T_OffRackRecordEntity>(o => o.OffRackID == task.OrderID && o.BarCode == task.BarCode && o.StationID == task.TagLocationID && o.State == "OffRacking").ToList();
                            foreach (T_OffRackRecordEntity record in curRecordList)
                            {
                                //record.State = "OffRacking";
                                record.IsArrive = "true";
                                db.Update<T_OffRackRecordEntity>(record);
                            }

                            /// 更新下架明细状态
                            //T_OffRackDetailEntity detail = db.FindEntity<T_OffRackDetailEntity>(o => o.F_Id == task.OrderDetailID);
                            //detail.State = "OffRacking";
                            //db.Update<T_OffRackDetailEntity>(detail);
                            //db.SaveChanges();

                            /// 更新下架单状态
                            //IList<T_OffRackDetailEntity> detailList = db.FindList<T_OffRackDetailEntity>(o => o.OffRackID == task.OrderID && o.State == "OffRacking");
                            //if (detailList.Count == 0)
                            //{
                            //    T_OffRackEntity offRack = db.FindEntity<T_OffRackEntity>(o => o.F_Id == task.OrderID);
                            //    offRack.State = "OffRacking";
                            //    db.Update<T_OffRackEntity>(offRack);
                            //}
                            db.SaveChanges();

                            result.IsSuccess = true;
                            result.FailCode = "0000";
                            return result;
                        }
                    default:
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "未知的任务类型";
                            return result;
                        }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region 完成任务后启用下一个任务明细
        public T_TaskEntity StartNextTask(IRepositoryBase db, string barCode, string curTaskID)
        {
            T_TaskEntity newTask = null;
            T_TaskDetailEntity nextDetailTask = db.FindEntity<T_TaskDetailEntity>(o => o.BarCode == barCode && o.TaskID != curTaskID && o.IsOver == "false");

            if (nextDetailTask != null)
            {
                newTask = new T_TaskEntity();
                newTask.F_Id = Guid.NewGuid().ToString();
                newTask.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                newTask.TaskInOutType = "OutType";

                string orderType = nextDetailTask.OrderType;
                switch (orderType)
                {
                    case "GetItemOut":
                        {
                            newTask.TaskType = "TaskType_GetItemOut";
                            newTask.OrderType = "GetItemOut";
                            newTask.Level = 20;
                        }
                        break;
                    case "VerBackOut":
                        {
                            newTask.TaskType = "TaskType_VerBackOut";
                            newTask.OrderType = "VerBackOut";
                            newTask.Level = 20;
                        }
                        break;
                    case "WarehouseBackOut":
                        {
                            newTask.TaskType = "TaskType_WarehouseBackOut";
                            newTask.OrderType = "WarehouseBackOut";
                            newTask.Level = 20;
                        }
                        break;
                    case "GetSample":
                        {
                            newTask.TaskType = "TaskType_CheckPickOut";
                            newTask.OrderType = "GetSample";
                            newTask.Level = 20;
                        }
                        break;
                    case "OtherOut":
                        {
                            newTask.TaskType = "TaskType_OtherOut";
                            newTask.OrderType = "OtherOut";
                            newTask.Level = 20;
                        }
                        break;
                    default:
                        {
                            throw new Exception("未知的任务明细单据类型");
                        }
                }


                newTask.ContainerID = nextDetailTask.ContainerID;
                newTask.BarCode = nextDetailTask.BarCode;
                newTask.ContainerType = nextDetailTask.ContainerType;
                newTask.SrcLocationID = null;   // nextDetailTask.SrcLocationID;
                newTask.SrcLocationCode = null; // nextDetailTask.SrcLocationCode;
                newTask.TagAreaID = "";
                newTask.TagLocationID = nextDetailTask.TagLocationID;
                newTask.TagLocationCode = nextDetailTask.TagLocationCode;
                T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == nextDetailTask.TagLocationID);
                newTask.TagWCSLocCode = loc.WCSLocCode;
                newTask.ApplyStationID = null;

                newTask.State = "New";
                newTask.IsWcsTask = "true";
                newTask.ExecEquID = null;
                newTask.IsCanExec = "true";
                newTask.SendWCSTime = null;
                newTask.WaveCode = nextDetailTask.WaveCode;
                newTask.WaveID = nextDetailTask.WaveID;
                newTask.SEQ = nextDetailTask.SEQ;
                newTask.WaveDetailID = nextDetailTask.WaveDetailID;

                newTask.OrderID = nextDetailTask.OrderID;
                newTask.OrderDetailID = nextDetailTask.OrderDetailID;
                newTask.OrderCode = nextDetailTask.OrderCode;
                newTask.OverTime = null;

                db.Insert<T_TaskEntity>(newTask);


                nextDetailTask.IsCurTask = "true";
                nextDetailTask.TaskID = newTask.F_Id;
                nextDetailTask.TaskNo = newTask.TaskNo;
                db.Update<T_TaskDetailEntity>(nextDetailTask);


                IList<T_OutRecordEntity> outReceordTempList = db.FindList<T_OutRecordEntity>(o => o.OutBoundDetailID == newTask.OrderDetailID && o.WaveCode == nextDetailTask.WaveCode && o.BarCode == newTask.BarCode).ToList();//同单据不同明细，公用一个容器的情况
                foreach (T_OutRecordEntity outcell in outReceordTempList)
                {
                    outcell.TaskID = newTask.F_Id;
                    outcell.TaskNo = newTask.TaskNo;
                    db.Update<T_OutRecordEntity>(outcell);
                }
            }
            db.SaveChanges();

            return newTask;
        }
        #endregion

        #region 取消WCS任务
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="taskNo"></param>
        /// <returns></returns>
        public WCSResult TaskReturn_Hand(string taskNo)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    WCSPost wcsPost = new WCSPost();
                    WCSResult result = wcsPost.TaskCancel(db, taskNo);
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region 暂停AGV
        /// <summary>
        /// 暂停AGV
        /// </summary>
        /// <param name="agvCode">Agv编码（为空则暂停所有AGV）</param>
        /// <returns></returns>
        public WCSResult PauseDev(string agvCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    WCSPost wcsPost = new WCSPost();
                    WCSResult result = wcsPost.PauseDev(db, agvCode);
                    db.CommitWithOutRollBack();
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

        #region 继续AGV
        /// <summary>
        /// 继续AGV
        /// </summary>
        /// <param name="agvCode">Agv编码（为空则继续所有AGV）</param>
        /// <returns></returns>
        public WCSResult ContinueDev(string agvCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    WCSPost wcsPost = new WCSPost();
                    WCSResult result = wcsPost.ContinueDev(db, agvCode);
                    db.CommitWithOutRollBack();
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion

    }
}
