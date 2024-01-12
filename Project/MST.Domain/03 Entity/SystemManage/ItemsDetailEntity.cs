/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.Entity.SystemManage
{
    public class ItemsDetailEntity : IEntity<ItemsDetailEntity>, ICreationAudited, IDeleteAudited, IModificationAudited
    {
        public string F_Id { get; set; }
        public string F_ItemId { get; set; }
        public string F_ParentId { get; set; }
        public string F_ItemCode { get; set; }
        public string F_ItemName { get; set; }

        public string F_Value { get; set; }
        public string F_SimpleSpelling { get; set; }
        public bool? F_IsDefault { get; set; }
        public int? F_Layers { get; set; }
        public int? F_SortCode { get; set; }
        public bool? F_DeleteMark { get; set; }
        public bool? F_EnabledMark { get; set; }
        public string F_Description { get; set; }
        public DateTime? F_CreatorTime { get; set; }
        public string F_CreatorUserId { get; set; }
        public DateTime? F_LastModifyTime { get; set; }
        public string F_LastModifyUserId { get; set; }
        public DateTime? F_DeleteTime { get; set; }
        public string F_DeleteUserId { get; set; }
        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
    }
}
