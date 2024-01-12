/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.APIPost;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Application.WebMsg;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using MST.Web.Areas.PC_BoardManage.Controllers;
using MST.Web.Areas.RF_EmptyContainerManage.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static MST.Application.WMSLogic.T_TaskApp;

namespace MST.Web.WebAPI.EBoard.Controllers
{
    [HandlerLogin(false)]
    public class EBoardController : Controller
    {
        #region 请求主方法
        [HttpPost]
        public string Ask()
        {
            string returnStr = "";
            bool logDB = true;

            UserEntity user = new UserApp().GetEntity("EBoard");
            OperatorModel operatorModel = new OperatorModel();
            operatorModel.UserCode = user.F_Account;
            operatorModel.UserId = user.F_Id;
            operatorModel.UserName = user.F_RealName;
            OperatorProvider.Provider.AddCurrent(operatorModel);

            StreamReader sRead = new StreamReader(HttpContext.Request.InputStream);
            string data = sRead.ReadToEnd();
            sRead.Close();


            LogObj logObj = new LogObj();
            logObj.Path = "EBoardController.Ask"; //按实际情况修改
            logObj.Parms = data; //按实际情况修改

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "EBoard请求Ask接口"; //按实际情况修改
            logEntity.F_Type = DbLogType.Visit.ToString(); //按实际情况修改
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = data;

            EBoardResult result = new EBoardResult();
            try
            {
                /*************************************************/

                EBoardModel askModel = data.ToObject<EBoardModel>();

                switch (askModel.Func)
                {
                    case "GetAreaLocInfo":  //获取所有货位及货位库存信息
                        {
                            GetAreaLocInfoModel getAreaLocInfoModel = askModel.Param.ToObject<GetAreaLocInfoModel>();
                            result = GetAreaLocInfo(getAreaLocInfoModel);
                        }
                        break;
                    case "GetConDetailByLocCode": //根据货位编码，获取库存信息
                        {
                            GetConDetailByLocCodeModel getConDetailByLocCodeModel = askModel.Param.ToObject<GetConDetailByLocCodeModel>();
                            result = GetConDetailByLocCode(getConDetailByLocCodeModel);
                        }
                        break;
                    case "GetAllTaskInfo": //获取所有任务信息
                        {
                            result = GetAllTaskInfo();
                        }
                        break;
                    case "GetCurTaskInfo": //获取指定容器编码的任务信息
                        {
                            GetCurTaskInfoModel model = askModel.Param.ToObject<GetCurTaskInfoModel>();
                            result = GetCurTaskInfo(model);
                        }
                        break;
                    case "CurContainerQtyData": //当前库存统计
                        {
                            result = GetStatisticsInfo("CurContainerQtyData");
                        }
                        break;
                    case "CurDayInOut": //每日出入库数量
                        {
                            result = GetStatisticsInfo("CurDayInOut");
                        }
                        break;
                    case "CurDayWorkData": //当日作业汇总
                        {
                            result = GetStatisticsInfo("CurDayWorkData");
                        }
                        break;
                    case "CurInOutCountData": //累计出入库数量
                        {
                            result = GetStatisticsInfo("CurInOutCountData");
                        }
                        break;
                    case "CurItemChangeData": //物料库存变化
                        {
                            result = GetStatisticsInfo("CurItemChangeData");
                        }
                        break;
                    case "CurLocPerData": //货位占用率
                        {
                            result = GetStatisticsInfo("CurLocPerData");
                        }
                        break;
                    case "CurOrderPerData": //单据进度
                        {
                            result = GetStatisticsInfo("CurOrderPerData");
                        }
                        break;
                    default:
                        {
                            throw new Exception("未知的方法类型");
                        };
                }

                /**************************************************/
                if (result.IsSuccess)
                {
                    logObj.Message = "操作成功";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    if (logDB)
                    {
                        logEntity.F_Result = true;
                        logEntity.F_Msg = result.ToJson();
                        logEntity.F_Description = askModel.Func;
                        new LogApp().WriteDbLog(logEntity);
                    }
                }
                else
                {
                    logObj.Message = "操作失败";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    if (logDB)
                    {
                        logEntity.F_Result = false;
                        logEntity.F_Msg = result.ToJson();
                        logEntity.F_Description = askModel.Func;
                        new LogApp().WriteDbLog(logEntity);
                    }
                }

                returnStr = result.ToJson();
                return returnStr;
            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                if (logDB)
                {
                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                }

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return returnStr;
            }
        }
        #endregion

