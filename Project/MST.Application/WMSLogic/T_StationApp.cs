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
    public class T_StationApp
    { 
        private IT_StationRepository service = new T_StationRepository();
        public IQueryable<T_StationEntity> FindList(Expression<Func<T_StationEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_StationEntity FindEntity(Expression<Func<T_StationEntity, bool>> predicate)
        {
            T_StationEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public IQueryable<T_StationEntity> FindListAsNoTracking(Expression<Func<T_StationEntity, bool>> predicate)
        {
            return service.IQueryableAsNoTracking(predicate);
        }

        public T_StationEntity FindEntityAsNoTracking(Expression<Func<T_StationEntity, bool>> predicate)
        {
            T_StationEntity entity = service.FindEntityAsNoTracking(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_StationEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_StationEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_StationEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.StationName.Contains(keyword) ||t.StationCode.Contains(keyword) );
            }
            return service.IQueryable(expression).OrderBy(t => t.StationCode).ToList();
        }
        public List<T_StationEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_StationEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.StationName.Contains(keyword) || t.StationCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public List<T_StationEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public T_StationEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_StationEntity itemsEntity, string keyValue)
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

        public void Insert(T_StationEntity entity)
        {
            service.Insert(entity);
        }
    }
}
