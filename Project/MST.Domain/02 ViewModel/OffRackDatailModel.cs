using System;

namespace MST.Domain.ViewModel
{
    /// <summary>
    /// 下架明细表
    /// </summary>
    public class OffRackDetailModel
    {
		/// <summary>
		/// 下架明细ID
		/// </summary>
		public string F_Id { get; set; }
		/// <summary>
		/// 下架单ID
		/// </summary>
		public string OffRackID { get; set; }
		/// <summary>
		/// 下架单来源单号
		/// </summary>
		public string RefOrderCode { get; set; }
		/// <summary>
		/// 项次
		/// </summary>
		public int SEQ { get; set; }
		/// <summary>
		/// 下架货位ID
		/// </summary>
		public string LocationID { get; set; }
		/// <summary>
		/// 下架货位编码
		/// </summary>
		public string LocationCode { get; set; }
		public string BarCode { get; set; }
		public string ContainerKind { get; set; }
		public string ContainerKindName { get; set; }

		/// <summary>
		/// 下架站台
		/// </summary>
		public string StationID { get; set; }
		/// <summary>
		/// 下架站台编码
		/// </summary>
		public string StationCode { get; set; }
		/// <summary>
		/// 下架站台名称
		/// </summary>
		public string StationName { get; set; }
		/// <summary>
		/// 下架明细状态	New 新建,	OffRacking 正在下架,	Over 结束
		/// </summary>
		public string State { get; set; }
		public string StateName { get; set; }
		/// <summary>
		/// 区域ID
		/// </summary>
		public string AreaID { get; set; }
		/// <summary>
		/// 区域编码
		/// </summary>
		public string AreaCode { get; set; }
		/// <summary>
		/// 区域名称
		/// </summary>
		public string AreaName { get; set; }
		/// <summary>
		/// 区域类型
		/// </summary>
		public string AreaType { get; set; }
		public int MustTimes { get; set; }
		public int NoOffRackTimes { get; set; }
		public string ReadyConfirmAndOrderNeed { get; set; }
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
