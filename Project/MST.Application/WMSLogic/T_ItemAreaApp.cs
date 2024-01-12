/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_ItemAreaApp
    {
        private IT_ItemAreaRepository service = new T_ItemAreaRepository();
        public IQueryable<T_ItemAreaEntity> FindList(Expression<Func<T_ItemAreaEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_ItemAreaEntity FindEntity(Expression<Func<T_ItemAreaEntity, bool>> predicate)
        {
            T_ItemAreaEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_ItemAreaEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public T_ItemAreaEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void Delete(Expression<Func<T_ItemAreaEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public void SubmitForm(T_ItemAreaEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.Create();
                service.Insert(itemsEntity);
            }
        }

        public void Insert(T_ItemAreaEntity entity)
        {
            service.Insert(entity);
        }
    }
}
