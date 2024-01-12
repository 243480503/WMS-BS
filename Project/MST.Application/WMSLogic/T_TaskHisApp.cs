/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_TaskHisApp
    {
        private IT_TaskHisRepository service = new T_TaskHisRepository();
        private IT_TaskRepository serviceTask = new T_TaskRepository();

        public IQueryable<T_TaskHisEntity> FindList(Expression<Func<T_TaskHisEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_TaskHisEntity FindEntity(Expression<Func<T_TaskHisEntity, bool>> predicate)
        {
            T_TaskHisEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_TaskHisEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_TaskHisEntity> GetList(Pagination pagination, string keyword = "")
        {
            var expression = ExtLinq.True<T_TaskHisEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.TaskNo.Contains(keyword) || t.BarCode.Contains(keyword) || t.SrcLocationCode.Contains(keyword) || t.TagLocationCode.Contains(keyword) || t.OrderCode.Contains(keyword));
            }
            return service.FindList(expression, pagination).ToList();
        }

        public List<T_TaskHisEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_TaskHisEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.TaskNo.Contains(keyword) ||  t.BarCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderByDescending(t => t.OverTime).ToList();
        }

        public void Delete(Expression<Func<T_TaskHisEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public T_TaskHisEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_TaskHisEntity itemsEntity, string keyValue)
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

        public void Insert(T_TaskHisEntity entity)
        {
            service.Insert(entity);
        }

    }
}
