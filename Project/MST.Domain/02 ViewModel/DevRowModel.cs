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
	public class DevRowModel
	{

        /// <summary>
        /// 设备行ID
        /// </summary>
        public string F_Id { get; set; }

        /// <summary>
        /// 所属区域ID
        /// </summary>
        public string AreaID { get; set; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string EquID { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string EquName { get; set; }

        /// <summary>
        /// 巷道编码
        /// </summary>
        public string WayCode { get; set; }

        /// <summary>
        /// 是否巷道内均分
        /// </summary>
        public string IsEvenInnerTunnel { get; set; }

        /// <summary>
        /// 是否行倒序
        /// </summary>
        public string IsLineDesc { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int? Num { get; set; }

        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }


        /// <summary>
        /// 是否游标
        /// </summary>
        public string IsCursor { get; set; }
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
