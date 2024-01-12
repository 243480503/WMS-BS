using MST.Application.SystemManage;
using MST.Domain.Entity.SystemManage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Application
{
    public static class RuleConfig
    {
        /// <summary>
        /// 接口
        /// </summary>
        public static class Intface
        {
            public static class WCS
            {
                private static string _URL;
                public static string URL
                {
                    get
                    {
                        if (_URL == null)
                        {
                            _URL = new ItemsApp().GetDetailByPath("RuleConfig", "Intface", "WCS", "URL").F_Value;
                        }
                        return _URL;
                    }
                }
            }
            public static class ERP
            {
                private static string _URL;
                public static string URL
                {
                    get
                    {
                        if (_URL == null)
                        {
                            _URL = new ItemsApp().GetDetailByPath("RuleConfig", "Intface", "ERP", "URL").F_Value;
                        }
                        return _URL;
                    }
                }
            }
        }

        /// <summary>
        /// 出库规则
        /// </summary>
        public static class OutConfig
        {
            /// <summary>
            /// 出库扫子条码
            /// </summary>
            public static class RFScanCode
            {
                private static string _IsItemBarCodeSame;

                /// <summary>
                /// 子条码一致
                /// </summary>
                public static bool IsItemBarCodeSame
                {
                    get
                    {
                        if (_IsItemBarCodeSame == null)
                        {
                            _IsItemBarCodeSame = new ItemsApp().GetDetailByPath("RuleConfig", "OutConfig", "RFScanCode", "IsItemBarCodeSame").F_Value;
                        }
                        return _IsItemBarCodeSame == "Yes";
                    }
                }
            }
        }

        /// <summary>
        /// ERP接口规则
        /// </summary>
        public static class ERPInterfaceRule
        {
            /// <summary>
            /// 出库单
            /// </summary>
            public static class ERPInterfaceOutOrder
            {
                private static string _GetItemOutERPAutoOut;

                /// <summary>
                /// 领料自动出库
                /// </summary>
                public static bool GetItemOutERPAutoOut
                {
                    get
                    {
                        if (_GetItemOutERPAutoOut == null)
                        {
                            _GetItemOutERPAutoOut = new ItemsApp().GetDetailByPath("RuleConfig", "ERPInterfaceRule", "ERPInterfaceOutOrder", "GetItemOutERPAutoOut").F_Value;
                        }
                        return _GetItemOutERPAutoOut == "Yes";
                    }
                }

                private static string _VerBackOutERPAutoOut;

                /// <summary>
                /// 验退自动出库
                /// </summary>
                public static bool VerBackOutERPAutoOut
                {
                    get
                    {
                        if (_VerBackOutERPAutoOut == null)
                        {
                            _VerBackOutERPAutoOut = new ItemsApp().GetDetailByPath("RuleConfig", "ERPInterfaceRule", "ERPInterfaceOutOrder", "VerBackOutERPAutoOut").F_Value;
                        }
                        return _VerBackOutERPAutoOut == "Yes";
                    }
                }

                private static string _WarehouseBackOutERPAutoOut;

                /// <summary>
                /// 仓退自动出库
                /// </summary>
                public static bool WarehouseBackOutERPAutoOut
                {
                    get
                    {
                        if (_WarehouseBackOutERPAutoOut == null)
                        {
                            _WarehouseBackOutERPAutoOut = new ItemsApp().GetDetailByPath("RuleConfig", "ERPInterfaceRule", "ERPInterfaceOutOrder", "WarehouseBackOutERPAutoOut").F_Value;
                        }
                        return _WarehouseBackOutERPAutoOut == "Yes";
                    }
                }

                private static string _OtherOutERPAutoOut;

                /// <summary>
                /// 其它自动出库
                /// </summary>
                public static bool OtherOutERPAutoOut
                {
                    get
                    {
                        if (_OtherOutERPAutoOut == null)
                        {
                            _OtherOutERPAutoOut = new ItemsApp().GetDetailByPath("RuleConfig", "ERPInterfaceRule", "ERPInterfaceOutOrder", "OtherOutERPAutoOut").F_Value;
                        }
                        return _OtherOutERPAutoOut == "Yes";
                    }
                }
            }
        }

        /// <summary>
        /// 质检规则
        /// </summary>
        public static class QARule
        {
            /// <summary>
            /// 质检结果
            /// </summary>
            public static class QAResultRule
            {
                private static string _AutoUsedQAResult;

                /// <summary>
                /// 自动应用质检结果	
                /// </summary>
                public static bool AutoUsedQAResult
                {
                    get
                    {
                        if (_AutoUsedQAResult == null)
                        {
                            _AutoUsedQAResult = new ItemsApp().GetDetailByPath("RuleConfig", "QARule", "QAResultRule", "AutoUsedQAResult").F_Value;
                        }
                        return _AutoUsedQAResult == "Yes";
                    }
                }
            }
        }

        /// <summary>
        /// 盘点规则
        /// </summary>
        public static class CountRule
        {
            /// <summary>
            /// 质检结果
            /// </summary>
            public static class AuditResult
            {
                private static string _AutoUsedCountResult;

                /// <summary>
                /// 自动应用盘点结果
                /// </summary>
                public static bool AutoUsedCountResult
                {
                    get
                    {
                        if (_AutoUsedCountResult == null)
                        {
                            _AutoUsedCountResult = new ItemsApp().GetDetailByPath("RuleConfig", "CountRule", "AuditResult", "AutoUsedCountResult").F_Value;
                        }
                        return _AutoUsedCountResult == "Yes";
                    }
                }
            }
        }

        /// <summary>
        /// 移库规则
        /// </summary>
        public static class MoveRule
        {
            /// <summary>
            /// 任务规则
            /// </summary>
            public static class MoveTaskRule
            {
                private static int? _MaxTaskNum;

                /// <summary>
                /// 当前移库单在任务表中可产生的最大任务数量（避免移库暂停时AGV任务量太多造成暂停延迟）
                /// </summary>
                public static int? MaxTaskNum
                {
                    get
                    {
                        if (_MaxTaskNum == null)
                        {
                            _MaxTaskNum = Convert.ToInt32(new ItemsApp().GetDetailByPath("RuleConfig", "MoveRule", "MoveTaskRule", "MaxTaskNum").F_Value);
                        }
                        return _MaxTaskNum;
                    }
                }

            }

            /// <summary>
            /// 巷道规则
            /// </summary>
            public static class RowLineRule
            {
                private static string _IsSameLine;

                /// <summary>
                /// 是否同排移动[同巷道移动(IsSameRowLine)配置为Yes则该配置有效，否则不做判断]
                /// </summary>
                public static bool IsSameLine
                {
                    get
                    {
                        if (_IsSameLine == null)
                        {
                            _IsSameLine = new ItemsApp().GetDetailByPath("RuleConfig", "MoveRule", "RowLineRule", "IsSameLine").F_Value;
                        }
                        return _IsSameLine == "Yes";
                    }
                }

                private static string _IsSameRowLine;

                /// <summary>
                /// 是否同巷道
                /// </summary>
                public static bool IsSameRowLine
                {
                    get
                    {
                        if (_IsSameRowLine == null)
                        {
                            _IsSameRowLine = new ItemsApp().GetDetailByPath("RuleConfig", "MoveRule", "RowLineRule", "IsSameRowLine").F_Value;
                        }
                        return _IsSameRowLine == "Yes";
                    }
                }
            }
        }

        #region 单据过账规则
        public static class OrderTransRule
        {
            /// <summary>
            /// 入库单
            /// </summary>
            public static class InBoundTransRule
            {
                private static string _InBoundTrans;
                public static bool InBoundTrans
                {
                    get
                    {
                        if (_InBoundTrans == null) _InBoundTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "InBoundTransRule", "InBoundTrans").F_Value;
                        return _InBoundTrans == "Yes";
                    }
                }
            }

            /// <summary>
            /// 出库单
            /// </summary>
            public static class OutBoundTransRule
            {
                private static string _OtherOutTrans;
                private static string _GetItemOutTrans;
                private static string _VerBackOutTrans;
                private static string _WarehouseBackOutTrans;

                public static bool GetItemOutTrans
                {
                    get
                    {
                        if (_GetItemOutTrans == null) _GetItemOutTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "OutBoundTransRule", "GetItemOutTrans").F_Value;
                        return _GetItemOutTrans == "Yes";
                    }
                }
                public static bool VerBackOutTrans
                {
                    get
                    {
                        if (_VerBackOutTrans == null) _VerBackOutTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "OutBoundTransRule", "VerBackOutTrans").F_Value;
                        return _VerBackOutTrans == "Yes";
                    }
                }
                public static bool WarehouseBackOutTrans
                {
                    get
                    {
                        if (_WarehouseBackOutTrans == null) _WarehouseBackOutTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "OutBoundTransRule", "WarehouseBackOutTrans").F_Value;
                        return _WarehouseBackOutTrans == "Yes";
                    }
                }
                public static bool OtherOutTrans
                {
                    get
                    {
                        if (_OtherOutTrans == null) _OtherOutTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "OutBoundTransRule", "OtherOutTrans").F_Value;
                        return _OtherOutTrans == "Yes";
                    }
                }
            }

            /// <summary>
            /// 质检单
            /// </summary>
            public static class QATransRule
            {
                private static string _QAGetTrans;
                private static string _QABackTrans;
                public static bool QAGetTrans
                {
                    get
                    {
                        if (_QAGetTrans == null) _QAGetTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "QATransRule", "QAGetTrans").F_Value;
                        return _QAGetTrans == "Yes";
                    }
                }
                public static bool QABackTrans
                {
                    get
                    {
                        if (_QABackTrans == null) _QABackTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "QATransRule", "QABackTrans").F_Value;
                        return _QABackTrans == "Yes";
                    }
                }
            }

            /// <summary>
            /// 盘点单
            /// </summary>
            public static class CountTransRule
            {
                private static string _CountTrans;
                public static bool CountTrans
                {
                    get
                    {
                        if (_CountTrans == null) _CountTrans = new ItemsApp().GetDetailByPath("RuleConfig", "OrderTransRule", "CountTransRule", "CountTrans").F_Value;
                        return _CountTrans == "Yes";
                    }
                }
            }
        }
        #endregion
    }
}
