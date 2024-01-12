/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

namespace MST.Code
{
    public class AjaxResult
    {
        public object state { get; set; }   /// 操作结果类型
        public string message { get; set; } /// 获取 消息内容
        public object data { get; set; }    /// 获取 返回数据
    }



    /// <summary>
    /// 表示 ajax 操作结果类型的枚举
    /// </summary>
    public enum ResultType
    {
        info,   /// 消息结果类型
        success,    /// 成功结果类型
        warning,     /// 警告结果类型
        error   /// 异常结果类型
    }


    /// <summary>
    /// WMS给WCS的返回值
    /// </summary>
    public class WCSResult
    {
        /// 状态 true 成功，false 失败
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 失败原因分类：0001 普通失败,0002 系统异常,0003 签名异常
        /// </summary>
        public string FailCode { get; set; }   
        public string FailMsg { get; set; } /// 失败时的错误消息
        public object Data { get; set; }    /// 成功时返回的数据
    }


    /// <summary>
    /// WMS给ERP的返回值
    /// </summary>
    public class ERPResult
    {
        public bool IsSuccess { get; set; } /// 状态 true 成功，false 失败
        public string FailCode { get; set; }    /// 失败原因分类：0001 普通失败,0002 系统异常,0003 签名异常
        public string FailMsg { get; set; } /// 失败时的错误消息
        public object Data { get; set; }    /// 成功时返回的数据
    }

    /// <summary>
    /// WMS给RF离线的返回值
    /// </summary>
    public class RFOffLineResult
    {
        public bool IsSuccess { get; set; } /// 状态 true 成功，false 失败
        public string FailCode { get; set; }    /// 失败原因分类：0001 普通失败,0002 系统异常,0003 签名异常
        public string FailMsg { get; set; } /// 失败时的错误消息
        public object Data { get; set; }    /// 成功时返回的数据
    }

    /// <summary>
    /// WMS给EBoard的返回值
    /// </summary>
    public class EBoardResult
    {
        public bool IsSuccess { get; set; } /// 状态 true 成功，false 失败
        public string FailCode { get; set; }    /// 失败原因分类：0001 普通失败,0002 系统异常,0003 签名异常
        public string FailMsg { get; set; } /// 失败时的错误消息
        public object Data { get; set; }    /// 成功时返回的数据
    }
}
