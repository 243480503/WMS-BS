using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class MoveRecordModel
    {
        /// <summary>
        /// 移库记录ID
        /// </summary>
        public string F_Id { get; set; }

        /// <summary>
        /// 移库单ID
        /// </summary>
        public string MoveID { get; set; }

        /// <summary>
        /// 容器ID
        /// </summary>
        public string ContainerID { get; set; }



        /// <summary>
        /// 容器条码
        /// </summary>
        public string BarCode { get; set; }

        public string ContainerKindName { get; set; }

        public string SrcLocationID { get; set; }

        public string LocationNo { get; set; }

        /// <summary>
        /// 穿梭框标记是更新还是新增
        /// </summary>
        public string IsNewAdd { get; set; }

        /// <summary>
        /// 源货位编码
        /// </summary>
        public string SrcLocationCode { get; set; }

        /// <summary>
        /// 移库创建方式（手动指定目标货位PointTag,手动指定源货位PointSource,自动生成Auto）
        /// </summary>
        public string GenType { get; set; }

        /// <summary>
        /// 目标货位ID
        /// </summary>
        public string TagLocationID { get; set; }

        /// <summary>
        /// 目标货位编码
        /// </summary>
        public string TagLocationCode { get; set; }

        public string ItemBarCode { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public string ItemID { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        public string Spec { get; set; }   /// 规格
        public string ItemUnitText { get; set; }   /// 计量单位
        public DateTime? OverdueDate { get; set; }   /// 失效日期
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }

        public string InBoundID { get; set; }

        public string InBoundDetailID { get; set; }

        public string ReceiveRecordID { get; set; }

        public decimal? Qty { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }

        public string StateName { get; set; }

        /// <summary>
        /// 容器大类
        /// </summary>
        public string ContainerKind { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? OverTime { get; set; }

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
