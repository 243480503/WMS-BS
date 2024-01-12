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
	public class RowLineModel
	{

        /// <summary>
        /// 巷道行ID
        /// </summary>
        public string F_Id { get; set; }

        /// <summary>
        /// 巷道ID
        /// </summary>
        public string DevRowID { get; set; }

        /// <summary>
        /// 行
        /// </summary>
        public int? Line { get; set; }

        /// <summary>
        /// 设置连续货位分配数量
        /// </summary>
        public int? ContinuityCount_In { get; set; }

        /// <summary>
        /// 已连续货位分配数量（切换到其它行时,清空原游标行该值）
        /// </summary>
        public int? CurOverCount_In { get; set; }

        /// <summary>
        /// 是否列倒序
        /// </summary>
        public string IsColumnDesc_In { get; set; }

        /// <summary>
        /// 是否层倒序
        /// </summary>
        public string IsLayerDesc_In { get; set; }

        /// <summary>
        /// 高低层分界(低层最高层)，0为不分层
        /// </summary>
        public int? SplitLowLayer_In { get; set; }

        /// <summary>
        /// 是否低层优先(SplitLowLayer不为0则有效)
        /// </summary>
        public string IsLowPriority_In { get; set; }

        /// <summary>
        /// 是否游标
        /// </summary>
        public string IsCursor { get; set; }

        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public string IsEnable { get; set; }

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
