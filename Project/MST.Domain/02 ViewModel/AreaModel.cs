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
    public class AreaModel
    {
        public string F_Id { get; set; }    /// 区域ID
		public string AreaCode { get; set; }    /// 区域编码
		public string AreaName { get; set; }    /// 区域名称
		public string LocationPrefix { get; set; }  /// 货位前缀
		public string ERPWarehouseID { get; set; }  /// ERP仓库ID
		public string ERPWarehouseCode { get; set; }    /// ERP仓库编码
		public string ERPWarehouseName { get; set; }    /// ERP仓库名称
		public string AreaType { get; set; }


        /// <summary>
        /// 是否主功能区域
        /// </summary>
        public string IsMain { get; set; }


        /// <summary>
        /// 是否巷道均分
        /// </summary>
        public string IsEvenTunnel { get; set; }
        /// <summary>
        /// 是否ERP仓位做物理区域
        /// </summary>
        public string IsERPPy { get; set; }

        /// <summary>
        /// 是否质检状态做物理区域
        /// </summary>
        public string IsCheckPy { get; set; }
        /// <summary>
        /// 巷道倒序
        /// </summary>
        public string IsTunnelDesc { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public string IsEnable { get; set; }

        public string AreaTypeName { get; set; }    /// 区域类型名称
                                                    /// Cube 立体库
                                                    /// Flat 平库
        public string IsBase { get; set; }  /// 是否基础数据

        public string IsAreaVir { get; set; }   /// 1是，0否

        public string Remark { get; set; }  /// 备注
        public string ParentID { get; set; }    /// 父ID
        public bool? F_DeleteMark { get; set; } /// 是否删除
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
