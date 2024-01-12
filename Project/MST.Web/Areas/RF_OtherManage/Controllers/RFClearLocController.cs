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
    public class RFClearLocController : ControllerBase
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
        public ActionResult GetLocInfo(string locCode, string barCode,string valueType)
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
                    IList<T_ContainerDetailEntity> containerDetailTempList = db.FindList<T_ContainerDetailEntity>(o=>o.BarCode == barCode);
                    if(containerDetailTempList.Count<1)
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

        #region 清理货位，容器，库存
        /// <summary>
        /// 获取扫码货位或容器的详细信息
        /// </summary>
        /// <param name="locCode"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult ClearLoc(string locCode, string barCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                if (!string.IsNullOrEmpty(locCode))
                {
                    T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locCode);
                    if (loc != null)
                    {
                        loc.State = "Empty";
                        db.Update<T_LocationEntity>(loc);
                    }

                    T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.LocationNo == locCode && o.F_DeleteMark == false);
                    if (container != null)
                    {
                        container.F_DeleteMark = true;
                        db.Update<T_ContainerEntity>(container);
                    }


                    IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.LocationNo == locCode);
                    foreach (T_ContainerDetailEntity cell in containerDetailList)
                    {
                        db.Delete<T_ContainerDetailEntity>(cell);
                    }
                    db.SaveChanges();
                    db.CommitWithOutRollBack();
                    return Success("清理成功", "");
                }
                else
                {
                    db.RollBack();
                    return Error("请扫货位编码", "");
                }
            }
        }
        #endregion
    }
}
