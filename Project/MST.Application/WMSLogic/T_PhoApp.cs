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
    public class T_PhoApp
    {
        private IT_PhoRepository service = new T_PhoRepository();
        private IT_PhoDetailRepository serviceDetail = new T_PhoDetailRepository();

        public IQueryable<T_PhoEntity> FindList(Expression<Func<T_PhoEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_PhoEntity FindEntity(Expression<Func<T_PhoEntity, bool>> predicate)
        {
            T_PhoEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_PhoEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public void Delete(Expression<Func<T_PhoEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public T_PhoEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }

        /// <summary>
        /// 界面创建快照
        /// </summary>
        /// <param name="itemsEntity"></param>
        /// <param name="keyValue"></param>
        public void SubmitForm(T_PhoEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.F_DeleteMark = false;
                itemsEntity.Create();
                service.Insert(itemsEntity);
                serviceDetail.ExecutePhoDetailBackup(itemsEntity);
            }
        }

        /// <summary>
        /// 定时器创建快照
        /// </summary>
        /// <param name="db"></param>
        /// <param name="itemsEntity"></param>
        /// <param name="user"></param>
        public void TimerCreatePhoe(IRepositoryBase db, T_PhoEntity itemsEntity,OperatorModel user)
        {
            db.Insert<T_PhoEntity>(itemsEntity, user);
            serviceDetail.ExecutePhoDetailBackup(itemsEntity);
        }

        public void Insert(T_PhoEntity entity)
        {
            service.Insert(entity);
        }

        public void Update(T_PhoEntity entity)
        {
            service.Update(entity);
        }
    }
}
