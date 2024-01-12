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
    public class ThreeModel
	{
		/// <summary>
		/// 所有任务
		/// </summary>
	    public IList<T_TaskEntity> TaskList { get; set; }

		/// <summary>
		/// 所有货位
		/// </summary>
		public IList<T_LocationEntity> LocationList { get; set; }

		/// <summary>
		/// 所有容器
		/// </summary>
		public IList<T_ContainerEntity> ContainerList { get; set; }

		/// <summary>
		/// 所有库存
		/// </summary>
		public IList<T_ContainerDetailEntity> ContainerDetailList { get; set; }

		/// <summary>
		/// 所有设备
		/// </summary>
		public IList<T_EquEntity> EquList { get; set; }
	}
}
