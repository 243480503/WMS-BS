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
    public class QAModel
    {
        /// <summary>
        /// 质检单ID
        /// </summary>
        public string F_Id { get; set; }
        /// <summary>
        /// 质检单编码
        /// </summary>
		public string QACode { get; set; }
        /// <summary>
        /// 质检单类型
        /// GetSample 抽样出库
        /// BackSample 还样入库
        /// </summary>
        public string QAOrderType { get; set; }
        public string QAOrderTypeName { get; set; }
        /// <summary>
        /// 单据状态
        /// New 新建
        /// Picking    取样中
        /// Picked     已取样
        /// Returning 还样中
        /// Over 结束
        /// </summary>
        public string State { get; set; }
        public string StateName { get; set; }
        /// <summary>
        /// 站台ID
        /// </summary>
        public string StationID { get; set; }
        /// <summary>
        /// 过账状态
        /// </summary>
		public string TransState { get; set; }
        /// <summary>
        /// 来源单号
        /// </summary>
		public string RefOrderCode { get; set; }
        /// <summary>
        /// 对应的来源入库单
        /// </summary>
        public string RefInBoundCode { get; set; }
        /// <summary>
        /// 生成方式
        /// ERP  接口
        /// MAN 手动
        /// Excel 表格导入
        /// </summary>
        public string GenType { get; set; }
        public string GenTypeName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
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
