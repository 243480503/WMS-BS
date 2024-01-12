/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.Entity.WMSLogic
{
    /// <summary>
    /// 容器类型表
    /// </summary>
    public class T_ContainerTypeEntity : IEntity<T_ContainerTypeEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 容器类型ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 类型编码
        /// </summary>
        public string ContainerTypeCode { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string ContainerTypeName { get; set; }

        /// <summary>
        /// 对应的AGV系统的箱型编码
        /// </summary>
        public string AGVContainerTypeCode { get; set; }
        /// <summary>
        /// 长
        /// </summary>
        public decimal? BorderLong { get; set; }
        /// <summary>
        /// 高
        /// </summary>
        public decimal? BorderHeight { get; set; }
        /// <summary>
        /// 宽
        /// </summary>
        public decimal? BorderWidth { get; set; }
        /// <summary>
        /// 货位最小长(深)
        /// </summary>
        public decimal? LocMinLong { get; set; }
        /// <summary>
        /// 货位最小高
        /// </summary>
        public decimal? LocMinHeight { get; set; }
        /// <summary>
        /// 货位最小宽
        /// </summary>
        public decimal? LocMinWidth { get; set; }

        /// <summary>
        /// 货位最大长(深)
        /// </summary>
        public decimal? LocMaxLong { get; set; }
        /// <summary>
        /// 货位最大高
        /// </summary>
        public decimal? LocMaxHeight { get; set; }
        /// <summary>
        /// 货位最大宽
        /// </summary>
        public decimal? LocMaxWidth { get; set; }

        /// <summary>
        /// 默认入库站台
        /// </summary>
        public string InStationID { get; set; }
        /// <summary>
        /// 默认出库站台
        /// </summary>
        public string OutStationID { get; set; }
        /// <summary>
        /// 容器大类(Box、Plastic、Rack)
        /// </summary>
        public string ContainerKind { get; set; }
        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool? F_DeleteMark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? F_CreatorTime { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public string F_CreatorUserId { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreatorUserName { get; set; }
        /// <summary>
        /// 删除操作人
        /// </summary>
        public string F_DeleteUserId { get; set; }
        /// <summary>
        /// 删除操作时间
        /// </summary>
        public DateTime? F_DeleteTime { get; set; }
        /// <summary>
        /// 删除操作人名称
        /// </summary>
        public string DeleteUserName { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? F_LastModifyTime { get; set; }
        /// <summary>
        /// 修改人ID
        /// </summary>
        public string F_LastModifyUserId { get; set; }
        /// <summary>
        /// 修改人名称
        /// </summary>
        public string ModifyUserName { get; set; }
    }
}
