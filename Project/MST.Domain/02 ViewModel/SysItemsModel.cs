/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class SysItemsModel
    {
        public string F_Id { get; set; }
        public string F_ParentId { get; set; }
        /// <summary>
        /// 字段名
        /// </summary>
        public string F_EnCode { get; set; }
        public string F_FullName { get; set; }
        public bool? F_IsTree { get; set; }
        public int? F_Layers { get; set; }
        public int? F_SortCode { get; set; }
        public bool? F_DeleteMark { get; set; }
        public bool? F_EnabledMark { get; set; }
        public string F_Description { get; set; }
        /// <summary>
        /// 该字段所有枚举
        /// </summary>
        public IList<ItemsDetailEntity> DetailList { get; set; }

        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
    }
}
