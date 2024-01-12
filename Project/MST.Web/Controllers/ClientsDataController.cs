/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.SystemManage;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MST.Web.Controllers
{
    [HandlerLogin]
    public class ClientsDataController : Controller
    {
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetClientsDataJson(bool isPc)
        {
            var data = new
            {
                dataItems = this.GetDataItemList(),
                organize = this.GetOrganizeList(),
                role = this.GetRoleList(),
                duty = this.GetDutyList(),
                user = "",
                authorizeMenu = this.GetMenuList(isPc),
                authorizeButton = this.GetMenuButtonList(),
            };
            return Content(data.ToJson());
        }
        private object GetDataItemList()
        {
            var itemdata = new ItemsDetailApp().GetList();
            Dictionary<string, object> dictionaryItem = new Dictionary<string, object>();
            IList<ItemsEntity> itemsList = new ItemsApp().GetList();
            //排除类型配置 by fxx
            ItemsEntity typeConfig = itemsList.Where(o => o.F_EnCode == "TypeConfig").FirstOrDefault();
            if(typeConfig != null)
            {
                IList<ItemsEntity> tabList = itemsList.Where(o => o.F_ParentId == typeConfig.F_Id).ToList();
                IList<string> tabStr = tabList.Select(o => o.F_Id).ToList();
                IList<ItemsEntity> FieldList = itemsList.Where(o => tabStr.Contains(o.F_ParentId)).ToList();
                IList<string> fieldStr = FieldList.Select(o => o.F_Id).ToList();
                itemsList = itemsList.Where(o=>(!tabStr.Contains(o.F_Id)) && (!fieldStr.Contains(o.F_Id)) && (!fieldStr.Contains(o.F_ParentId)) && o.F_Id!= typeConfig.F_Id).ToList();
            }
            //排除业务配置 by fxx
            ItemsEntity ruleConfig = itemsList.Where(o => o.F_EnCode == "RuleConfig").FirstOrDefault();
            if (ruleConfig != null)
            {
                IList<ItemsEntity> tabList = itemsList.Where(o => o.F_ParentId == ruleConfig.F_Id).ToList();
                IList<string> tabStr = tabList.Select(o => o.F_Id).ToList();
                IList<ItemsEntity> FieldList = itemsList.Where(o => tabStr.Contains(o.F_ParentId)).ToList();
                IList<string> fieldStr = FieldList.Select(o => o.F_Id).ToList();
                itemsList = itemsList.Where(o => (!tabStr.Contains(o.F_Id)) && (!fieldStr.Contains(o.F_Id)) && (!fieldStr.Contains(o.F_ParentId)) && o.F_Id != ruleConfig.F_Id).ToList();
            }

            foreach (var item in itemsList)
            {
                var dataItemList = itemdata.FindAll(t => t.F_ItemId.Equals(item.F_Id));
                Dictionary<string, string> dictionaryItemList = new Dictionary<string, string>();
                foreach (var itemList in dataItemList)
                {
                    dictionaryItemList.Add(itemList.F_ItemCode, itemList.F_ItemName);
                }
                dictionaryItem.Add(item.F_EnCode, dictionaryItemList);
            }
            return dictionaryItem;
        }
        private object GetOrganizeList()
        {
            OrganizeApp organizeApp = new OrganizeApp();
            var data = organizeApp.GetList();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (OrganizeEntity item in data)
            {
                var fieldItem = new
                {
                    encode = item.F_EnCode,
                    fullname = item.F_FullName
                };
                dictionary.Add(item.F_Id, fieldItem);
            }
            return dictionary;
        }
        private object GetRoleList()
        {
            RoleApp roleApp = new RoleApp();
            var data = roleApp.GetList();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (RoleEntity item in data)
            {
                var fieldItem = new
                {
                    encode = item.F_EnCode,
                    fullname = item.F_FullName
                };
                dictionary.Add(item.F_Id, fieldItem);
            }
            return dictionary;
        }
        private object GetDutyList()
        {
            DutyApp dutyApp = new DutyApp();
            var data = dutyApp.GetList();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (RoleEntity item in data)
            {
                var fieldItem = new
                {
                    encode = item.F_EnCode,
                    fullname = item.F_FullName
                };
                dictionary.Add(item.F_Id, fieldItem);
            }
            return dictionary;
        }
        private object GetMenuList(bool isPc)
        {
            var roleId = OperatorProvider.Provider.GetCurrent().RoleId;
            return ToMenuJson(new RoleAuthorizeApp().GetMenuList(roleId), "0",isPc);
        }
        private string ToMenuJson(List<ModuleEntity> data, string parentId,bool isPc)
        {
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("[");
            List<ModuleEntity> entitys = new List<ModuleEntity>();
            if (isPc)
            {
                entitys = data.FindAll(t => t.F_ParentId == parentId && t.F_IsRF == false);
            }
            else
            {
                entitys = data.FindAll(t => t.F_ParentId == parentId  && t.F_IsRF == true);
            }
            if (entitys.Count > 0)
            {
                foreach (var item in entitys)
                {
                    string strJson = item.ToJson();
                    strJson = strJson.Insert(strJson.Length - 1, ",\"ChildNodes\":" + ToMenuJson(data, item.F_Id,isPc) + "");
                    sbJson.Append(strJson + ",");
                }
                sbJson = sbJson.Remove(sbJson.Length - 1, 1);
            }
            sbJson.Append("]");
            return sbJson.ToString();
        }
        private object GetMenuButtonList()
        {
            var roleId = OperatorProvider.Provider.GetCurrent().RoleId;
            var data = new RoleAuthorizeApp().GetButtonList(roleId);
            var dataModuleId = data.Distinct(new ExtList<ModuleButtonEntity>("F_ModuleId"));
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (ModuleButtonEntity item in dataModuleId)
            {
                var buttonList = data.Where(t => t.F_ModuleId.Equals(item.F_ModuleId));
                dictionary.Add(item.F_ModuleId, buttonList);
            }
            return dictionary;
        }
    }
}
