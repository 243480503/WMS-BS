/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;
using System.Reflection;

namespace MST.Repository.WMSLogic
{
    public class T_TimerRepository : RepositoryBase<T_TimerEntity> , IT_TimerRepository
    {
        public new int Update(T_TimerEntity entity)
        {
            dbcontext.Set<T_TimerEntity>().Attach(entity);
            PropertyInfo[] props = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null || prop.Name == "BeginTime" || prop.Name == "EndTime")
                {
                    if (prop.GetValue(entity, null) == null || prop.GetValue(entity, null).ToString() == "&nbsp;")
                    {
                        dbcontext.Entry(entity).Property(prop.Name).CurrentValue = null;
                    }
                    dbcontext.Entry(entity).Property(prop.Name).IsModified = true;
                }
            }
            return dbcontext.SaveChanges();
        }
    }
}
