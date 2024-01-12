/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_BaseDataManage.Controllers
{
    public class ContainerTypeController : ControllerBase
    {
        private T_ContainerTypeApp ContainerTypeApp = new T_ContainerTypeApp();
        private T_StationApp stationApp = new T_StationApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            var data = ContainerTypeApp.GetList(pagination, keyword);
            IList<ContainerTypeModel> typeModels = new List<ContainerTypeModel>();
            foreach (T_ContainerTypeEntity item in data)
            {
                ContainerTypeModel model = item.ToObject<ContainerTypeModel>();
                model.ContainerKindName = new ItemsDetailApp().FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == item.ContainerKind).F_ItemName;
                T_StationEntity inStation = stationApp.FindEntity(o => o.F_Id == model.InStationID);
                if(inStation!=null)
                {
                    model.InStationName = inStation.StationName;
                }
                T_StationEntity outStation = stationApp.FindEntity(o => o.F_Id == model.OutStationID);
                if(outStation!=null)
                {
                    model.OutStationName = outStation.StationName;
                }
                typeModels.Add(model);
            }
            //return Content(typeModels.ToJson());

            var resultList = new
            {
                rows = typeModels,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            T_ContainerTypeEntity data = ContainerTypeApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        #region 获取默认入库点列表
        /// <summary>
        /// 获取默认入库点列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetStationDicList(string type)
        {
            List<T_StationEntity> stationList = new List<T_StationEntity>();
            if(type == "In")
            {
                stationList = stationApp.FindList(o => o.StationCode == FixType.Station.StationIn_BigItem.ToString() || o.StationCode == FixType.Station.StationIn_Normal.ToString()).ToList();
            }
            else if(type == "Out")
            {
                stationList = stationApp.FindList(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString() || o.StationCode == FixType.Station.StationOut_Normal.ToString()).ToList();
            }
            else
            {
                return Error("参数有误", "");
            }
            return Content(stationList.ToJson());
        }
        #endregion

        #region 新增/修改容器列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_ContainerTypeEntity itemsEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ContainerTypeController.SubmitForm";
            logObj.Parms = new { itemsEntity = itemsEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "容器管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改容器";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    if (!user.IsSystem)
                    {
                        T_ContainerTypeEntity entity = ContainerTypeApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                        if (entity.IsBase == "true")
                        {
                            return Error("系统数据不允许修改。", "");
                        }
                    }
                }

                int isExistsCode = ContainerTypeApp.FindList(o => o.ContainerTypeCode == itemsEntity.ContainerTypeCode && o.F_Id != keyValue).Count();
                if (isExistsCode > 0)
                {
                    return Error("编码已存在", "");
                }

                itemsEntity.F_DeleteMark = false;
                ContainerTypeApp.SubmitForm(itemsEntity, keyValue);

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


        #region 删除容器
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ContainerTypeController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "容器管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除容器列表";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                T_ContainerTypeEntity entity = ContainerTypeApp.FindEntity(o=>o.F_Id == keyValue);
                if (!user.IsSystem)
                {
                    
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。","");
                    }
                }
                int countInContainer = new T_ItemApp().FindList(o => o.ContainerType == entity.ContainerTypeCode).Count();
                if(countInContainer>0)
                {
                    return Error("容器信息已被应用。", "");
                }


                ContainerTypeApp.DeleteForm(keyValue);

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
    }
}