        #region 获取指定区域所有货位及货位库存信息
        public class GetAreaLocInfoModel
        {
            public string AreaCode { get; set; }
        }

        public EBoardResult GetAreaLocInfo(GetAreaLocInfoModel model)
        {
            using (IRepositoryBase db = new RepositoryBase().BeginTrans())
            {
                EBoardResult result = new EBoardResult();

                if (string.IsNullOrEmpty(model.AreaCode))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "区域编码不可为空";
                    return result;
                }

                var data = db.IQueryable<T_LocationEntity>(o => o.AreaCode == model.AreaCode).GroupJoin(db.IQueryable<T_ContainerEntity>(o => true), loc => loc.F_Id, con => con.LocationID,
                   (loc, con) => new { loc = loc, con = con }).SelectMany(temp => temp.con.DefaultIfEmpty(), (temp, grp)
                    => new
                    {
                        LocationID = temp.loc.F_Id,
                        AreaID = temp.loc.AreaID,
                        AreaCode = temp.loc.AreaCode,
                        AreaName = temp.loc.AreaName,
                        AreaType = temp.loc.AreaType,
                        IsAreaVir = temp.loc.IsAreaVir,
                        LocationCode = temp.loc.LocationCode,
                        LocationType = temp.loc.LocationType,
                        Line = temp.loc.Line,
                        ColNum = temp.loc.ColNum,
                        Layer = temp.loc.Layer,
                        Deep = temp.loc.Deep,
                        High = temp.loc.High,
                        Width = temp.loc.Width,
                        Long = temp.loc.Long,
                        Weight = temp.loc.Weight,
                        LocationState = temp.loc.State,
                        ForbiddenState = temp.loc.ForbiddenState,

                        ContainerID = grp == null ? null : grp.F_Id,
                        BarCode = grp == null ? null : grp.BarCode,
                        ContainerType = grp == null ? null : grp.ContainerType,
                        ContainerKind = grp == null ? null : grp.ContainerKind,
                        IsContainerVir = grp == null ? null : grp.IsContainerVir,
                        ContainerSpec = grp == null ? null : grp.ContainerSpec
                    })

                   .GroupJoin(db.IQueryable<T_ContainerDetailEntity>(o => true), loc => loc.ContainerID, det => det.ContainerID,
                   (loc, det) => new { loc = loc, det = det }).SelectMany(temp => temp.det.DefaultIfEmpty(), (temp, grp)
                             => new
                             {
                                 LocationID = temp.loc.LocationID,
                                 AreaID = temp.loc.AreaID,
                                 AreaCode = temp.loc.AreaCode,
                                 AreaName = temp.loc.AreaName,
                                 AreaType = temp.loc.AreaType,
                                 IsAreaVir = temp.loc.IsAreaVir,
                                 LocationCode = temp.loc.LocationCode,
                                 LocationType = temp.loc.LocationType,
                                 Line = temp.loc.Line,
                                 ColNum = temp.loc.ColNum,
                                 Layer = temp.loc.Layer,
                                 Deep = temp.loc.Deep,
                                 High = temp.loc.High,
                                 Width = temp.loc.Width,
                                 Weight = temp.loc.Weight,
                                 LocationState = temp.loc.LocationState,
                                 ForbiddenState = temp.loc.ForbiddenState,

                                 ContainerID = temp.loc.ContainerID,
                                 BarCode = temp.loc.BarCode,
                                 ContainerType = temp.loc.ContainerType,
                                 ContainerKind = temp.loc.ContainerKind,
                                 IsContainerVir = temp.loc.IsContainerVir,
                                 ContainerSpec = temp.loc.ContainerSpec,

                                 ContainerDetailID = grp == null ? null : grp.F_Id,
                                 ItemID = grp == null ? null : grp.ItemID,
                                 KindCode = grp == null ? null : grp.KindCode,
                                 KindName = grp == null ? null : grp.KindName,
                                 ItemCode = grp == null ? null : grp.ItemCode,
                                 ItemName = grp == null ? null : grp.ItemName,
                                 ItemBarCode = grp == null ? null : grp.ItemBarCode,
                                 Factory = grp == null ? null : grp.Factory,
                                 Qty = grp == null ? null : grp.Qty,
                                 OutQty = grp == null ? null : grp.OutQty,
                                 CheckQty = grp == null ? null : grp.CheckQty,
                                 ItemUnitText = grp == null ? null : grp.ItemUnitText,
                                 CheckState = grp == null ? null : grp.CheckState,
                                 CheckID = grp == null ? null : grp.CheckID,
                                 IsItemMark = grp == null ? null : grp.IsItemMark,
                                 IsCheckFreeze = grp == null ? null : grp.IsCheckFreeze,
                                 IsCountFreeze = grp == null ? null : grp.IsCountFreeze,
                                 Lot = grp == null ? null : grp.Lot,
                                 ERPWarehouseCode = grp == null ? null : grp.ERPWarehouseCode,
                                 ValidityDayNum = grp == null ? null : grp.ValidityDayNum,
                                 ProductDate = grp == null ? null : grp.ProductDate,
                                 OverdueDate = grp == null ? null : grp.OverdueDate,
                                 SupplierID = grp == null ? null : grp.SupplierID,
                                 SupplierCode = grp == null ? null : grp.SupplierCode,
                                 SupplierName = grp == null ? null : grp.SupplierName,
                                 ReceiveRecordID = grp == null ? null : grp.ReceiveRecordID,
                                 IsVirItemBarCode = grp == null ? null : grp.IsVirItemBarCode,
                                 InBoundID = grp == null ? null : grp.InBoundID,
                                 RefInBoundCode = grp == null ? null : grp.RefInBoundCode,
                                 InBoundDetailID = grp == null ? null : grp.InBoundDetailID,
                                 SEQ = grp == null ? null : grp.SEQ,
                                 ContainerDetailState = grp == null ? null : grp.State,



                             })

                   .GroupBy(o => new
                   {
                       o.LocationID,
                       o.AreaID,
                       o.AreaCode,
                       o.AreaName,
                       o.AreaType,
                       o.IsAreaVir,
                       o.LocationCode,
                       o.LocationType,
                       o.Line,
                       o.ColNum,
                       o.Layer,
                       o.Deep,
                       o.High,
                       o.Width,
                       o.Weight,
                       o.LocationState,
                       o.ForbiddenState
                   })
                   .Select(m => new
                   {
                       m.Key.LocationID,
                       m.Key.AreaID,
                       m.Key.AreaCode,
                       m.Key.AreaName,
                       m.Key.AreaType,
                       m.Key.IsAreaVir,
                       m.Key.LocationCode,
                       m.Key.LocationType,
                       m.Key.Line,
                       m.Key.ColNum,
                       m.Key.Layer,
                       m.Key.Deep,
                       m.Key.High,
                       m.Key.Width,
                       m.Key.Weight,
                       m.Key.LocationState,
                       m.Key.ForbiddenState,

                       Container = m.GroupBy(o => new
                       {
                           o.ContainerID,
                           o.BarCode,
                           o.ContainerType,
                           o.ContainerKind,
                           o.IsContainerVir,
                           o.ContainerSpec,

                           o.ContainerDetailID,
                           o.ItemID,
                           o.KindCode,
                           o.KindName,
                           o.ItemCode,
                           o.ItemName,
                           o.ItemBarCode,
                           o.Factory,
                           o.Qty,
                           o.OutQty,
                           o.CheckQty,
                           o.ItemUnitText,
                           o.CheckState,
                           o.CheckID,
                           o.IsItemMark,
                           o.IsCheckFreeze,
                           o.IsCountFreeze,
                           o.Lot,
                           o.ERPWarehouseCode,
                           o.ValidityDayNum,
                           o.ProductDate,
                           o.OverdueDate,
                           o.SupplierID,
                           o.SupplierCode,
                           o.SupplierName,
                           o.ReceiveRecordID,
                           o.IsVirItemBarCode,
                           o.InBoundID,
                           o.RefInBoundCode,
                           o.InBoundDetailID,
                           o.SEQ,
                           o.ContainerDetailState
                       }).Select(j => new
                       {

                           ContainerID =j.Key.ContainerID,
                           BarCode = j.Key.BarCode,
                           ContainerType =j.Key.ContainerType,
                           ContainerKind = j.Key.ContainerKind,
                           IsContainerVir = j.Key.IsContainerVir,
                           ContainerSpec = j.Key.ContainerSpec,
                           ContainerDetailList =j.Select(o=>new {
                               o.ContainerDetailID,
                               o.ItemID,
                               o.KindCode,
                               o.KindName,
                               o.ItemCode,
                               o.ItemName,
                               o.ItemBarCode,
                               o.Factory,
                               o.Qty,
                               o.OutQty,
                               o.CheckQty,
                               o.ItemUnitText,
                               o.CheckState,
                               o.CheckID,
                               o.IsItemMark,
                               o.IsCheckFreeze,
                               o.IsCountFreeze,
                               o.Lot,
                               o.ERPWarehouseCode,
                               o.ValidityDayNum,
                               o.ProductDate,
                               o.OverdueDate,
                               o.SupplierID,
                               o.SupplierCode,
                               o.SupplierName,
                               o.ReceiveRecordID,
                               o.IsVirItemBarCode,
                               o.InBoundID,
                               o.RefInBoundCode,
                               o.InBoundDetailID,
                               o.SEQ,
                               o.ContainerDetailState
                           }).ToList()
                       }).Where(o=>o.ContainerID!=null).ToList().FirstOrDefault()
                   }).OrderBy(o => o.AreaCode).ToList();

                result.IsSuccess = true;
                result.FailCode = "0000";
                result.Data = data;
                return result;
            }
        }
        #endregion

