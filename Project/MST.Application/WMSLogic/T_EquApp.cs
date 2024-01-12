/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.APIPost;
using MST.Application.SystemManage;
using MST.Application.WebMsg;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Domain.ViewModel;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_EquApp
    {

        private IT_EquRepository service = new T_EquRepository();
        public IQueryable<T_EquEntity> FindList(Expression<Func<T_EquEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_EquEntity FindEntity(Expression<Func<T_EquEntity, bool>> predicate)
        {
            T_EquEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_EquEntity> FindListAsNoTracking(Expression<Func<T_EquEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_EquEntity FindEntityAsNoTracking(Expression<Func<T_EquEntity, bool>> predicate)
        {
            T_EquEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public List<T_EquEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_EquEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.EquName.Contains(keyword) || t.EquCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public void Delete(Expression<Func<T_EquEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_EquEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_EquEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_EquEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(itemsEntity.Remark)) itemsEntity.Remark = itemsEntity.Remark.Replace("\n", " ");
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

        public void Update(T_EquEntity entity)
        {
            service.Update(entity);
        }

        public void Insert(T_EquEntity entity)
        {
            service.Insert(entity);
        }
    }
}
