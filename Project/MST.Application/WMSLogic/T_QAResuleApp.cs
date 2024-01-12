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
    public class T_QAResultApp
    {
        private IT_QAResultRepository service = new T_QAResultRepository();
        public IQueryable<T_QAResultEntity> FindList(Expression<Func<T_QAResultEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_QAResultEntity FindEntity(Expression<Func<T_QAResultEntity, bool>> predicate)
        {
            T_QAResultEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_QAResultEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_QAResultEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public List<T_QAResultEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_QAResultEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.RefOrderCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderByDescending(t => t.F_CreatorTime).ToList();
        }
        public T_QAResultEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_QAResultEntity itemsEntity, string keyValue)
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

        public void Insert(T_QAResultEntity entity)
        {
            service.Insert(entity);
        }
    }
}
