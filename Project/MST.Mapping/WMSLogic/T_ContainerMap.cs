/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.WMSLogic;
using System.Data.Entity.ModelConfiguration;

namespace MST.Domain.IRepository.WMSLogic
{
    public class T_ContainerMap : EntityTypeConfiguration<T_ContainerEntity>
    {
        public T_ContainerMap()
        {
            this.ToTable("T_Container");
            this.HasKey(t => t.F_Id);
        }
    }
}
