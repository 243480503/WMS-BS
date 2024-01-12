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
    public class GenLocationModel
    {
        public int BeginLine { get; set; }
        public int EndLine { get; set; }
        public int BeginColNum { get; set; }
        public int EndColNum { get; set; }
        public int BeginLayer { get; set; }
        public int EndLayer { get; set; }

        public decimal? High { get; set; }
        public decimal? Width { get; set; }
        /// <summary>
        /// 防潮
        /// </summary>
        public string IsDampproof { get; set; }

        /// <summary>
        /// 靠窗
        /// </summary>
        public string IsItemPriority { get; set; }

        /// <summary>
        /// 顶层
        /// </summary>
        public string IsLocTop { get; set; }
        public string State { get; set; }
        public string ForbiddenState { get; set; }
        public string AreaID { get; set; }
        public string AreaName { get; set; }
        public string AreaCode { get; set; }
        public string AreaType { get; set; }

        public string LocationType { get; set; }

        public string LocationCode { get; set; }

        public string IsAreaVir { get; set; }

        /// <summary>
        /// 是否基础数据
        /// </summary>
        public string IsBase { get; set; }
    }

}
