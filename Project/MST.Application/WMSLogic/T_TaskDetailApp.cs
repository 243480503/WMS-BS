/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.SystemSecurity;
using MST.Code;
using MST.Data;
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
    public class T_TaskDetailApp
    {
        private static object syncObj = new object(); /// 单线程

        private IT_TaskDetailRepository service = new T_TaskDetailRepository();
        public IQueryable<T_TaskDetailEntity> FindList(Expression<Func<T_TaskDetailEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_TaskDetailEntity FindEntity(Expression<Func<T_TaskDetailEntity, bool>> predicate)
        {
            T_TaskDetailEntity entity = service.FindEntity(predicate);
            return entity;
        }
        public List<T_TaskDetailEntity> GetList()
        {
            return service.IQueryable().ToList();
        }

        public List<T_TaskDetailEntity> GetList(Pagination pagination, string taskID = "", string keyword = "")
        {
            var expression = ExtLinq.True<T_TaskDetailEntity>();
            if (!string.IsNullOrEmpty(taskID))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.TaskID == taskID && (t.TaskNo.Contains(keyword) || t.BarCode.Contains(keyword)));
                }
                else expression = expression.And(t => t.TaskID == taskID);
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.TaskNo.Contains(keyword) ||t.BarCode.Contains(keyword) );
                }
            }
            return service.FindList(expression, pagination).ToList();
        }

        public void Delete(Expression<Func<T_TaskDetailEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public T_TaskDetailEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_TaskDetailEntity itemsEntity, string keyValue)
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

        public void Insert(T_TaskDetailEntity entity)
        {
            service.Insert(entity);
        }
    }
}
