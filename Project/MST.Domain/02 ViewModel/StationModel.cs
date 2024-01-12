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
	public class StationModel
	{

        /// <summary>
        /// 站台ID
        /// </summary>
        public string F_Id { get; set; }

        /// <summary>
        /// 站台编码
        /// </summary>
        public string StationCode { get; set; }

        /// <summary>
        /// 站台名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 当前单据ID
        /// </summary>
        public string CurOrderID { get; set; }

        /// <summary>
        /// 当前单据编码
        /// </summary>
        public string CurOrderCode { get; set; }

        /// <summary>
        /// 当前单据类型
        /// </summary>
        public string OrderType { get; set; }

        public string CurOrderDetailID { get; set; }

        public string CurModel { get; set; }

        public string CurModelName { get; set; }

        /// <summary>
        /// 当前单据类型名称
        /// </summary>
        public string CurOrderTypeName { get; set; }

        /// <summary>
        /// 用途
        /// </summary>
        public string UseCode { get; set; }

        /// <summary>
        /// 用途名称列表
        /// </summary>
        public IList<T_ItemEntity> UseCodeList { get; set; }

        /// <summary>
        /// 用途名称
        /// </summary>
        public string UseCodeName { get; set; }

        public int? SEQ { get; set; }

        public string WaveCode { get; set; }


        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 到达地址
        /// </summary>
        public string TagAddress { get; set; }

        /// <summary>
        /// 离开地址
        /// </summary>
        public string LeaveAddress { get; set; }

        /// <summary>
        /// 是否故障
        /// </summary>
        public string IsErr { get; set; }

        public string WaveID { get; set; }

        public string BarCode { get; set; }

        /// <summary>
        /// 故障消息
        /// </summary>
        public string ErrMsg { get; set; }

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
