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
    public class EquController : ControllerBase
    {
        private T_EquApp equApp = new T_EquApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_DevRowApp devRowApp = new T_DevRowApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_AreaApp areaApp = new T_AreaApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            IList<T_EquEntity> data = equApp.GetList(pagination, keyword);
            IList<EquModel> modelList = new List<EquModel>();
            IList<ItemsDetailEntity> detailEntityEquTypeList = itemsDetailApp.FindEnum<T_EquEntity>(o => o.EquType);
            IList<ItemsDetailEntity> detailEntityStateList = itemsDetailApp.FindEnum<T_EquEntity>(o => o.State);
            foreach (T_EquEntity eq in data)
            {
                EquModel model = eq.ToObject<EquModel>();
                model.EquTypeName = detailEntityEquTypeList.FirstOrDefault(o => o.F_ItemCode == eq.EquType).F_ItemName;
                model.StateName = detailEntityStateList.FirstOrDefault(o => o.F_ItemCode == eq.State).F_ItemName;
                modelList.Add(model);
            }
            //return Content(modelList.ToJson());

            var resultList = new
            {
                rows = modelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        #region 获取设备列表
        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetAreaDicList()
        {
            List<ItemsDetailEntity> enumList = new List<ItemsDetailEntity>();
            List<T_AreaEntity> areaList = areaApp.GetList();
            foreach (T_AreaEntity item in areaList)
            {
                ItemsDetailEntity temp = new ItemsDetailEntity();
                temp.F_ItemCode = item.F_Id;
                temp.F_ItemName = item.AreaName;
                enumList.Add(temp);
            }
            return Content(enumList.ToJson());
        }
        #endregion


        #region 新增/修改设备列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = equApp.GetForm(keyValue);
            EquModel model = data.ToObject<EquModel>();
            if (model.EquType == "Stacker") //堆垛机
            {
                model.IsNeedRow = true;
                model.equDevRow = devRowApp.GetListByEquID(model.F_Id);
                model.allRow = locationApp.GetAllRow();
            }
            else
            {
                model.IsNeedRow = false;
            }
            return Content(model.ToJson());
        }


        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_EquEntity equEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "EquController.SubmitForm";
            logObj.Parms = new { equEntity = equEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "设备管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新增/修改设备";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    if (!user.IsSystem)
                    {
                        T_EquEntity entity = equApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                        if (entity.IsBase == "true")
                        {
                            return Error("系统数据不允许修改。", "");
                        }
                    }
                }

                int isExistsCode = equApp.FindListAsNoTracking(o => o.EquCode == equEntity.EquCode && o.F_Id != keyValue).Count();
                if (isExistsCode > 0)
                {
                    return Error("编码已存在", "");
                }

                equApp.SubmitForm(equEntity, keyValue);

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

        #region 删除设备列表
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "EquController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "设备管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除设备列表";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    T_EquEntity entity = equApp.FindEntity(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                equApp.DeleteForm(keyValue);

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
