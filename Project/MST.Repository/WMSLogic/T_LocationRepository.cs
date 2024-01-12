/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Domain.ViewModel;
using MST.Repository.WMSLogic;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace MST.Repository.WMSLogic
{
    public class T_LocationRepository : RepositoryBase<T_LocationEntity> , IT_LocationRepository
    {

    }
}
