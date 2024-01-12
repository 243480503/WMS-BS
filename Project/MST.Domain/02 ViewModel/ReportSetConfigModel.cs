/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Domain.Entity.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Domain.ViewModel
{
    public class ReportSetConfigModel
    {
        public List<ReportOwner> ReportOwnerList { get; set; }
        public ReportOwner DefOwner { get; set; }
    }

    public class ReportOwner
    {
        /// <summary>
        /// 角色
        /// </summary>
        public RoleEntity Role { get; set; }

        /// <summary>
        /// 报表表头
        /// </summary>
        public IList<ReportHeadCol> ReportHeadColList { get; set; }

        /// <summary>
        /// 报表过滤表单控件
        /// </summary>
        public IList<ReportInPut> ReportInPutList { get; set; }


        /// <summary>
        /// 脚本
        /// </summary>
        public ReportScript ReportScript { get; set; }
    }



    /// <summary>
    /// 报表过滤表单控件
    /// </summary>
    public class ReportInPut
    {
        public string FieldCode { get; set; }
        public string DataType { get; set; }
        public string ReName { get; set; }
        public int? Sort { get; set; }

        public bool IsLike { get; set; }

        public string DefVal { get; set; }

        /// <summary>
        /// 数字、字符串、下拉框
        /// </summary>
        public string InPutType { get; set; }

        /// <summary>
        /// 是否为值段
        /// </summary>
        public bool IsBetween { get; set; }

        public string Text { get; set; }

        IList<Dictionary<object, object>> DropDownList { get; set; }
    }


    public class ReportRow
    {

    }

    public class ReportHeadCol
    {
        public string FieldCode { get; set; }
        public string ReName { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public string IsShow { get; set; }
        /// <summary>
        /// 是否允许导出
        /// </summary>
        public string IsCanExport { get; set; }

        public int? ColWidth { get; set; }
        public string SortType { get; set; }

        public int? SortIndex { get; set; }

        public string Where { get; set; }

        public string IsLike { get; set; }

        public string WhereValueType { get; set; }
        public string DataType { get; set; }
        public int? Sort { get; set; }

        /// <summary>
        /// 是否为查询控件
        /// </summary>
        public string IsFilterInput { get; set; }
    }

    public class ReportScript
    {
        public string ScriptText { get; set; }
    }
}
