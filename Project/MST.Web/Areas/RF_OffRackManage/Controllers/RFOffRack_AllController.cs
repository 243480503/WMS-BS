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

namespace MST.Web.Areas.RF_OffRackManage.Controllers
{
    public class RFOffRack_AllController : ControllerBase
    {
        private T_OffRackApp offRackApp = new T_OffRackApp();
        private T_OffRackDetailApp offRackDetailApp = new T_OffRackDetailApp();
        private T_OffRackRecordApp offRackRecordApp = new T_OffRackRecordApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
        private T_TaskApp taskApp = new T_TaskApp();

        #region 扫描容器条码，获取下架信息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetOffRackJson(string barCode)
        {
            if (string.IsNullOrEmpty(barCode)) return Error("箱码不能为空", "");
            T_ContainerEntity container = containerApp.FindEntity(o => o.BarCode == barCode);
            if (container == null) return Error("箱码不存在", "");

            T_StationEntity station = null;
            if (container.ContainerKind == "Box" || container.ContainerKind == "Plastic") //纸箱或料箱
            {
                station = stationApp.FindEntity(o => o.BarCode == barCode && o.StationCode == FixType.Station.StationOut_Normal.ToString());
            }
            else if (container.ContainerKind == "Rack") //料架
            {
                station = stationApp.FindEntity(o => o.BarCode == barCode && o.StationCode == FixType.Station.StationOut_BigItem.ToString());
            }
            else return Error("未知的容器大类", "");

            if (station == null) return Error("容器未到达站台", "");
            if (string.IsNullOrEmpty(station.CurOrderID)) return Error("该站台没有作业单据", "");


            T_OffRackEntity offRack = offRackApp.FindEntity(o => o.F_Id == station.CurOrderID);
            List<T_OffRackDetailEntity> needOffRackList = offRackDetailApp.FindList(o => o.OffRackID == station.CurOrderID && o.State != "Over").ToList();
            List<T_OffRackDetailEntity> offRackingList = needOffRackList.Where(o => o.State == "OffRacking").ToList();

            T_OffRackDetailEntity offRackDetail = needOffRackList.FirstOrDefault(o => o.BarCode == barCode);
            if (offRackDetail == null) return Error("未找到容器下架信息", "");
            if (offRackDetail.State == "Over") return Error("容器下架已处理", "");

            T_OffRackEntity offRackEntity = offRackApp.FindEntity(o => o.F_Id == offRackDetail.OffRackID);
            OffRackDetailModel detailModel = offRackDetail.ToObject<OffRackDetailModel>();
            detailModel.F_Id = offRackDetail.F_Id;
            detailModel.StationID = station.F_Id;
            detailModel.OffRackID = station.CurOrderID;
            detailModel.RefOrderCode = offRackEntity.RefOrderCode;
            detailModel.ContainerKind = container.ContainerKind;
            detailModel.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == container.ContainerKind).F_ItemName;
            detailModel.MustTimes = needOffRackList.Count;
            detailModel.NoOffRackTimes = offRackingList.Count;
            detailModel.ReadyConfirmAndOrderNeed = (needOffRackList.Count - offRackingList.Count).ToString("0.##") + "/" + needOffRackList.Count.ToString("0.##");
            return Content(detailModel.ToJson());
        }
        #endregion

        #region 切换选项卡后，根据箱码获取单据，实现单据所有下架容器列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetOffRackDetailList(string barCode)
        {
            List<T_OffRackDetailEntity> dataList = new List<T_OffRackDetailEntity>();

            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_OffRackDetailEntity>(o => o.State).ToList();

            if (string.IsNullOrEmpty(barCode)) return Error("容器不能为空", "");
            else
            {
                T_StationEntity station = stationApp.FindEntity(o => o.BarCode == barCode);

                T_OffRackDetailEntity offRackDetail = offRackDetailApp.FindEntity(o => o.BarCode == barCode && o.State == "OffRacking");

                if (offRackDetail == null) return Error("未找到容器下架信息", "");
                if (station == null) return Error("容器未到站", "");

                if (offRackDetail.OffRackID != station.CurOrderID) return Error("未找到容器下架信息", "");

                dataList = offRackDetailApp.FindList(o => o.OffRackID == offRackDetail.OffRackID && o.State != "Over").OrderBy(o => o.F_LastModifyTime).ToList();

                List<OffRackDetailModel> offRackDetailList = new List<OffRackDetailModel>();
                foreach (var item in dataList)
                {
                    OffRackDetailModel model = item.ToObject<OffRackDetailModel>();
                    model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                    offRackDetailList.Add(model);
                }
                return Content(offRackDetailList.ToJson());
            }
        }
        #endregion


