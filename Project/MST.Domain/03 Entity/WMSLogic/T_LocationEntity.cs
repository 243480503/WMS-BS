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
    /// 货位表
    /// </summary>
    public class T_LocationEntity : IEntity<T_LocationEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
    {
        /// <summary>
        /// 货位ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 区域ID
        /// </summary>
        public string AreaID { get; set; }
        /// <summary>
        /// 区域编码
        /// </summary>
        public string AreaCode { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 区域类型.Cube 立体库,Flat 平库
        /// </summary>
        public string AreaType { get; set; }
        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }

        /// <summary>
        /// 深度
        /// </summary>
        public decimal? Long { get; set; }

        /// <summary>
        /// 是否虚拟区域
        /// </summary>
        public string IsAreaVir { get; set; }
        /// <summary>
        /// 货位编码
        /// </summary>
        public string LocationCode { get; set; }
        /// <summary>
        /// Flat 平库,Cube 立库
        /// </summary>
        public string LocationType { get; set; }

        /// <summary>
        /// 是否防潮
        /// </summary>
        public string IsDampproof { get; set; }
        /// <summary>
        /// 行
        /// </summary>
        public int? Line { get; set; }
        /// <summary>
        /// 列
        /// </summary>
        public int? ColNum { get; set; }
        /// <summary>
        /// 层
        /// </summary>
        public int? Layer { get; set; }
        /// <summary>
        /// 深
        /// </summary>
        public int? Deep { get; set; }

        /// <summary>
        /// 行(地图)
        /// </summary>
        public int? MapLine { get; set; }
        /// <summary>
        /// 列(地图)
        /// </summary>
        public int? MapColNum { get; set; }
        /// <summary>
        /// 层(地图)
        /// </summary>
        public int? MapLayer { get; set; }

        /// <summary>
        /// 货位高度
        /// </summary>
        public decimal? High { get; set; }
        /// <summary>
        /// 货位宽度
        /// </summary>
        public decimal? Width { get; set; }

        /// <summary>
        /// 货位质检状态（物理区域）
        /// </summary>
        public string CheckPyType { get; set; }
        /// <summary>
        /// 载重
        /// </summary>
        public decimal? Weight { get; set; }
        /// <summary>
        /// In 待入库,   Out 待出库,   Empty 空,   Stored 已存储
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// OnlyIn 可入不可出,   OnlyOut 可出不可入,   Lock  锁定 ,   Normal 正常
        /// </summary>
        public string ForbiddenState { get; set; }
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string ERPHouseCode { get; set; }
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string ERPHouseName { get; set; }
        /// <summary>
        /// 状态变更单据类型
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// 是否顶层
        /// </summary>
        public string IsLocTop { get; set; }

        /// <summary>
        /// 是否按物料的区域内优先标记(物料表的IsPriority)，先行存放到此货位
        /// </summary>
        public string IsItemPriority { get; set; }

        /// <summary>
		/// AGV所需映射的货位编码
		/// </summary>
		public string WCSLocCode { get; set; }
        /// <summary>
        /// 状态变更单据编码
        /// </summary>
        public string OrderCode { get; set; }
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
