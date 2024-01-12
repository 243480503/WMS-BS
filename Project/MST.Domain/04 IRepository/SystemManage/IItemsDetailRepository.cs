/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.ViewModel;
using System.Collections.Generic;

namespace MST.Domain.IRepository.SystemManage
{
    public interface IItemsDetailRepository : IRepositoryBase<ItemsDetailEntity>
    {
        List<ItemsDetailEntity> GetItemList(string enCode);

        List<ItemsDetailEntity> GetEnumList(string tableCode, string fieldCode);

        IList<SysItemsModel> GetEnumList(string tableCode);
    }
}
