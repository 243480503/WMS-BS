/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace MST.Web.Areas.SystemManage.Controllers
{
    public class ItemsDataController : ControllerBase
    {
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private ItemsApp itemsApp = new ItemsApp();

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(string itemId, string keyword)
        {
            var data = itemsDetailApp.GetList(itemId, keyword);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetSelectJson(string enCode)
        {
            var data = itemsDetailApp.GetItemList(enCode);
            List<object> list = new List<object>();
            foreach (ItemsDetailEntity item in data)
            {
                list.Add(new { id = item.F_ItemCode, text = item.F_ItemName });
            }
            return Content(list.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = itemsDetailApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        #region 新增/修改字典
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(ItemsDetailEntity itemsDetailEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ItemsDataController.SubmitForm";
            logObj.Parms = new { itemsDetailEntity = itemsDetailEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "字典管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改字典";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                itemsDetailApp.SubmitForm(itemsDetailEntity, keyValue);

                //配置热更新
                //ItemsDetailEntity itemsDetail =new ItemsDetailApp().FindEntity(o => o.F_Id == keyValue);

                //ItemsEntity items = itemsApp.FindEntity(o=>o.F_Id == itemsDetail.F_ItemId);

                //ItemsEntity parent = itemsApp.FindEntity(o => o.F_Id == items.F_ParentId); //第二级
                //if(parent !=null)
                //{
                //    ItemsEntity parentRoot = itemsApp.FindEntity(o => o.F_Id == parent.F_ParentId); //最上级
                //    if(parentRoot !=null && parentRoot.F_EnCode == "RuleConfig")
                //    {
                //        TypeInfo type = typeof(RuleConfig).GetTypeInfo();
                //        IList<MemberInfo> menberList = type.DeclaredMembers.ToList();
                //        foreach(MemberInfo o in menberList)
                //        {
                //            if(o.Name == parent.F_EnCode)
                //            {
                //                //RuleConfig
                //            }
                //        }
                //    }
                //}
               

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

        #region 删除字典
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ItemsDataController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "字典管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除字典";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                itemsDetailApp.DeleteForm(keyValue);

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

        #region 获取下拉字典数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDicList(string entityName,string fieldName)
        {
            IList<ItemsDetailEntity> enumList = itemsDetailApp.FindEnum(entityName, fieldName).Where(o=>o.F_EnabledMark!=false && o.F_DeleteMark!=true).ToList();
            return Content(enumList.ToJson());
        }
        #endregion
    }
}
