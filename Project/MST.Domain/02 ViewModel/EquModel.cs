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
    public class EquModel
    {

        /// <summary>
        /// 设备ID
        /// </summary>
        public string F_Id { get; set; }

        /// <summary>
        /// 设备编码
        /// </summary>
        public string EquCode { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string EquName { get; set; }

        /// <summary>
        /// 所属区域ID
        /// </summary>
        public string BelongAreaID { get; set; }

        /// <summary>
        /// 所属区域名称
        /// </summary>
        public string BelongAreaName { get; set; }

        /// <summary>
        /// 站台,   堆垛机,   AGV
        /// </summary>
        public string EquType { get; set; }

        public string CurTaskNo { get; set; }

        public string IsHaveContainer { get; set; }
        public string ContainerType { get; set; }

        public string CurBarCode { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string EquTypeName { get; set; }

        /// <summary>
        /// 设备行
        /// </summary>
        public IList<T_DevRowEntity> equDevRow { get; set; }

        /// <summary>
        /// 所有行
        /// </summary>
        public IList<int> allRow { get; set; }

        public string WCSCode { get; set; }

        public string WCSData { get; set; }

        /// <summary>
        /// WCS旧数据
        /// </summary>
        public string OldWCSData { get; set; }


        public DateTime? WCSRecTime { get; set; }

        public int? Sort { get; set; }

        public string GroupCode { get; set; }

        public decimal? XPoint { get; set; }

        public decimal? YPoint { get; set; }

        /// <summary>
        /// 设备对应关系 流道对应的离开光电和进入光电
        /// </summary>
        public string Relation { get; set; }

        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }

        /// <summary>
        /// 设备是否需要行信息(堆垛机、穿梭车)
        /// </summary>
        public bool IsNeedRow { get; set; }

        /// <summary>
        /// 用途备注
        /// </summary>
        public string UseDes { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// 设备状态名称
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// 故障消息
        /// </summary>
        public string ErrMsg { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public string IsEnable { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 呈现界面时的排序顺序
        /// </summary>
        public int? ShowOrder { get; set; }

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