        #region RF提交下架信息
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitOffRackRecordForm(OffRackRecordModel postOffRackRecord)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFOffRack_AllController.SubmitOffRackRecordForm";
                logObj.Parms = new { postOffRackRecord = postOffRackRecord };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "下架单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF下架";

                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (string.IsNullOrEmpty(postOffRackRecord.BarCode)) return Error("箱码不能为空", "");

                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == postOffRackRecord.StationID);
                    string CurOrderID = station.CurOrderID;

                    IList<T_OffRackRecordEntity> offRackRecList = db.FindList<T_OffRackRecordEntity>(o =>o.OffRackDetailID == station.CurOrderDetailID && o.BarCode == postOffRackRecord.BarCode);
                    T_OffRackDetailEntity offRackDetailEntity = db.FindEntity<T_OffRackDetailEntity>(o => o.F_Id == station.CurOrderDetailID);
                    T_OffRackEntity offRackEntity = db.FindEntity<T_OffRackEntity>(o => o.F_Id == station.CurOrderID);

                    IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == offRackDetailEntity.BarCode);
                    if (containerDetailList.Count < 1)
                    {
                        return Error("未找到对应库存信息", "");
                    }

                    T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                    foreach (T_OffRackRecordEntity offRackRec in offRackRecList) //下架该容器所有库存
                    {
                        T_ContainerDetailEntity cdEntity = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == offRackRec.ContainerDetailID);
                        inOutDetailApp.SyncInOutDetail(db, cdEntity, "OutType", "OffRack", cdEntity.Qty, cdEntity.Qty, offRackRec.TaskNo);
                        db.Delete<T_ContainerDetailEntity>(cdEntity);

                        offRackRec.State = "Over";
                        db.Update<T_OffRackRecordEntity>(offRackRec);
                        db.SaveChanges();
                    }

                    //删除托盘
                    T_ContainerEntity con = db.FindEntity<T_ContainerEntity>(o=>o.BarCode == station.BarCode && o.F_DeleteMark == false);
                    con.F_DeleteMark = true;
                    db.Update<T_ContainerEntity>(con);
                    db.SaveChanges();

                    IList<T_OffRackRecordEntity> noOffRackRecList = db.FindList<T_OffRackRecordEntity>(o => o.OffRackDetailID == station.CurOrderDetailID && o.BarCode != postOffRackRecord.BarCode && o.State != "Over");
                    if(noOffRackRecList.Count==0) //明细已完成
                    {
                        offRackDetailEntity.State = "Over";
                        db.Update<T_OffRackDetailEntity>(offRackDetailEntity);
                        db.SaveChanges();
                    }


                    /// 更新下架单状态
                    IList<T_OffRackDetailEntity> noDetailList = db.FindList<T_OffRackDetailEntity>(o => o.OffRackID == station.CurOrderID && o.State != "Over");
                    if (noDetailList.Count == 0)
                    {
                        offRackEntity.State = "Over";
                        db.Update<T_OffRackEntity>(offRackEntity);

                        //清空站台

                        station.BarCode = "";
                        station.CurOrderDetailID = "";
                        station.CurOrderID = "";
                        station.OrderType = "";
                        station.WaveID = "";
                        db.Update<T_StationEntity>(station);
                    }
                    else
                    {
                        station.BarCode = "";
                        station.CurOrderDetailID = "";
                        db.Update<T_StationEntity>(station);
                    }



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
    }
}