        #region 获取指定货位的容器与容器库存信息
        public class GetConDetailByLocCodeModel
        {
            public string LocCode { get; set; }
        }
        public EBoardResult GetConDetailByLocCode(GetConDetailByLocCodeModel model)
        {
            using (IRepositoryBase db = new RepositoryBase().BeginTrans())
            {
                EBoardResult result = new EBoardResult();

                if (string.IsNullOrEmpty(model.LocCode))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "货位编码不可为空";
                    return result;
                }


                var data = db.IQueryable<T_ContainerEntity>(o => o.LocationNo == model.LocCode && o.F_DeleteMark == false)
                     .GroupJoin(db.IQueryable<T_ContainerDetailEntity>(o => true), con => con.F_Id, detail => detail.ContainerID, (con, detail) => new { con, detail })
                     .SelectMany(temp => temp.detail.DefaultIfEmpty(), (temp, grp) => new
                     {
                         AreaCode = temp.con.AreaCode,
                         AreaID = temp.con.AreaID,
                         AreaName = temp.con.AreaName,
                         BarCode = temp.con.BarCode,
                         ContainerKind = temp.con.ContainerKind,
                         ContainerSpec = temp.con.ContainerSpec,
                         ContainerType = temp.con.ContainerType,
                         IsContainerVir = temp.con.IsContainerVir,
                         LocationID = temp.con.LocationID,
                         LocationNo = temp.con.LocationNo,
                         ContainerDetail = temp.detail
                     }).ToList();

                result.IsSuccess = true;
                result.FailCode = "0000";
                result.Data = data;
                return result;
            }
        }
        #endregion

        #region 获取所有任务信息

        public EBoardResult GetAllTaskInfo()
        {
            using (IRepositoryBase db = new RepositoryBase().BeginTrans())
            {
                EBoardResult result = new EBoardResult();
                IList<T_TaskEntity> TaskList = new T_TaskApp().FindList(o => true).ToList();

                result.IsSuccess = true;
                result.FailCode = "0000";
                result.Data = TaskList;
                return result;
            }
        }
        #endregion

        #region 获取指定容器编码的任务信息
        public class GetCurTaskInfoModel
        {
            public string BarCode { get; set; }
        }

        public EBoardResult GetCurTaskInfo(GetCurTaskInfoModel model)
        {
            using (IRepositoryBase db = new RepositoryBase().BeginTrans())
            {
                EBoardResult result = new EBoardResult();

                if (string.IsNullOrEmpty(model.BarCode))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "容器编码不可为空";
                    return result;
                }

                IList<T_TaskEntity> TaskList = new T_TaskApp().FindList(o => o.BarCode == model.BarCode).ToList();

                result.IsSuccess = true;
                result.FailCode = "0000";
                result.Data = TaskList;
                return result;
            }
        }
        #endregion

        #region
        /// <summary>
        /// 获取统计信息
        /// </summary>
        /// <returns></returns>
        public EBoardResult GetStatisticsInfo(string type)
        {
            EBoardResult result = new BoardController().GetStatisticsInfo(type);
            return result;
        }
        #endregion

    }
}
