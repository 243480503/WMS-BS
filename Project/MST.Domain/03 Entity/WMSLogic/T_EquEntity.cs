/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using System;

namespace MST.Domain.Entity.WMSLogic
{
    /// <summary>
    /// 设备表
    /// </summary>
    public class T_EquEntity : IEntity<T_EquEntity>, ICreationAudited, IModificationAudited, IDeleteAudited, IWMSEntity
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
        /// 堆垛机,   AGV
        /// </summary>
        public string EquType { get; set; }
        /// <summary>
		/// 设备状态
		/// </summary>
		public string State { get; set; }
        /// <summary>
        /// 流道排序
        /// </summary>
        public int? Sort { get; set; }
        /// <summary>
        /// 当前任务编码
        /// </summary>
        public string CurTaskNo { get; set; }
        /// <summary>
        /// 流道上当前的容器编码(入库扫码前为空)
        /// </summary>
        public string CurBarCode { get; set; }
        /// <summary>
        /// 流道是否存在容器
        /// </summary>
        public string IsHaveContainer { get; set; }
        /// <summary>
        /// 容器类型
        /// </summary>
        public string ContainerType { get; set; }
        /// <summary>
        /// X坐标点
        /// </summary>
        public decimal? XPoint { get; set; }
        /// <summary>
        /// Y坐标点
        /// </summary>
        public decimal? YPoint { get; set; }
        /// <summary>
        /// 对应的WCS编码
        /// </summary>
        public string WCSCode { get;set;}
        /// <summary>
        /// WCS实时数据
        /// </summary>
        public string WCSData { get; set; }
        /// <summary>
        /// WCS旧数据
        /// </summary>
        public string OldWCSData { get; set; }
        /// <summary>
        /// 设备对应关系 “流道位置(First,Middle,Last)|扫码器|照相机|按钮|顶升机|顶升皮带|电机|光电|正转时进入（光电1&光电2$ZW）,正转时离开（光电1&光电2$WZ）,反转时进入,反转时离开”  流道对应的离开光电和进入光电
        /// 光电1&光电2$ZW 表示 当电机正转时，判断进入流道的方式，是 光电1和光电2 ，由遮挡变为无遮挡。
        /// </summary>
        public string Relation { get; set; }
        /// <summary>
        /// 实时数据接收时间
        /// </summary>
        public DateTime? WCSRecTime { get; set; }
        /// <summary>
        /// 故障消息
        /// </summary>
        public string ErrMsg { get; set; }
        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
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
        /// 流道分组（如出库流道组，入库流道组）
        /// </summary>
        public string GroupCode { get; set; }

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
