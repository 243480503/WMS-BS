/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.IRepository.SystemManage;
using MST.Domain.ViewModel;
using MST.Repository.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MST.Application.SystemManage
{
    public class RoleAuthorizeApp
    {
        private IRoleAuthorizeRepository service = new RoleAuthorizeRepository();
        private ModuleApp moduleApp = new ModuleApp();
        private ModuleButtonApp moduleButtonApp = new ModuleButtonApp();

        public List<RoleAuthorizeEntity> GetList(string ObjectId)
        {
            return service.IQueryable(t => t.F_ObjectId == ObjectId).ToList();
        }
        public List<ModuleEntity> GetMenuList(string roleId)
        {
            var data = new List<ModuleEntity>();
            if (OperatorProvider.Provider.GetCurrent().IsSystem)
            {
                data = moduleApp.GetList();
            }
            else
            {
                var moduledata = moduleApp.GetList();
                var authorizedata = service.IQueryable(t => t.F_ObjectId == roleId && t.F_ItemType == 1).ToList();
                foreach (var item in authorizedata)
                {
                    ModuleEntity moduleEntity = moduledata.Find(t => t.F_Id == item.F_ItemId);
                    if (moduleEntity != null)
                    {
                        data.Add(moduleEntity);
                    }
                }
            }
            return data.OrderBy(t => t.F_SortCode).ToList();
        }
        public List<ModuleButtonEntity> GetButtonList(string roleId)
        {
            var data = new List<ModuleButtonEntity>();
            if (OperatorProvider.Provider.GetCurrent().IsSystem)
            {
                data = moduleButtonApp.GetList();
            }
            else
            {
                var buttondata = moduleButtonApp.GetList();
                var authorizedata = service.IQueryable(t => t.F_ObjectId == roleId && t.F_ItemType == 2).ToList();
                foreach (var item in authorizedata)
                {
                    ModuleButtonEntity moduleButtonEntity = buttondata.Find(t => t.F_Id == item.F_ItemId);
                    if (moduleButtonEntity != null)
                    {
                        data.Add(moduleButtonEntity);
                    }
                }
            }
            return data.OrderBy(t => t.F_SortCode).ToList();
        }
        public bool ActionValidate(string roleId, string moduleId, string action,DateTime lastModifyRoleAuthorize)
        {
            var authorizeurldata = new List<AuthorizeActionModel>();
            var cachedata = CacheFactory.Cache().GetCache<List<AuthorizeActionModel>>($"authorizeurldata_{ roleId }");

            bool isNeedRefrsh = false;
            if(cachedata !=null && cachedata.Count>0)
            {
                AuthorizeActionModel last = cachedata.OrderBy(o => o.LastGetTime).FirstOrDefault();
                if(last.LastGetTime < lastModifyRoleAuthorize)
                {
                    isNeedRefrsh = true;
                }
            }

            if (cachedata == null || isNeedRefrsh == true)
            {
                var moduledata = moduleApp.GetList();
                var buttondata = moduleButtonApp.GetList();
                var authorizedata = service.IQueryable(t => t.F_ObjectId == roleId).OrderBy(o=>o.F_SortCode).ToList();
                DateTime lastTime = DateTime.Now;
                foreach (var item in authorizedata)
                {
                    if (item.F_ItemType == 1)
                    {
                        ModuleEntity moduleEntity = moduledata.Find(t => t.F_Id == item.F_ItemId);
                        authorizeurldata.Add(new AuthorizeActionModel { F_Id = moduleEntity.F_Id, F_UrlAddress = moduleEntity.F_UrlAddress,Name = moduleEntity.F_FullName, LastGetTime = lastTime });
                    }
                    else if (item.F_ItemType == 2)
                    {
                        ModuleButtonEntity moduleButtonEntity = buttondata.Find(t => t.F_Id == item.F_ItemId);
                        authorizeurldata.Add(new AuthorizeActionModel { F_Id = moduleButtonEntity.F_ModuleId, F_UrlAddress = moduleButtonEntity.F_UrlAddress,Name=moduleButtonEntity.F_FullName,LastGetTime = lastTime });
                    }
                }
                CacheFactory.Cache().WriteCache(authorizeurldata, $"authorizeurldata_{ roleId }", DateTime.Now.AddDays(1));
            }
            else
            {
                authorizeurldata = cachedata;
            }
            //authorizeurldata = authorizeurldata.FindAll(t => t.F_Id.Equals(moduleId));
            //foreach (var item in authorizeurldata)
            //{
            //    if (!string.IsNullOrEmpty(item.F_UrlAddress))
            //    {
            //        string[] url = item.F_UrlAddress.Split('?');
            //        if (item.F_Id == moduleId && url[0] == action)
            //        {
            //            return true;
            //        }
            //    }
            //}
            var test = authorizeurldata.FindAll(t => t.F_Id.Equals(moduleId));
            var testJson = authorizeurldata.FindAll(t => t.F_Id.Equals(moduleId)).Select(o=>new { Name=o.Name,URL =o.F_UrlAddress }).ToJson();
            foreach (var item in authorizeurldata)
            {
                if (!string.IsNullOrEmpty(item.F_UrlAddress))
                {
                    string[] url = item.F_UrlAddress.Split('?');
                    if (url[0].ToUpper() == action.ToUpper())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
