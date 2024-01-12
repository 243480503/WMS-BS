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
using MST.Web.WebAPI.RFOffLine.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static MST.Application.WMSLogic.T_TaskApp;
using static MST.Web.WebAPI.RFOffLine.Controllers.RFOffLineController;

namespace MST.Web.Areas.RF_OtherManage.Controllers
{
    [HandlerLogin]
    public class RFClearConDetailController : ControllerBase
    {
        #region 获取扫码货位或容器的详细信息
        /// <summary>
        /// 获取扫码货位或容器的详细信息
        /// </summary>
        /// <param name="locCode"></param>
        /// <param name="barCode"></param>
        /// <param name="valueType">货位为主：LocCode，容器条码为主：ConBarCode</param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLocInfo(string locCode, string barCode, string valueType,string ItemBarCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                ItemsDetailApp dicDetail = new ItemsDetailApp();


                LocationModel locModel = null;
                IList<T_ContainerDetailEntity> containerDetailList = null;
                if (valueType == "ConDetailLocationCode") //扫容器码
                {
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locCode);
                    if (loc != null)
                    {
                        locModel = loc.ToObject<LocationModel>();
                        locModel.StateName = dicDetail.FindEnum<T_LocationEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == locModel.State).F_ItemName;
                        locModel.ForbiddenStateName = dicDetail.FindEnum<T_LocationEntity>(o => o.ForbiddenState).FirstOrDefault(o => o.F_ItemCode == locModel.ForbiddenState).F_ItemName;
                    }
                    else
                    {
                        return Error("货位编码不存在", "");
                    }

                    containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.LocationNo == locModel.LocationCode);
                }
                else if (valueType == "BarCode") //扫条码
                {
                    IList<T_ContainerDetailEntity> containerDetailTempList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == barCode);
                    if (containerDetailTempList.Count < 1)
                    {
                        return Error("容器条码不存在", "");
                    }
                    T_ContainerDetailEntity containerDetail = containerDetailTempList.FirstOrDefault();
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == containerDetail.LocationNo);
                    if (loc != null)
                    {
                        locModel = loc.ToObject<LocationModel>();
                        locModel.StateName = dicDetail.FindEnum<T_LocationEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == locModel.State).F_ItemName;
                        locModel.ForbiddenStateName = dicDetail.FindEnum<T_LocationEntity>(o => o.ForbiddenState).FirstOrDefault(o => o.F_ItemCode == locModel.ForbiddenState).F_ItemName;
                    }

                    containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.LocationNo == locModel.LocationCode);
                }
                else if(valueType == "ItemBarCode") //扫子条码
                {
                    containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemBarCode == ItemBarCode);
                    if (containerDetailList.Count < 1)
                    {
                        return Error("子条码不存在", "");
                    }

                    if (containerDetailList.Count > 1)
                    {
                        return Error("子条码不唯一", "");
                    }

                    T_ContainerDetailEntity containerDetail = containerDetailList.FirstOrDefault();
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == containerDetail.LocationNo);
                    if (loc != null)
                    {
                        locModel = loc.ToObject<LocationModel>();
                        locModel.StateName = dicDetail.FindEnum<T_LocationEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == locModel.State).F_ItemName;
                        locModel.ForbiddenStateName = dicDetail.FindEnum<T_LocationEntity>(o => o.ForbiddenState).FirstOrDefault(o => o.F_ItemCode == locModel.ForbiddenState).F_ItemName;
                    }

                }
                else
                {
                    return Error("参数有误", "");
                }

                ContainerModel containerModel = null;
                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.LocationNo == locModel.LocationCode && o.F_DeleteMark == false);
                if (container != null)
                {
                    containerModel = container.ToObject<ContainerModel>();
                    T_ContainerTypeEntity conType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerKind == containerModel.ContainerKind);
                    containerModel.ContainerKindName = conType.ContainerTypeName;
                }
                
                var data = new
                {
                    Loc = locModel,
                    Container = containerModel,
                    ContainerDetailList = containerDetailList
                };
                return Content(data.ToJson());
            }

        }
        #endregion

        #region 扣减库存，容器，库存
        /// <summary>
        /// 获取扫码货位或容器的详细信息
        /// </summary>
        /// <param name="locCode"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult ClearConDetail(T_OffLineDownEntity offlineEntity)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "RFClearConDetailController.ClearConDetail";
            logObj.Parms = new { offlineEntity = offlineEntity };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "RF在线出库";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新增/修改RF在线";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {

                IList<RFUploadDownModel> rfUploadDownModelList = new List<RFUploadDownModel>();
                RFUploadDownModel model = new RFUploadDownModel();
                model.BarCode = offlineEntity.BarCode;
                model.ItemBarCode = offlineEntity.ItemBarCode;
                model.OutQty = offlineEntity.Qty;
                rfUploadDownModelList.Add(model);

                RFOffLineResult res = new RFOffLineController().RFUploadDown(rfUploadDownModelList, OperatorProvider.Provider.GetCurrent(), "RF",true);
                if (!res.IsSuccess)
                {
                    return Error(res.FailMsg, "");
                }

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
        #endregion
    }
}
