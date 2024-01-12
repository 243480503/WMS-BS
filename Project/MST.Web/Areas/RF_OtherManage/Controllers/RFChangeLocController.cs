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
using static MST.Application.WMSLogic.T_TaskApp;

namespace MST.Web.Areas.RF_OtherManage.Controllers
{
    [HandlerLogin]
    public class RFChangeLocController : ControllerBase
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
        public ActionResult GetLocInfo(string locCode, string barCode, string valueType)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                ItemsDetailApp dicDetail = new ItemsDetailApp();


                LocationModel locModel = null;
                if (valueType == "LocCode")
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
                }
                else if (valueType == "ConBarCode")
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


                IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.LocationNo == locModel.LocationCode);
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

        #region 清理调整
        /// <summary>
        /// 清理调整
        /// </summary>
        /// <param name="locCode"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult ChangeLoc(string locCode, string newLocCode)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ChangeLocController.ChangeLoc";
            logObj.Parms = new { locCode = locCode, newLocCode = newLocCode };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "RF替换货位";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "RF替换货位";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();
            try
            {

                using (var db = new RepositoryBase().BeginTrans())
                {
                    if (string.IsNullOrEmpty(locCode))
                    {
                        return Error("请扫原货位编码", "");
                    }

                    if (string.IsNullOrEmpty(newLocCode))
                    {
                        return Error("请扫新货位编码", "");
                    }

                    if (locCode == newLocCode)
                    {
                        return Error("原货位与新货位不能相同", "");
                    }

                    T_LocationEntity newLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == newLocCode);
                    if (newLoc == null)
                    {
                        return Error("新货位不存在", "");
                    }

                    if (newLoc.State != "Empty")
                    {
                        return Error("新货位状态不为空", "");
                    }


                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locCode); //原货位
                    if (loc == null)
                    {
                        return Error("原货位不存在", "");
                    }

                    if (loc.State != "Stored")
                    {
                        return Error("原货位状态不为已存储", "");
                    }
                    T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.LocationNo == locCode && o.F_DeleteMark == false);
                    if (container == null)
                    {
                        return Error("原货位容器不存在", "");
                    }
                    T_ContainerTypeEntity conType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == container.ContainerType);
                    if (conType == null)
                    {
                        return Error("原容器没有容器种类", "");
                    }

                    IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.LocationNo == locCode);
                    if (containerDetailList.Count < 1)
                    {
                        return Error("原容器没有库存", "");
                    }

                    T_ContainerDetailEntity firstDetail = containerDetailList[0];
                    T_LocationApp locApp = new T_LocationApp();
                    string msg = null;
                    locApp.CheckLocIn(ref msg, db, conType, firstDetail.AreaID, false, firstDetail.ERPWarehouseCode, firstDetail.ItemID, firstDetail.CheckState, newLoc.LocationCode, false);

                    if (!string.IsNullOrEmpty(msg))
                    {
                        return Error(msg, "");
                    }

                    loc.State = "Empty";
                    db.Update<T_LocationEntity>(loc);

                    newLoc.State = "Stored";
                    db.Update<T_LocationEntity>(newLoc);

                    container.LocationID = newLoc.F_Id;
                    container.LocationNo = newLoc.LocationCode;
                    db.Update<T_ContainerEntity>(container);

                    foreach (T_ContainerDetailEntity cell in containerDetailList)
                    {
                        cell.LocationID = newLoc.F_Id;
                        cell.LocationNo = newLoc.LocationCode;
                        db.Update<T_ContainerDetailEntity>(cell);
                    }
                    db.SaveChanges();

                    db.CommitWithOutRollBack();
                    return Success("操作成功", "");
                }
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

