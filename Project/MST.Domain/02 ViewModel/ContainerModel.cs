/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 容器表
    /// </summary>
    public class ContainerModel
    {
        public string F_Id { get; set; }    /// 容器ID
		public string LocationID { get; set; }  /// 货位ID
		public string BarCode { get; set; } /// 容器条码
		public string ContainerType { get; set; }   /// 容器类型编码
		public string ContainerKind { get; set; }   /// 容器大类
		public string ContainerKindName { get; set; } /// 容器大类名称
        public string IsContainerVir { get; set; }  /// 是否虚拟容器
		public string ContainerSpec { get; set; }   /// 容器规格
		public string LocationNo { get; set; }  /// 货位编码
		public string AreaID { get; set; }  /// 区域ID
		public string AreaCode { get; set; }    /// 区域编码
		public string AreaName { get; set; }    /// 区域名称
		public bool? F_DeleteMark { get; set; } /// 是否删除

        public IList<ContainerDetailModel> containerDetailModel { get; set; }

        public TaskModel Task { get; set; }
        public decimal? HandQty { get; set; }
        public DateTime? F_CreatorTime { get; set; }    /// 创建时间
		public string F_CreatorUserId { get; set; } /// 创建人ID
		public string CreatorUserName { get; set; } /// 创建人名称
		public string F_DeleteUserId { get; set; }  /// 删除操作人
		public DateTime? F_DeleteTime { get; set; } /// 删除操作时间
		public string DeleteUserName { get; set; }  /// 删除操作人名称
		public DateTime? F_LastModifyTime { get; set; } /// 修改时间
		public string F_LastModifyUserId { get; set; }  /// 修改人ID
		public string ModifyUserName { get; set; }  /// 修改人名称
	}
}
