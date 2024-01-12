/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/

using MST.Application.SystemSecurity;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST.Application.APIPost
{
    public class ERPPost
    {
        #region 请求主方法 PostERP

        private enum PostERPFunType
        {
            [Description("盘点数据推送")]
            PostCountData,
            [Description("实收实入推送")]
            PostFactInOutQty
        }

        /// <summary>
        /// 发送数据到ERP
        /// </summary>
        /// <param name="funType">方法类型</param>
        /// <param name="postParam">传入次方法的参数</param>
        /// <param name="postData">抛送到ERP的数据</param>
        /// <param name="user">当前操作人</param>
        /// <returns>ERP的返回值</returns>
        private ERPResult PostERP(PostERPFunType funType, object postParam, ref ERPModel postData, OperatorModel user = null)
        {
            string returnStr = "";

            LogObj logObj = new LogObj();
            logObj.Path = "APIPost.PostERP";

            postData.MsgID = Guid.NewGuid().ToString();
            postData.DateTime = DateTime.Now;
            postData.Func = funType.ToString();
            postData.Param = postParam;

            logObj.Parms = new { funType = funType.ToString(), SendToERPPrams = postData };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "WMS请求ERP接口";
            logEntity.F_Type = DbLogType.Visit.ToString();

            logEntity.F_Description = "WMS请求接口";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            if (user == null)
            {
                user = OperatorProvider.Provider.GetCurrent();
            }

            ERPResult result = new ERPResult();
            string param = "";
            try
            {
                /*************************************************/
                string erp_url = RuleConfig.Intface.ERP.URL;


                switch (funType)
                {
                    case PostERPFunType.PostCountData:   /// 盘点数据推送
                        {
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                        }
                        break;
                    case PostERPFunType.PostFactInOutQty:  /// 实收实入推送
                        {
                            logEntity.F_Account = user.UserCode;
                            logEntity.F_NickName = user.UserName;
                        }
                        break;
                    default:
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "未知的方法名";
                            return result;
                        }
                }
                param = postData.ToJson();

                string responseStr = "";
                if ((!string.IsNullOrEmpty(erp_url)) && erp_url != "http://")   //ERP地址不为空，则请求接口
                {
                    try
                    {
                        //接口报错依旧判断为继续执行
                        responseStr = HttpMethods.HttpPost(erp_url, param);
                        result = responseStr.ToObject<ERPResult>();
                    }
                    catch (Exception ex)
                    {
                        result = new ERPResult() { IsSuccess = true, FailCode = "0002", FailMsg = "操作成功，但ERP接口请求失败，将继续执行:" + ex.Message + ",response:" + responseStr };
                    }
                }
                else //否则直接返回true
                {
                    result = new ERPResult() { IsSuccess = true };
                }


                /**************************************************/
                if (result.IsSuccess)
                {
                    logObj.Message = "操作成功";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    logEntity.F_Msg = result.ToJson();
                    new LogApp().WriteDbLogThread(logEntity, user);
                }
                else
                {
                    logObj.Message = "操作失败";
                    logObj.ReturnData = result.ToJson();
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = result.ToJson();
                    new LogApp().WriteDbLogThread(logEntity, user);
                }

                returnStr = result.ToJson();
                return result;
            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                logObj.ReturnData = ex.ToJson();
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLogThread(logEntity, user);

                result.IsSuccess = false;
                result.FailCode = "0002";
                result.FailMsg = ex.Message;
                result.Data = param;

                returnStr = result.ToJson();
                return result;
            }
        }
        #endregion

        #region 盘点数据推送到ERP
        public class PostCountDataModel
        {
            public string ActionType { get; set; }  /// 1：代表新增,2：代表删除,3：代表修改
            public string CountOrderCode { get; set; }  /// WMS盘点单编码
            public string RefOrderCode { get; set; }    /// ERP盘点单编码
            public IList<PostCountDataModelCell> postCountDataModelCellList { get; set; }
        }

        public class PostCountDataModelCell
        {
            public string SEQ { get; set; }
            public string ItemCode { get; set; }
            public string SupplierCode { get; set; }
            public string ERPWarehouseCode { get; set; }
            public string Lot { get; set; }
            public string DBQty { get; set; }
            public string CountQty { get; set; }
        }
        /// <summary>
        /// 盘点数据推送到ERP
        /// </summary>
        public ERPResult PostCountData(IRepositoryBase db, string countID)
        {
            ERPResult erpRes = new ERPResult();
            try
            {
                PostCountDataModel model = new PostCountDataModel();
                T_CountEntity count = db.FindEntity<T_CountEntity>(o => o.F_Id == countID);
                if (count == null)
                {
                    erpRes.IsSuccess = false;
                    erpRes.FailMsg = "盘点单不存在";
                    return erpRes;
                }
                if (count.State == "WaitResult" || count.State == "Over" || count.AuditState != "WaitAudit")
                {
                    erpRes.IsSuccess = false;
                    erpRes.FailMsg = "盘点单已提交审核";
                    return erpRes;
                }
                if (count.State != "WaitAudit")
                {
                    erpRes.IsSuccess = false;
                    erpRes.FailMsg = "盘点单未结束";
                    return erpRes;
                }

                IList<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == countID);
                if (detailList.Count() < 1)
                {
                    erpRes.IsSuccess = false;
                    erpRes.FailMsg = "盘点单明细不存在";
                    return erpRes;
                }

                /// TODO 有盘点明细，没有盘点记录的情况
                /// 情况1：WMS正常数据，ERP判定为不通过

                model.ActionType = "1";
                model.CountOrderCode = count.CountCode;
                model.RefOrderCode = count.RefOrderCode;
                model.postCountDataModelCellList = new List<PostCountDataModelCell>();

                /// 更新盘点记录审核状态
                List<T_CountRecordEntity> recordList = db.FindList<T_CountRecordEntity>(o => o.CountID == count.F_Id);
                foreach (var record in recordList)
                {
                    record.AuditState = "Auditing";
                    db.Update<T_CountRecordEntity>(record);
                }

 
                //预留多个ERP仓库物料在一个盘点单中，则ERP盘点单据接口需将仓库编码放到明细中
                var recordGroup = recordList.GroupBy(o => new { ERPCode = o.ERPCode, ItemCode = o.ItemCode, Lot = o.Lot,ChildList =o }).ToList();
                /// 封装ERP接口数据，更新盘点明细审核状态
                foreach (T_CountDetailEntity detail in detailList)
                {
                    T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.F_Id == detail.SupplierUserID);

                    var rceordTemp = recordGroup.Where(o => o.Key.ItemCode == detail.ItemCode && o.Key.Lot == detail.Lot).ToList();

                    
                    foreach(var recordCell in rceordTemp)
                    {
                        PostCountDataModelCell cell = new PostCountDataModelCell();
                        cell.SEQ = detail.SEQ.ToString();
                        cell.ItemCode = detail.ItemCode;
                        cell.Lot = detail.Lot;
                        cell.SupplierCode = supplier == null ? "" : supplier.SupplierCode;
                        cell.ERPWarehouseCode = recordCell.Key.ERPCode;
                        cell.DBQty = (detail.ERPQty ?? 0).ToString();   /// 原数量为ERP库存数量
                        cell.CountQty = (detail.CountQty ?? 0).ToString();

                        model.postCountDataModelCellList.Add(cell);
                    }
                    

                    /// 更新盘点明细审核状态
                    detail.AuditState = "Auditing";
                    detail.CountState = "WaitResult";
                    db.Update<T_CountDetailEntity>(detail);
                }

                /// 更新盘点单审核状态
                count.AuditState = "Auditing";
                count.State = "WaitResult";
                db.Update<T_CountEntity>(count);

                ERPModel postData = new ERPModel();
                erpRes = PostERP(PostERPFunType.PostCountData, model, ref postData);
                /// 记录对象erpRes

                db.SaveChanges();
                return erpRes;
            }
            catch (Exception ex)
            {
                erpRes.IsSuccess = false;
                erpRes.FailCode = "0002";
                erpRes.FailMsg = "错误：" + ex.Message;
                return erpRes;
            }
        }
        #endregion

        #region 实收实入推送到ERP

        public class PostFactInOutQtyModel
        {
            /// <summary>
            /// 1：代表新增,2：代表删除,3：代表修改
            /// </summary>
            public string ActionType { get; set; }
            /// <summary>
            /// 单据类型
            /// </summary>
            public string OrderType { get; set; }
            /// <summary>
            /// 单据编码
            /// </summary>
            public string RefOrderCode { get; set; }
            public IList<PostFactInOutQtyModelCell> Items { get; set; }
        }

        public class PostFactInOutQtyModelCell
        {
            public int? SEQ { get; set; }
            public string ItemCode { get; set; }
            public string ERPWarehouseCode { get; set; }
            public string Lot { get; set; }

            /// <summary>
            /// 出入库类型（In入库，Out出库）
            /// </summary>
            public string InOutType { get; set; }
            public decimal? FactQty { get; set; }
        }

        /// <summary>
        /// 实收实入推送到ERP(整单推送)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="countID">盘点单ID</param>
        /// <returns></returns>
        public ERPResult PostFactInOutQty(IRepositoryBase db, string orderType, string transID, OperatorModel user = null)
        {
            ERPResult erpRes = new ERPResult();
            try
            {
                PostFactInOutQtyModel model = new PostFactInOutQtyModel();
                model.ActionType = "1";
                model.OrderType = orderType.ToString();
                T_TransRecordEntity trans = db.FindEntity<T_TransRecordEntity>(o => o.F_Id == transID);
                string orderID = trans.OrderID;
                switch (orderType)
                {
                    case "Count": /// 盘点单（针对盘点审核通过或造成的库存变更）
                        {
                            T_CountEntity count = db.FindEntity<T_CountEntity>(o => o.F_Id == orderID);
                            if (count == null)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "盘点单不存在";
                                return erpRes;
                            }
                            IList<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == count.F_Id);
                            if (detailList.Count() < 1)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "盘点单明细不存在";
                                return erpRes;
                            }

                            model.RefOrderCode = count.RefOrderCode;
                            IList<T_CountRecordEntity> countRecList = db.FindList<T_CountRecordEntity>(o => o.CountID == count.F_Id);
                            foreach (T_CountRecordEntity r in countRecList)
                            {
                                r.TransState = "OverTrans";
                                db.Update<T_CountRecordEntity>(r, user);
                            }

                            model.Items = detailList.GroupJoin(countRecList, m => m.F_Id, n => n.CountDetailID, (m, n) => new { detail = m, rec = n })
                                .SelectMany(o => o.rec.DefaultIfEmpty(), (detail, grp) => new { SEQ = grp.SEQ, ERPWarehouseCode = count.ERPHouseCode, Lot = grp.Lot, ItemCode = grp.ItemCode, FactQty = grp.Qty, CountQty = grp.CountQty, Qty = grp.Qty }).GroupBy(o => new { o.SEQ, o.ERPWarehouseCode, Lot = o.Lot, ItemCode = o.ItemCode })
                                .Select(o => new PostFactInOutQtyModelCell { SEQ = o.Key.SEQ, ERPWarehouseCode = o.Key.ERPWarehouseCode, Lot = o.Key.Lot, ItemCode = o.Key.ItemCode, FactQty = o.Sum(j => j.CountQty - j.Qty) }).ToList();

                            foreach (PostFactInOutQtyModelCell cell in model.Items)
                            {
                                if (cell.FactQty > 0)
                                {
                                    cell.InOutType = "In";
                                }
                                else if (cell.FactQty < 0)
                                {
                                    cell.InOutType = "Out";
                                }
                            }
                        }
                        break;
                    case "OtherOut": /// 其它出库
                    case "GetItemOut": /// 领料出库单
                    case "VerBackOut": /// 验退出库单
                    case "WarehouseBackOut": /// 仓退出库单
                        {
                            T_OutBoundEntity outBound = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == orderID);
                            if (outBound == null)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "出库单不存在";
                                return erpRes;
                            }
                            IList<T_OutBoundDetailEntity> detailList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == outBound.F_Id);
                            if (detailList.Count() < 1)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "出库单明细不存在";
                                return erpRes;
                            }


                            model.RefOrderCode = outBound.RefOrderCode;
                            IList<T_OutRecordEntity> outRecList = db.FindList<T_OutRecordEntity>(o => o.OutBoundID == outBound.F_Id);
                            foreach (T_OutRecordEntity r in outRecList)
                            {
                                r.TransState = "OverTrans";
                                db.Update<T_OutRecordEntity>(r, user);
                            }

                            model.Items = detailList.GroupJoin(outRecList, m => m.F_Id, n => n.OutBoundDetailID, (m, n) => new { detail = m, rec = n })
                                                                            .SelectMany(o => o.rec.DefaultIfEmpty(), (detail, grp) => new { SEQ = grp.SEQ, ERPWarehouseCode = grp.ERPHouseCode, Lot = grp.Lot, ItemCode = grp.ItemCode, FactQty = grp.PickedQty })
                                                                            .GroupBy(o => new { o.SEQ, o.ERPWarehouseCode, Lot = o.Lot, ItemCode = o.ItemCode })
                                                                            .Select(o => new PostFactInOutQtyModelCell { SEQ = o.Key.SEQ, ERPWarehouseCode = o.Key.ERPWarehouseCode, Lot = o.Key.Lot, InOutType = "Out", ItemCode = o.Key.ItemCode, FactQty = o.Sum(j => j.FactQty) })
                                                                            .ToList();
                        }
                        break;
                    case "BackSample": /// 质检还样单
                        {
                            T_QAEntity qABack = db.FindEntity<T_QAEntity>(o => o.F_Id == orderID);
                            if (qABack == null)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "质检还样单不存在";
                                return erpRes;
                            }
                            IList<T_QADetailEntity> detailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qABack.F_Id);
                            if (detailBackList.Count() < 1)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "质检单明细不存在";
                                return erpRes;
                            }



                            T_QAEntity qAGet = db.FindEntity<T_QAEntity>(o => o.QACode == qABack.RefOrderCode);

                            model.RefOrderCode = qAGet.RefOrderCode;

                            //取样单明细列表
                            IList<T_QADetailEntity> detailGetList = db.FindList<T_QADetailEntity>(o => o.QAID == qAGet.F_Id && o.IsAppearQA == "false");

                            //取样记录
                            IList<T_QARecordEntity> qaRecList = db.FindList<T_QARecordEntity>(o => o.QAID == qAGet.F_Id && o.IsAppearQA == "false" && o.IsNeedBack == "true" && o.IsReturnOver == "true");
                            foreach (T_QARecordEntity r in qaRecList)
                            {
                                r.TransState = "OverTrans";
                                db.Update<T_QARecordEntity>(r, user);
                            }

                            if (qaRecList.Count == 0)
                            {
                                model.Items = null;
                            }
                            else
                            {
                                model.Items = detailGetList.Join(qaRecList, m => m.F_Id, n => n.QADetailID, (m, n) => new { SEQ = m.SEQ, ERPWarehouseCode = n.ERPHouseCode, Lot = n.Lot, ItemCode = n.ItemCode, FactQty = n.ReturnQty })
                                                                           .GroupBy(o => new { SEQ = o.SEQ, o.ERPWarehouseCode, Lot = o.Lot, ItemCode = o.ItemCode })
                                                                           .Select(o => new PostFactInOutQtyModelCell { SEQ = o.Key.SEQ, ERPWarehouseCode = o.Key.ERPWarehouseCode, Lot = o.Key.Lot, InOutType = "In", ItemCode = o.Key.ItemCode, FactQty = o.Sum(j => j.FactQty) })
                                                                           .ToList();
                            }

                        }
                        break;
                    case "GetSample": /// 质检取样单
                        {
                            T_QAEntity qAGet = db.FindEntity<T_QAEntity>(o => o.F_Id == orderID);
                            if (qAGet == null)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "质检单取样不存在";
                                return erpRes;
                            }
                            IList<T_QADetailEntity> detailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAGet.F_Id && o.IsAppearQA == "false");
                            if (detailList.Count() < 1)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "质检单明细无需过账";
                                return erpRes;
                            }

                            model.RefOrderCode = qAGet.RefOrderCode;

                            IList<T_QARecordEntity> qaRecList = db.FindList<T_QARecordEntity>(o => o.QAID == qAGet.F_Id && o.IsAppearQA == "false");
                            foreach (T_QARecordEntity r in qaRecList)
                            {
                                r.TransState = "OverTrans";
                                db.Update<T_QARecordEntity>(r, user);
                            }

                            if (qaRecList.Count == 0) model.Items = null;
                            else
                            {
                                model.Items = detailList.GroupJoin(qaRecList, m => m.F_Id, n => n.QADetailID, (m, n) => new { detail = m, rec = n })
                                                                           .SelectMany(o => o.rec.DefaultIfEmpty(), (detail, grp) => new { SEQ = grp.SEQ, ERPWarehouseCode = grp.ERPHouseCode, Lot = grp.Lot, ItemCode = grp.ItemCode, FactQty = grp.PickedQty })
                                                                           .GroupBy(o => new { SEQ = o.SEQ, o.ERPWarehouseCode, Lot = o.Lot, ItemCode = o.ItemCode })
                                                                           .Select(o => new PostFactInOutQtyModelCell { SEQ = o.Key.SEQ, ERPWarehouseCode = o.Key.ERPWarehouseCode, Lot = o.Key.Lot, InOutType = "Out", ItemCode = o.Key.ItemCode, FactQty = o.Sum(j => j.FactQty) })
                                                                           .ToList();
                            }
                        }
                        break;
                    case "PurchaseIn": /// 采购入库
                        {
                            T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == orderID);
                            if (inbound == null)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "入库单不存在";
                                return erpRes;
                            }
                            IList<T_InBoundDetailEntity> detailList = db.FindList<T_InBoundDetailEntity>(o => o.InBoundID == inbound.F_Id);
                            if (detailList.Count() < 1)
                            {
                                erpRes.IsSuccess = false;
                                erpRes.FailMsg = "入库单明细不存在";
                                return erpRes;
                            }

                            model.RefOrderCode = inbound.RefOrderCode;

                            IList<T_ReceiveRecordEntity> inRecList = db.FindList<T_ReceiveRecordEntity>(o => o.InBoundID == inbound.F_Id);
                            foreach (T_ReceiveRecordEntity r in inRecList)
                            {
                                r.TransState = "OverTrans";
                                db.Update<T_ReceiveRecordEntity>(r, user);
                            }
                            model.Items = detailList.GroupJoin(inRecList, m => m.F_Id, n => n.InBoundDetailID, (m, n) => new { detail = m, rec = n })
                                                                           .SelectMany(o => o.rec.DefaultIfEmpty(), (detail, grp) => new { SEQ = grp.SEQ, ERPWarehouseCode = grp.ERPWarehouseCode, Lot = grp.Lot, ItemCode = grp.ItemCode, FactQty = grp.Qty })
                                                                           .GroupBy(o => new { o.SEQ, o.ERPWarehouseCode, Lot = o.Lot, ItemCode = o.ItemCode })
                                                                           .Select(o => new PostFactInOutQtyModelCell { SEQ = o.Key.SEQ, ERPWarehouseCode = o.Key.ERPWarehouseCode, Lot = o.Key.Lot, InOutType = "In", ItemCode = o.Key.ItemCode, FactQty = o.Sum(j => j.FactQty) })
                                                                           .ToList();

                        }
                        break;
                    default:
                        {
                            erpRes.IsSuccess = false;
                            erpRes.FailMsg = "未知的单据类型";
                            return erpRes;
                        }
                }

                ERPModel postData = new ERPModel();
                erpRes = PostERP(PostERPFunType.PostFactInOutQty, model, ref postData, user);
                trans.SendText = postData.ToJson();
                trans.GetText = erpRes.ToJson();
                trans.LastTime = DateTime.Now;
                if (erpRes.IsSuccess)
                {
                    if (erpRes.FailCode == "0002") //表示ERP接口请求失败,ERP失败，但WMS依旧继续执行
                    {
                        trans.ErrCount = (trans.ErrCount ?? 0) + 1;
                        trans.State = "Err";
                    }
                    else
                    {
                        trans.State = "OK";
                    }

                    db.Update<T_TransRecordEntity>(trans, user);
                    db.SaveChanges();
                }
                return erpRes;
            }
            catch (Exception ex)
            {
                erpRes.IsSuccess = false;
                erpRes.FailCode = "0002";
                erpRes.FailMsg = "错误：" + ex.Message;
                return erpRes;
            }
        }
        #endregion
    }
}
