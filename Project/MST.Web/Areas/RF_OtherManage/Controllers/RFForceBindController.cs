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
    public class RFForceBindController : ControllerBase
    {
        #region 获取扫码容器的详细信息
        /// <summary>
        /// 获取扫码容器的详细信息
        /// </summary>
        /// <param name="barCode"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetContainerInfo(string barCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                ItemsDetailApp dicDetail = new ItemsDetailApp();


                LocationModel locModel = null;
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

                ContainerModel containerModel = null;
                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == barCode && o.F_DeleteMark == false);
                if (container != null)
                {
                    containerModel = container.ToObject<ContainerModel>();
                    T_ContainerTypeEntity conType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerKind == containerModel.ContainerKind);
                    containerModel.ContainerKindName = conType.ContainerTypeName;
                }


                IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == container.F_Id);
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

        #region 强制绑定
        /// <summary>
        /// 强制绑定
        /// </summary>
        /// <param name="locCode"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult ForceBind(string conBarCode, string newLocCode)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "RFForceBindController.ForceBind";
            logObj.Parms = new { conBarCode = conBarCode, newLocCode = newLocCode };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "RF强制绑定";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "RF强制绑定";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();
            try
            {

                using (var db = new RepositoryBase().BeginTrans())
                {
                    if (string.IsNullOrEmpty(conBarCode))
                    {
                        return Error("请扫容器编码", "");
                    }

                    T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == conBarCode && o.F_DeleteMark == false);
                    if (container == null)
                    {
                        return Error("容器不存在", "");
                    }

                    IList<T_ContainerDetailEntity> containerDetailTempList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == conBarCode);
                    if (containerDetailTempList.Count < 1)
                    {
                        return Error("容器库存不存在", "");
                    }

                    T_ContainerTypeEntity conType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == container.ContainerType);
                    if (conType == null)
                    {
                        return Error("原容器没有容器种类", "");
                    }

                    if (string.IsNullOrEmpty(newLocCode))
                    {
                        return Error("请扫绑定的货位编码", "");
                    }
                    T_LocationEntity newLoc = db.FindEntity<T_LocationEntity>(o => o.LocationCode == newLocCode);
                    if (newLoc == null)
                    {
                        return Error("绑定的货位不存在", "");
                    }


                    //解绑容器的原货位(此操作可能产生游离的容器，即LocationID为空字符串)
                    if (!string.IsNullOrEmpty(container.LocationID))
                    {
                        T_LocationEntity oldLoc = db.FindEntity<T_LocationEntity>(o => o.F_Id == container.LocationID);
                        oldLoc.State = "Empty";
                        db.Update<T_LocationEntity>(oldLoc);
                    }


                    //解绑货位的原容器

                    T_ContainerEntity oldCon = db.FindEntity<T_ContainerEntity>(o => o.LocationID == newLoc.F_Id && o.F_DeleteMark == false);
                    if (oldCon != null)
                    {
                        oldCon.LocationID = "";
                        oldCon.LocationNo = "";
                        db.Update<T_ContainerEntity>(oldCon);

                        //解绑货位的原容器库存
                        IList<T_ContainerDetailEntity> oldConDetail = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == oldCon.F_Id);
                        foreach (T_ContainerDetailEntity detail in oldConDetail)
                        {
                            detail.LocationID = "";
                            detail.LocationNo = "";
                            db.Update<T_ContainerDetailEntity>(detail);
                        }
                    }




                    //绑定容器与货位

                    container.LocationID = newLoc.F_Id;
                    container.LocationNo = newLoc.LocationCode;
                    container.AreaCode = newLoc.AreaCode;
                    container.AreaID = newLoc.AreaID;
                    container.AreaName = newLoc.AreaName;

                    db.Update<T_ContainerEntity>(container);

                    foreach (T_ContainerDetailEntity detail in containerDetailTempList)
                    {
                        detail.LocationID = newLoc.F_Id;
                        detail.LocationNo = newLoc.LocationCode;
                        detail.AreaCode = newLoc.AreaCode;
                        detail.AreaID = newLoc.AreaID;
                        detail.AreaName = newLoc.AreaName;
                        db.Update<T_ContainerDetailEntity>(detail);
                    }

                    newLoc.State = "Stored";
                    db.Update<T_LocationEntity>(newLoc);


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

