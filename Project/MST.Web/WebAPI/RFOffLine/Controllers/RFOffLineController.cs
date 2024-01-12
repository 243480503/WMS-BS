/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.WebAPI.RFOffLine.Controllers
{
    [HandlerLogin(false)]
    public class RFOffLineController : Controller
    {
        #region 请求主方法
        [HttpPost]
        [CrossSite]
        public string Ask()
        {
            ControllerContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            string returnStr = "";

            UserEntity user = new UserApp().GetEntity("Sys");
            OperatorModel operatorModel = new OperatorModel();
            operatorModel.UserCode = user.F_Account;
            operatorModel.UserId = user.F_Id;
            operatorModel.UserName = user.F_RealName;

            StreamReader sRead = new StreamReader(HttpContext.Request.InputStream);
            string data = sRead.ReadToEnd();
            sRead.Close();


            LogObj logObj = new LogObj();
            logObj.Path = "RFOffLineController.Ask"; //按实际情况修改
            logObj.Parms = new { data = data }; //按实际情况修改

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "RF离线请求Ask接口"; //按实际情况修改
            logEntity.F_Type = DbLogType.Visit.ToString(); //按实际情况修改
            logEntity.F_Account = operatorModel.UserCode;
            logEntity.F_NickName = operatorModel.UserName;
            logEntity.F_Description = "RF离线出库请求接口"; //按实际情况修改
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            RFOffLineResult result = new RFOffLineResult();
            try
            {
                /*************************************************/



                RFOffLineModel askModel = data.ToObject<RFOffLineModel>();

                switch (askModel.Method)
                {
                    case "IsHostOnLine":  //判断服务器是否在线
                        {
                            result.IsSuccess = true;
                            logObj.Message = "服务器在线";
                        }
                        break;
                    case "RFUploadDownData":  //上传离线数据
                        {
                            IList<RFUploadDownModel> rfUploadDownModelList = askModel.PostData.ToString().ToObject<IList<RFUploadDownModel>>();
                            result = RFUploadDown(rfUploadDownModelList, operatorModel, "RFOffLine",false);
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

                    logEntity.F_Result = true;
                    logEntity.F_Msg = result.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                }
                else
                {
                    logObj.Message = "操作失败";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = result.ToJson();
                    new LogApp().WriteDbLog(logEntity);
                }

                returnStr = result.ToJson();
                return returnStr;
            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;

                returnStr = result.ToJson();
                return returnStr;
            }
        }
        #endregion

        #region 上传离线数据
        public class RFUploadDownModel
        {
            public string ID { get; set; } /// 数据ID
            public string IsUpload { get; set; }   /// 是否已上传（Yes，No）
            public string LocationCode { get; set; }  /// 货位编码(暂未用到)

            public string BarCode { get; set; }

            public string ItemBarCode { get; set; } //标签码
            public decimal? OutQty { get; set; } //出库数量
        }

        /// <summary>
        /// 上传非单据扣减库存数据
        /// </summary>
        /// <param name="rfUploadDownModelList"></param>
        /// <param name="operatorModel"></param>
        /// <param name="genType"></param>
        /// <param name="isAuto">是否直接执行扣减</param>
        /// <returns></returns>
        public RFOffLineResult RFUploadDown(IList<RFUploadDownModel> rfUploadDownModelList, OperatorModel operatorModel, string genType,bool isAuto)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                RFOffLineResult result = new RFOffLineResult();
                try
                {
                    if(rfUploadDownModelList.Count<1)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "没有处理任何数据";
                        return result;
                    }

                    foreach (RFUploadDownModel cell in rfUploadDownModelList)
                    {
                        if (string.IsNullOrEmpty(cell.BarCode))
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "容器编码不能为空" + cell.BarCode;
                            return result;
                        }

                        string msg = "";
                        T_OffLineDownEntity entityTemp = null;
                        IList<T_ContainerDetailEntity> conDetailList = new List<T_ContainerDetailEntity>();
                        T_ContainerDetailEntity firstConDetail = new T_ContainerDetailEntity();
                        T_LocationEntity loc = new T_LocationEntity();
                        if (string.IsNullOrEmpty(cell.ItemBarCode))
                        {
                            msg = "容器条码" + cell.BarCode;

                            entityTemp = db.FindEntity<T_OffLineDownEntity>(o => o.BarCode == cell.BarCode && o.State == "New");
                            conDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == cell.BarCode);
                        }
                        else
                        {
                            msg = "容器条码" + cell.BarCode + ",标签条码" + cell.ItemBarCode;

                            entityTemp = db.FindEntity<T_OffLineDownEntity>(o => o.BarCode == cell.BarCode && o.ItemBarCode == cell.BarCode && o.State == "New");
                            conDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == cell.BarCode && o.ItemBarCode == cell.ItemBarCode );
                        }

                        if (entityTemp != null)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "已存在未处理的记录：" + msg;
                            return result;
                        }

                        if (conDetailList.Count < 1)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "库存未找到：" + msg;
                            return result;
                        }

                        if (conDetailList.Count > 1)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "库存不唯一：" + msg;
                            return result;
                        }

                        firstConDetail = conDetailList[0];

                        if (firstConDetail.ItemCode == FixType.Item.EmptyPlastic.ToString() || firstConDetail.ItemCode == FixType.Item.EmptyRack.ToString())
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "空容器不可操作：" + msg;
                            return result;
                        }

                        loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == firstConDetail.LocationID);

                        if(loc == null)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "库存对应货位不存在：" + msg;
                            return result;
                        }

                        if(loc.State != "Stored")
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "货位不为已存储：" + msg;
                            return result;
                        }

                        T_OffLineDownEntity entity = new T_OffLineDownEntity();
                        entity.Create();
                        entity.RFID = cell.ID;
                        //entity.GroupNum = "";
                        //entity.ERPRefCode ="";
                        //entity.IsTransTas = "";
                        entity.GenType = genType.ToString();
                        entity.BarCode = cell.BarCode;
                        entity.ItemBarCode = cell.ItemBarCode;
                        entity.IsEnable = "true";
                        entity.IsTransTask = "false";
                        entity.State = "New";

                        entity.ConDetailAreaCode = firstConDetail.AreaCode;
                        entity.ConDetailAreaName = firstConDetail.AreaName;
                        entity.ConDetailContainerDetailID = firstConDetail.F_Id;
                        entity.ConDetailContainerKind = firstConDetail.ContainerKind;
                        entity.ConDetailContainerType = firstConDetail.ContainerType;
                        entity.ConDetailItemCode = firstConDetail.ItemCode;
                        entity.ConDetailItemName = firstConDetail.ItemName;
                        entity.ConDetailLocationCode = firstConDetail.LocationNo;
                        entity.ConDetailLocationState = loc.State;
                        entity.ConDetailLot = firstConDetail.Lot;
                        entity.ConDetailPhoTime = DateTime.Now;
                        entity.ConDetailQty = firstConDetail.Qty;
                        entity.ConDetailState = firstConDetail.State;
                        entity.ConDetailSupplierCode = firstConDetail.SupplierCode;
                        entity.ConDetailSupplierName = firstConDetail.SupplierName;
                        entity.F_DeleteMark = false;

                        if(cell.OutQty == null || cell.OutQty == 0)
                        {
                            entity.Qty = firstConDetail.Qty;
                        }
                        else
                        {
                            entity.Qty = cell.OutQty;
                        }

                        db.Insert<T_OffLineDownEntity>(entity, operatorModel);
                        db.SaveChanges();

                        if(isAuto)
                        {
                            T_OffLineDownApp offlineDownApp = new T_OffLineDownApp();
                            string[] offLineDownIDArray = new string[] { entity.F_Id};
                            AjaxResult res = offlineDownApp.OffLineDownSub(db, offLineDownIDArray);
                            if(res.state.ToString() == ResultType.error.ToString())
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = res.message;
                                return result;
                            }
                        }
                    }
                    result.IsSuccess = true;
                    result.FailCode = "0000";
                    db.CommitWithOutRollBack();
                    return result;
                }
                catch (Exception)
                {
                    db.RollBack();
                    throw;
                }
            }
        }
        #endregion
    }
}
