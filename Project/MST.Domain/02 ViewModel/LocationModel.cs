/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
	public class LocationModel
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
		/// Cube 立体库,   Flat    平库
		/// </summary>
		public string AreaType { get; set; }
		/// <summary>
		/// Cube 立体库,   Flat    平库
		/// </summary>
		public string AreaTypeName { get; set; }
		/// <summary>
		/// 1是，0否
		/// </summary>
		public string IsAreaVir { get; set; }

		/// <summary>
		/// 深度
		/// </summary>
		public decimal? Long { get; set; }

		/// <summary>
		/// 货位编码
		/// </summary>
		public string LocationCode { get; set; }

		/// <summary>
		/// 是否防潮
		/// </summary>
		public string IsDampproof { get; set; }
		/// <summary>
		/// Flat 平库,Cube 立库
		/// </summary>
		public string LocationType { get; set; }
		/// <summary>
		/// 货位类型名称 Flat 平库,Cube 立库
		/// </summary>
		public string LocationTypeName { get; set; }
		public ContainerModel containerModel {get;set;}
		public bool IsCurSysUser { get; set; }
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
		/// 货位质检状态（物理区域）
		/// </summary>
		public string CheckPyType { get; set; }


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
		/// 巷道编码
		/// </summary>
		public string WayCode { get; set; }
		/// <summary>
		/// 货位高度
		/// </summary>
		public decimal? High { get; set; }
		/// <summary>
		/// 货位宽度
		/// </summary>
		public decimal? Width { get; set; }
		/// <summary>
		/// 载重
		/// </summary>
		public decimal? Weight { get; set; }

		public string BarCode { get; set; }

		/// <summary>
		/// 是否该储位排除纸箱
		/// </summary>
		public string IsLocTop { get; set; }

		/// <summary>
		/// 是否按物料的区域内优先标记(物料表的IsPriority)，先行存放到此货位
		/// </summary>
		public string IsItemPriority { get; set; }

		/// <summary>
		/// In 待入库,   Out 待出库,   Empty 空,   Stored 已存储
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// AGV所需映射的货位编码
		/// </summary>
		public string WCSLocCode { get; set; }

		public string StateName { get; set; }
		/// <summary>
		/// OnlyIn 可入不可出,   OnlyOut 可出不可入,   Lock  锁定 ,   Normal 正常
		/// </summary>
		public string ForbiddenState { get; set; }
		public string ForbiddenStateName { get; set; }
		/// <summary>
		/// 是否基础数据
		/// </summary>
		public string IsBase { get; set; }
		/// <summary>
		/// 仓库编码
		/// </summary>
		public string ERPHouseCode { get; set; }
		/// <summary>
		/// 仓库名称
		/// </summary>
		public string ERPHouseName { get; set; }
		/// <summary>
		/// 是否显示ERP仓信息
		/// </summary>
		public string ShowingERPHouse { get; set; }
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
