/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_WaveDetailApp
    {
        private IT_WaveDetailRepository service = new T_WaveDetailRepository();
        public IQueryable<T_WaveDetailEntity> FindList(Expression<Func<T_WaveDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_WaveDetailEntity FindEntity(Expression<Func<T_WaveDetailEntity, bool>> predicate)
        {
            T_WaveDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_WaveDetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_WaveDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_WaveDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_WaveDetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_WaveDetailEntity entity)
        {
            service.Insert(entity);
        }
    }
}
