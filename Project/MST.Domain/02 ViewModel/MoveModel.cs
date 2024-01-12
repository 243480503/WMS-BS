using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class MoveModel
    {
        /// <summary>
        /// 移库单ID
        /// </summary>
        public string F_Id { get; set; }

        /// <summary>
        /// 移库单编码
        /// </summary>
        public string MoveCode { get; set; }

        /// <summary>
        /// New 新建,   Outing 出库中,   Over 结束
        /// </summary>
        public string State { get; set; }

        public string StateName { get; set; }

        public string AreaID { get; set; }

        public string AreaCode { get; set; }

        public string AreaCodeName { get; set; }

        /// <summary>
        /// 结束一组移库后，否自动产生下一组移库任务
        /// </summary>
        public string IsAuto { get; set; }

        /// <summary>
        /// ERP 接口,   MAN 手动
        /// </summary>
        public string GenType { get; set; }

        public string GenTypeName { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 任务单元数
        /// </summary>
        public int? TaskCellNum { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 是否已添加明细信息
        /// </summary>
        public string IsHaveDetail { get; set; }

        /// <summary>
        /// 执行人
        /// </summary>
        public string DoneUserID { get; set; }

        /// <summary>
        /// 执行人名称
        /// </summary>
        public string DoneUserName { get; set; }
        /// <summary>
        /// 是否逆向整理
        /// </summary>
        public string IsReverse { get; set; }
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
