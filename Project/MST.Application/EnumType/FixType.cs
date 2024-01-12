using MST.Application.SystemManage;
using MST.Domain.Entity.SystemManage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Application
{
    /// <summary>
    /// 固有基础数据
    /// </summary>
    public static class FixType
    {
        /// <summary>
        /// 扫码器
        /// </summary>
        public enum Scan
        {
            /// <summary>
            /// 入库扫码器
            /// </summary>
            [Description("入库扫码器")]
            ScanIn,

            /// <summary>
            /// 出库扫码器
            /// </summary>
            [Description("出库扫码器")]
            ScanOut
        }


        /// <summary>
        /// 站台
        /// </summary>

        public enum Station
        {
            /// <summary>
            /// 出库站台(正常)【与货位编码一致】
            /// </summary>
            [Description("出库站台(正常)")]
            StationOut_Normal,

            /// <summary>
            /// 出库站台(大件)【与货位编码一致】
            /// </summary>
            [Description("出库站台(大件)")]
            StationOut_BigItem,

            /// <summary>
            /// 入库站台(正常)【与货位编码一致】
            /// </summary>
            [Description("入库站台(正常)")]
            StationIn_Normal,

            /// <summary>
            /// 入库站台(大件)【与货位编码一致】
            /// </summary>
            [Description("入库站台(大件)")]
            StationIn_BigItem,

            /// <summary>
            /// 空料箱站台【无对应货位编码】
            /// </summary>
            [Description("空料箱站台")]
            StationEmpty
        }


        /// <summary>
        /// 容器类型（对应列表数据）
        /// </summary>
        public enum ContainerType
        {
            /// <summary>
            /// 标准料箱
            /// </summary>
            [Description("标准料箱")]
            StandardPlastic,

            /// <summary>
            /// 外部料架
            /// </summary>
            [Description("外部料架")]
            OuterRack,

            /// <summary>
            /// 内部料架
            /// </summary>
            [Description("内部料架")]
            InnerRack
        }

        public enum Item
        {
            /// <summary>
            /// 空料箱
            /// </summary>
            [Description("空料箱")]
            EmptyPlastic,

            /// <summary>
            /// 空料架
            /// </summary>
            [Description("空料架")]
            EmptyRack
        }

        public enum Area
        {
            /// <summary>
            /// 一般存储区
            /// </summary>
            [Description("一般存储区")]
            NormalArea,

            /// <summary>
            /// 二楼仓库区
            /// </summary>
            [Description("二楼仓库区")]
            SecondArea,

            /// <summary>
            /// 大件存储区	
            /// </summary>
            [Description("大件存储区")]
            BigItemArea,

            /// <summary>
            /// 空料箱暂存区
            /// </summary>
            [Description("空料箱暂存区")]
            EmptyArea,
            
            /// <summary>
            /// 站台存储区
            /// </summary>
            [Description("站台存储区")]
            StationLoc
        }

        public enum TimerCode
        {
            /// <summary>
            /// 过账
            /// </summary>
            [Description("过账")]
            Finance,

            /// <summary>
            /// 过期物料检测
            /// </summary>
            [Description("过期物料检测")]
            Overdue,

            /// <summary>
            /// 最低库存预警	
            /// </summary>
            [Description("最低库存预警")]
            ItemLower,

            /// <summary>
            /// 库存快照
            /// </summary>
            [Description("库存快照")]
            ContainerDetailPho,

            /// <summary>
            /// 数据备份	
            /// </summary>
            [Description("数据备份")]
            DBBak
        }

        /// <summary>
        /// 推送权限
        /// </summary>
        public enum WarringInfo
        {
            /// <summary>
            /// 库存预警
            /// </summary>
            [Description("库存预警")]
            NumWarringInfo,

            /// <summary>
            /// 过期预警
            /// </summary>
            [Description("过期预警")]
            OutExpWarringInfo
        }

        /// <summary>
        /// 推送UI点类型
        /// </summary>
        public enum WebUIPoint
        {
            /// <summary>
            /// 主页圆点消息
            /// </summary>
            [Description("主页圆点消息")]
            WebUIPoint=0,

            /// <summary>
            /// 3D看板消息
            /// </summary>
            [Description("3D看板消息")]
            Board3DUIPoint=1,

            /// <summary>
            /// 流道界面
            /// </summary>
            [Description("流道界面")]
            Runner=2
        }
    }
}
