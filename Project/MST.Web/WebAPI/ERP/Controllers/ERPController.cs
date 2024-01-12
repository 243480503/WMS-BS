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
using MST.Domain;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using MST.Web.Areas.PC_CountManage.Controllers;
using MST.Web.Areas.PC_InBoundManage.Controllers;
using MST.Web.Areas.PC_QAManage.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.WebAPI.ERP.Controllers
{
    [HandlerLogin(false)]
    public class ERPController : Controller
    {
        private static object lockObj = new object();
        private static object qaLockObj = new object();
        private static object countlockObj = new object();

        #region 请求主方法
        [HttpPost]
        public string Ask()
        {
            string returnStr = "";

            UserEntity user = new UserApp().GetEntity("ERP");
            OperatorModel operatorModel = new OperatorModel();
            operatorModel.UserCode = user.F_Account;
            operatorModel.UserId = user.F_Id;
            operatorModel.UserName = user.F_RealName;
            OperatorProvider.Provider.AddCurrent(operatorModel);

            StreamReader sRead = new StreamReader(HttpContext.Request.InputStream);
            string data = sRead.ReadToEnd();
            sRead.Close();


            LogObj logObj = new LogObj();
            logObj.Path = "ERPController.Ask"; /// 按实际情况修改
            logObj.Parms = data; /// 按实际情况修改

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "ERP请求Ask接口"; /// 按实际情况修改
            logEntity.F_Type = DbLogType.Visit.ToString(); /// 按实际情况修改
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "ERP请求接口"; /// 按实际情况修改
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = data;

            ERPResult result = new ERPResult();
            try
            {
                /*************************************************/


                ERPModel askModel = data.ToObject<ERPModel>();

                switch (askModel.Func)
                {
                    case "SyncSupplier":  /// 供应商数据
                        {
                            SupplierFaceModel supplierFaceModel = askModel.Param.ToObject<SupplierFaceModel>();
                            result = this.SyncSupplier(supplierFaceModel);
                        }
                        break;
                    case "SyncItemKind":  /// 物料种类数据
                        {
                            ItemKindFaceModel itemKindFaceModel = askModel.Param.ToObject<ItemKindFaceModel>();
                            result = this.SyncItemKind(itemKindFaceModel);
                        }
                        break;
                    case "SyncItem":  /// 物料基础数据
                        {
                            ItemFaceModel itemFaceModel = askModel.Param.ToObject<ItemFaceModel>();
                            result = this.SyncItem(itemFaceModel);
                        }
                        break;
                    case "SyncDept": //部门数据
                        {
                            DeptModel deptModel = askModel.Param.ToObject<DeptModel>();
                            result = this.SyncDept(deptModel);
                        }
                        break;
                    case "SyncUserInfo": //人员
                        {
                            UserModel model = askModel.Param.ToObject<UserModel>();
                            result = this.SyncUser(model);
                        }
                        break;
                    case "SyncInOrder":  /// 入库单
                        {
                            InOrderFaceModel inOrderFaceModel = askModel.Param.ToObject<InOrderFaceModel>();
                            result = this.SyncInOrder(inOrderFaceModel);
                        }
                        break;
                    case "SyncOutOrder":  /// 出库单
                        {
                            OutOrderFaceModel outOrderFaceModel = askModel.Param.ToObject<OutOrderFaceModel>();
                            result = this.SyncOutOrder(outOrderFaceModel);
                        }
                        break;
                    case "SyncQAOutOrder":  /// 质检单
                        {
                            QAOutOrderFaceModel qAOutOrderFaceModel = askModel.Param.ToObject<QAOutOrderFaceModel>();
                            result = this.SyncQAOutOrder(qAOutOrderFaceModel);
                        }
                        break;
                    case "SyncCountOrder":  /// 盘点单
                        {
                            CountOrderFaceModel countOrderFaceModel = askModel.Param.ToObject<CountOrderFaceModel>();
                            result = this.SyncCountOrder(countOrderFaceModel);
                        }
                        break;
                    case "SyncQAResult":  /// 质检结果反馈
                        {
                            QAResultFaceModel qAResultFaceModel = askModel.Param.ToObject<QAResultFaceModel>();
                            result = this.SyncQAResult(qAResultFaceModel);
                        }
                        break;
                    case "SyncCountAuditResult":  /// 盘点审核结果反馈
                        {
                            CountAuditResultFaceModel countAuditResultFaceModel = askModel.Param.ToObject<CountAuditResultFaceModel>();
                            result = this.SyncCountAuditResult(countAuditResultFaceModel);
                        }
                        break;
                    case "SyncERPOrderCode":  /// 同步到货单与入库单编号
                        {
                            SyncERPOrderCodeModel syncERPOrderCodeModel = askModel.Param.ToObject<SyncERPOrderCodeModel>();
                            result = this.SyncERPOrderCode(syncERPOrderCodeModel);
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

        /*******ERP请求方法实现********/

        #region 供应商数据
        public ERPResult SyncSupplier(SupplierFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.SupplierCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "供应商编码必填";
                        return result;
                    }
                    T_SupplierEntity existsEntity = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == model.SupplierCode && o.F_DeleteMark == false);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "供应商编码已存在";
                                    return result;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(model.SupplierName))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "供应商名称必填";
                                        return result;
                                    }
                                    result = InsertSupplien(db, model);
                                }
                            }
                            break;
                        case "2":   /// 删除
                            {
                                if (existsEntity != null)
                                {
                                    existsEntity.F_DeleteMark = true;
                                    db.Update<T_SupplierEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "供应商不存在";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (string.IsNullOrEmpty(model.SupplierName))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "供应商名称必填";
                                    return result;
                                }
                                if (existsEntity != null)
                                {


                                    existsEntity.SupplierAddr = model.SupplierAddr;
                                    existsEntity.SupplierCode = model.SupplierCode;
                                    existsEntity.SupplierName = model.SupplierName;
                                    existsEntity.SupplierPho = model.SupplierPho;
                                    db.Update<T_SupplierEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertSupplien(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult InsertSupplien(IRepositoryBase db, SupplierFaceModel model)
        {
            ERPResult result = new ERPResult();
            try
            {
                T_SupplierEntity entity = new T_SupplierEntity();
                entity.F_Id = Guid.NewGuid().ToString();
                entity.SupplierAddr = model.SupplierAddr;
                entity.SupplierCode = model.SupplierCode;
                entity.SupplierName = model.SupplierName;
                entity.SupplierPho = model.SupplierPho;
                entity.IsEnable = "true";
                entity.F_DeleteMark = false;
                db.Insert<T_SupplierEntity>(entity);
                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 物料种类数据
        public ERPResult SyncItemKind(ItemKindFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.KindCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "种类编码必填";
                        return result;
                    }

                    T_ItemKindEntity existsEntity = db.FindEntity<T_ItemKindEntity>(o => o.KindCode == model.KindCode && o.F_DeleteMark == false);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "种类编码已存在";
                                    return result;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(model.KindName))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "种类名称必填";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ParentKindCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "父种类编码必填";
                                        return result;
                                    }
                                    result = InsertItemKind(db, model);
                                }
                            }
                            break;
                        case "2":   /// 删除
                            {
                                if (existsEntity != null)
                                {
                                    IList<T_ItemEntity> itemList = db.FindList<T_ItemEntity>(o => o.ItemKindID == existsEntity.F_Id && o.F_DeleteMark == false);
                                    if (itemList.Count != 0)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "物料种类包含在使用物料";
                                    }
                                    else
                                    {
                                        DelItemKind(db, existsEntity);
                                        existsEntity.F_DeleteMark = true;
                                        db.Update<T_ItemKindEntity>(existsEntity);
                                        result.IsSuccess = true;
                                    }
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "种类编码不存在";
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (string.IsNullOrEmpty(model.KindName))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "种类名称必填";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.ParentKindCode))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "父种类编码必填";
                                    return result;
                                }

                                if (existsEntity != null)
                                {
                                    string parentID = "";
                                    if (model.ParentKindCode == "0")
                                    {
                                        parentID = "0";
                                    }
                                    else
                                    {
                                        T_ItemKindEntity parent = db.FindEntity<T_ItemKindEntity>(o => o.KindCode == model.ParentKindCode && o.F_DeleteMark == false);
                                        if (parent == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "父类编码不存在";
                                            return result;
                                        }
                                        else
                                        {
                                            parentID = parent.F_Id;
                                        }
                                    }

                                    existsEntity.KindCode = model.KindCode;
                                    existsEntity.KindName = model.KindName;
                                    existsEntity.ParentID = parentID;
                                    db.Update<T_ItemKindEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {

                                    result = InsertItemKind(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                            }
                            break;
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult DelItemKind(IRepositoryBase db, T_ItemKindEntity itemKind)
        {
            ERPResult result = new ERPResult();
            IList<T_ItemKindEntity> childList = db.FindList<T_ItemKindEntity>(o => o.ParentID == itemKind.F_Id && o.F_DeleteMark == false);
            foreach (T_ItemKindEntity cell in childList)
            {
                DelItemKind(db, cell);
                cell.F_DeleteMark = true;
                db.Update<T_ItemKindEntity>(cell);
            }

            result.IsSuccess = true;
            return result;
        }

        private ERPResult InsertItemKind(IRepositoryBase db, ItemKindFaceModel model)
        {
            ERPResult result = new ERPResult();
            try
            {
                string parentID = "";
                if (model.ParentKindCode == "0")
                {
                    parentID = "0";
                }
                else
                {
                    T_ItemKindEntity parent = db.FindEntity<T_ItemKindEntity>(o => o.KindCode == model.ParentKindCode && o.F_DeleteMark == false);
                    if (parent == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "父类编码不存在";
                        return result;
                    }
                    else
                    {
                        parentID = parent.F_Id;
                    }
                }

                T_ItemKindEntity entity = new T_ItemKindEntity();
                entity.F_Id = Guid.NewGuid().ToString();
                entity.KindCode = model.KindCode;
                entity.KindName = model.KindName;
                entity.ParentID = parentID;
                entity.F_DeleteMark = false;
                entity.IsBase = "false";
                db.Insert<T_ItemKindEntity>(entity);
                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 物料基础数据
        public ERPResult SyncItem(ItemFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.ItemCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料编码必填";
                        return result;
                    }

                    T_ItemEntity existsEntity = db.FindEntity<T_ItemEntity>(o => o.ItemCode == model.ItemCode && o.F_DeleteMark == false);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "物料编码已存在";
                                    return result;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(model.KindCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "种类编码必填";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ItemName))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "物料名称必填";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ItemUnitText))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "物料单位必填";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.MinQty))
                                    {
                                        model.MinQty = "0";
                                    }
                                    else if (!model.MinQty.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "保留库存应为非负整数";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.WarningQty))
                                    {
                                        model.WarningQty = "0";
                                    }
                                    else if (!model.WarningQty.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "预警数量应为非负整数";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.MaxQty))
                                    {
                                        model.MaxQty = "0";
                                    }
                                    else if (!model.MaxQty.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "最大库存数量应为非负整数";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.Spec))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "规格必填";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ValidityDayNum))
                                    {
                                        model.ValidityDayNum = "0";
                                    }
                                    else if (!model.ValidityDayNum.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "有限期天数应为非负整数";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.UnitQty))
                                    {
                                        model.UnitQty = "0";
                                    }
                                    else if (!model.UnitQty.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "整箱数量应为非负整数";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ValidityWarning))
                                    {
                                        model.ValidityWarning = "0";
                                    }
                                    else if (!model.ValidityWarning.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "预警天数应为非负整数";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.CheckPerc))
                                    {
                                        model.CheckPerc = "0";
                                    }
                                    else if (!model.CheckPerc.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "抽检百分比应为非负数";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.CheckBoxPerc))
                                    {
                                        model.CheckBoxPerc = "0";
                                    }
                                    else if (!model.CheckBoxPerc.IsNaturalZero())
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "每箱抽检百分比应为非负数";
                                        return result;
                                    }

                                    //if (string.IsNullOrEmpty(model.Factory))
                                    //{
                                    //    result.IsSuccess = false;
                                    //    result.FailCode = "0001";
                                    //    result.FailMsg = "生产厂家不能为空";
                                    //    return result;
                                    //}

                                    if (string.IsNullOrEmpty(model.IsNeedCheck))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "是否需要质检不能为空";
                                        return result;
                                    }
                                    else if (model.IsNeedCheck != "true" && model.IsNeedCheck != "false")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "是否需要质检参数不正确(应为true或false)";
                                        return result;
                                    }

                                    if (model.IsNeedCheck != "true" && model.IsNeedCheck != "false")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "是否质检参数不正确(应为true或false)";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.IsMustLot))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "是否强制批号不能为空";
                                        return result;
                                    }
                                    else if (model.IsNeedCheck != "true" && model.IsNeedCheck != "false")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "是否强制批号参数不正确(应为true或false)";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ERPWarehouseCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "物料存放的ERP仓库编码不能为空";
                                        return result;
                                    }
                                    result = InsertItem(db, model);
                                }
                            }
                            break;
                        case "2":   /// 删除
                            {
                                if (existsEntity != null)
                                {
                                    existsEntity.F_DeleteMark = true;
                                    db.Update<T_ItemEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "物料不存在";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (string.IsNullOrEmpty(model.KindCode))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "种类编码必填";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.ItemName))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "物料名称必填";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.ItemUnitText))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "物料单位必填";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.MinQty))
                                {
                                    model.MinQty = "0";
                                }
                                else if (!model.MinQty.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "保留库存应为非负整数";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.WarningQty))
                                {
                                    model.WarningQty = "0";
                                }
                                else if (!model.WarningQty.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "预警数量应为非负整数";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.MaxQty))
                                {
                                    model.MaxQty = "0";
                                }
                                else if (!model.MaxQty.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "最大库存数量应为非负整数";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.Spec))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "规格必填";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.ValidityDayNum))
                                {
                                    model.ValidityDayNum = "0";
                                }
                                else if (!model.ValidityDayNum.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "有限期天数应为非负整数";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.UnitQty))
                                {
                                    model.UnitQty = "0";
                                }
                                else if (!model.UnitQty.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "整箱数量应为非负整数";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.ValidityWarning))
                                {
                                    model.ValidityWarning = "0";
                                }
                                else if (!model.ValidityWarning.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "预警天数应为非负整数";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.CheckPerc))
                                {
                                    model.CheckPerc = "0";
                                }
                                else if (!model.CheckPerc.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "抽检百分比应为非负数";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.CheckBoxPerc))
                                {
                                    model.CheckBoxPerc = "0";
                                }
                                else if (!model.CheckBoxPerc.IsNaturalZero())
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "每箱抽检百分比应为非负数";
                                    return result;
                                }

                                //if (string.IsNullOrEmpty(model.Factory))
                                //{
                                //    result.IsSuccess = false;
                                //    result.FailCode = "0001";
                                //    result.FailMsg = "生产厂家不能为空";
                                //    return result;
                                //}

                                if (string.IsNullOrEmpty(model.IsNeedCheck))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "是否需要质检不能为空";
                                    return result;
                                }
                                else if (model.IsNeedCheck != "true" && model.IsNeedCheck != "false")
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "是否需要质检参数不正确(应为true或false)";
                                    return result;
                                }

                                if (model.IsNeedCheck != "true" && model.IsNeedCheck != "false")
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "是否质检参数不正确(应为true或false)";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.IsMustLot))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "是否强制批号不能为空";
                                    return result;
                                }
                                else if (model.IsNeedCheck != "true" && model.IsNeedCheck != "false")
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "是否强制批号参数不正确(应为true或false)";
                                    return result;
                                }

                                if (string.IsNullOrEmpty(model.ERPWarehouseCode))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "物料存放的ERP仓库编码不能为空";
                                    return result;
                                }
                                if (existsEntity != null)
                                {
                                    T_ItemKindEntity itemKind = db.FindEntity<T_ItemKindEntity>(o => o.KindCode == model.KindCode && o.F_DeleteMark == false);
                                    if (itemKind == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "种类编码不存在";
                                        return result;
                                    }

                                    T_ERPWarehouseEntity erpWare = db.FindEntity<T_ERPWarehouseEntity>(o => o.ERPHouseCode == model.ERPWarehouseCode && o.F_DeleteMark == false);
                                    if (erpWare == null)
                                    {
                                        /// 新增ERP仓库
                                        T_ERPWarehouseEntity erpEntity = new T_ERPWarehouseEntity();
                                        erpEntity.F_Id = Guid.NewGuid().ToString();
                                        erpEntity.ERPHouseCode = model.ERPWarehouseCode;    /// 暂定：ERP编码和名称一致
                                        erpEntity.ERPHouseName = model.ERPWarehouseCode;
                                        erpEntity.F_DeleteMark = false;
                                        db.Insert<T_ERPWarehouseEntity>(erpEntity);
                                    }

                                    existsEntity.ItemKindID = itemKind.F_Id;
                                    existsEntity.KindCode = model.KindCode;
                                    existsEntity.KindName = itemKind.KindName;
                                    existsEntity.ItemCode = model.ItemCode;
                                    existsEntity.ItemName = model.ItemName;
                                    existsEntity.ItemUnitText = model.ItemUnitText;
                                    //existsEntity.WarningQty = Convert.ToDecimal(model.WarningQty); //注释的原因是ERP不传此参数，WMS修改后，ERP再次修改物料信息，会覆盖WMS的设置
                                    existsEntity.MaxQty = Convert.ToDecimal(model.MaxQty);
                                    //existsEntity.MinQty = Convert.ToDecimal(model.MinQty);
                                    existsEntity.Spec = model.Spec == null ? "" : model.Spec;  /// 规格
                                    existsEntity.ValidityDayNum = Convert.ToInt32(model.ValidityDayNum);
                                    //existsEntity.UnitQty = Convert.ToDecimal(model.UnitQty);
                                    existsEntity.Factory = model.Factory == null ? "" : model.Factory;
                                    existsEntity.IsNeedCheck = model.IsNeedCheck;
                                    existsEntity.IsMustLot = model.IsMustLot;
                                    existsEntity.ERPWarehouseCode = model.ERPWarehouseCode;
                                    //existsEntity.CheckBoxPerc = Convert.ToDecimal(model.CheckBoxPerc);
                                    //existsEntity.CheckPerc = Convert.ToDecimal(model.CheckPerc);
                                    //existsEntity.IsMixLot = "false";
                                    //existsEntity.IsSpecial = "false";
                                    //existsEntity.IsMixItem = "false";
                                    //existsEntity.IsMixQA = "false";
                                    //existsEntity.IsBase = "false";
                                    existsEntity.F_DeleteMark = false;
                                    db.Update<T_ItemEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertItem(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult InsertItem(IRepositoryBase db, ItemFaceModel model)
        {
            ERPResult result = new ERPResult();
            try
            {
                T_ItemKindEntity itemKind = db.FindEntity<T_ItemKindEntity>(o => o.KindCode == model.KindCode && o.F_DeleteMark == false);
                if (itemKind == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "种类编码不存在";
                    return result;
                }

                T_ERPWarehouseEntity erpWare = db.FindEntity<T_ERPWarehouseEntity>(o => o.ERPHouseCode == model.ERPWarehouseCode && o.F_DeleteMark == false);
                if (erpWare == null)
                {
                    /// 新增ERP仓库
                    T_ERPWarehouseEntity erpEntity = new T_ERPWarehouseEntity();
                    erpEntity.F_Id = Guid.NewGuid().ToString();
                    erpEntity.ERPHouseCode = model.ERPWarehouseCode;    /// 暂定：ERP编码和名称一致
                    erpEntity.ERPHouseName = model.ERPWarehouseCode;
                    erpEntity.F_DeleteMark = false;
                    db.Insert<T_ERPWarehouseEntity>(erpEntity);
                }

                T_ItemEntity entity = new T_ItemEntity();
                entity.F_Id = Guid.NewGuid().ToString();
                entity.ItemKindID = itemKind.F_Id;
                entity.KindCode = model.KindCode;
                entity.KindName = itemKind.KindName;
                entity.ItemCode = model.ItemCode;
                entity.ItemName = model.ItemName;
                entity.ItemUnitText = model.ItemUnitText;
                entity.ValidityWarning = Convert.ToInt32(model.ValidityWarning);
                entity.WarningQty = Convert.ToDecimal(model.WarningQty);
                entity.MaxQty = Convert.ToDecimal(model.MaxQty);
                entity.MinQty = Convert.ToDecimal(model.MinQty);
                entity.Spec = model.Spec == null ? "" : model.Spec;  /// 规格
                entity.ValidityDayNum = Convert.ToInt32(model.ValidityDayNum);
                entity.UnitQty = Convert.ToDecimal(model.UnitQty);
                entity.Factory = model.Factory == null ? "" : model.Spec;  /// 生产厂家
                entity.IsNeedCheck = model.IsNeedCheck;
                entity.IsMustLot = model.IsMustLot;
                entity.ERPWarehouseCode = model.ERPWarehouseCode;
                entity.CheckBoxPerc = Convert.ToDecimal(model.CheckBoxPerc);
                entity.CheckPerc = Convert.ToDecimal(model.CheckPerc);
                //entity.IsItemMark;
                entity.IsMixLot = "false";
                entity.IsMixItem = "false";
                entity.IsMixQA = "false";
                //entity.ContainerType;
                //entity.StackType;
                entity.IsBase = "false";
                entity.F_DeleteMark = false;
                db.Insert<T_ItemEntity>(entity);
                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 部门数据
        public ERPResult SyncDept(DeptModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.DeptCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "部门编码必填";
                        return result;
                    }

                    OrganizeEntity existsEntity = db.FindEntity<OrganizeEntity>(o => o.F_EnCode == model.DeptCode && o.F_DeleteMark == false);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "部门编码已存在";
                                    return result;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(model.DeptName))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "部门名称必填";
                                        return result;
                                    }
                                    if (string.IsNullOrEmpty(model.ParentDeptCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "父部门编码必填";
                                        return result;
                                    }
                                    result = InsertDept(db, model);
                                }
                            }
                            break;
                        case "2":   /// 删除
                            {
                                if (existsEntity != null)
                                {
                                    existsEntity.F_DeleteMark = true;
                                    db.Update<OrganizeEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "部门不存在";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (string.IsNullOrEmpty(model.DeptName))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "部门名称必填";
                                    return result;
                                }
                                if (string.IsNullOrEmpty(model.ParentDeptCode))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "父部门编码必填";
                                    return result;
                                }
                                if (existsEntity != null)
                                {
                                    existsEntity.F_EnCode = model.DeptCode;
                                    existsEntity.F_FullName = model.DeptName;
                                    if (model.ParentDeptCode == "0")
                                    {
                                        existsEntity.F_ParentId = model.ParentDeptCode;
                                    }
                                    else
                                    {

                                        OrganizeEntity parent = db.FindEntity<OrganizeEntity>(o => o.F_EnCode == model.ParentDeptCode && o.F_DeleteMark == false);
                                        if (parent == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "父部门编码不存在";
                                            return result;
                                        }
                                        existsEntity.F_ParentId = parent.F_Id;
                                    }
                                    existsEntity.F_DeleteMark = false;
                                    db.Update<OrganizeEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertDept(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult InsertDept(IRepositoryBase db, DeptModel model)
        {
            ERPResult result = new ERPResult();
            try
            {


                OrganizeEntity entity = new OrganizeEntity();
                entity.F_Id = Guid.NewGuid().ToString();
                entity.F_EnCode = model.DeptCode;
                entity.F_FullName = model.DeptName;
                entity.F_CategoryId = "Department"; //部门类型，Department指部门
                entity.F_EnabledMark = true;
                if (model.ParentDeptCode == "0")
                {
                    entity.F_ParentId = model.ParentDeptCode;
                }
                else
                {

                    OrganizeEntity parent = db.FindEntity<OrganizeEntity>(o => o.F_EnCode == model.ParentDeptCode && o.F_DeleteMark == false);
                    if (parent == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "父部门编码不存在";
                        return result;
                    }
                    entity.F_ParentId = parent.F_Id;
                }
                entity.F_DeleteMark = false;
                db.Insert<OrganizeEntity>(entity);
                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 用户数据
        public ERPResult SyncUser(UserModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }

                    if (string.IsNullOrEmpty(model.UserCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "用户编码必填";
                        return result;
                    }

                    UserEntity existsEntity = db.FindEntity<UserEntity>(o => o.F_Account == model.UserCode && o.F_DeleteMark == false);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "用户编码已存在";
                                    return result;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(model.DeptCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "所属部门编码必填";
                                        return result;
                                    }
                                    if (string.IsNullOrEmpty(model.UserName))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "用户名称必填";
                                        return result;
                                    }
                                    result = InsertUser(db, model);
                                }
                            }
                            break;
                        case "2":   /// 删除
                            {
                                if (existsEntity != null)
                                {
                                    existsEntity.F_DeleteMark = true;
                                    db.Update<UserEntity>(existsEntity);
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "该用户不存在";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (string.IsNullOrEmpty(model.DeptCode))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "所属部门编码必填";
                                    return result;
                                }
                                if (string.IsNullOrEmpty(model.UserName))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "用户名称必填";
                                    return result;
                                }
                                if (existsEntity != null)
                                {
                                    existsEntity.F_Account = model.UserCode;
                                    existsEntity.F_RealName = model.UserName;
                                    existsEntity.F_NickName = model.UserName;
                                    existsEntity.F_Gender = model.Gender == "1" ? true : false;

                                    OrganizeEntity org = db.FindEntity<OrganizeEntity>(o => o.F_EnCode == model.DeptCode);
                                    if (org == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "不存在此部门编码";
                                        return result;
                                    }

                                    OrganizeEntity orgRoot = GetRootOrg(db, org.F_Id);
                                    if (orgRoot == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "该部门编码不存在根部门";
                                        return result;
                                    }

                                    existsEntity.F_OrganizeId = orgRoot.F_Id;
                                    existsEntity.F_DepartmentId = org.F_Id;

                                    existsEntity.F_DeleteMark = false;
                                    db.Update<UserEntity>(existsEntity);
                                    db.SaveChanges();

                                    if (!string.IsNullOrEmpty(model.Pwd)) //密码不为空则更新，否则不改变
                                    {
                                        UserLogOnEntity userLogOnEntity = db.FindEntity<UserLogOnEntity>(o => o.F_UserId == existsEntity.F_Id);
                                        userLogOnEntity.F_UserPassword = Md5.md5(DESEncrypt.Encrypt(Md5.md5(model.Pwd, 32).ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();
                                        db.Update<UserLogOnEntity>(userLogOnEntity);
                                        db.SaveChanges();
                                    }

                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertUser(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }


        private ERPResult InsertUser(IRepositoryBase db, UserModel model)
        {
            ERPResult result = new ERPResult();
            try
            {

                UserEntity entity = new UserEntity();
                entity.F_Id = Guid.NewGuid().ToString();
                entity.F_Account = model.UserCode;
                entity.F_RealName = model.UserName;
                entity.F_NickName = model.UserName;
                entity.F_Gender = model.Gender == "1" ? true : false;
                entity.F_EnabledMark = true;
                entity.F_IsAdministrator = false;

                OrganizeEntity org = db.FindEntity<OrganizeEntity>(o => o.F_EnCode == model.DeptCode);
                if (org == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "不存在此部门编码";
                    return result;
                }

                OrganizeEntity orgRoot = GetRootOrg(db, org.F_Id);
                if (orgRoot == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "该部门编码不存在根部门";
                    return result;
                }
                entity.F_OrganizeId = orgRoot.F_Id;
                entity.F_DepartmentId = org.F_Id;
                entity.IsBase = "false";
                entity.F_DeleteMark = false;
                db.Insert<UserEntity>(entity);

                if (string.IsNullOrEmpty(model.Pwd))
                {
                    model.Pwd = "123456";
                }
                UserLogOnEntity userLogOnEntity = new UserLogOnEntity();
                userLogOnEntity.F_Id = entity.F_Id;// Guid.NewGuid().ToString();
                userLogOnEntity.F_UserId = entity.F_Id;
                userLogOnEntity.F_UserSecretkey = Md5.md5(Common.CreateNo(), 16).ToLower();
                userLogOnEntity.F_UserPassword = Md5.md5(DESEncrypt.Encrypt(Md5.md5(model.Pwd, 32).ToLower(), userLogOnEntity.F_UserSecretkey).ToLower(), 32).ToLower();

                db.Insert<UserLogOnEntity>(userLogOnEntity);
                db.SaveChanges();

                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private OrganizeEntity GetRootOrg(IRepositoryBase db, string orgId)
        {
            OrganizeEntity curOrg = db.FindEntity<OrganizeEntity>(o => o.F_Id == orgId);
            if (curOrg == null)
            {
                return null;
            }
            if (curOrg.F_ParentId == "0")
            {
                return curOrg;
            }
            else
            {
                return GetRootOrg(db, curOrg.F_ParentId);
            }
        }


        #endregion

        #region 入库单
        public ERPResult SyncInOrder(InOrderFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.RefOrderCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "ERP单据编码必填";
                        return result;
                    }

                    if (string.IsNullOrEmpty(model.InBoundType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "入库单类型必填";
                        return result;
                    }
                    else if (model.InBoundType != "PurchaseInType")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "入库单类型不正确";
                        return result;
                    }

                    T_InBoundEntity existsEntity = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == model.RefOrderCode);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {
                                if (model.Items.Count == 0)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "单据明细不能为空";
                                    return result;
                                }

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "ERP单据编码已存在";
                                    return result;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(model.SupplierCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "供应商编码必填";
                                        return result;
                                    }
                                    result = InsertInOrder(db, model);
                                }
                            }
                            break;
                        case "2":/// 删除
                            {
                                if (existsEntity != null)
                                {
                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }

                                    string[] DelDetail;
                                    if (model.Items == null || model.Items.Count < 1) /// 删除整个单据
                                    {
                                        DelDetail = db.FindList<T_InBoundDetailEntity>(o => o.InBoundID == existsEntity.F_Id).Select(o => (o.SEQ ?? 0).ToString()).ToArray();
                                        db.Delete<T_InBoundDetailEntity>(o => o.InBoundID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                        db.Delete<T_InBoundEntity>(existsEntity);
                                    }
                                    else /// 删除部分单据
                                    {
                                        DelDetail = model.Items.Select(o => o.SEQ).ToArray();
                                        db.Delete<T_InBoundDetailEntity>(o => o.InBoundID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                    }

                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = true;
                                    result.FailCode = "0000";
                                    result.FailMsg = "";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (string.IsNullOrEmpty(model.SupplierCode))
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "供应商编码必填";
                                    return result;
                                }


                                if (existsEntity != null)
                                {
                                    if (model.Items.Count == 0)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据明细不能为空";
                                        return result;
                                    }

                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }

                                    existsEntity.InBoundType = model.InBoundType;
                                    existsEntity.RefOrderCode = model.RefOrderCode;
                                    existsEntity.ERPInDocCode = model.RefOrderCode;
                                    existsEntity.GenType = "ERP";
                                    existsEntity.StationID = "";
                                    T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == model.SupplierCode && o.F_DeleteMark == false);
                                    if (supplier == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "供应商不存在";
                                        return result;
                                    }
                                    existsEntity.SupplierUserID = supplier.F_Id;
                                    existsEntity.SupplierUserCode = supplier.SupplierCode;
                                    existsEntity.SupplierUserName = supplier.SupplierName;

                                    db.Update<T_InBoundEntity>(existsEntity);
                                    db.SaveChanges();

                                    IList<InOrderFaceCellModel> items = model.Items;
                                    foreach (InOrderFaceCellModel cell in items)
                                    {
                                        if (!(cell.SEQ ?? "").IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不正确";
                                            return result;
                                        }

                                        if (!(cell.Qty ?? "").IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "数量不正确";
                                            return result;
                                        }

                                        if (!(cell.ProductDate ?? "").IsDate())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "生产日期不正确";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(cell.ItemCode))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料编码必填";
                                            return result;
                                        }

                                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                                        if (item == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料不存在";
                                            return result;
                                        }

                                        if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料批号必填";
                                            return result;
                                        }

                                        if (cell.CheckState == "UnQua")
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "不合格物料不可入库";
                                            return result;
                                        }

                                        T_ERPWarehouseEntity erp = db.FindEntity<T_ERPWarehouseEntity>(o => o.ERPHouseCode == cell.ERPWarehouseCode && o.F_DeleteMark == false);
                                        if (erp == null)
                                        {
                                            /// 新增ERP仓库
                                            T_ERPWarehouseEntity erpEntity = new T_ERPWarehouseEntity();
                                            erpEntity.F_Id = Guid.NewGuid().ToString();
                                            erpEntity.ERPHouseCode = cell.ERPWarehouseCode;    /// 暂定：ERP编码和名称一致
                                            erpEntity.ERPHouseName = cell.ERPWarehouseCode;
                                            erpEntity.F_DeleteMark = false;
                                            db.Insert<T_ERPWarehouseEntity>(erpEntity);
                                        }

                                        //T_ItemInStationEntity itemInStation = db.FindEntity<T_ItemInStationEntity>(o => o.ItemID == item.F_Id);
                                        //if (itemInStation == null)
                                        //{
                                        //    result.IsSuccess = false;
                                        //    result.FailCode = "0001";
                                        //    result.FailMsg = "未设置物料入库地点：" + cell.SEQ;
                                        //    return result;
                                        //}
                                        //T_ItemAreaEntity itemArea = db.FindEntity<T_ItemAreaEntity>(o => o.ItemID == item.F_Id);
                                        //if (itemArea == null)
                                        //{
                                        //    result.IsSuccess = false;
                                        //    result.FailCode = "0001";
                                        //    result.FailMsg = "未设置物料存储区域：" + cell.SEQ;
                                        //    return result;
                                        //}

                                        T_InBoundDetailEntity detail = db.FindEntity<T_InBoundDetailEntity>(o => o.InBoundID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        if (detail == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不存在：" + cell.SEQ;
                                            return result;
                                        }

                                        if (cell.CheckState != "Qua"
                                            && cell.CheckState != "UnNeed"
                                            && cell.CheckState != "UnQua"
                                            && cell.CheckState != "WaitCheck")
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "质检状态不正确：项次" + cell.SEQ;
                                            return result;
                                        }

                                        detail.ItemID = item.F_Id;
                                        detail.ItemName = item.ItemName;
                                        detail.ItemCode = item.ItemCode;
                                        detail.ERPWarehouseCode = cell.ERPWarehouseCode;
                                        detail.IsMustQtySame = string.IsNullOrEmpty(cell.IsMustQtySame) ? "true" : cell.IsMustQtySame;
                                        detail.Factory = item.Factory;
                                        detail.ProductDate = Convert.ToDateTime(cell.ProductDate);
                                        if (item.ValidityUnitType == "Day")
                                        {
                                            detail.OverdueDate = detail.ProductDate.Value.AddDays(item.ValidityDayNum ?? 0);
                                        }
                                        else if (item.ValidityUnitType == "Month")
                                        {
                                            detail.OverdueDate = detail.ProductDate.Value.AddMonths(item.ValidityDayNum ?? 0);
                                        }
                                        else if (item.ValidityUnitType == "Year")
                                        {
                                            detail.OverdueDate = detail.ProductDate.Value.AddYears(item.ValidityDayNum ?? 0);
                                        }
                                        detail.Qty = Convert.ToDecimal(cell.Qty);
                                        detail.Lot = cell.Lot;
                                        detail.State = "New";
                                        detail.Spec = item.Spec;
                                        detail.ItemUnitText = item.ItemUnitText;
                                        //if (item.IsNeedCheck == "true")
                                        //{
                                        //    detail.CheckState = "WaitCheck";
                                        //}
                                        //else
                                        //{
                                        //    detail.CheckState = "UnNeed";
                                        //}
                                        detail.CheckState = cell.CheckState;
                                        detail.CurQty = 0;
                                        //detail.StationID = itemInStation.StationID;
                                        //detail.StoreAreaID = itemArea.AreaID;
                                        db.Update<T_InBoundDetailEntity>(detail);
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertInOrder(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess) //插入正常
                    {
                        db.CommitWithOutRollBack();
                    }
                    else
                    {
                        db.RollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult InsertInOrder(IRepositoryBase db, InOrderFaceModel model)
        {
            ERPResult result = new ERPResult();
            try
            {
                T_InBoundEntity existsEntity = new T_InBoundEntity();
                existsEntity.F_Id = Guid.NewGuid().ToString();
                existsEntity.InBoundCode = T_CodeGenApp.GenNum("InBoundRule");
                existsEntity.InBoundType = model.InBoundType;
                existsEntity.State = "New";
                existsEntity.RefOrderCode = model.RefOrderCode;
                existsEntity.ERPInDocCode = model.RefOrderCode;
                existsEntity.GenType = "ERP";
                existsEntity.TransState = "New";
                existsEntity.F_DeleteMark = false;

                existsEntity.StationID = "";
                T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == model.SupplierCode && o.F_DeleteMark == false);
                if (supplier == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "供应商不存在";
                    return result;
                }
                existsEntity.SupplierUserID = supplier.F_Id;
                existsEntity.SupplierUserCode = supplier.SupplierCode;
                existsEntity.SupplierUserName = supplier.SupplierName;

                db.Insert<T_InBoundEntity>(existsEntity);
                db.SaveChanges();

                IList<InOrderFaceCellModel> items = model.Items;
                foreach (InOrderFaceCellModel cell in items)
                {
                    if (!(cell.SEQ ?? "").IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次不正确";
                        return result;
                    }

                    if (!(cell.Qty ?? "").IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "数量不正确";
                        return result;
                    }

                    if (!(cell.ProductDate ?? "").IsDate())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "生产日期不正确";
                        return result;
                    }

                    if (string.IsNullOrEmpty(cell.ItemCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料编码必填";
                        return result;
                    }

                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                    if (item == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料不存在";
                        return result;
                    }

                    if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料批号必填";
                        return result;
                    }

                    if (cell.CheckState == "UnQua")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "不合格物料不可入库";
                        return result;
                    }

                    T_ERPWarehouseEntity erp = db.FindEntity<T_ERPWarehouseEntity>(o => o.ERPHouseCode == cell.ERPWarehouseCode && o.F_DeleteMark == false);
                    if (erp == null)
                    {
                        /// 新增ERP仓库
                        T_ERPWarehouseEntity erpEntity = new T_ERPWarehouseEntity();
                        erpEntity.F_Id = Guid.NewGuid().ToString();
                        erpEntity.ERPHouseCode = cell.ERPWarehouseCode;    /// 暂定：ERP编码和名称一致
                        erpEntity.ERPHouseName = cell.ERPWarehouseCode;
                        erpEntity.F_DeleteMark = false;
                        db.Insert<T_ERPWarehouseEntity>(erpEntity);
                    }


                    T_InBoundDetailEntity detail = db.FindEntity<T_InBoundDetailEntity>(o => o.InBoundID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                    if (detail != null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次已存在：" + cell.SEQ;
                        return result;
                    }

                    if (cell.CheckState != "Qua"
                        && cell.CheckState != "UnNeed"
                        && cell.CheckState != "UnQua"
                        && cell.CheckState != "WaitCheck")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "质检状态不正确：项次" + cell.SEQ;
                        return result;
                    }

                    detail = new T_InBoundDetailEntity();
                    detail.F_Id = Guid.NewGuid().ToString();
                    detail.InBoundID = existsEntity.F_Id;
                    detail.SEQ = Convert.ToInt32(cell.SEQ);

                    detail.ItemID = item.F_Id;
                    detail.ItemName = item.ItemName;
                    detail.ItemCode = item.ItemCode;
                    detail.ERPWarehouseCode = cell.ERPWarehouseCode;
                    detail.Factory = item.Factory;
                    detail.ProductDate = Convert.ToDateTime(cell.ProductDate);
                    if (item.ValidityUnitType == "Day")
                    {
                        detail.OverdueDate = detail.ProductDate.Value.AddDays(item.ValidityDayNum ?? 0);
                    }
                    else if (item.ValidityUnitType == "Month")
                    {
                        detail.OverdueDate = detail.ProductDate.Value.AddMonths(item.ValidityDayNum ?? 0);
                    }
                    else if (item.ValidityUnitType == "Year")
                    {
                        detail.OverdueDate = detail.ProductDate.Value.AddYears(item.ValidityDayNum ?? 0);
                    }
                    detail.Qty = Convert.ToDecimal(cell.Qty);
                    detail.Lot = cell.Lot;
                    detail.State = "New";
                    detail.Spec = item.Spec;
                    detail.ItemUnitText = item.ItemUnitText;
                    detail.CheckState = cell.CheckState;
                    detail.CurQty = 0;
                    detail.ActionType = "Init";
                    detail.IsMustQtySame = string.IsNullOrEmpty(cell.IsMustQtySame) ? "true" : cell.IsMustQtySame;

                    T_ItemInStationEntity itemInStation = db.FindEntity<T_ItemInStationEntity>(o => o.ItemID == item.F_Id);
                    T_ItemAreaEntity itemArea = db.FindEntity<T_ItemAreaEntity>(o => o.ItemID == item.F_Id);

                    detail.StationID = itemInStation == null ? null : itemInStation.StationID;
                    detail.StoreAreaID = itemArea == null ? null : itemArea.AreaID;
                    detail.OverInQty = 0;
                    detail.F_DeleteMark = false;
                    db.Insert<T_InBoundDetailEntity>(detail);
                }

                db.SaveChanges();

                if ((!string.IsNullOrEmpty(model.IsAuto)) && model.IsAuto.ToLower() == "true")
                {
                    InBoundDetailController inboundCtr = new InBoundDetailController();
                    T_InBoundDetailEntity inboundDetail = db.FindList<T_InBoundDetailEntity>(o => o.InBoundID == existsEntity.F_Id && o.State == "New").OrderBy(o => o.SEQ).FirstOrDefault();
                    if (inboundDetail != null)
                    {
                        AjaxResult ajaxRes = inboundCtr.ReceivePlayPause(db, inboundDetail.F_Id, "Play");
                        if ((ResultType)ajaxRes.state == ResultType.success)
                        {
                            result.IsSuccess = true;
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.FailMsg = ajaxRes.message;
                            return result;
                        }
                    }
                    else
                    {
                        result.IsSuccess = true;
                    }
                }
                else
                {
                    result.IsSuccess = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion 

        #region 出库单
        public ERPResult SyncOutOrder(OutOrderFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.RefOrderCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "ERP单据编码必填";
                        return result;
                    }

                    if (string.IsNullOrEmpty(model.OutBoundType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "单据类型必填";
                        return result;
                    }
                    else if (model.OutBoundType != "GetItemOut"
                          && model.OutBoundType != "WarehouseBackOut"
                          && model.OutBoundType != "VerBackOut"
                          && model.OutBoundType != "OtherOut")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "单据类型不正确";
                        return result;
                    }

                    T_OutBoundEntity existsEntity = db.FindEntity<T_OutBoundEntity>(o => o.RefOrderCode == model.RefOrderCode);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {
                                if (model.Items.Count < 1)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "单据明细不可为空";
                                    return result;
                                }

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "ERP单据编码已存在";
                                    return result;
                                }
                                else
                                {
                                    result = InsertOutOrder(db, model);
                                }
                            }
                            break;
                        case "2":/// 删除
                            {
                                if (existsEntity != null)
                                {
                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }

                                    string[] DelDetail;
                                    if (model.Items == null || model.Items.Count < 1) /// 删除整个单据
                                    {
                                        DelDetail = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == existsEntity.F_Id).Select(o => (o.SEQ ?? 0).ToString()).ToArray();
                                        db.Delete<T_OutBoundDetailEntity>(o => o.OutBoundID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                        db.Delete<T_OutBoundEntity>(existsEntity);
                                    }
                                    else /// 删除部分单据
                                    {
                                        DelDetail = model.Items.Select(o => o.SEQ).ToArray();
                                        db.Delete<T_OutBoundDetailEntity>(o => o.OutBoundID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                    }

                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = true;
                                    result.FailCode = "0000";
                                    result.FailMsg = "";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (existsEntity != null)
                                {
                                    if (model.Items.Count == 0)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据明细不能为空";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ReceiveDepartmentCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "接收部门编码不可为空";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ReceiveDepartmentName))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "接收部门编码名称不可为空";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ReceiveUserCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "接收人编码不可为空";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(model.ReceiveUserName))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "接收人名称不可为空";
                                        return result;
                                    }

                                    if (model.IsUrgent != "true" && model.IsUrgent != "false")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "紧急出库参数只能是true或false";
                                        return result;
                                    }

                                    if (model.OutBoundType != "GetItemOut")
                                    {
                                        if (model.IsUrgent == "true")
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "单据类型不支持紧急出库";
                                            return result;
                                        }
                                    }

                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }


                                    existsEntity.OutBoundType = model.OutBoundType;
                                    existsEntity.RefOrderCode = model.RefOrderCode;
                                    existsEntity.IsUrgent = model.IsUrgent;

                                    OrganizeEntity organize = db.FindEntity<OrganizeEntity>(o => o.F_EnCode == model.ReceiveDepartmentCode);
                                    if (organize == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "接收部门编码不存在";
                                        return result;
                                    }

                                    UserEntity recUser = db.FindEntity<UserEntity>(o => o.F_Account == model.ReceiveUserCode);
                                    if (recUser == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "接收人员编码不存在";
                                        return result;
                                    }

                                    existsEntity.ReceiveDepartment = organize.F_FullName;
                                    existsEntity.ReceiveDepartmentId = organize.F_Id;
                                    existsEntity.ReceiveUserName = recUser.F_RealName;
                                    existsEntity.ReceiveUserId = recUser.F_Id;

                                    db.Update<T_OutBoundEntity>(existsEntity);
                                    db.SaveChanges();

                                    IList<OutOrderFaceCellModel> items = model.Items;
                                    foreach (OutOrderFaceCellModel cell in items)
                                    {
                                        if (!(cell.SEQ ?? "").IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不正确";
                                            return result;
                                        }

                                        if (!(cell.Qty ?? "").IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "数量不正确";
                                            return result;
                                        }

                                        T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == cell.SupplierCode && o.F_DeleteMark == false);

                                        if (string.IsNullOrEmpty(cell.ItemCode))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料编码必填";
                                            return result;
                                        }

                                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                                        if (item == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料不存在";
                                            return result;
                                        }

                                        if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料批号必填";
                                            return result;
                                        }

                                        T_OutBoundDetailEntity detail = db.FindEntity<T_OutBoundDetailEntity>(o => o.OutBoundID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        if (detail == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不存在：" + cell.SEQ;
                                            return result;
                                        }

                                        detail.ItemID = item.F_Id;
                                        detail.ItemName = item.ItemName;
                                        detail.ItemCode = item.ItemCode;
                                        detail.Factory = item.Factory;
                                        detail.SupplierUserID = supplier == null ? null : supplier.F_Id;
                                        detail.SupplierUserName = supplier == null ? null : supplier.SupplierName;
                                        detail.SupplierCode = supplier == null ? null : supplier.SupplierCode;
                                        detail.Qty = Convert.ToDecimal(cell.Qty);
                                        detail.OutQty = 0;
                                        detail.WaveQty = 0;
                                        detail.Lot = cell.Lot;
                                        detail.Spec = item.Spec;
                                        detail.ItemUnitText = item.ItemUnitText;
                                        detail.State = "New";
                                        detail.SourceInOrderCode = cell.SourceInOrderCode;
                                        /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                                        T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == item.F_Id && o.Lot == cell.Lot);
                                        if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                                        T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                        string containerKind = containerType.ContainerKind;
                                        if (containerKind == "Rack")
                                        {
                                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                            detail.StationID = station.F_Id;
                                            detail.StationCode = station.StationCode;
                                        }
                                        else if (containerKind == "Plastic" || containerKind == "Box")
                                        {
                                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                            detail.StationID = station.F_Id;
                                            detail.StationCode = station.StationCode;
                                        }
                                        else
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料的容器类型未知:" + item.ItemCode;
                                            return result;
                                        }

                                        db.Update<T_OutBoundDetailEntity>(detail);
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertOutOrder(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }

        }

        private ERPResult InsertOutOrder(IRepositoryBase db, OutOrderFaceModel model)
        {
            ERPResult result = new ERPResult();
            try
            {
                if (string.IsNullOrEmpty(model.ReceiveDepartmentCode))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "接收部门编码不可为空";
                    return result;
                }

                if (string.IsNullOrEmpty(model.ReceiveDepartmentName))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "接收部门编码名称不可为空";
                    return result;
                }

                if (string.IsNullOrEmpty(model.ReceiveUserCode))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "接收人编码不可为空";
                    return result;
                }

                if (string.IsNullOrEmpty(model.ReceiveUserName))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "接收人名称不可为空";
                    return result;
                }

                OrganizeEntity organize = db.FindEntity<OrganizeEntity>(o => o.F_EnCode == model.ReceiveDepartmentCode);
                if (organize == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "接收部门编码不存在";
                    return result;
                }

                UserEntity recUser = db.FindEntity<UserEntity>(o => o.F_Account == model.ReceiveUserCode);
                if (recUser == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "接收人员编码不存在";
                    return result;
                }

                if (model.IsUrgent != "true" && model.IsUrgent != "false")
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "紧急出库参数只能是true或false";
                    return result;
                }

                if (model.OutBoundType != "GetItemOut")
                {
                    if (model.IsUrgent == "true")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "单据类型不支持紧急出库";
                        return result;
                    }
                }


                T_OutBoundEntity existsEntity = new T_OutBoundEntity();
                existsEntity.F_Id = Guid.NewGuid().ToString();

                existsEntity.ReceiveDepartment = organize.F_FullName;
                existsEntity.ReceiveDepartmentId = organize.F_Id;
                existsEntity.ReceiveUserName = recUser.F_RealName;
                existsEntity.ReceiveUserId = recUser.F_Id;

                existsEntity.OutBoundCode = T_CodeGenApp.GenNum("OutBoundRule");
                existsEntity.OutBoundType = model.OutBoundType;
                existsEntity.State = "New";
                existsEntity.RefOrderCode = model.RefOrderCode;
                existsEntity.GenType = "ERP";
                existsEntity.IsUrgent = model.IsUrgent;
                existsEntity.TransState = "New";
                existsEntity.F_DeleteMark = false;

                db.Insert<T_OutBoundEntity>(existsEntity);
                db.SaveChanges();

                IList<T_OutBoundDetailEntity> detailList = new List<T_OutBoundDetailEntity>();
                IList<OutOrderFaceCellModel> items = model.Items;
                foreach (OutOrderFaceCellModel cell in items)
                {
                    if (!(cell.SEQ ?? "").IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次不正确";
                        return result;
                    }

                    if (!(cell.Qty ?? "").IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "数量不正确";
                        return result;
                    }

                    T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == cell.SupplierCode && o.F_DeleteMark == false);

                    if (string.IsNullOrEmpty(cell.ItemCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料编码必填";
                        return result;
                    }

                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                    if (item == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料不存在";
                        return result;
                    }

                    if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料批号必填";
                        return result;
                    }

                    T_OutBoundDetailEntity detail = db.FindEntity<T_OutBoundDetailEntity>(o => o.OutBoundID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                    if (detail != null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次已存在：" + cell.SEQ;
                        return result;
                    }

                    detail = new T_OutBoundDetailEntity();
                    detail.F_Id = Guid.NewGuid().ToString();
                    detail.OutBoundID = existsEntity.F_Id;
                    detail.SEQ = Convert.ToInt32(cell.SEQ);
                    detail.ItemID = item.F_Id;
                    detail.ItemName = item.ItemName;
                    detail.ItemCode = item.ItemCode;
                    detail.Factory = item.Factory;
                    detail.SupplierUserID = supplier == null ? null : supplier.F_Id;
                    detail.SupplierUserName = supplier == null ? null : supplier.SupplierName;
                    detail.SupplierCode = supplier == null ? null : supplier.SupplierCode;
                    detail.Qty = Convert.ToDecimal(cell.Qty);
                    detail.OutQty = 0;
                    detail.WaveQty = 0;
                    detail.Lot = cell.Lot;
                    detail.Spec = item.Spec;
                    detail.ItemUnitText = item.ItemUnitText;
                    detail.State = "New";
                    detail.ActionType = "Init";
                    detail.SourceInOrderCode = cell.SourceInOrderCode;
                    /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                    T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == item.F_Id && o.Lot == cell.Lot);
                    if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                    string containerKind = containerType.ContainerKind;
                    if (containerKind == "Rack")
                    {
                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                        detail.StationID = station.F_Id;
                        detail.StationCode = station.StationCode;
                    }
                    else if (containerKind == "Plastic" || containerKind == "Box")
                    {
                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                        detail.StationID = station.F_Id;
                        detail.StationCode = station.StationCode;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料的容器类型未知:" + item.ItemCode;
                        return result;
                    }

                    detailList.Add(detail);
                    db.Insert<T_OutBoundDetailEntity>(detail);
                }
                db.SaveChanges();

                bool isAuto = false;
                if (model.OutBoundType == "GetItemOut"
                    || model.OutBoundType == "VerBackOut"
                    || model.OutBoundType == "WarehouseBackOut"
                    || model.OutBoundType == "OtherOut")
                {
                    if (RuleConfig.ERPInterfaceRule.ERPInterfaceOutOrder.GetItemOutERPAutoOut && model.OutBoundType == "GetItemOut")
                    {
                        isAuto = true;
                    }
                    else if (RuleConfig.ERPInterfaceRule.ERPInterfaceOutOrder.VerBackOutERPAutoOut && model.OutBoundType == "VerBackOut")
                    {
                        isAuto = true;
                    }
                    else if (RuleConfig.ERPInterfaceRule.ERPInterfaceOutOrder.WarehouseBackOutERPAutoOut && model.OutBoundType == "WarehouseBackOut")
                    {
                        isAuto = true;
                    }
                    else if (RuleConfig.ERPInterfaceRule.ERPInterfaceOutOrder.OtherOutERPAutoOut && model.OutBoundType == "OtherOut")
                    {
                        isAuto = true;
                    }
                    else
                    {
                        isAuto = false;
                    }
                }
                else
                {
                    isAuto = (!string.IsNullOrEmpty(model.IsAuto)) && model.IsAuto.ToLower() == "true";
                }

                bool isOrderAuto = (model.IsAuto != null && model.IsAuto.ToLower() == "true") ? true : false;
                if (isAuto && isOrderAuto)
                {
                    lock (lockObj)
                    {
                        IList<T_OutBoundDetailEntity> emptyStationList = detailList.Where(o => string.IsNullOrEmpty(o.StationID)).ToList();
                        if (emptyStationList.Count > 0)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "项次[" + string.Join(",", emptyStationList.Select(o => o.SEQ).OrderBy(o => o.Value).ToList().ToArray()) + "]未指定出库站台";
                            return result;
                        }

                        IList<string> stationArray = detailList.Select(o => o.StationID).Distinct().ToArray();
                        IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => stationArray.Contains(o.F_Id));

                        stationList = stationList.Where(o => string.IsNullOrEmpty(o.CurOrderID)).ToList();
                        if (stationList.Count != stationArray.Count)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "站台已被占用";
                            return result;
                        }

                        /// 波次运算
                        string[] needOutArray = detailList.Select(o => o.F_Id).ToArray();
                        T_OutRecordApp outRecApp = new T_OutRecordApp();
                        AjaxResult rst = outRecApp.WaveGen(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                        if ((ResultType)rst.state != ResultType.success)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = rst.message;
                            return result;
                        }

                        /// 发送任务给WCS
                        IList<string> outBoundDetailIDList = detailList.Select(o => o.F_Id).ToList();
                        rst = outRecApp.OutDetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                        if ((ResultType)rst.state != ResultType.success)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = rst.message;
                            return result;
                        }
                        db.SaveChanges();
                    }
                }
                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 质检单
        public ERPResult SyncQAOutOrder(QAOutOrderFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.RefOrderCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "ERP单据编码必填";
                        return result;
                    }


                    T_QAEntity existsEntity = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == model.RefOrderCode);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {
                                if (model.Items.Count < 1)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "单据明细不可为空";
                                    return result;
                                }

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "ERP单据编码已存在";
                                    return result;
                                }

                                result = InsertQAOutOrder(db, model);

                            }
                            break;
                        case "2":/// 删除
                            {
                                if (existsEntity != null)
                                {
                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }

                                    string[] DelDetail;
                                    if (model.Items == null || model.Items.Count < 1) /// 删除整个单据
                                    {
                                        DelDetail = db.FindList<T_QADetailEntity>(o => o.QAID == existsEntity.F_Id).Select(o => (o.SEQ ?? 0).ToString()).ToArray();
                                        db.Delete<T_QADetailEntity>(o => o.QAID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                        db.Delete<T_QAEntity>(existsEntity);
                                    }
                                    else /// 删除部分单据
                                    {
                                        DelDetail = model.Items.Select(o => o.SEQ).ToArray();
                                        db.Delete<T_QADetailEntity>(o => o.QAID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                    }

                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = true;
                                    result.FailCode = "0000";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (existsEntity != null)
                                {
                                    if (model.Items.Count < 1)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据明细不可为空";
                                        return result;
                                    }

                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }

                                    if (string.IsNullOrEmpty(existsEntity.RefInBoundCode))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "ERP对应入库单来源编码不可为空";
                                        return result;
                                    }

                                    T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == existsEntity.RefInBoundCode);
                                    if (inbound == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "ERP对应入库单来源编码不存在";
                                        return result;
                                    }

                                    existsEntity.RefOrderCode = model.RefOrderCode;
                                    existsEntity.RefInBoundCode = model.RefInBoundCode;
                                    existsEntity.GenType = "ERP";
                                    existsEntity.F_DeleteMark = false;

                                    db.Update<T_QAEntity>(existsEntity);
                                    db.SaveChanges();

                                    IList<QAOutOrderFaceCellModel> items = model.Items;
                                    foreach (QAOutOrderFaceCellModel cell in items)
                                    {
                                        if (!(cell.SEQ ?? "").IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不正确";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(cell.SupplierCode))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "供应商编码不能为空";
                                            return result;
                                        }

                                        T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == cell.SupplierCode && o.F_DeleteMark == false);
                                        if (supplier == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "供应商不存在";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(cell.ItemCode))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料编码必填";
                                            return result;
                                        }

                                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                                        if (item == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料不存在";
                                            return result;
                                        }

                                        if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料批号必填";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(cell.IsBroken))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "是否破坏性质检必填";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(cell.IsAppearQA))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "是否外观质检必填";
                                            return result;
                                        }

                                        if (cell.IsAppearQA == "true")
                                        {
                                            if (!(cell.SampleSumCnt ?? "").IsNaturalNum())
                                            {
                                                result.IsSuccess = false;
                                                result.FailCode = "0001";
                                                result.FailMsg = "取样标签个数不正确";
                                                return result;
                                            }
                                        }
                                        else if (cell.IsAppearQA == "false")
                                        {
                                            if (!(cell.SampleSumNum ?? "").IsNaturalNum())
                                            {
                                                result.IsSuccess = false;
                                                result.FailCode = "0001";
                                                result.FailMsg = "取样总数量不正确";
                                                return result;
                                            }
                                        }
                                        else
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "是否外观质检参数填写错误";
                                            return result;
                                        }


                                        T_QADetailEntity detail = db.FindEntity<T_QADetailEntity>(o => o.QAID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        if (detail == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不存在：" + cell.SEQ;
                                            return result;
                                        }

                                        detail.ItemID = item.F_Id;
                                        detail.ItemName = item.ItemName;
                                        detail.ItemCode = item.ItemCode;
                                        detail.Factory = item.Factory;
                                        detail.SupplierUserID = supplier.F_Id;
                                        detail.SupplierUserName = supplier.SupplierName;
                                        detail.SupplierCode = supplier.SupplierCode;
                                        detail.QAResult = "WaitCheck";
                                        detail.ResultState = "New";
                                        detail.IsBroken = cell.IsBroken;
                                        detail.IsAppearQA = cell.IsAppearQA;
                                        if (cell.IsAppearQA == "true")
                                        {
                                            detail.SampleSumNum = 0;
                                            detail.SampleSumCnt = Convert.ToDecimal(cell.SampleSumCnt);
                                        }
                                        else
                                        {
                                            detail.SampleSumNum = Convert.ToDecimal(cell.SampleSumNum);
                                            detail.SampleSumCnt = 0;
                                        }
                                        detail.ResultSendTime = null;
                                        detail.SampleType = "Auto";
                                        detail.Lot = cell.Lot;
                                        detail.Spec = item.Spec;
                                        detail.ItemUnitText = item.ItemUnitText;
                                        detail.State = "New";
                                        /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                                        T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == item.F_Id && o.Lot == cell.Lot);
                                        if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                                        T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                        string containerKind = containerType.ContainerKind;
                                        if (containerKind == "Rack")
                                        {
                                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                            detail.StationID = station.F_Id;
                                        }
                                        else if (containerKind == "Plastic" || containerKind == "Box")
                                        {
                                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                            detail.StationID = station.F_Id;
                                        }
                                        else
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料的容器类型未知:" + item.ItemCode;
                                            return result;
                                        }

                                        db.Update<T_QADetailEntity>(detail);
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertQAOutOrder(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult InsertQAOutOrder(IRepositoryBase db, QAOutOrderFaceModel model)
        {
            ERPResult result = new ERPResult();

            if (string.IsNullOrEmpty(model.RefInBoundCode))
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "ERP对应入库单来源编码不可为空";
                return result;
            }

            T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == model.RefInBoundCode);
            if (inbound == null)
            {
                result.IsSuccess = false;
                result.FailCode = "0001";
                result.FailMsg = "ERP对应入库单来源编码不存在";
                return result;
            }

            try
            {
                T_QAEntity existsEntity = new T_QAEntity();
                existsEntity.F_Id = Guid.NewGuid().ToString();
                existsEntity.QACode = T_CodeGenApp.GenNum("QARule");
                existsEntity.QAOrderType = "GetSample";
                existsEntity.RefOrderCode = model.RefOrderCode;
                existsEntity.RefInBoundCode = model.RefInBoundCode;
                existsEntity.GenType = "ERP";
                existsEntity.State = "New";
                existsEntity.TransState = "New";
                existsEntity.F_DeleteMark = false;

                db.Insert<T_QAEntity>(existsEntity);
                db.SaveChanges();

                IList<QAOutOrderFaceCellModel> items = model.Items;
                foreach (QAOutOrderFaceCellModel cell in items)
                {
                    if (!(cell.SEQ ?? "").IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次不正确";
                        return result;
                    }

                    if (string.IsNullOrEmpty(cell.SupplierCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "供应商编码不能为空";
                        return result;
                    }

                    T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == cell.SupplierCode && o.F_DeleteMark == false);
                    if (supplier == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "供应商不存在";
                        return result;
                    }

                    if (string.IsNullOrEmpty(cell.ItemCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料编码必填";
                        return result;
                    }

                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                    if (item == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料不存在";
                        return result;
                    }

                    if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料批号必填";
                        return result;
                    }

                    if (string.IsNullOrEmpty(cell.IsBroken))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "是否破坏性质检必填";
                        return result;
                    }

                    if (string.IsNullOrEmpty(cell.IsAppearQA))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "是否外观质检必填";
                        return result;
                    }

                    if (cell.IsAppearQA == "true")
                    {
                        if (!(cell.SampleSumCnt ?? "").IsNaturalNum())
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "取样标签个数不正确";
                            return result;
                        }
                    }
                    else if (cell.IsAppearQA == "false")
                    {
                        if (!(cell.SampleSumNum ?? "").IsNaturalNum())
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "取样总数量不正确";
                            return result;
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "是否外观质检参数填写错误";
                        return result;
                    }


                    T_QADetailEntity detail = db.FindEntity<T_QADetailEntity>(o => o.QAID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                    if (detail != null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次已存在：" + cell.SEQ;
                        return result;
                    }

                    detail = new T_QADetailEntity();
                    detail.F_Id = Guid.NewGuid().ToString();
                    detail.QAID = existsEntity.F_Id;
                    detail.SEQ = Convert.ToInt32(cell.SEQ);

                    detail.ItemID = item.F_Id;
                    detail.ItemName = item.ItemName;
                    detail.ItemCode = item.ItemCode;
                    detail.Factory = item.Factory;
                    detail.SupplierUserID = supplier.F_Id;
                    detail.SupplierUserName = supplier.SupplierName;
                    detail.SupplierCode = supplier.SupplierCode;
                    detail.QAResult = "WaitCheck";
                    detail.ResultState = "New";
                    detail.IsBroken = cell.IsBroken;
                    detail.IsAppearQA = cell.IsAppearQA;
                    if (cell.IsAppearQA == "true")
                    {
                        detail.SampleSumNum = 0;
                        detail.SampleSumCnt = Convert.ToDecimal(cell.SampleSumCnt);
                    }
                    else
                    {
                        detail.SampleSumNum = Convert.ToDecimal(cell.SampleSumNum);
                        detail.SampleSumCnt = 0;
                    }
                    detail.ResultSendTime = null;
                    detail.SampleType = "Auto";
                    detail.Lot = cell.Lot;
                    detail.Spec = item.Spec;
                    detail.ItemUnitText = item.ItemUnitText;
                    detail.State = "New";
                    detail.ActionType = "Init";
                    detail.F_DeleteMark = false;
                    /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                    T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == item.F_Id && o.Lot == cell.Lot);
                    if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                    string containerKind = containerType.ContainerKind;
                    if (containerKind == "Rack")
                    {
                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                        detail.StationID = station.F_Id;
                    }
                    else if (containerKind == "Plastic" || containerKind == "Box")
                    {
                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                        detail.StationID = station.F_Id;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料的容器类型未知:" + item.ItemCode;
                        return result;
                    }

                    db.Insert<T_QADetailEntity>(detail);
                }

                db.SaveChanges();

                if ((!string.IsNullOrEmpty(model.IsAuto)) && model.IsAuto.ToLower() == "true")
                {
                    lock (qaLockObj)
                    {
                        /// 波次运算
                        AjaxResult rst = new AjaxResult();

                        rst = new QAGetController().WaveGen_QA_All(db, existsEntity.F_Id);
                        if ((ResultType)rst.state != ResultType.success)
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = rst.message;
                            return result;
                        }

                        /// 执行并发送任务
                        IList<T_QADetailEntity> qADetailList = db.FindList<T_QADetailEntity>(o => o.QAID == existsEntity.F_Id);
                        IList<string> outBoundDetailIDList = qADetailList.Select(o => o.F_Id).ToList();
                        rst = new T_QARecordApp().QADetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                        if ((ResultType)rst.state != ResultType.success)
                        {
                            db.RollBack();
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = rst.message;
                            return result;
                        }

                        db.SaveChanges();
                    }
                }

                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 盘点单
        public ERPResult SyncCountOrder(CountOrderFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.RefOrderCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "ERP单据编码必填";
                        return result;
                    }

                    T_CountEntity existsEntity = db.FindEntity<T_CountEntity>(o => o.RefOrderCode == model.RefOrderCode);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {
                                if (model.Items.Count == 0)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "单据明细不能为空";
                                    return result;
                                }

                                if (existsEntity != null)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "ERP单据编码已存在";
                                    return result;
                                }
                                else
                                {
                                    result = InsertCountOrder(db, model);
                                }
                            }
                            break;
                        case "2": /// 删除
                            {
                                if (existsEntity != null)
                                {
                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }

                                    string[] DelDetail;
                                    if (model.Items == null || model.Items.Count == 0) /// 删除整个单据
                                    {
                                        DelDetail = db.FindList<T_CountDetailEntity>(o => o.CountID == existsEntity.F_Id).Select(o => (o.SEQ ?? 0).ToString()).ToArray();
                                        db.Delete<T_CountDetailEntity>(o => o.CountID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                        db.Delete<T_CountEntity>(existsEntity);
                                    }
                                    else /// 删除部分单据
                                    {
                                        DelDetail = model.Items.Select(o => o.SEQ).ToArray();
                                        db.Delete<T_CountDetailEntity>(o => o.CountID == existsEntity.F_Id && DelDetail.Contains((o.SEQ ?? 0).ToString()));
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = true;
                                    result.FailCode = "0000";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (existsEntity != null)
                                {
                                    if (model.Items.Count == 0)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据明细不能为空";
                                        return result;
                                    }

                                    if (existsEntity.State != "New")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "单据不是新建状态";
                                        return result;
                                    }

                                    existsEntity.State = "New";
                                    existsEntity.RefOrderCode = model.RefOrderCode;
                                    existsEntity.GenType = "ERP";
                                    if (!(model.CountMethod == "ByItem" || model.CountMethod == "ByLocation"))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "指定盘点方法不正确";
                                        return result;
                                    }
                                    existsEntity.CountMethod = model.CountMethod;

                                    T_ERPWarehouseEntity erpWarehouse = db.FindEntity<T_ERPWarehouseEntity>(o => o.ERPHouseCode == model.ERPWarehouseCode && o.F_DeleteMark == false);
                                    if (erpWarehouse == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "指定ERP不存在";
                                        return result;
                                    }
                                    existsEntity.ERPHouseCode = model.ERPWarehouseCode;
                                    existsEntity.State = "New";
                                    existsEntity.AuditState = "WaitAudit";
                                    existsEntity.IsOpen = model.IsOpen;
                                    existsEntity.Remark = model.Remark;
                                    existsEntity.F_DeleteMark = false;

                                    db.Update<T_CountEntity>(existsEntity);
                                    db.SaveChanges();

                                    IList<CountOrderFaceCellModel> items = model.Items;
                                    if (items.Count == 0)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "盘点明细列表不能为空";
                                        return result;
                                    }

                                    foreach (CountOrderFaceCellModel cell in items)
                                    {
                                        if (!(cell.SEQ ?? "").IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不正确";
                                            return result;
                                        }

                                        T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == cell.SupplierCode && o.F_DeleteMark == false);

                                        if (cell.ERPQty == null || cell.ERPQty == 0)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "ERP账目数量不可为空或为0";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(cell.ItemCode))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料编码必填";
                                            return result;
                                        }
                                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                                        if (item == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料不存在";
                                            return result;
                                        }

                                        if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料批号必填";
                                            return result;
                                        }



                                        T_CountDetailEntity detail = db.FindEntity<T_CountDetailEntity>(o => o.CountID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        if (detail != null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = $"项次已存在：{cell.SEQ}";
                                            return result;
                                        }

                                        detail.ItemID = item.F_Id;
                                        detail.ItemName = item.ItemName;
                                        detail.ItemCode = item.ItemCode;
                                        detail.Factory = item.Factory;
                                        detail.Qty = 0;
                                        detail.CountQty = 0;
                                        detail.SupplierUserID = supplier == null ? null : supplier.F_Id;
                                        detail.SupplierUserName = supplier == null ? null : supplier.SupplierName;
                                        detail.CountState = "New";
                                        detail.AuditState = "WaitAudit";
                                        detail.Lot = cell.Lot;
                                        detail.Spec = item.Spec;
                                        detail.ItemUnitText = item.ItemUnitText;
                                        detail.F_DeleteMark = false;
                                        detail.ERPQty = cell.ERPQty;
                                        /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                                        T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == item.F_Id && o.Lot == cell.Lot);
                                        if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                                        T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                        string containerKind = containerType.ContainerKind;
                                        if (containerKind == "Rack")
                                        {
                                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                            detail.StationID = station.F_Id;
                                        }
                                        else if (containerKind == "Plastic")
                                        {
                                            T_StationEntity t_Station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                            detail.StationID = t_Station.F_Id;
                                        }
                                        else if (containerKind == "Box")
                                        {
                                            // 纸箱
                                        }
                                        else
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = $"未知的容器类型：{containerKind}";
                                            return result;
                                        }

                                        db.Update<T_CountDetailEntity>(detail);
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertCountOrder(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult InsertCountOrder(IRepositoryBase db, CountOrderFaceModel model)
        {
            ERPResult result = new ERPResult();
            try
            {
                T_CountEntity existsEntity = new T_CountEntity();
                existsEntity.F_Id = Guid.NewGuid().ToString();
                existsEntity.CountCode = T_CodeGenApp.GenNum("CountRule");
                existsEntity.RefOrderCode = model.RefOrderCode;
                existsEntity.GenType = "ERP";
                if (!(model.CountMethod == "ByItem" || model.CountMethod == "ByLocation"))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "指定盘点方法不正确";
                    return result;
                }
                existsEntity.CountMethod = model.CountMethod;

                T_ERPWarehouseEntity erpWarehouse = db.FindEntity<T_ERPWarehouseEntity>(o => o.ERPHouseCode == model.ERPWarehouseCode && o.F_DeleteMark == false);
                if (erpWarehouse == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "指定ERP不存在";
                    return result;
                }
                existsEntity.ERPHouseCode = model.ERPWarehouseCode;
                existsEntity.State = "New";
                existsEntity.AuditState = "WaitAudit";
                existsEntity.IsOpen = model.IsOpen;
                existsEntity.Remark = model.Remark;
                existsEntity.AreaType = "Tunnel";
                existsEntity.CountMode = "GoodsToPeople";
                existsEntity.AuditResult = "WaitApply";
                existsEntity.F_DeleteMark = false;

                db.Insert<T_CountEntity>(existsEntity);
                db.SaveChanges();

                IList<CountOrderFaceCellModel> items = model.Items;
                foreach (CountOrderFaceCellModel cell in items)
                {
                    if (!(cell.SEQ ?? "").IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次不正确";
                        return result;
                    }

                    if (cell.ERPQty == null || cell.ERPQty == 0)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "ERP账目数量不可为空或为0";
                        return result;
                    }

                    T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == cell.SupplierCode && o.F_DeleteMark == false);

                    if (string.IsNullOrEmpty(cell.ItemCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料编码必填";
                        return result;
                    }
                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                    if (item == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料不存在";
                        return result;
                    }

                    if (string.IsNullOrEmpty(item.ContainerType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料未设置容器类型，需在WMS物料模块进行维护";
                        return result;
                    }

                    if (item.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料批号必填";
                        return result;
                    }

                    T_CountDetailEntity detail = db.FindEntity<T_CountDetailEntity>(o => o.CountID == existsEntity.F_Id && (o.SEQ ?? 0).ToString() == cell.SEQ);
                    if (detail != null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"项次已存在：{cell.SEQ}";
                        return result;
                    }

                    detail = new T_CountDetailEntity();
                    detail.F_Id = Guid.NewGuid().ToString();
                    detail.CountID = existsEntity.F_Id;
                    detail.SEQ = Convert.ToInt32(cell.SEQ);
                    detail.ItemID = item.F_Id;
                    detail.ItemName = item.ItemName;
                    detail.ItemCode = item.ItemCode;
                    detail.Factory = item.Factory;
                    detail.Qty = 0;
                    detail.CountQty = 0;
                    detail.ERPQty = cell.ERPQty;
                    detail.SupplierUserID = supplier == null ? null : supplier.F_Id;
                    detail.SupplierUserName = supplier == null ? null : supplier.SupplierName;
                    detail.CountState = "New";
                    detail.AuditState = "WaitAudit";
                    detail.Lot = cell.Lot;
                    detail.Spec = item.Spec;
                    detail.ItemUnitText = item.ItemUnitText;
                    detail.F_DeleteMark = false;
                    /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                    T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == item.F_Id && o.Lot == cell.Lot);
                    if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                    string containerKind = containerType.ContainerKind;
                    if (containerKind == "Rack")
                    {
                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                        detail.StationID = station.F_Id;
                    }
                    else if (containerKind == "Plastic")
                    {
                        T_StationEntity t_Station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                        detail.StationID = t_Station.F_Id;
                    }
                    else if (containerKind == "Box")
                    {
                        // 纸箱
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"未知的容器类型：{containerKind}";
                        return result;
                    }

                    db.Insert<T_CountDetailEntity>(detail);
                }

                db.SaveChanges();

                if ((!string.IsNullOrEmpty(model.IsAuto)) && model.IsAuto.ToLower() == "true")
                {
                    lock (countlockObj)
                    {
                        IList<string> TaskListNo = new List<string>();
                        AjaxResult res = new CountController().CountOnOff(db, existsEntity.F_Id, ref TaskListNo);
                        if ((ResultType)res.state != ResultType.success)
                        {
                            result.IsSuccess = false;
                            result.FailMsg = res.message;
                            return result;
                        }
                        else
                        {
                            /// 推送WCS任务
                            WCSResult wcsRes = new WCSPost().SendTask(db, TaskListNo);
                            if (wcsRes.IsSuccess)
                            {
                                result.IsSuccess = true;
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.FailMsg = res.message;
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    result.IsSuccess = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 质检结果反馈
        public ERPResult SyncQAResult(QAResultFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.RefOrderCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "ERP质检单编码必填";
                        return result;
                    }


                    IList<T_QAResultEntity> existsEntityList = db.FindList<T_QAResultEntity>(o => o.RefOrderCode == model.RefOrderCode);
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {
                                if (model.Items.Count < 1)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "质检明细不可为空";
                                    return result;
                                }

                                if (existsEntityList.Count() > 0)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "ERP质检单编码已存在";
                                    return result;
                                }
                                else
                                {
                                    result = InsertQAResult(db, model);
                                }
                            }
                            break;
                        case "2":/// 删除(有一个被应用，则所有不允许删除)
                            {
                                if (existsEntityList.Count() > 0)
                                {
                                    if (existsEntityList.Where(o => o.IsUsed == "true").Count() > 0)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "质检结果已被应用";
                                        return result;
                                    }

                                    if (model.Items.Count < 1) /// 删除整个结果
                                    {
                                        db.Delete<T_QAResultEntity>(o => o.RefOrderCode == model.RefOrderCode);
                                    }
                                    else /// 删除部分单据
                                    {
                                        foreach (QAResultFaceCellModel cell in model.Items)
                                        {
                                            db.Delete<T_QAResultEntity>(o => o.RefOrderCode == model.RefOrderCode && (o.SEQ ?? 0).ToString() == cell.SEQ);    /// ERP质检单单号+项次
                                        }
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "质检结果不存在";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (existsEntityList.Count() > 0)
                                {
                                    if (model.Items.Count == 0)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "结果明细不能为空";
                                        return result;
                                    }
                                    if (existsEntityList.Any(o => o.IsUsed == "true"))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "质检结果已被应用";
                                        return result;
                                    }

                                    /// 获取质检单
                                    T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == model.RefOrderCode && o.QAOrderType == "GetSample");
                                    if (qAEntity == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "质检单不存在";
                                        return result;
                                    }
                                    if (qAEntity.State == "Over")
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "质检单据已结束";
                                        return result;
                                    }
                                    if (!(qAEntity.State == "Picked" || qAEntity.State == "WaitReturn"
                                        || qAEntity.State == "WaitResult" || qAEntity.State == "WaitApply"))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "质检单据未完成取样";
                                        return result;
                                    }

                                    /// 对应还样单
                                    T_QAEntity qaReturnOrder = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == qAEntity.QACode && o.QAOrderType == "BackSample");
                                    /// 质检单对应入库单
                                    T_InBoundEntity inBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == qAEntity.RefInBoundCode);
                                    if (inBoundEntity == null)
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "未找到对应入库单";
                                        return result;
                                    }

                                    /// 获取质检单明细
                                    List<T_QADetailEntity> detailListAll = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id).ToList();
                                    foreach (QAResultFaceCellModel cell in model.Items)
                                    {
                                        if (string.IsNullOrEmpty(cell.SEQ) || !(cell.SEQ ?? "").IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不正确";
                                            return result;
                                        }

                                        T_QADetailEntity qADetail = detailListAll.FirstOrDefault(o => (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        if (qADetail == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "不存在质检明细";
                                            return result;
                                        }

                                        //if (string.IsNullOrEmpty(cell.SupplierCode) || cell.SupplierCode != qADetail.SupplierCode)
                                        //{
                                        //    result.IsSuccess = false;
                                        //    result.FailCode = "0001";
                                        //    result.FailMsg = "供应商错误";
                                        //    return result;
                                        //}

                                        if (string.IsNullOrEmpty(cell.ItemCode) || cell.ItemCode != qADetail.ItemCode)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料编码错误";
                                            return result;
                                        }

                                        T_ItemEntity itemEntity = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                                        if (itemEntity == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料不存在";
                                            return result;
                                        }

                                        if (itemEntity.IsMustLot == "true" && string.IsNullOrEmpty(cell.Lot))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料批号必填";
                                            return result;
                                        }

                                        if (itemEntity.IsMustLot == "true" && cell.Lot != qADetail.Lot)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料批号错误";
                                            return result;
                                        }

                                        if (cell.QAResult != "Qua" && cell.QAResult != "UnQua")
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "质检结果参数不正确";
                                            return result;
                                        }

                                        T_QAResultEntity existsEntity = db.FindEntity<T_QAResultEntity>(o => o.RefOrderCode == model.RefOrderCode && (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        if (existsEntity == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "结果明细不存在，单据:" + model.RefOrderCode + ",项次:" + cell.SEQ;
                                            return result;
                                        }

                                        existsEntity.RefOrderCode = model.RefOrderCode;
                                        existsEntity.QAResult = cell.QAResult; // "New";

                                        existsEntity.ItemID = itemEntity.F_Id;
                                        existsEntity.ItemCode = itemEntity.ItemCode;
                                        existsEntity.ItemName = itemEntity.ItemName;
                                        existsEntity.Lot = cell.Lot;
                                        existsEntity.SEQ = Convert.ToInt32(cell.SEQ);
                                        existsEntity.F_CreatorTime = DateTime.Now;
                                        existsEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                                        existsEntity.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                                        existsEntity.AccessTime = DateTime.Now;
                                        existsEntity.F_DeleteMark = false;

                                        existsEntity.QAID = qADetail.QAID;
                                        existsEntity.QADetailID = qADetail.F_Id;
                                        db.SaveChanges();

                                        if (RuleConfig.QARule.QAResultRule.AutoUsedQAResult) /// 判断是否自动应用质检结果
                                        {
                                            existsEntity.IsUsed = "true";

                                            /// 更新库存质检状态
                                            List<T_ContainerDetailEntity> detailList = new List<T_ContainerDetailEntity>();
                                            if (string.IsNullOrEmpty(qADetail.Lot))
                                            {
                                                detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == inBoundEntity.F_Id);
                                            }
                                            else
                                            {
                                                detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && o.Lot == qADetail.Lot && o.InBoundID == inBoundEntity.F_Id);
                                            }

                                            /// 结合质检取样记录，解冻库存
                                            var recordList = db.FindList<T_QARecordEntity>(o => o.QADetailID == qADetail.F_Id);
                                            foreach (T_ContainerDetailEntity cd in detailList)
                                            {
                                                if (existsEntity.QAResult == "Qua")
                                                {
                                                    cd.CheckState = "Qua";   /// 合格
                                                }
                                                else if (existsEntity.QAResult == "UnQua")
                                                {
                                                    cd.CheckState = "UnQua";  /// 不合格
                                                }
                                                else
                                                {
                                                    result.IsSuccess = false;
                                                    result.FailCode = "0001";
                                                    result.FailMsg = "未知的质检结果参数";
                                                    return result;
                                                }

                                                T_QARecordEntity res = recordList.Find(o => o.ContainerDetailID == cd.F_Id);
                                                if (res != null && res.IsReturnOver == "false" && res.IsAppearQA == "false")  /// 结果录入时，只有一种库存不能解冻：1.已取样 2.未还样 3.取样质检（不是外观质检）
                                                {
                                                }
                                                else
                                                {
                                                    cd.IsCheckFreeze = "false";
                                                }
                                                db.Update<T_ContainerDetailEntity>(cd);
                                            }

                                            /// 修改明细质检状态
                                            qADetail.ResultState = "ERP";
                                            qADetail.QAResult = existsEntity.QAResult;
                                            qADetail.ResultSendTime = existsEntity.AccessTime;
                                            qADetail.SampleType = "Auto";
                                            db.SaveChanges();

                                            /// 同步修改还样单质检结果
                                            if (qaReturnOrder != null) /// 存在质检还样单
                                            {
                                                if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false") /// 非破坏性质检，且需要还样明细
                                                {
                                                    T_QADetailEntity qaDetailReturn = db.FindEntity<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id && o.SEQ == qADetail.SEQ);
                                                    if (qaDetailReturn.State == "WaitResult" || qaDetailReturn.State == "WaitApply")
                                                    {
                                                        qaDetailReturn.State = "Over";   /// 待录入结果/待应用结果 ---> 结束
                                                    }
                                                    qaDetailReturn.ResultState = "ERP";
                                                    qaDetailReturn.QAResult = existsEntity.QAResult;
                                                    qaDetailReturn.ResultSendTime = existsEntity.AccessTime;
                                                    qaDetailReturn.SampleType = "Auto";
                                                    db.Update<T_QADetailEntity>(qaDetailReturn);
                                                }

                                                /// 还样单据
                                                List<T_QADetailEntity> qADetailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id).ToList();
                                                if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitApply"))
                                                {
                                                    qaReturnOrder.State = "Over";  /// 部分结束/待应用结果 ---> 全部结束
                                                }
                                                else if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitApply" || o.State == "WaitResult"))
                                                {
                                                    qaReturnOrder.State = "WaitResult";    /// 部分结束/待应用结果/待录入结果 ---> 待录入结果
                                                }
                                                db.Update<T_QAEntity>(qaReturnOrder);
                                            }

                                            if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false" && qaReturnOrder == null) qADetail.State = "WaitReturn";
                                            else qADetail.State = "Over";
                                            db.Update<T_QADetailEntity>(qADetail);
                                            db.SaveChanges();

                                            /// 判断单据关闭
                                            if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply"))
                                            {
                                                qAEntity.State = "Over";  /// 部分结束/待应用结果 ---> 全部结束
                                            }
                                            else if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply" || o.State == "WaitResult"))
                                            {
                                                qAEntity.State = "WaitResult";    /// 部分结束/待应用结果/待录入结果 ---> 待录入结果
                                            }
                                            else if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply" || o.State == "WaitReturn"))
                                            {
                                                qAEntity.State = "WaitReturn"; /// 部分结束/待应用结果/待还样 ---> 待还样
                                            }
                                            db.Update<T_QAEntity>(qAEntity);
                                            db.SaveChanges();

                                            /// 生成验退单：取样单结束 && 还样单结束或未生成
                                            if (qAEntity.State == "Over" && (qaReturnOrder == null || qaReturnOrder.State == "Over") && inBoundEntity.State == "Over")
                                            {
                                                /// 不合格明细
                                                List<T_QADetailEntity> unQuaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id && o.QAResult == "UnQua");
                                                if (unQuaDetailList.Count != 0)
                                                {
                                                    /// 验退出库单
                                                    T_OutBoundEntity outBound = new T_OutBoundEntity();
                                                    outBound.F_Id = Guid.NewGuid().ToString();

                                                    outBound.ReceiveDepartment = "";
                                                    outBound.ReceiveDepartmentId = "";
                                                    outBound.ReceiveUserName = "";
                                                    outBound.ReceiveUserId = "";

                                                    outBound.OutBoundCode = T_CodeGenApp.GenNum("OutBoundRule");
                                                    outBound.RefOrderCode = outBound.OutBoundCode;
                                                    outBound.QAID = qAEntity.F_Id;
                                                    outBound.PointInBoundID = inBoundEntity.F_Id;

                                                    outBound.OutBoundType = "VerBackOut";
                                                    outBound.State = "New";
                                                    outBound.GenType = "ERP";
                                                    outBound.IsUrgent = "false";
                                                    outBound.TransState = "New";
                                                    outBound.Remark = $"质检单：{model.RefOrderCode} 有物料未通过质检，ERP结果反馈执行验退单";
                                                    outBound.F_DeleteMark = false;

                                                    db.Insert<T_OutBoundEntity>(outBound);
                                                    db.SaveChanges();

                                                    List<T_OutBoundDetailEntity> outDetailList = new List<T_OutBoundDetailEntity>();
                                                    foreach (T_QADetailEntity unQua in unQuaDetailList)
                                                    {
                                                        T_OutBoundDetailEntity detail = new T_OutBoundDetailEntity();
                                                        detail.F_Id = Guid.NewGuid().ToString();
                                                        detail.OutBoundID = outBound.F_Id;
                                                        detail.SEQ = Convert.ToInt32(unQua.SEQ);
                                                        detail.ItemID = unQua.ItemID;
                                                        detail.ItemName = unQua.ItemName;
                                                        detail.ItemCode = unQua.ItemCode;
                                                        detail.Factory = unQua.Factory;
                                                        detail.SupplierUserID = unQua.SupplierUserID;
                                                        detail.SupplierUserName = unQua.SupplierUserName;
                                                        detail.SupplierCode = unQua.SupplierCode;
                                                        detail.SourceInOrderCode = inBoundEntity.ERPInDocCode;

                                                        /// 出库库存数量
                                                        List<T_ContainerDetailEntity> cdOutList = new List<T_ContainerDetailEntity>();
                                                        if (string.IsNullOrEmpty(unQua.Lot)) cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && string.IsNullOrEmpty(o.Lot));
                                                        else cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && o.Lot == unQua.Lot);

                                                        detail.Qty = cdOutList.Sum(o => o.Qty);
                                                        detail.OutQty = 0;
                                                        detail.WaveQty = 0;
                                                        detail.Lot = unQua.Lot;
                                                        detail.Spec = itemEntity.Spec;
                                                        detail.ItemUnitText = itemEntity.ItemUnitText;
                                                        detail.State = "New";
                                                        detail.ActionType = "Init";
                                                        /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                                                        T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == itemEntity.F_Id && o.Lot == cell.Lot);
                                                        if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                                                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == detail.ItemID);
                                                        /// 物料出库站台
                                                        T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                                        string containerKind = containerType.ContainerKind;
                                                        if (containerKind == "Rack")
                                                        {
                                                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                                            detail.StationID = station.F_Id;
                                                            detail.StationCode = station.StationCode;
                                                        }
                                                        else if (containerKind == "Plastic" || containerKind == "Box")
                                                        {
                                                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                                            detail.StationID = station.F_Id;
                                                            detail.StationCode = station.StationCode;
                                                        }
                                                        else
                                                        {
                                                            result.IsSuccess = false;
                                                            result.FailCode = "0001";
                                                            result.FailMsg = "物料的容器类型未知:" + item.ItemCode;
                                                            return result;
                                                        }

                                                        outDetailList.Add(detail);
                                                        db.Insert<T_OutBoundDetailEntity>(detail);
                                                    }
                                                    db.SaveChanges();

                                                    #region 验退单自动出库（已注释）
                                                    /*
                                                    if (RuleConfig.OutConfig.ERPPost.AutoOutInterface)
                                                    {
                                                        lock (lockObj)
                                                        {
                                                            /// 波次运算
                                                            string[] needOutArray = outDetailList.Select(o => o.F_Id).ToArray();
                                                            T_OutRecordApp outRecApp = new T_OutRecordApp();
                                                            AjaxResult rst = outRecApp.WaveGen(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                                                            if ((ResultType)rst.state != ResultType.success)
                                                            {
                                                                result.IsSuccess = false;
                                                                result.FailCode = "0001";
                                                                result.FailMsg = rst.message;
                                                                return result;
                                                            }

                                                            /// 发送任务给WCS
                                                            IList<string> outBoundDetailIDList = detailList.Select(o => o.F_Id).ToList();
                                                            rst = outRecApp.OutDetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                                                            if ((ResultType)rst.state != ResultType.success)
                                                            {
                                                                result.IsSuccess = false;
                                                                result.FailCode = "0001";
                                                                result.FailMsg = rst.message;
                                                                return result;
                                                            }
                                                            db.SaveChanges();

                                                            //发送消息到UI
                                                            IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => o.CurOrderID == qAEntity.F_Id);
                                                            foreach (T_StationEntity station in stationList)
                                                            {
                                                                RunnerOrder order = T_EquApp.GetRunnerOrderInfo(station.StationCode);
                                                                T_EquApp.UIManage.RunnerList.First(o => o.Station.StationCode == station.StationCode).Order = order;
                                                                UIManage uiManage = new UIManage();
                                                                uiManage.RunnerList = new List<Runner>();
                                                                uiManage.RunnerList.Add(new Runner() { Station = station.ToObject<StationModel>(), Order = order });
                                                                SendMsg msg = new SendMsg();
                                                                msg.WebUIPoint = FixType.WebUIPoint.Runner;
                                                                msg.Data = new RunnerMsgModel() { Data = uiManage };
                                                                WebSocketPost.SendToAll(msg);
                                                            }
                                                        }
                                                    }
                                                     * */
                                                    #endregion
                                                }
                                            }
                                        }
                                        else
                                        {
                                            existsEntity.IsUsed = "false";

                                            /// 更新质检明细状态为待应用结果
                                            qADetail.State = "WaitApply";
                                            db.Update<T_QADetailEntity>(qADetail);
                                            db.SaveChanges();

                                            /// 修改质检单状态
                                            if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply"))
                                            {
                                                qAEntity.State = "WaitApply";  /// 部分结束/待应用结果 ---> 待应用
                                            }
                                            db.Update<T_QAEntity>(qAEntity);
                                            db.SaveChanges();

                                            /// 同步更新还样单状态
                                            if (qaReturnOrder != null) /// 存在质检还样单
                                            {
                                                if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false") /// 非破坏性质检，且需要还样明细
                                                {
                                                    T_QADetailEntity qaDetailReturn = db.FindEntity<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id && o.SEQ == qADetail.SEQ);
                                                    if (qaDetailReturn.State == "WaitResult")
                                                    {
                                                        qaDetailReturn.State = "WaitApply";   /// 待录入结果 ---> 待应用结果
                                                    }
                                                    db.Update<T_QADetailEntity>(qaDetailReturn);
                                                }

                                                /// 还样单据
                                                List<T_QADetailEntity> qADetailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id).ToList();
                                                if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitApply"))
                                                {
                                                    qaReturnOrder.State = "WaitApply";  /// 部分结束/待应用结果 ---> 待应用
                                                }
                                                db.Update<T_QAEntity>(qaReturnOrder);
                                            }
                                        }
                                        db.Update<T_QAResultEntity>(existsEntity);
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result = InsertQAResult(db, model);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }

        private ERPResult InsertQAResult(IRepositoryBase db, QAResultFaceModel model)
        {
            ERPResult result = new ERPResult();
            try
            {
                /// 获取质检单
                T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == model.RefOrderCode && o.QAOrderType == "GetSample");
                if (qAEntity == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "质检单不存在";
                    return result;
                }
                if (qAEntity.State == "Over")
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "质检单据已结束";
                    return result;
                }
                if (!(qAEntity.State == "Picked" || qAEntity.State == "WaitReturn"
                    || qAEntity.State == "WaitResult" || qAEntity.State == "WaitApply"))
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "质检单据未完成取样";
                    return result;
                }

                /// 对应还样单
                T_QAEntity qaReturnOrder = db.FindEntity<T_QAEntity>(o => o.RefOrderCode == qAEntity.QACode && o.QAOrderType == "BackSample");
                /// 质检单对应入库单
                T_InBoundEntity inBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == qAEntity.RefInBoundCode);
                if (inBoundEntity == null)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "未找到对应入库单";
                    return result;
                }

                List<T_QADetailEntity> detailListAll = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id).ToList();
                if (detailListAll.Count != model.Items.Count)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "结果反馈个数与单据物料个数不一致";
                    return result;
                }

                foreach (QAResultFaceCellModel cell in model.Items)
                {
                    if (string.IsNullOrEmpty(cell.SEQ) || !(cell.SEQ ?? "").IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次不正确";
                        return result;
                    }

                    T_QADetailEntity qADetail = detailListAll.FirstOrDefault(o => (o.SEQ ?? 0).ToString() == cell.SEQ);
                    if (qADetail == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "不存在质检明细";
                        return result;
                    }

                    //if (string.IsNullOrEmpty(cell.SupplierCode) || cell.SupplierCode != qADetail.SupplierCode)
                    //{
                    //    result.IsSuccess = false;
                    //    result.FailCode = "0001";
                    //    result.FailMsg = "供应商错误";
                    //    return result;
                    //}

                    if (string.IsNullOrEmpty(cell.ItemCode) || cell.ItemCode != qADetail.ItemCode)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料编码错误";
                        return result;
                    }

                    T_ItemEntity itemEntity = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                    if (itemEntity == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料不存在";
                        return result;
                    }

                    if (!((string.IsNullOrEmpty(cell.Lot) && string.IsNullOrEmpty(qADetail.Lot)) || cell.Lot == qADetail.Lot))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料批号错误";
                        return result;
                    }

                    if (cell.QAResult != "Qua" && cell.QAResult != "UnQua")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "质检结果参数不正确";
                        return result;
                    }

                    T_QAResultEntity existsEntity = db.FindEntity<T_QAResultEntity>(o => o.RefOrderCode == model.RefOrderCode && (o.SEQ ?? 0).ToString() == cell.SEQ);
                    if (existsEntity != null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料与批号的组合已存在";
                        return result;
                    }

                    existsEntity = new T_QAResultEntity();
                    existsEntity.F_Id = Guid.NewGuid().ToString();
                    existsEntity.SEQ = Convert.ToInt32(cell.SEQ);
                    existsEntity.RefOrderCode = model.RefOrderCode;
                    existsEntity.QAResult = cell.QAResult; // "New";

                    existsEntity.ItemID = itemEntity.F_Id;
                    existsEntity.ItemCode = itemEntity.ItemCode;
                    existsEntity.ItemName = itemEntity.ItemName;
                    existsEntity.Lot = cell.Lot;
                    existsEntity.F_CreatorTime = DateTime.Now;
                    existsEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                    existsEntity.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                    existsEntity.AccessTime = DateTime.Now;
                    existsEntity.F_DeleteMark = false;

                    existsEntity.QAID = qADetail.QAID;
                    existsEntity.QADetailID = qADetail.F_Id;
                    db.SaveChanges();

                    if (RuleConfig.QARule.QAResultRule.AutoUsedQAResult) /// 判断是否自动应用质检结果
                    {
                        existsEntity.IsUsed = "true";

                        /// 更新库存质检状态
                        List<T_ContainerDetailEntity> detailList = new List<T_ContainerDetailEntity>();
                        if (string.IsNullOrEmpty(qADetail.Lot)) detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == inBoundEntity.F_Id);
                        else detailList = db.FindList<T_ContainerDetailEntity>(o => o.CheckDetailID == qADetail.F_Id && o.ItemID == qADetail.ItemID && o.Lot == qADetail.Lot && o.InBoundID == inBoundEntity.F_Id);

                        /// 结合质检取样记录，解冻库存
                        var recordList = db.FindList<T_QARecordEntity>(o => o.QADetailID == qADetail.F_Id);
                        foreach (T_ContainerDetailEntity cd in detailList)
                        {
                            if (existsEntity.QAResult == "Qua") cd.CheckState = "Qua";   /// 合格
                            else if (existsEntity.QAResult == "UnQua") cd.CheckState = "UnQua";  /// 不合格
                            else
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的质检结果参数";
                                return result;
                            }

                            T_QARecordEntity res = recordList.Find(o => o.ContainerDetailID == cd.F_Id);
                            if (res != null && res.IsReturnOver == "false" && res.IsAppearQA == "false") { }    /// 结果录入时，只有一种库存不能解冻：1.已取样 2.未还样 3.取样质检（不是外观质检）
                            else cd.IsCheckFreeze = "false";

                            db.Update<T_ContainerDetailEntity>(cd);
                        }

                        /// 修改明细质检状态
                        qADetail.ResultState = "ERP";
                        qADetail.QAResult = existsEntity.QAResult;
                        qADetail.ResultSendTime = existsEntity.AccessTime;
                        qADetail.SampleType = "Auto";
                        db.SaveChanges();

                        /// 同步修改还样单质检结果
                        if (qaReturnOrder != null) /// 存在质检还样单
                        {
                            if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false") /// 非破坏性质检，且需要还样明细
                            {
                                T_QADetailEntity qaDetailReturn = db.FindEntity<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id && o.SEQ == qADetail.SEQ);
                                if (qaDetailReturn.State == "WaitResult" || qaDetailReturn.State == "WaitApply")
                                {
                                    qaDetailReturn.State = "Over";   /// 待录入结果/待应用结果 ---> 结束
                                }
                                qaDetailReturn.ResultState = "ERP";
                                qaDetailReturn.QAResult = existsEntity.QAResult;
                                qaDetailReturn.ResultSendTime = existsEntity.AccessTime;
                                qaDetailReturn.SampleType = "Auto";
                                db.Update<T_QADetailEntity>(qaDetailReturn);
                            }

                            /// 还样单据
                            List<T_QADetailEntity> qADetailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id).ToList();
                            if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitApply"))
                            {
                                qaReturnOrder.State = "Over";  /// 部分结束/待应用结果 ---> 全部结束
                            }
                            else if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitApply" || o.State == "WaitResult"))
                            {
                                qaReturnOrder.State = "WaitResult";    /// 部分结束/待应用结果/待录入结果 ---> 待录入结果
                            }
                            db.Update<T_QAEntity>(qaReturnOrder);
                        }

                        if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false" && qaReturnOrder == null) qADetail.State = "WaitReturn";
                        else qADetail.State = "Over";
                        db.Update<T_QADetailEntity>(qADetail);
                        db.SaveChanges();

                        /// 判断单据关闭
                        if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply"))
                        {
                            qAEntity.State = "Over";  /// 部分结束/待应用结果 ---> 全部结束
                        }
                        else if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply" || o.State == "WaitResult"))
                        {
                            qAEntity.State = "WaitResult";    /// 部分结束/待应用结果/待录入结果 ---> 待录入结果
                        }
                        else if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply" || o.State == "WaitReturn"))
                        {
                            qAEntity.State = "WaitReturn"; /// 部分结束/待应用结果/待还样 ---> 待还样
                        }
                        db.Update<T_QAEntity>(qAEntity);
                        db.SaveChanges();

                        /// 生成验退单：取样单结束 && 还样单结束或未生成 && 入库单结束
                        if (qAEntity.State == "Over" && (qaReturnOrder == null || qaReturnOrder.State == "Over") && inBoundEntity.State == "Over")
                        {
                            /// 不合格明细
                            List<T_QADetailEntity> unQuaDetailList = db.FindList<T_QADetailEntity>(o => o.QAID == qAEntity.F_Id && o.QAResult == "UnQua");
                            if (unQuaDetailList.Count != 0)
                            {
                                /// 验退出库单
                                T_OutBoundEntity outBound = new T_OutBoundEntity();
                                outBound.F_Id = Guid.NewGuid().ToString();

                                outBound.ReceiveDepartment = "";
                                outBound.ReceiveDepartmentId = "";
                                outBound.ReceiveUserName = "";
                                outBound.ReceiveUserId = "";

                                outBound.OutBoundCode = T_CodeGenApp.GenNum("OutBoundRule");
                                outBound.RefOrderCode = outBound.OutBoundCode;
                                outBound.QAID = qAEntity.F_Id;
                                outBound.PointInBoundID = inBoundEntity.F_Id;

                                outBound.OutBoundType = "VerBackOut";
                                outBound.State = "New";
                                outBound.GenType = "ERP";
                                outBound.IsUrgent = "false";
                                outBound.TransState = "New";
                                outBound.Remark = $"质检单：{model.RefOrderCode} 有物料未通过质检，ERP结果反馈执行验退单";
                                outBound.F_DeleteMark = false;

                                db.Insert<T_OutBoundEntity>(outBound);
                                db.SaveChanges();

                                List<T_OutBoundDetailEntity> outDetailList = new List<T_OutBoundDetailEntity>();
                                foreach (T_QADetailEntity unQua in unQuaDetailList)
                                {
                                    T_OutBoundDetailEntity detail = new T_OutBoundDetailEntity();
                                    detail.F_Id = Guid.NewGuid().ToString();
                                    detail.OutBoundID = outBound.F_Id;
                                    detail.SEQ = Convert.ToInt32(unQua.SEQ);
                                    detail.ItemID = unQua.ItemID;
                                    detail.ItemName = unQua.ItemName;
                                    detail.ItemCode = unQua.ItemCode;
                                    detail.Factory = unQua.Factory;
                                    detail.SupplierUserID = unQua.SupplierUserID;
                                    detail.SupplierUserName = unQua.SupplierUserName;
                                    detail.SupplierCode = unQua.SupplierCode;

                                    /// 出库库存数量
                                    List<T_ContainerDetailEntity> cdOutList = new List<T_ContainerDetailEntity>();
                                    if (string.IsNullOrEmpty(unQua.Lot)) cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && string.IsNullOrEmpty(o.Lot));
                                    else cdOutList = db.FindList<T_ContainerDetailEntity>(o => o.InBoundID == inBoundEntity.F_Id && o.ItemCode == unQua.ItemCode && o.Lot == unQua.Lot);

                                    detail.Qty = cdOutList.Sum(o => o.Qty);
                                    detail.OutQty = 0;
                                    detail.WaveQty = 0;
                                    detail.Lot = unQua.Lot;
                                    detail.Spec = itemEntity.Spec;
                                    detail.ItemUnitText = itemEntity.ItemUnitText;
                                    detail.State = "New";
                                    /// 从【库存】中获取当前物料的过期时间  ---> 调整为：从【收货明细】中获取，防止库存没有当前物料
                                    T_ReceiveRecordEntity cdEntity = db.FindEntity<T_ReceiveRecordEntity>(o => o.ItemID == itemEntity.F_Id && o.Lot == cell.Lot);
                                    if (cdEntity != null) detail.OverdueDate = cdEntity.OverdueDate;

                                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == detail.ItemID);
                                    /// 物料出库站台
                                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                    string containerKind = containerType.ContainerKind;
                                    if (containerKind == "Rack")
                                    {
                                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                                        detail.StationID = station.F_Id;
                                        detail.StationCode = station.StationCode;
                                    }
                                    else if (containerKind == "Plastic" || containerKind == "Box")
                                    {
                                        T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                                        detail.StationID = station.F_Id;
                                        detail.StationCode = station.StationCode;
                                    }
                                    else
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "物料的容器类型未知:" + item.ItemCode;
                                        return result;
                                    }

                                    outDetailList.Add(detail);
                                    db.Insert<T_OutBoundDetailEntity>(detail);
                                }
                                db.SaveChanges();

                                #region 验退单自动出库 （已注释）
                                /*
                                if (RuleConfig.OutConfig.ERPPost.AutoOutInterface)
                                {
                                    lock (lockObj)
                                    {
                                        /// 波次运算
                                        string[] needOutArray = outDetailList.Select(o => o.F_Id).ToArray();
                                        T_OutRecordApp outRecApp = new T_OutRecordApp();
                                        AjaxResult rst = outRecApp.WaveGen(db, "Auto", new List<ContainerDetailModel>(), needOutArray, true);
                                        if ((ResultType)rst.state != ResultType.success)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = rst.message;
                                            return result;
                                        }

                                        /// 发送任务给WCS
                                        IList<string> outBoundDetailIDList = detailList.Select(o => o.F_Id).ToList();
                                        rst = outRecApp.OutDetail_ExecTaskAndSendWCS(db, outBoundDetailIDList);
                                        if ((ResultType)rst.state != ResultType.success)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = rst.message;
                                            return result;
                                        }
                                        db.SaveChanges();

                                        //发送消息到UI
                                        IList<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => o.CurOrderID == qAEntity.F_Id);
                                        foreach (T_StationEntity station in stationList)
                                        {
                                            RunnerOrder order = T_EquApp.GetRunnerOrderInfo(station.StationCode);
                                            T_EquApp.UIManage.RunnerList.First(o => o.Station.StationCode == station.StationCode).Order = order;
                                            UIManage uiManage = new UIManage();
                                            uiManage.RunnerList = new List<Runner>();
                                            uiManage.RunnerList.Add(new Runner() { Station = station.ToObject<StationModel>(), Order = order });
                                            SendMsg msg = new SendMsg();
                                            msg.WebUIPoint = FixType.WebUIPoint.Runner;
                                            msg.Data = new RunnerMsgModel() { Data = uiManage };
                                            WebSocketPost.SendToAll(msg);
                                        }
                                    }
                                }
                                */
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        existsEntity.IsUsed = "false";

                        /// 更新质检明细状态为待应用结果
                        qADetail.State = "WaitApply";
                        db.Update<T_QADetailEntity>(qADetail);
                        db.SaveChanges();

                        /// 修改质检单状态
                        if (detailListAll.All(o => o.State == "Over" || o.State == "WaitApply"))
                        {
                            qAEntity.State = "WaitApply";  /// 部分结束/待应用结果 ---> 待应用
                        }
                        db.Update<T_QAEntity>(qAEntity);
                        db.SaveChanges();

                        /// 同步更新还样单状态
                        if (qaReturnOrder != null) /// 存在质检还样单
                        {
                            if (qADetail.IsBroken == "false" && qADetail.IsAppearQA == "false") /// 非破坏性质检，且需要还样明细
                            {
                                T_QADetailEntity qaDetailReturn = db.FindEntity<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id && o.SEQ == qADetail.SEQ);
                                if (qaDetailReturn.State == "WaitResult")
                                {
                                    qaDetailReturn.State = "WaitApply";   /// 待录入结果 ---> 待应用结果
                                }
                                db.Update<T_QADetailEntity>(qaDetailReturn);
                            }

                            /// 还样单据
                            List<T_QADetailEntity> qADetailBackList = db.FindList<T_QADetailEntity>(o => o.QAID == qaReturnOrder.F_Id).ToList();
                            if (qADetailBackList.All(o => o.State == "Over" || o.State == "WaitApply"))
                            {
                                qaReturnOrder.State = "WaitApply";  /// 部分结束/待应用结果 ---> 待应用
                            }
                            db.Update<T_QAEntity>(qaReturnOrder);
                        }
                    }
                    db.Insert<T_QAResultEntity>(existsEntity);
                }

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 盘点审核反馈
        public ERPResult SyncCountAuditResult(CountAuditResultFaceModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.RefOrderCode))   /// ERP作业单据
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "来源编码必填";
                        return result;
                    }

                    //if (string.IsNullOrEmpty(model.CountOrderCode)) /// WMS盘点单据
                    //{
                    //    result.IsSuccess = false;
                    //    result.FailCode = "0001";
                    //    result.FailMsg = "盘点单编码必填";
                    //    return result;
                    //}

                    T_CountEntity count = db.FindEntity<T_CountEntity>(o => o.RefOrderCode == model.RefOrderCode);
                    if (count == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "盘点单据不存在";
                        return result;
                    }
                    if (count.State == "Over")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "盘点单据已结束";
                        return result;
                    }
                    if (count.State != "WaitResult")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "盘点单据不是待审核反馈状态";
                        return result;
                    }

                    List<T_CountResultEntity> existsEntityList = db.FindList<T_CountResultEntity>(o => o.RefOrderCode == model.RefOrderCode).ToList();
                    switch (model.ActionType)
                    {
                        case "1": /// 新增
                            {
                                if (model.Items.Count == 0)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "盘点明细不可为空";
                                    return result;
                                }

                                if (existsEntityList.Count() > 0)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "来源编码已存在";
                                    return result;
                                }
                                else
                                {
                                    result = InsertCountAuditResult(db, model, count);
                                }
                            }
                            break;
                        case "2": /// 删除(有一个被应用，则所有不允许删除)
                            {
                                if (existsEntityList.Count() > 0)
                                {
                                    if (existsEntityList.Any(o => o.IsUsed == "true"))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "盘点结果已被应用";
                                        return result;
                                    }

                                    if (model.Items.Count < 1) /// 删除整个结果
                                    {
                                        db.Delete<T_CountResultEntity>(o => o.RefOrderCode == model.RefOrderCode);
                                    }
                                    else /// 删除部分单据
                                    {
                                        foreach (CountAuditResultFaceCellModel cell in model.Items)
                                        {
                                            db.Delete<T_CountResultEntity>(o => o.RefOrderCode == model.RefOrderCode && (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        }
                                    }
                                    result.IsSuccess = true;
                                }
                                else
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "盘点结果不存在";
                                    return result;
                                }
                            }
                            break;
                        case "3": /// 修改
                            {
                                if (model.Items.Count == 0)
                                {
                                    result.IsSuccess = false;
                                    result.FailCode = "0001";
                                    result.FailMsg = "盘点明细不可为空";
                                    return result;
                                }

                                if (existsEntityList.Count() > 0)
                                {
                                    if (existsEntityList.Any(o => o.IsUsed == "true"))
                                    {
                                        result.IsSuccess = false;
                                        result.FailCode = "0001";
                                        result.FailMsg = "审核结果已被应用";
                                        return result;
                                    }

                                    IList<CountAuditResultFaceCellModel> items = model.Items;
                                    IList<T_CountDetailEntity> detailListAll = db.FindList<T_CountDetailEntity>(o => o.CountID == count.F_Id).ToList();
                                    IList<T_CountResultEntity> resultListAll = db.FindList<T_CountResultEntity>(o => o.RefOrderCode == model.RefOrderCode || o.CountID == count.F_Id).ToList();
                                    IList<T_CountRecordEntity> recordListAll = db.FindList<T_CountRecordEntity>(o => o.CountID == count.F_Id).ToList();
                                    bool isChangeCD = false;

                                    foreach (CountAuditResultFaceCellModel cell in items)
                                    {
                                        if (string.IsNullOrEmpty(cell.SEQ) || !cell.SEQ.IsNaturalNum())
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "项次不正确";
                                            return result;
                                        }
                                        T_CountDetailEntity detailSEQ = detailListAll.FirstOrDefault(o => (o.SEQ ?? 0).ToString() == cell.SEQ);
                                        if (detailSEQ == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = $"未找到盘点项次 { cell.SEQ }";
                                            return result;
                                        }
                                        T_CountDetailEntity detail = detailListAll.FirstOrDefault(o => o.ItemCode == cell.ItemCode && (o.Lot == cell.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(cell.Lot)))); ;
                                        if (detail.F_Id != detailSEQ.F_Id)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = $"WMS项次物料与ERP接口项次物料不一致:项次[{ cell.SEQ }], 对应物料[{ detailSEQ.ItemCode }], 批号[{ detailSEQ.Lot }]";
                                            return result;
                                        }


                                        //T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.SupplierCode == cell.SupplierCode && o.F_DeleteMark == false);

                                        if (string.IsNullOrEmpty(cell.ItemCode))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料编码必填";
                                            return result;
                                        }

                                        if (string.IsNullOrEmpty(cell.ERPWarehouseCode))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "ERP仓库编码必填";
                                            return result;
                                        }

                                        T_ItemEntity itemEntity = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                                        if (itemEntity == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料不存在";
                                            return result;
                                        }
                                        if (itemEntity.IsMustLot == "true")
                                        {
                                            if (string.IsNullOrEmpty(cell.Lot))
                                            {
                                                result.IsSuccess = false;
                                                result.FailCode = "0001";
                                                result.FailMsg = "物料批号必填";
                                                return result;
                                            }
                                        }

                                        if (!(cell.CountResult == "Pass" || cell.CountResult == "UnPass"))
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "盘点结果参数不正确";
                                            return result;
                                        }

                                        T_CountResultEntity existsEntity = resultListAll.FirstOrDefault(o => o.ItemCode == cell.ItemCode && o.Lot == cell.Lot);
                                        if (existsEntity == null)
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "物料与批号的组合不存在";
                                            return result;
                                        }

                                        existsEntity.SEQ = Convert.ToInt32(cell.SEQ);
                                        existsEntity.CountID = count.F_Id;
                                        existsEntity.CountDetailID = detail.F_Id;
                                        existsEntity.CountResult = cell.CountResult;
                                        existsEntity.ItemID = itemEntity.F_Id;
                                        existsEntity.ItemCode = itemEntity.ItemCode;
                                        existsEntity.ItemName = itemEntity.ItemName;
                                        existsEntity.Lot = cell.Lot;
                                        existsEntity.ERPWarehouseCode = cell.ERPWarehouseCode;
                                        existsEntity.F_LastModifyTime = DateTime.Now;
                                        existsEntity.ModifyUserName = OperatorProvider.Provider.GetCurrent().UserName;
                                        existsEntity.AccessTime = DateTime.Now;

                                        if (RuleConfig.CountRule.AuditResult.AutoUsedCountResult) /// 判断是否自动应用盘点结果
                                        {
                                            existsEntity.IsUsed = "true";

                                            existsEntity.CountID = detail.CountID;
                                            existsEntity.CountDetailID = detail.F_Id;
                                            db.SaveChanges();

                                            List<T_CountRecordEntity> recordList = recordListAll.Where(o => o.ItemID == existsEntity.ItemID && (o.Lot == existsEntity.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(existsEntity.Lot)))).ToList();
                                            foreach (T_CountRecordEntity rec in recordList)
                                            {
                                                /// 更新盘点记录审核状态
                                                rec.AuditState = existsEntity.CountResult;
                                                db.Update<T_CountRecordEntity>(rec);

                                                /// 盘点解冻
                                                T_ContainerDetailEntity cd = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == rec.ContainerDetailID);
                                                if (cd != null)
                                                {
                                                    cd.IsCountFreeze = "false";
                                                    db.Update<T_ContainerDetailEntity>(cd);
                                                }

                                                /// 根据结果更新库存
                                                if (existsEntity.CountResult == "Pass")
                                                {
                                                    string curCountResult = rec.CountResult;
                                                    switch (curCountResult)
                                                    {
                                                        case "Inner_SameBoxCode":  /// 箱码一致（正常库存，不做任何修改）
                                                            break;
                                                        case "Inner_Empty":        /// 正常空货位（正常，不做任何修改）
                                                            break;
                                                        case "Inner_DiffBoxCode": /// 箱码不一致（修改库存箱码） /// 待完善 TODO
                                                            {
                                                                isChangeCD = true;
                                                                cd.ItemBarCode = rec.FactBarCode;
                                                                T_ReceiveRecordEntity receiveRecord = db.FindEntity<T_ReceiveRecordEntity>(o => o.F_Id == cd.ReceiveRecordID);
                                                                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == rec.ItemID);
                                                                T_ContainerTypeEntity containerTypeEntity = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                                                T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.F_Id == receiveRecord.AreaID);
                                                                T_ContainerEntity containerCur = db.FindEntity<T_ContainerEntity>(o => o.BarCode == rec.FactBarCode && o.F_DeleteMark == false);
                                                                if (containerCur == null) /// 新容器
                                                                {

                                                                    containerCur = new T_ContainerEntity();
                                                                    containerCur.F_Id = Guid.NewGuid().ToString();
                                                                    containerCur.BarCode = rec.FactBarCode;
                                                                    containerCur.ContainerType = containerTypeEntity.ContainerTypeCode;
                                                                    containerCur.ContainerKind = containerTypeEntity.ContainerKind;
                                                                    containerCur.IsContainerVir = "0";

                                                                    containerCur.AreaID = areaEntnty.F_Id;
                                                                    containerCur.AreaCode = areaEntnty.AreaCode;
                                                                    containerCur.AreaName = areaEntnty.AreaName;
                                                                    containerCur.F_DeleteMark = false;
                                                                    db.Insert<T_ContainerEntity>(containerCur);
                                                                }
                                                                else
                                                                {
                                                                    if (containerCur.F_DeleteMark == true)
                                                                    {
                                                                        containerCur.BarCode = rec.FactBarCode;
                                                                        containerCur.ContainerType = containerTypeEntity.ContainerTypeCode;
                                                                        containerCur.ContainerKind = containerTypeEntity.ContainerKind;
                                                                        containerCur.IsContainerVir = "0";

                                                                        containerCur.AreaID = areaEntnty.F_Id;
                                                                        containerCur.AreaCode = areaEntnty.AreaCode;
                                                                        containerCur.AreaName = areaEntnty.AreaName;
                                                                        containerCur.F_DeleteMark = false;
                                                                        db.Update<T_ContainerEntity>(containerCur);
                                                                    }
                                                                    else
                                                                    {
                                                                        result.IsSuccess = false;
                                                                        result.FailCode = "0001";
                                                                        result.FailMsg = "容器被占用";
                                                                        return result;
                                                                    }
                                                                }

                                                                cd.ContainerID = containerCur.F_Id;
                                                                cd.BarCode = containerCur.BarCode;
                                                                db.Update<T_ContainerDetailEntity>(cd);
                                                            }
                                                            break;
                                                        case "Inner_MoreBoxCode": /// 多余箱码（新增库存） /// 待完善 TODO
                                                            {
                                                                isChangeCD = true;
                                                                /// TODO
                                                            }
                                                            break;
                                                        case "Inner_NotFindBoxCode": /// 未找到箱码（删除库存）
                                                            {
                                                                /// 清空货位
                                                                T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == cd.LocationID);
                                                                loc.State = "Empty";
                                                                db.Update<T_LocationEntity>(loc);

                                                                /// 删除容器
                                                                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cd.ContainerID);
                                                                container.F_DeleteMark = true;
                                                                db.Update<T_ContainerEntity>(container);

                                                                /// 库存流水
                                                                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                                                inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");

                                                                /// 删除库存
                                                                isChangeCD = true;
                                                                db.Delete<T_ContainerDetailEntity>(cd);
                                                                db.SaveChanges();
                                                            }
                                                            break;
                                                        case "Outer_Normal": /// 正常（正常库存，不做任何修改）
                                                            break;
                                                        case "Outer_LessQty": /// 少数量（修改库存数量。当盘点数量为0时，删除库存，变更空容器）
                                                            {
                                                                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();

                                                                isChangeCD = true;
                                                                if (rec.CountQty == 0) /// 删除库存，可能变更为空容器
                                                                {
                                                                    List<T_ContainerDetailEntity> cdList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == rec.BarCode && o.F_Id != cd.F_Id); /// 容器其它库存
                                                                    List<T_CountRecordEntity> noRecList = recordList.Where(o => o.BarCode == rec.BarCode && o.F_Id != rec.F_Id).ToList();   /// 容器其它盘点记录
                                                                    List<T_CountRecordEntity> otherRecList = recordList.Where(o => o.ItemBarCode == rec.ItemBarCode && o.F_Id != rec.F_Id && o.CountResult == "Outer_MoreItemBarcode").ToList();    /// 标签其它盘点记录

                                                                    /// 变更空容器，删除库存
                                                                    if (cdList.Count == 0 && noRecList.Count == 0 && otherRecList.Count == 0)
                                                                    {
                                                                        /// 删除库存
                                                                        inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");

                                                                        /// 变更空容器
                                                                        T_ContainerDetailEntity cdOne = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == rec.BarCode);
                                                                        if (cdOne.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdOne.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                                        {
                                                                            T_ItemEntity item = null;
                                                                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cdOne.ContainerID);
                                                                            if (container.ContainerKind == "Plastic")   /// 料箱
                                                                            {
                                                                                item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                                            }
                                                                            else if (container.ContainerKind == "Rack")
                                                                            {
                                                                                item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                                            }
                                                                            if (item == null)
                                                                            {
                                                                                result.IsSuccess = false;
                                                                                result.FailCode = "0001";
                                                                                result.FailMsg = "未定义空容器物料";
                                                                                return result;
                                                                            }

                                                                            cdOne.KindCode = item.KindCode;
                                                                            cdOne.KindName = item.KindName;
                                                                            cdOne.ItemID = item.F_Id;
                                                                            cdOne.ItemCode = item.ItemCode;
                                                                            cdOne.ItemName = item.ItemName;
                                                                            cdOne.ItemBarCode = "";
                                                                            cdOne.Qty = 1;
                                                                            cdOne.OutQty = 0;
                                                                            cdOne.CheckQty = 0;
                                                                            cdOne.ItemUnitText = item.ItemUnitText;
                                                                            cdOne.CheckState = "UnNeed";
                                                                            cdOne.CheckDetailID = "";
                                                                            cdOne.CheckID = "";
                                                                            cdOne.State = "Normal";
                                                                            cdOne.IsCheckFreeze = "false";
                                                                            cdOne.IsCountFreeze = "false";
                                                                            cdOne.Lot = "";
                                                                            cdOne.Spec = "";
                                                                            cdOne.ERPWarehouseCode = "";
                                                                            cdOne.Factory = "";
                                                                            cdOne.ProductDate = null;
                                                                            cdOne.OverdueDate = null;
                                                                            cdOne.SupplierID = "";
                                                                            cdOne.SupplierCode = "";
                                                                            cdOne.SupplierName = "";
                                                                            cdOne.ReceiveRecordID = "";
                                                                            //cdOne.IsSpecial = "false";
                                                                            cdOne.IsItemMark = "";
                                                                            cdOne.F_DeleteMark = false;
                                                                            db.Update<T_ContainerDetailEntity>(cdOne);
                                                                            db.SaveChanges();

                                                                            /// 库存流水
                                                                            inOutDetailApp.SyncInOutDetail(db, cdOne, "InType", "EmptyIn", 0, cdOne.Qty, "");
                                                                        }
                                                                    }
                                                                    /// 新增一个空容器，不删除库存
                                                                    else if (cdList.Count == 0 && noRecList.Count == 0 && otherRecList.Count != 0)
                                                                    {
                                                                        T_ContainerDetailEntity cdEmpty = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode);
                                                                        if (cdEmpty.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdEmpty.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                                        {
                                                                            T_ItemEntity item = null;
                                                                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cd.ContainerID);
                                                                            if (container.ContainerKind == "Plastic")   /// 料箱
                                                                            {
                                                                                item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                                            }
                                                                            else if (container.ContainerKind == "Rack")
                                                                            {
                                                                                item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                                            }
                                                                            if (item == null)
                                                                            {
                                                                                result.IsSuccess = false;
                                                                                result.FailCode = "0001";
                                                                                result.FailMsg = "未定义空容器物料";
                                                                                return result;
                                                                            }

                                                                            cdEmpty = new T_ContainerDetailEntity();
                                                                            cdEmpty.F_Id = Guid.NewGuid().ToString();
                                                                            cdEmpty.AreaCode = cd.AreaCode;
                                                                            cdEmpty.AreaID = cd.AreaID;
                                                                            cdEmpty.AreaName = cd.AreaName;
                                                                            cdEmpty.ContainerID = cd.ContainerID;
                                                                            cdEmpty.ContainerKind = cd.ContainerKind;
                                                                            cdEmpty.ContainerType = cd.ContainerType;
                                                                            cdEmpty.BarCode = cd.BarCode;
                                                                            cdEmpty.CheckDetailID = "";
                                                                            cdEmpty.CheckID = "";
                                                                            cdEmpty.CheckQty = 0;
                                                                            cdEmpty.CheckState = "UnNeed";
                                                                            cdEmpty.LocationID = cd.LocationID;
                                                                            cdEmpty.LocationNo = cd.LocationNo;
                                                                            cdEmpty.KindCode = item.KindCode;
                                                                            cdEmpty.KindName = item.KindName;
                                                                            cdEmpty.ItemBarCode = "";
                                                                            cdEmpty.ItemCode = item.ItemCode;
                                                                            cdEmpty.ItemID = item.F_Id;
                                                                            cdEmpty.ItemName = item.ItemName;
                                                                            cdEmpty.Qty = 1;
                                                                            cdEmpty.OutQty = 0;
                                                                            cdEmpty.ItemUnitText = item.ItemUnitText;
                                                                            cdEmpty.State = "Normal";
                                                                            cdEmpty.IsCheckFreeze = "false";
                                                                            cdEmpty.IsCountFreeze = "false";
                                                                            cdEmpty.Lot = "";
                                                                            cdEmpty.Spec = "";
                                                                            cdEmpty.ERPWarehouseCode = "";
                                                                            cdEmpty.Factory = "";
                                                                            cdEmpty.OverdueDate = null;
                                                                            cdEmpty.ProductDate = null;
                                                                            cdEmpty.SupplierCode = "";
                                                                            cdEmpty.SupplierID = "";
                                                                            cdEmpty.SupplierName = "";
                                                                            cdEmpty.ReceiveRecordID = "";
                                                                            //cdEmpty.IsSpecial = "false";
                                                                            cdEmpty.IsItemMark = "";
                                                                            cdEmpty.IsVirItemBarCode = "";
                                                                            cdEmpty.ValidityDayNum = 0;
                                                                            cdEmpty.F_DeleteMark = false;
                                                                            db.Insert<T_ContainerDetailEntity>(cdEmpty);
                                                                            db.SaveChanges();

                                                                            /// 库存流水
                                                                            inOutDetailApp.SyncInOutDetail(db, cdEmpty, "InType", "EmptyIn", 0, cdEmpty.Qty, "");
                                                                        }
                                                                    }
                                                                    /// 删除库存
                                                                    else if (otherRecList.Count == 0 && (cdList.Count != 0 || noRecList.Count != 0))
                                                                    {
                                                                        inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");
                                                                        db.Delete<T_ContainerDetailEntity>(cd);
                                                                        db.SaveChanges();
                                                                    }
                                                                    /// 不空，也不删
                                                                    else if (otherRecList.Count != 0 && (cdList.Count != 0 || noRecList.Count != 0))
                                                                    {
                                                                        /// 不做任何处理
                                                                    }
                                                                }
                                                                else /// 减少库存数量
                                                                {
                                                                    inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty - rec.CountQty, "");
                                                                    cd.Qty = rec.CountQty;
                                                                    db.Update<T_ContainerDetailEntity>(cd);
                                                                }
                                                            }
                                                            break;
                                                        case "Outer_MoreItemBarcode": /// 多标签（新增库存）
                                                            {
                                                                /// 容器内有相同物料，直接获取原物料信息，作为新标签的基本信息
                                                                /// 空容器不会出库，空容器物料无法在此步骤进行新建库存

                                                                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                                                isChangeCD = true;

                                                                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == rec.ItemID);

                                                                #region 标签条码，需要贴标
                                                                if (item.IsItemMark == "true")
                                                                {
                                                                    T_MarkRuleEntity rule = db.FindEntity<T_MarkRuleEntity>(o => o.InBoundDetailID == detail.F_Id);
                                                                    if (rule != null)
                                                                    {
                                                                        rule.OverPicNum = rule.OverPicNum + 1;
                                                                        rule.Qty = rule.Qty + rec.CountQty;
                                                                        db.Update<T_MarkRuleEntity>(rule);

                                                                        T_MarkRecordEntity record = db.FindEntity<T_MarkRecordEntity>(o => o.BarCode == rec.ItemBarCode);
                                                                        if (record == null)
                                                                        {
                                                                            record = new T_MarkRecordEntity();
                                                                            record.F_Id = Guid.NewGuid().ToString();
                                                                            record.MarkRuleID = rule.F_Id;
                                                                            record.BarCode = rec.ItemBarCode;
                                                                            record.SupplierCode = rule.SupplierCode;// supplier.SupplierCode;
                                                                            record.SupplierName = rule.SupplierName; //supplier.SupplierName;
                                                                            record.ItemCode = item.ItemCode;
                                                                            record.ItemName = item.ItemName;
                                                                            record.ItemID = item.F_Id;
                                                                            record.IsUsed = "true"; /// 自动默认使用
                                                                            record.Qty = rec.CountQty;
                                                                            record.Lot = detail.Lot;
                                                                            record.RepairPicNum = 0;
                                                                            record.PicNum = 1;
                                                                            record.IsHandPrint = "false";
                                                                            record.F_DeleteMark = false;
                                                                            record.F_CreatorTime = DateTime.Now;
                                                                            record.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                                                                            record.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                                                                            db.Insert<T_MarkRecordEntity>(record);
                                                                        }
                                                                        else
                                                                        {
                                                                            record.IsUsed = "true";
                                                                            record.Qty = rec.CountQty;
                                                                            db.Update<T_MarkRecordEntity>(record);
                                                                        }
                                                                    }
                                                                }
                                                                #endregion

                                                                #region 收货记录表
                                                                T_ReceiveRecordEntity receiveOther = db.FindEntity<T_ReceiveRecordEntity>(o => o.BarCode == rec.BarCode); /// 获取一条相同物料的收货记录
                                                                T_ReceiveRecordEntity receive = db.FindEntity<T_ReceiveRecordEntity>(o => o.BarCode == rec.BarCode && o.ItemBarCode == rec.ItemBarCode);
                                                                if (receiveOther != null && receive == null)
                                                                {
                                                                    receive = new T_ReceiveRecordEntity();
                                                                    receive.Create();
                                                                    receive.InBoundID = receiveOther.InBoundID;
                                                                    receive.InBoundDetailID = receiveOther.InBoundDetailID;
                                                                    receive.ReceiveStaionID = receiveOther.ReceiveStaionID;
                                                                    receive.ContainerType = receiveOther.ContainerType;
                                                                    receive.BarCode = receiveOther.BarCode;
                                                                    receive.ItemBarCode = rec.ItemBarCode;
                                                                    receive.ItemID = receiveOther.ItemID;
                                                                    receive.ItemCode = receiveOther.ItemCode;
                                                                    receive.Qty = rec.CountQty;
                                                                    receive.ProductDate = receiveOther.ProductDate;
                                                                    receive.OverdueDate = receiveOther.OverdueDate;
                                                                    receive.ERPWarehouseCode = receiveOther.ERPWarehouseCode;
                                                                    receive.AreaID = receiveOther.AreaID;
                                                                    receive.Lot = receiveOther.Lot;
                                                                    receive.Spec = receiveOther.Spec;
                                                                    receive.ItemUnitText = receiveOther.ItemUnitText;
                                                                    receive.CheckState = receiveOther.CheckState;
                                                                    receive.SupplierUserID = receiveOther.SupplierUserID;
                                                                    receive.DoneUserID = receiveOther.DoneUserID;
                                                                    receive.DoneUserName = receiveOther.DoneUserName;
                                                                    receive.LocationID = receiveOther.LocationID;
                                                                    receive.LocationCode = receiveOther.LocationCode;
                                                                    receive.State = receiveOther.State;
                                                                    receive.TransState = receiveOther.TransState;
                                                                    receive.ContainerKind = receiveOther.ContainerKind;
                                                                    receive.FailDesc = null;
                                                                    receive.F_DeleteMark = false;
                                                                    receive.IsItemMark = receiveOther.IsItemMark;
                                                                    receive.Factory = receiveOther.Factory;
                                                                    receive.ValidityDayNum = receiveOther.ValidityDayNum;
                                                                    receive.OverdueDate = receiveOther.OverdueDate;
                                                                    receive.SEQ = receiveOther.SEQ;
                                                                    db.Insert<T_ReceiveRecordEntity>(receive);
                                                                }
                                                                #endregion

                                                                #region 新增库存
                                                                T_ContainerDetailEntity cdOther = db.FindEntity<T_ContainerDetailEntity>(o => o.ItemID == rec.ItemID && (o.Lot == rec.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(rec.Lot))));    /// 获取一条相同库存
                                                                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == rec.BarCode && o.F_DeleteMark == false);
                                                                if (cdOther == null)
                                                                {
                                                                    result.IsSuccess = false;
                                                                    result.FailCode = "0001";
                                                                    result.FailMsg = "无其它参考库存，无法新建";
                                                                    return result;
                                                                }

                                                                /// 直接新增库存
                                                                if (cd == null)
                                                                {
                                                                    cd = new T_ContainerDetailEntity();
                                                                    cd.F_Id = Guid.NewGuid().ToString();
                                                                    cd.AreaCode = container.AreaCode;
                                                                    cd.AreaID = container.AreaID;
                                                                    cd.AreaName = container.AreaName;
                                                                    cd.ContainerID = container.F_Id;
                                                                    cd.ContainerKind = container.ContainerKind;
                                                                    cd.ContainerType = container.ContainerType;
                                                                    cd.LocationID = container.LocationID;
                                                                    cd.LocationNo = container.LocationNo;

                                                                    cd.CheckDetailID = cdOther.CheckDetailID;
                                                                    cd.CheckID = cdOther.CheckID;
                                                                    cd.CheckQty = 0;
                                                                    cd.CheckState = cdOther.CheckState;

                                                                    cd.KindCode = cdOther.KindCode;
                                                                    cd.KindName = cdOther.KindName;
                                                                    cd.ItemID = cdOther.ItemID;
                                                                    cd.ItemCode = cdOther.ItemCode;
                                                                    cd.ItemName = cdOther.ItemName;
                                                                    cd.ItemUnitText = cdOther.ItemUnitText;
                                                                    cd.State = cdOther.State;
                                                                    cd.IsCheckFreeze = cdOther.IsCheckFreeze;
                                                                    cd.Lot = cdOther.Lot;
                                                                    cd.Spec = cdOther.Spec;
                                                                    cd.ProductDate = cdOther.ProductDate;
                                                                    cd.OverdueDate = cdOther.OverdueDate;
                                                                    cd.ERPWarehouseCode = cdOther.ERPWarehouseCode;
                                                                    cd.IsItemMark = cdOther.IsItemMark;
                                                                    cd.Factory = cdOther.Factory;
                                                                    cd.ValidityDayNum = cdOther.ValidityDayNum;
                                                                    cd.SupplierID = cdOther.SupplierID;
                                                                    cd.SupplierCode = cdOther.SupplierCode;
                                                                    cd.SupplierName = cdOther.SupplierName;
                                                                    //cd.IsSpecial = cdOther.IsSpecial;
                                                                    cd.F_DeleteMark = false;
                                                                    cd.OutQty = 0;

                                                                    cd.BarCode = rec.BarCode;
                                                                    cd.ItemBarCode = rec.ItemBarCode;
                                                                    cd.Qty = rec.CountQty;
                                                                    cd.IsCountFreeze = "false";
                                                                    cd.ReceiveRecordID = receive.F_Id;
                                                                    cd.InBoundID = receive.InBoundID;
                                                                    cd.InBoundDetailID = receive.InBoundDetailID;
                                                                    cd.SEQ = receive.SEQ;
                                                                    db.Insert<T_ContainerDetailEntity>(cd);

                                                                    /// 库存流水
                                                                    inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", 0, cd.Qty, "");
                                                                }
                                                                /// 从库存变更，可能产生空容器
                                                                else
                                                                {
                                                                    List<T_ContainerDetailEntity> cdList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode && o.ItemBarCode != rec.ItemBarCode); /// 原容器其它库存
                                                                    List<T_CountRecordEntity> noRecList = recordList.Where(o => o.BarCode == cd.BarCode && o.F_Id != rec.F_Id && o.CountResult != "Outer_LessQty").ToList();   /// 原容器其它盘点记录

                                                                    /// 原容器 ---> 空容器，新增一个空容器
                                                                    if (cdList.Count == 0 && noRecList.Count == 0)
                                                                    {
                                                                        T_ContainerDetailEntity cdEmpty = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode);
                                                                        if (cdEmpty.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdEmpty.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                                        {
                                                                            T_ItemEntity itemEmpty = null;
                                                                            T_ContainerEntity containerEmpty = db.FindEntity<T_ContainerEntity>(o => o.BarCode == cd.BarCode && o.F_DeleteMark == false);
                                                                            if (containerEmpty.ContainerKind == "Plastic")   /// 料箱
                                                                            {
                                                                                itemEmpty = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                                            }
                                                                            else if (containerEmpty.ContainerKind == "Rack")
                                                                            {
                                                                                itemEmpty = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                                            }
                                                                            if (itemEmpty == null)
                                                                            {
                                                                                result.IsSuccess = false;
                                                                                result.FailCode = "0001";
                                                                                result.FailMsg = "未定义空容器物料";
                                                                                return result;
                                                                            }

                                                                            cdEmpty = new T_ContainerDetailEntity();
                                                                            cdEmpty.F_Id = Guid.NewGuid().ToString();
                                                                            cdEmpty.AreaCode = cd.AreaCode;
                                                                            cdEmpty.AreaID = cd.AreaID;
                                                                            cdEmpty.AreaName = cd.AreaName;
                                                                            cdEmpty.ContainerID = cd.ContainerID;
                                                                            cdEmpty.ContainerKind = cd.ContainerKind;
                                                                            cdEmpty.ContainerType = cd.ContainerType;
                                                                            cdEmpty.BarCode = cd.BarCode;
                                                                            cdEmpty.CheckDetailID = "";
                                                                            cdEmpty.CheckID = "";
                                                                            cdEmpty.CheckQty = 0;
                                                                            cdEmpty.CheckState = "UnNeed";
                                                                            cdEmpty.LocationID = cd.LocationID;
                                                                            cdEmpty.LocationNo = cd.LocationNo;
                                                                            cdEmpty.KindCode = itemEmpty.KindCode;
                                                                            cdEmpty.KindName = itemEmpty.KindName;
                                                                            cdEmpty.ItemBarCode = "";
                                                                            cdEmpty.ItemCode = itemEmpty.ItemCode;
                                                                            cdEmpty.ItemID = itemEmpty.F_Id;
                                                                            cdEmpty.ItemName = itemEmpty.ItemName;
                                                                            cdEmpty.Qty = 1;
                                                                            cdEmpty.OutQty = 0;
                                                                            cdEmpty.ItemUnitText = itemEmpty.ItemUnitText;
                                                                            cdEmpty.State = "Normal";
                                                                            cdEmpty.IsCheckFreeze = "false";
                                                                            cdEmpty.IsCountFreeze = "false";
                                                                            cdEmpty.Lot = "";
                                                                            cdEmpty.Spec = "";
                                                                            cdEmpty.ERPWarehouseCode = "";
                                                                            cdEmpty.Factory = "";
                                                                            cdEmpty.OverdueDate = null;
                                                                            cdEmpty.ProductDate = null;
                                                                            cdEmpty.SupplierCode = "";
                                                                            cdEmpty.SupplierID = "";
                                                                            cdEmpty.SupplierName = "";
                                                                            cdEmpty.ReceiveRecordID = "";
                                                                            //cdEmpty.IsSpecial = "false";
                                                                            cdEmpty.IsItemMark = "";
                                                                            cdEmpty.IsVirItemBarCode = "";
                                                                            cdEmpty.ValidityDayNum = 0;
                                                                            cdEmpty.F_DeleteMark = false;
                                                                            db.Insert<T_ContainerDetailEntity>(cdEmpty);
                                                                            db.SaveChanges();

                                                                            /// 库存流水
                                                                            inOutDetailApp.SyncInOutDetail(db, cdEmpty, "InType", "EmptyIn", 0, cdEmpty.Qty, "");
                                                                        }
                                                                    }

                                                                    /// 变更库存容器信息
                                                                    cd.AreaID = container.AreaID;
                                                                    cd.AreaCode = container.AreaCode;
                                                                    cd.AreaName = container.AreaName;
                                                                    cd.ContainerID = container.F_Id;
                                                                    cd.ContainerKind = container.ContainerKind;
                                                                    cd.ContainerType = container.ContainerType;
                                                                    cd.LocationID = container.LocationID;
                                                                    cd.LocationNo = container.LocationNo;
                                                                    cd.State = cdOther.State;
                                                                    cd.ERPWarehouseCode = cdOther.ERPWarehouseCode;
                                                                    cd.BarCode = rec.BarCode;
                                                                    cd.ItemBarCode = rec.ItemBarCode;
                                                                    cd.Qty = rec.CountQty;
                                                                    cd.IsCountFreeze = "false";
                                                                    db.Update<T_ContainerDetailEntity>(cd);

                                                                    /// 产生流水
                                                                    if (cd.Qty != rec.CountQty)
                                                                    {
                                                                        decimal? changeQty = rec.CountQty - cd.Qty;
                                                                        if (changeQty > 0) inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", cd.Qty, changeQty, "");
                                                                        else inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", 0, -changeQty, "");
                                                                    }
                                                                }
                                                                #endregion
                                                            }
                                                            break;
                                                        case "Outer_MoreQty": /// 多余数量（修改库存数量）
                                                            {
                                                                /// 库存流水
                                                                T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                                                inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", cd.Qty, rec.CountQty - cd.Qty, "");

                                                                isChangeCD = true;
                                                                cd.Qty = rec.CountQty;
                                                                db.Update<T_ContainerDetailEntity>(cd);
                                                            }
                                                            break;
                                                        default:
                                                            {
                                                                result.IsSuccess = false;
                                                                result.FailCode = "0001";
                                                                result.FailMsg = "未知的盘点结果";
                                                                return result;
                                                            }
                                                    }
                                                }
                                                else if (existsEntity.CountResult == "UnPass")
                                                {
                                                    /// 不通过，什么都不做
                                                }
                                                else
                                                {
                                                    result.IsSuccess = false;
                                                    result.FailCode = "0001";
                                                    result.FailMsg = "未知的盘点结果参数";
                                                    return result;
                                                }
                                            }

                                            /// 盘点明细状态
                                            detail.AuditState = existsEntity.CountResult;
                                            detail.CountResult = existsEntity.CountResult;
                                            detail.CountState = "Over";
                                            db.Update<T_CountDetailEntity>(detail);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            existsEntity.IsUsed = "false";
                                        }
                                        db.Update<T_CountResultEntity>(existsEntity);
                                    }

                                    /// 更新盘点单
                                    if (RuleConfig.CountRule.AuditResult.AutoUsedCountResult) /// 判断是否自动应用盘点结果
                                    {
                                        count.State = "Over";
                                        count.AuditResult = "Applied";
                                        if (detailListAll.Any(o => o.AuditState == "UnPass"))
                                        {
                                            count.AuditState = "UnPass";
                                        }
                                        else count.AuditState = "Pass";
                                        db.Update<T_CountEntity>(count);
                                        db.SaveChanges();

                                        if (isChangeCD && RuleConfig.OrderTransRule.CountTransRule.CountTrans)  /// 库存变动 && 允许自动过账
                                        {
                                            if (count.GenType == "ERP")
                                            {
                                                /// 产生过账信息，并发送过账信息
                                                string transType = "Count";
                                                AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, count.F_Id, transType);
                                                if ((ResultType)rst.state == ResultType.success)
                                                {
                                                    T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                                    ERPPost post = new ERPPost();
                                                    ERPResult erpRst = post.PostFactInOutQty(db, transType, trans.F_Id);
                                                }
                                                else
                                                {
                                                    result.IsSuccess = false;
                                                    result.FailCode = "0001";
                                                    result.FailMsg = "过账信息产生失败";
                                                    return result;
                                                }
                                            }

                                        }
                                    }

                                    db.SaveChanges();
                                    result.IsSuccess = true;
                                }
                                else //盘点结果不存在，插入盘点审核结果
                                {
                                    result = InsertCountAuditResult(db, model, count);
                                }
                            }
                            break;
                        default:
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的操作类型";
                                return result;
                            }
                    }
                    if (result.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }

        }

        private ERPResult InsertCountAuditResult(IRepositoryBase db, CountAuditResultFaceModel model, T_CountEntity count)
        {
            ERPResult result = new ERPResult();
            try
            {
                IList<CountAuditResultFaceCellModel> items = model.Items;
                IList<T_CountDetailEntity> detailListAll = db.FindList<T_CountDetailEntity>(o => o.CountID == count.F_Id).ToList();
                IList<T_CountResultEntity> resultListAll = db.FindList<T_CountResultEntity>(o => o.RefOrderCode == model.RefOrderCode || o.CountID == count.F_Id).ToList();
                IList<T_CountRecordEntity> recordListAll = db.FindList<T_CountRecordEntity>(o => o.CountID == count.F_Id).ToList();
                bool isChangeCD = false;

                if (detailListAll.Count != items.Count)
                {
                    result.IsSuccess = false;
                    result.FailCode = "0001";
                    result.FailMsg = "物料个数与提交审核物料个数不一致";
                    return result;
                }

                foreach (CountAuditResultFaceCellModel cell in items)
                {
                    if (string.IsNullOrEmpty(cell.SEQ) || !cell.SEQ.IsNaturalNum())
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "项次不正确";
                        return result;
                    }

                    T_CountDetailEntity detailSEQ = detailListAll.FirstOrDefault(o => (o.SEQ ?? 0).ToString() == cell.SEQ);
                    if (detailSEQ == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"未找到盘点项次 { cell.SEQ }";
                        return result;
                    }
                    T_CountDetailEntity detail = detailListAll.FirstOrDefault(o => o.ItemCode == cell.ItemCode && (o.Lot == cell.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(cell.Lot))));
                    if (detail == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"物料+批号组合未盘点:物料编码[{detailSEQ.ItemCode}],批号[{detailSEQ.Lot}]";
                        return result;
                    }
                    if (detail.F_Id != detailSEQ.F_Id)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = $"WMS项次物料与ERP接口项次物料不一致:项次[{cell.SEQ}]。对应物料编码[{detailSEQ.ItemCode}],批号[{detailSEQ.Lot}]";
                        return result;
                    }


                    if (string.IsNullOrEmpty(cell.ItemCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料编码必填";
                        return result;
                    }

                    if (string.IsNullOrEmpty(cell.ERPWarehouseCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "ERP仓库编码必填";
                        return result;
                    }

                    T_ItemEntity itemEntity = db.FindEntity<T_ItemEntity>(o => o.ItemCode == cell.ItemCode && o.F_DeleteMark == false);
                    if (itemEntity == null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料不存在";
                        return result;
                    }

                    if (itemEntity.IsMustLot == "true")
                    {
                        if (string.IsNullOrEmpty(cell.Lot))
                        {
                            result.IsSuccess = false;
                            result.FailCode = "0001";
                            result.FailMsg = "物料批号必填";
                            return result;
                        }
                    }

                    if (!(cell.CountResult == "Pass" || cell.CountResult == "UnPass"))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "盘点结果参数不正确";
                        return result;
                    }

                    T_CountResultEntity countResult = resultListAll.FirstOrDefault(o => o.ItemCode == cell.ItemCode && o.Lot == cell.Lot);
                    if (countResult != null)
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "物料与批号的组合已存在";
                        return result;
                    }

                    T_CountResultEntity existsEntity = new T_CountResultEntity();
                    existsEntity.F_Id = Guid.NewGuid().ToString();
                    existsEntity.RefOrderCode = model.RefOrderCode;
                    existsEntity.SEQ = Convert.ToInt32(cell.SEQ);
                    existsEntity.CountID = count.F_Id;
                    existsEntity.CountDetailID = detail.F_Id;
                    existsEntity.CountResult = cell.CountResult;
                    existsEntity.ItemID = itemEntity.F_Id;
                    existsEntity.ItemCode = itemEntity.ItemCode;
                    existsEntity.ItemName = itemEntity.ItemName;
                    existsEntity.Lot = cell.Lot;
                    existsEntity.ERPWarehouseCode = cell.ERPWarehouseCode;
                    existsEntity.CountResult = cell.CountResult;
                    existsEntity.F_CreatorTime = DateTime.Now;
                    existsEntity.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                    existsEntity.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                    existsEntity.AccessTime = DateTime.Now;
                    existsEntity.F_DeleteMark = false;

                    if (RuleConfig.CountRule.AuditResult.AutoUsedCountResult) /// 判断是否自动应用盘点结果
                    {
                        existsEntity.IsUsed = "true";

                        existsEntity.CountID = detail.CountID;
                        existsEntity.CountDetailID = detail.F_Id;
                        db.SaveChanges();

                        List<T_CountRecordEntity> recordList = recordListAll.Where(o => o.ItemID == existsEntity.ItemID && (o.Lot == existsEntity.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(existsEntity.Lot)))).ToList();
                        foreach (T_CountRecordEntity rec in recordList)
                        {
                            /// 更新盘点记录审核状态
                            rec.AuditState = existsEntity.CountResult;
                            db.Update<T_CountRecordEntity>(rec);

                            /// 盘点解冻
                            T_ContainerDetailEntity cd = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == rec.ContainerDetailID);
                            if (cd != null)
                            {
                                cd.IsCountFreeze = "false";
                                db.Update<T_ContainerDetailEntity>(cd);
                            }

                            /// 根据结果更新库存
                            if (existsEntity.CountResult == "Pass")
                            {
                                string curCountResult = rec.CountResult;
                                switch (curCountResult)
                                {
                                    case "Inner_SameBoxCode":  /// 箱码一致（正常库存，不做任何修改）
                                        break;
                                    case "Inner_Empty":        /// 正常空货位（正常，不做任何修改）
                                        break;
                                    case "Inner_DiffBoxCode": /// 箱码不一致（修改库存箱码） /// 待完善 TODO
                                        {
                                            isChangeCD = true;
                                            cd.ItemBarCode = rec.FactBarCode;
                                            T_ReceiveRecordEntity receiveRecord = db.FindEntity<T_ReceiveRecordEntity>(o => o.F_Id == cd.ReceiveRecordID);
                                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == rec.ItemID);
                                            T_ContainerTypeEntity containerTypeEntity = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                            T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.F_Id == receiveRecord.AreaID);
                                            T_ContainerEntity containerCur = db.FindEntity<T_ContainerEntity>(o => o.BarCode == rec.FactBarCode && o.F_DeleteMark == false);
                                            if (containerCur == null) /// 新容器
                                            {
                                                containerCur = new T_ContainerEntity();
                                                containerCur.F_Id = Guid.NewGuid().ToString();
                                                containerCur.BarCode = rec.FactBarCode;
                                                containerCur.ContainerType = containerTypeEntity.ContainerTypeCode;
                                                containerCur.ContainerKind = containerTypeEntity.ContainerKind;
                                                containerCur.IsContainerVir = "0";

                                                containerCur.AreaID = areaEntnty.F_Id;
                                                containerCur.AreaCode = areaEntnty.AreaCode;
                                                containerCur.AreaName = areaEntnty.AreaName;
                                                containerCur.F_DeleteMark = false;
                                                db.Insert<T_ContainerEntity>(containerCur);
                                            }
                                            else
                                            {
                                                if (containerCur.F_DeleteMark == true)
                                                {
                                                    containerCur.BarCode = rec.FactBarCode;
                                                    containerCur.ContainerType = containerTypeEntity.ContainerTypeCode;
                                                    containerCur.ContainerKind = containerTypeEntity.ContainerKind;
                                                    containerCur.IsContainerVir = "0";

                                                    containerCur.AreaID = areaEntnty.F_Id;
                                                    containerCur.AreaCode = areaEntnty.AreaCode;
                                                    containerCur.AreaName = areaEntnty.AreaName;
                                                    containerCur.F_DeleteMark = false;
                                                    db.Update<T_ContainerEntity>(containerCur);
                                                }
                                                else
                                                {
                                                    result.IsSuccess = false;
                                                    result.FailCode = "0001";
                                                    result.FailMsg = "容器被占用";
                                                    return result;
                                                }
                                            }

                                            cd.ContainerID = containerCur.F_Id;
                                            cd.BarCode = containerCur.BarCode;
                                            db.Update<T_ContainerDetailEntity>(cd);
                                        }
                                        break;
                                    case "Inner_MoreBoxCode": /// 多余箱码（新增库存） /// 待完善 TODO
                                        {
                                            isChangeCD = true;
                                            /// TODO
                                        }
                                        break;
                                    case "Inner_NotFindBoxCode": /// 未找到箱码（删除库存）
                                        {
                                            /// 清空货位
                                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == cd.LocationID);
                                            loc.State = "Empty";
                                            db.Update<T_LocationEntity>(loc);

                                            /// 删除容器
                                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cd.ContainerID);
                                            container.F_DeleteMark = true;
                                            db.Update<T_ContainerEntity>(container);

                                            /// 库存流水
                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                            inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");

                                            /// 删除库存
                                            isChangeCD = true;
                                            db.Delete<T_ContainerDetailEntity>(cd);
                                            db.SaveChanges();
                                        }
                                        break;
                                    case "Outer_Normal": /// 正常（正常库存，不做任何修改）
                                        break;
                                    case "Outer_LessQty": /// 少数量（修改库存数量。当盘点数量为0时，删除库存，变更空容器）
                                        {
                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();

                                            isChangeCD = true;
                                            if (rec.CountQty == 0) /// 删除库存，可能变更为空容器
                                            {
                                                List<T_ContainerDetailEntity> cdList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == rec.BarCode && o.F_Id != cd.F_Id); /// 容器其它库存
                                                List<T_CountRecordEntity> noRecList = recordList.Where(o => o.BarCode == rec.BarCode && o.F_Id != rec.F_Id).ToList();   /// 容器其它盘点记录
                                                List<T_CountRecordEntity> otherRecList = recordList.Where(o => o.ItemBarCode == rec.ItemBarCode && o.F_Id != rec.F_Id && o.CountResult == "Outer_MoreItemBarcode").ToList();    /// 标签其它盘点记录

                                                                                                                                                                                                                                                                            /// 变更空容器，删除库存
                                                if (cdList.Count == 0 && noRecList.Count == 0 && otherRecList.Count == 0)
                                                {
                                                    /// 删除库存
                                                    inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");

                                                    /// 变更空容器
                                                    T_ContainerDetailEntity cdOne = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == rec.BarCode);
                                                    if (cdOne.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdOne.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                    {
                                                        T_ItemEntity item = null;
                                                        T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cdOne.ContainerID);
                                                        if (container.ContainerKind == "Plastic")   /// 料箱
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                        }
                                                        else if (container.ContainerKind == "Rack")
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                        }
                                                        if (item == null)
                                                        {
                                                            result.IsSuccess = false;
                                                            result.FailCode = "0001";
                                                            result.FailMsg = "未定义空容器物料";
                                                            return result;
                                                        }

                                                        cdOne.KindCode = item.KindCode;
                                                        cdOne.KindName = item.KindName;
                                                        cdOne.ItemID = item.F_Id;
                                                        cdOne.ItemCode = item.ItemCode;
                                                        cdOne.ItemName = item.ItemName;
                                                        cdOne.ItemBarCode = "";
                                                        cdOne.Qty = 1;
                                                        cdOne.OutQty = 0;
                                                        cdOne.CheckQty = 0;
                                                        cdOne.ItemUnitText = item.ItemUnitText;
                                                        cdOne.CheckState = "UnNeed";
                                                        cdOne.CheckDetailID = "";
                                                        cdOne.CheckID = "";
                                                        cdOne.State = "Normal";
                                                        cdOne.IsCheckFreeze = "false";
                                                        cdOne.IsCountFreeze = "false";
                                                        cdOne.Lot = "";
                                                        cdOne.Spec = "";
                                                        cdOne.ERPWarehouseCode = "";
                                                        cdOne.Factory = "";
                                                        cdOne.ProductDate = null;
                                                        cdOne.OverdueDate = null;
                                                        cdOne.SupplierID = "";
                                                        cdOne.SupplierCode = "";
                                                        cdOne.SupplierName = "";
                                                        cdOne.ReceiveRecordID = "";
                                                        //cdOne.IsSpecial = "false";
                                                        cdOne.IsItemMark = "";
                                                        cdOne.F_DeleteMark = false;
                                                        db.Update<T_ContainerDetailEntity>(cdOne);
                                                        db.SaveChanges();

                                                        /// 库存流水
                                                        inOutDetailApp.SyncInOutDetail(db, cdOne, "InType", "EmptyIn", 0, cdOne.Qty, "");
                                                    }
                                                }
                                                /// 新增一个空容器，不删除库存
                                                else if (cdList.Count == 0 && noRecList.Count == 0 && otherRecList.Count != 0)
                                                {
                                                    T_ContainerDetailEntity cdEmpty = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode);
                                                    if (cdEmpty.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdEmpty.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                    {
                                                        T_ItemEntity item = null;
                                                        T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cd.ContainerID);
                                                        if (container.ContainerKind == "Plastic")   /// 料箱
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                        }
                                                        else if (container.ContainerKind == "Rack")
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                        }
                                                        if (item == null)
                                                        {
                                                            result.IsSuccess = false;
                                                            result.FailCode = "0001";
                                                            result.FailMsg = "未定义空容器物料";
                                                            return result;
                                                        }

                                                        cdEmpty = new T_ContainerDetailEntity();
                                                        cdEmpty.F_Id = Guid.NewGuid().ToString();
                                                        cdEmpty.AreaCode = cd.AreaCode;
                                                        cdEmpty.AreaID = cd.AreaID;
                                                        cdEmpty.AreaName = cd.AreaName;
                                                        cdEmpty.ContainerID = cd.ContainerID;
                                                        cdEmpty.ContainerKind = cd.ContainerKind;
                                                        cdEmpty.ContainerType = cd.ContainerType;
                                                        cdEmpty.BarCode = cd.BarCode;
                                                        cdEmpty.CheckDetailID = "";
                                                        cdEmpty.CheckID = "";
                                                        cdEmpty.CheckQty = 0;
                                                        cdEmpty.CheckState = "UnNeed";
                                                        cdEmpty.LocationID = cd.LocationID;
                                                        cdEmpty.LocationNo = cd.LocationNo;
                                                        cdEmpty.KindCode = item.KindCode;
                                                        cdEmpty.KindName = item.KindName;
                                                        cdEmpty.ItemBarCode = "";
                                                        cdEmpty.ItemCode = item.ItemCode;
                                                        cdEmpty.ItemID = item.F_Id;
                                                        cdEmpty.ItemName = item.ItemName;
                                                        cdEmpty.Qty = 1;
                                                        cdEmpty.OutQty = 0;
                                                        cdEmpty.ItemUnitText = item.ItemUnitText;
                                                        cdEmpty.State = "Normal";
                                                        cdEmpty.IsCheckFreeze = "false";
                                                        cdEmpty.IsCountFreeze = "false";
                                                        cdEmpty.Lot = "";
                                                        cdEmpty.Spec = "";
                                                        cdEmpty.ERPWarehouseCode = "";
                                                        cdEmpty.Factory = "";
                                                        cdEmpty.OverdueDate = null;
                                                        cdEmpty.ProductDate = null;
                                                        cdEmpty.SupplierCode = "";
                                                        cdEmpty.SupplierID = "";
                                                        cdEmpty.SupplierName = "";
                                                        cdEmpty.ReceiveRecordID = "";
                                                        //cdEmpty.IsSpecial = "false";
                                                        cdEmpty.IsItemMark = "";
                                                        cdEmpty.IsVirItemBarCode = "";
                                                        cdEmpty.ValidityDayNum = 0;
                                                        cdEmpty.F_DeleteMark = false;
                                                        db.Insert<T_ContainerDetailEntity>(cdEmpty);
                                                        db.SaveChanges();

                                                        /// 库存流水
                                                        inOutDetailApp.SyncInOutDetail(db, cdEmpty, "InType", "EmptyIn", 0, cdEmpty.Qty, "");
                                                    }
                                                }
                                                /// 删除库存
                                                else if (otherRecList.Count == 0 && (cdList.Count != 0 || noRecList.Count != 0))
                                                {
                                                    inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");
                                                    db.Delete<T_ContainerDetailEntity>(cd);
                                                    db.SaveChanges();
                                                }
                                                /// 不空，也不删
                                                else if (otherRecList.Count != 0 && (cdList.Count != 0 || noRecList.Count != 0))
                                                {
                                                    /// 不做任何处理
                                                }
                                            }
                                            else /// 减少库存数量
                                            {
                                                inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty - rec.CountQty, "");
                                                cd.Qty = rec.CountQty;
                                                db.Update<T_ContainerDetailEntity>(cd);
                                            }
                                        }
                                        break;
                                    case "Outer_MoreItemBarcode": /// 多标签（新增库存）
                                        {
                                            /// 容器内有相同物料，直接获取原物料信息，作为新标签的基本信息
                                            /// 空容器不会出库，空容器物料无法在此步骤进行新建库存

                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                            isChangeCD = true;

                                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == rec.ItemID);

                                            #region 标签条码，需要贴标
                                            if (item.IsItemMark == "true")
                                            {
                                                T_MarkRuleEntity rule = db.FindEntity<T_MarkRuleEntity>(o => o.InBoundDetailID == detail.F_Id);
                                                if (rule != null)
                                                {
                                                    rule.OverPicNum = rule.OverPicNum + 1;
                                                    rule.Qty = rule.Qty + rec.CountQty;
                                                    db.Update<T_MarkRuleEntity>(rule);

                                                    T_MarkRecordEntity record = db.FindEntity<T_MarkRecordEntity>(o => o.BarCode == rec.ItemBarCode);
                                                    if (record == null)
                                                    {
                                                        record = new T_MarkRecordEntity();
                                                        record.F_Id = Guid.NewGuid().ToString();
                                                        record.MarkRuleID = rule.F_Id;
                                                        record.BarCode = rec.ItemBarCode;
                                                        record.SupplierCode = rule.SupplierCode;// supplier.SupplierCode;
                                                        record.SupplierName = rule.SupplierName;// supplier.SupplierName;
                                                        record.ItemCode = item.ItemCode;
                                                        record.ItemName = item.ItemName;
                                                        record.ItemID = item.F_Id;
                                                        record.IsUsed = "true"; /// 自动默认使用
                                                        record.Qty = rec.CountQty;
                                                        record.Lot = detail.Lot;
                                                        record.RepairPicNum = 0;
                                                        record.PicNum = 1;
                                                        record.IsHandPrint = "false";
                                                        record.F_DeleteMark = false;
                                                        record.F_CreatorTime = DateTime.Now;
                                                        record.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                                                        record.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                                                        db.Insert<T_MarkRecordEntity>(record);
                                                    }
                                                    else
                                                    {
                                                        record.IsUsed = "true";
                                                        record.Qty = rec.CountQty;
                                                        db.Update<T_MarkRecordEntity>(record);
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region 收货记录表
                                            T_ReceiveRecordEntity receiveOther = db.FindEntity<T_ReceiveRecordEntity>(o => o.BarCode == rec.BarCode); /// 获取一条相同物料的收货记录
                                            T_ReceiveRecordEntity receive = db.FindEntity<T_ReceiveRecordEntity>(o => o.BarCode == rec.BarCode && o.ItemBarCode == rec.ItemBarCode);
                                            if (receiveOther != null && receive == null)
                                            {
                                                receive = new T_ReceiveRecordEntity();
                                                receive.Create();
                                                receive.InBoundID = receiveOther.InBoundID;
                                                receive.InBoundDetailID = receiveOther.InBoundDetailID;
                                                receive.ReceiveStaionID = receiveOther.ReceiveStaionID;
                                                receive.ContainerType = receiveOther.ContainerType;
                                                receive.BarCode = receiveOther.BarCode;
                                                receive.ItemBarCode = rec.ItemBarCode;
                                                receive.ItemID = receiveOther.ItemID;
                                                receive.ItemCode = receiveOther.ItemCode;
                                                receive.Qty = rec.CountQty;
                                                receive.ProductDate = receiveOther.ProductDate;
                                                receive.OverdueDate = receiveOther.OverdueDate;
                                                receive.ERPWarehouseCode = receiveOther.ERPWarehouseCode;
                                                receive.AreaID = receiveOther.AreaID;
                                                receive.Lot = receiveOther.Lot;
                                                receive.Spec = receiveOther.Spec;
                                                receive.ItemUnitText = receiveOther.ItemUnitText;
                                                receive.CheckState = receiveOther.CheckState;
                                                receive.SupplierUserID = receiveOther.SupplierUserID;
                                                receive.DoneUserID = receiveOther.DoneUserID;
                                                receive.DoneUserName = receiveOther.DoneUserName;
                                                receive.LocationID = receiveOther.LocationID;
                                                receive.LocationCode = receiveOther.LocationCode;
                                                receive.State = receiveOther.State;
                                                receive.TransState = receiveOther.TransState;
                                                receive.ContainerKind = receiveOther.ContainerKind;
                                                receive.FailDesc = null;
                                                receive.F_DeleteMark = false;
                                                receive.IsItemMark = receiveOther.IsItemMark;
                                                receive.Factory = receiveOther.Factory;
                                                receive.ValidityDayNum = receiveOther.ValidityDayNum;
                                                receive.OverdueDate = receiveOther.OverdueDate;
                                                receive.SEQ = receiveOther.SEQ;
                                                db.Insert<T_ReceiveRecordEntity>(receive);
                                            }
                                            #endregion

                                            #region 新增库存
                                            T_ContainerDetailEntity cdOther = db.FindEntity<T_ContainerDetailEntity>(o => o.ItemID == rec.ItemID && (o.Lot == rec.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(rec.Lot))));    /// 获取一条相同库存
                                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == rec.BarCode && o.F_DeleteMark == false);
                                            if (cdOther == null)
                                            {
                                                result.IsSuccess = false;
                                                result.FailCode = "0001";
                                                result.FailMsg = "无其它参考库存，无法新建";
                                                return result;
                                            }

                                            /// 直接新增库存
                                            if (cd == null)
                                            {
                                                cd = new T_ContainerDetailEntity();
                                                cd.F_Id = Guid.NewGuid().ToString();
                                                cd.AreaCode = container.AreaCode;
                                                cd.AreaID = container.AreaID;
                                                cd.AreaName = container.AreaName;
                                                cd.ContainerID = container.F_Id;
                                                cd.ContainerKind = container.ContainerKind;
                                                cd.ContainerType = container.ContainerType;
                                                cd.LocationID = container.LocationID;
                                                cd.LocationNo = container.LocationNo;

                                                cd.CheckDetailID = cdOther.CheckDetailID;
                                                cd.CheckID = cdOther.CheckID;
                                                cd.CheckQty = 0;
                                                cd.CheckState = cdOther.CheckState;

                                                cd.KindCode = cdOther.KindCode;
                                                cd.KindName = cdOther.KindName;
                                                cd.ItemID = cdOther.ItemID;
                                                cd.ItemCode = cdOther.ItemCode;
                                                cd.ItemName = cdOther.ItemName;
                                                cd.ItemUnitText = cdOther.ItemUnitText;
                                                cd.State = cdOther.State;
                                                cd.IsCheckFreeze = cdOther.IsCheckFreeze;
                                                cd.Lot = cdOther.Lot;
                                                cd.Spec = cdOther.Spec;
                                                cd.ProductDate = cdOther.ProductDate;
                                                cd.OverdueDate = cdOther.OverdueDate;
                                                cd.ERPWarehouseCode = cdOther.ERPWarehouseCode;
                                                cd.IsItemMark = cdOther.IsItemMark;
                                                cd.Factory = cdOther.Factory;
                                                cd.ValidityDayNum = cdOther.ValidityDayNum;
                                                cd.SupplierID = cdOther.SupplierID;
                                                cd.SupplierCode = cdOther.SupplierCode;
                                                cd.SupplierName = cdOther.SupplierName;
                                                //cd.IsSpecial = cdOther.IsSpecial;
                                                cd.F_DeleteMark = false;
                                                cd.OutQty = 0;

                                                cd.BarCode = rec.BarCode;
                                                cd.ItemBarCode = rec.ItemBarCode;
                                                cd.Qty = rec.CountQty;
                                                cd.IsCountFreeze = "false";
                                                cd.ReceiveRecordID = receive.F_Id;
                                                cd.InBoundID = receive.InBoundID;
                                                cd.InBoundDetailID = receive.InBoundDetailID;
                                                cd.SEQ = receive.SEQ;
                                                db.Insert<T_ContainerDetailEntity>(cd);

                                                /// 库存流水
                                                inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", 0, cd.Qty, "");
                                            }
                                            /// 从库存变更，可能产生空容器
                                            else
                                            {
                                                List<T_ContainerDetailEntity> cdList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode && o.ItemBarCode != rec.ItemBarCode); /// 原容器其它库存
                                                List<T_CountRecordEntity> noRecList = recordList.Where(o => o.BarCode == cd.BarCode && o.F_Id != rec.F_Id && o.CountResult != "Outer_LessQty").ToList();   /// 原容器其它盘点记录

                                                                                                                                                                                                                                                       /// 原容器 ---> 空容器，新增一个空容器
                                                if (cdList.Count == 0 && noRecList.Count == 0)
                                                {
                                                    T_ContainerDetailEntity cdEmpty = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode);
                                                    if (cdEmpty.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdEmpty.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                    {
                                                        T_ItemEntity itemEmpty = null;
                                                        T_ContainerEntity containerEmpty = db.FindEntity<T_ContainerEntity>(o => o.BarCode == cd.BarCode && o.F_DeleteMark == false);
                                                        if (containerEmpty.ContainerKind == "Plastic")   /// 料箱
                                                        {
                                                            itemEmpty = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                        }
                                                        else if (containerEmpty.ContainerKind == "Rack")
                                                        {
                                                            itemEmpty = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                        }
                                                        if (itemEmpty == null)
                                                        {
                                                            result.IsSuccess = false;
                                                            result.FailCode = "0001";
                                                            result.FailMsg = "未定义空容器物料";
                                                            return result;
                                                        }

                                                        cdEmpty = new T_ContainerDetailEntity();
                                                        cdEmpty.F_Id = Guid.NewGuid().ToString();
                                                        cdEmpty.AreaCode = cd.AreaCode;
                                                        cdEmpty.AreaID = cd.AreaID;
                                                        cdEmpty.AreaName = cd.AreaName;
                                                        cdEmpty.ContainerID = cd.ContainerID;
                                                        cdEmpty.ContainerKind = cd.ContainerKind;
                                                        cdEmpty.ContainerType = cd.ContainerType;
                                                        cdEmpty.BarCode = cd.BarCode;
                                                        cdEmpty.CheckDetailID = "";
                                                        cdEmpty.CheckID = "";
                                                        cdEmpty.CheckQty = 0;
                                                        cdEmpty.CheckState = "UnNeed";
                                                        cdEmpty.LocationID = cd.LocationID;
                                                        cdEmpty.LocationNo = cd.LocationNo;
                                                        cdEmpty.KindCode = itemEmpty.KindCode;
                                                        cdEmpty.KindName = itemEmpty.KindName;
                                                        cdEmpty.ItemBarCode = "";
                                                        cdEmpty.ItemCode = itemEmpty.ItemCode;
                                                        cdEmpty.ItemID = itemEmpty.F_Id;
                                                        cdEmpty.ItemName = itemEmpty.ItemName;
                                                        cdEmpty.Qty = 1;
                                                        cdEmpty.OutQty = 0;
                                                        cdEmpty.ItemUnitText = itemEmpty.ItemUnitText;
                                                        cdEmpty.State = "Normal";
                                                        cdEmpty.IsCheckFreeze = "false";
                                                        cdEmpty.IsCountFreeze = "false";
                                                        cdEmpty.Lot = "";
                                                        cdEmpty.Spec = "";
                                                        cdEmpty.ERPWarehouseCode = "";
                                                        cdEmpty.Factory = "";
                                                        cdEmpty.OverdueDate = null;
                                                        cdEmpty.ProductDate = null;
                                                        cdEmpty.SupplierCode = "";
                                                        cdEmpty.SupplierID = "";
                                                        cdEmpty.SupplierName = "";
                                                        cdEmpty.ReceiveRecordID = "";
                                                        //cdEmpty.IsSpecial = "false";
                                                        cdEmpty.IsItemMark = "";
                                                        cdEmpty.IsVirItemBarCode = "";
                                                        cdEmpty.ValidityDayNum = 0;
                                                        cdEmpty.F_DeleteMark = false;
                                                        db.Insert<T_ContainerDetailEntity>(cdEmpty);
                                                        db.SaveChanges();

                                                        /// 库存流水
                                                        inOutDetailApp.SyncInOutDetail(db, cdEmpty, "InType", "EmptyIn", 0, cdEmpty.Qty, "");
                                                    }
                                                }

                                                /// 变更库存容器信息
                                                cd.AreaID = container.AreaID;
                                                cd.AreaCode = container.AreaCode;
                                                cd.AreaName = container.AreaName;
                                                cd.ContainerID = container.F_Id;
                                                cd.ContainerKind = container.ContainerKind;
                                                cd.ContainerType = container.ContainerType;
                                                cd.LocationID = container.LocationID;
                                                cd.LocationNo = container.LocationNo;
                                                cd.State = cdOther.State;
                                                cd.ERPWarehouseCode = cdOther.ERPWarehouseCode;
                                                cd.BarCode = rec.BarCode;
                                                cd.ItemBarCode = rec.ItemBarCode;
                                                cd.Qty = rec.CountQty;
                                                cd.IsCountFreeze = "false";
                                                db.Update<T_ContainerDetailEntity>(cd);

                                                /// 产生流水
                                                if (cd.Qty != rec.CountQty)
                                                {
                                                    decimal? changeQty = rec.CountQty - cd.Qty;
                                                    if (changeQty > 0) inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", cd.Qty, changeQty, "");
                                                    else inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", 0, -changeQty, "");
                                                }
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "Outer_MoreQty": /// 多余数量（修改库存数量）
                                        {
                                            /// 库存流水
                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                            inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", cd.Qty, rec.CountQty - cd.Qty, "");

                                            isChangeCD = true;
                                            cd.Qty = rec.CountQty;
                                            db.Update<T_ContainerDetailEntity>(cd);
                                        }
                                        break;
                                    default:
                                        {
                                            result.IsSuccess = false;
                                            result.FailCode = "0001";
                                            result.FailMsg = "未知的盘点结果";
                                            return result;
                                        }
                                }
                            }
                            else if (existsEntity.CountResult == "UnPass")
                            {
                                /// 不通过，什么都不做
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "未知的盘点结果参数";
                                return result;
                            }
                        }

                        /// 盘点明细状态
                        detail.AuditState = existsEntity.CountResult;
                        detail.CountResult = existsEntity.CountResult;
                        detail.CountState = "Over";
                        db.Update<T_CountDetailEntity>(detail);
                        db.SaveChanges();
                    }
                    else
                    {
                        existsEntity.IsUsed = "false";
                    }
                    db.Insert<T_CountResultEntity>(existsEntity);
                }

                /// 更新盘点单
                if (RuleConfig.CountRule.AuditResult.AutoUsedCountResult) /// 判断是否自动应用盘点结果
                {
                    count.State = "Over";
                    count.AuditResult = "Applied";
                    if (detailListAll.Any(o => o.AuditState == "UnPass"))
                    {
                        count.AuditState = "UnPass";
                    }
                    else count.AuditState = "Pass";
                    db.Update<T_CountEntity>(count);
                    db.SaveChanges();

                    if (isChangeCD && RuleConfig.OrderTransRule.CountTransRule.CountTrans) /// 库存变动 && 允许自动过账
                    {
                        if (count.GenType == "ERP")
                        {
                            /// 产生过账信息，并发送过账信息
                            string transType = "Count";
                            AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, count.F_Id, transType);
                            if ((ResultType)rst.state == ResultType.success)
                            {
                                T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                ERPPost post = new ERPPost();
                                ERPResult erpRst = post.PostFactInOutQty(db, transType, trans.F_Id);
                            }
                            else
                            {
                                result.IsSuccess = false;
                                result.FailCode = "0001";
                                result.FailMsg = "过账信息产生失败";
                                return result;
                            }
                        }
                    }
                }
                db.SaveChanges();

                result.IsSuccess = true;

                return result;
            }
            catch (DbEntityValidationException ex)
            {
                foreach (DbValidationError error in ex.EntityValidationErrors.SelectMany(s => s.ValidationErrors))
                {

                }
                throw ex;
            }

            catch (System.Data.Entity.Core.UpdateException ex)
            {
                throw ex;
            }

            catch (System.Data.Entity.Infrastructure.DbUpdateException ex) //DbContext
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 同步到货单与入库单单号
        public ERPResult SyncERPOrderCode(SyncERPOrderCodeModel model)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                try
                {
                    ERPResult result = new ERPResult();
                    if (string.IsNullOrEmpty(model.ActionType))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "操作类型必填";
                        return result;
                    }
                    if (string.IsNullOrEmpty(model.ERPArriveDocCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "到货单编码必填";
                        return result;
                    }

                    if (string.IsNullOrEmpty(model.ERPInDocCode))
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "入库单编码必填";
                        return result;
                    }

                    T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.RefOrderCode == model.ERPArriveDocCode);
                    if (inbound.State != "Over")
                    {
                        result.IsSuccess = false;
                        result.FailCode = "0001";
                        result.FailMsg = "入库单据状态不为结束";
                        return result;
                    }
                    inbound.ERPInDocCode = model.ERPInDocCode;
                    db.Update<T_InBoundEntity>(inbound);
                    db.SaveChanges();

                    IList<T_OutBoundDetailEntity> outDetailList = db.FindList<T_OutBoundDetailEntity>(o => o.SourceInOrderCode == model.ERPArriveDocCode);
                    foreach (T_OutBoundDetailEntity cell in outDetailList)
                    {
                        cell.SourceInOrderCode = model.ERPInDocCode;
                        db.Update<T_OutBoundDetailEntity>(cell);
                        db.SaveChanges();
                    }

                    IList<T_ContainerDetailEntity> conDetailList = db.FindList<T_ContainerDetailEntity>(o => o.RefInBoundCode == model.ERPArriveDocCode);
                    foreach (T_ContainerDetailEntity detail in conDetailList)
                    {
                        detail.ERPInDocCode = model.ERPInDocCode;
                        db.Update<T_ContainerDetailEntity>(detail);
                        db.SaveChanges();
                    }

                    db.CommitWithOutRollBack();

                    result.IsSuccess = true;
                    return result;
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    throw ex;
                }
            }
        }
        #endregion

        /*******************************/
    }

    #region ERP数据模型定义
    /// <summary>
    /// 供应商数据
    /// </summary>
    public class SupplierFaceModel
    {
        /// <summary>
        /// 1：代表新增,2：代表删除,3：代表修改
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// 供应商地址
        /// </summary>
        public string SupplierAddr { get; set; }
        /// <summary>
        /// 供应商电话
        /// </summary>
        public string SupplierPho { get; set; }
    }

    /// <summary>
    /// 物料种类数据
    /// </summary>
    public class ItemKindFaceModel
    {
        /// <summary>
        /// 1：代表新增,2：代表删除,3：代表修改
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 种类编码
        /// </summary>
        public string KindCode { get; set; }
        /// <summary>
        /// 种类名称
        /// </summary>
        public string KindName { get; set; }
        /// <summary>
        /// 父种类编码，顶级父类为0
        /// </summary>
        public string ParentKindCode { get; set; }
    }

    /// <summary>
    /// 物料基础数据
    /// </summary>
    public class ItemFaceModel
    {
        /// <summary>
        /// 1：代表新增,2：代表删除,3：代表修改
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 种类编码
        /// </summary>
        public string KindCode { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 物料单位
        /// </summary>
        public string ItemUnitText { get; set; }
        /// <summary>
        /// 预警数量,库存低于该值则出现提示
        /// </summary>
        public string WarningQty { get; set; }
        /// <summary>
        /// 最大库存数量
        /// </summary>
        public string MaxQty { get; set; }

        /// <summary>
        /// 保留库存
        /// </summary>
        public string MinQty { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 有效期天数（正整数，以天计，无有效期填0）
        /// </summary>
        public string ValidityDayNum { get; set; }
        /// <summary>
        /// 整箱数量
        /// </summary>
        public string UnitQty { get; set; }
        /// <summary>
        /// 生产厂家
        /// </summary>
        public string Factory { get; set; }
        /// <summary>
        /// 是否需要质检（true：需质检，false：免检）
        /// </summary>
        public string IsNeedCheck { get; set; }

        /// <summary>
        /// 应存放ERP仓库的仓库编码
        /// </summary>
        public string ERPWarehouseCode { get; set; }
        /// <summary>
        /// 是否强制批号
        /// </summary>
        public string IsMustLot { get; set; }

        /// <summary>
        /// 预警天数
        /// </summary>
        public string ValidityWarning { get; set; }

        /// <summary>
        /// 每箱抽检百分比（%）
        /// </summary>
        public string CheckBoxPerc { get; set; }

        /// <summary>
        /// 抽检百分比（%）
        /// </summary>
        public string CheckPerc { get; set; }
    }

    /// <summary>
    /// 部门信息
    /// </summary>
    public class DeptModel
    {
        /// <summary>
        /// 1：代表新增,2：代表删除,3：代表修改
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 部门编码
        /// </summary>
        public string DeptCode { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; }
        /// <summary>
        /// 父部门编码，顶级父类为0
        /// </summary>
        public string ParentDeptCode { get; set; }
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// 1：代表新增,2：代表删除,3：代表修改
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 用户编码(登录账号)
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 所属部门编码
        /// </summary>
        public string DeptCode { get; set; }
        /// <summary>
        /// 密码明文
        /// </summary>
        public string Pwd { get; set; }
        /// <summary>
        /// 性别 1男，0女
        /// </summary>
        public string Gender { get; set; }
    }

    /// <summary>
    /// 入库单
    /// </summary>
    public class InOrderFaceModel
    {
        /// <summary>
        /// 1新增、2删除、3修改，Items只需包含需新增、需删除、需修改的项，修改时，Items为空表示修改主数据，删除时，Items为空表示删除整个单据主表和明细。WMS操作数据以SEQ为唯一标识
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// ERP单据编码
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 入库单类型，此项目固定值采购入库：PurchaseInType
        /// </summary>
        public string InBoundType { get; set; }
        /// <summary>
        /// 是否自动入库
        /// </summary>
        public string IsAuto { get; set; }
        /// <summary>
        /// 入库单明细
        /// </summary>
        public IList<InOrderFaceCellModel> Items { get; set; }
    }

    public class InOrderFaceCellModel
    {
        /// <summary>
        /// 项次
        /// </summary>
        public string SEQ { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 需要入库到ERP仓库的仓库编码
        /// </summary>
        public string ERPWarehouseCode { get; set; }
        /// <summary>
        /// 生产日期 格式 YYYY-MM-DD HH:mm:ss
        /// </summary>
        public string ProductDate { get; set; }
        /// <summary>
        /// 入库数量
        /// </summary>
        public string Qty { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 质检状态：Qua：合格, UnQua：不合格, UnNeed：免检 , WaitCheck：待检
        /// </summary>
        public string CheckState { get; set; }

        /// <summary>
        /// 是否实收数量必须和采购数量一致
        /// </summary>
        public string IsMustQtySame { get; set; }
    }

    /// <summary>
    /// 出库单
    /// </summary>
    public class OutOrderFaceModel
    {
        /// <summary>
        /// 1新增、2删除、3修改，Items只需包含需新增、需删除、需修改的项，修改时，Items为空表示修改主数据，删除时，Items为空表示删除整个单据主表和明细。WMS操作数据以SEQ为唯一标识
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// ERP单据编码
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// 出库单类型， GetItemOut：领料出库（正常物料出库），WarehouseBackOut：仓退出库（合格但过期物料出库）,VerBackOut：验退出库（不合格物料出库）
        /// </summary>
        public string OutBoundType { get; set; }
        /// <summary>
        /// 是否紧急出库
        /// </summary>
        public string IsUrgent { get; set; }
        /// <summary>
        /// 是否自动出库
        /// </summary>
        public string IsAuto { get; set; }
        /// <summary>
        /// 出库单明细
        /// </summary>
        public IList<OutOrderFaceCellModel> Items { get; set; }

        /// <summary>
        /// 接收部门编码
        /// </summary>
        public string ReceiveDepartmentCode { get; set; }

        /// <summary>
        /// 接收部门名称
        /// </summary>
        public string ReceiveDepartmentName { get; set; }

        /// <summary>
        /// 接收人编码
        /// </summary>
        public string ReceiveUserCode { get; set; }

        /// <summary>
        /// 接收人名称
        /// </summary>
        public string ReceiveUserName { get; set; }
    }

    public class OutOrderFaceCellModel
    {
        /// <summary>
        /// 项次
        /// </summary>
        public string SEQ { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 出库数量
        /// </summary>
        public string Qty { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
        /// 按对应入库单出库（入库单的来源单号）
        /// </summary>
        public string SourceInOrderCode { get; set; }

    }

    /// <summary>
    /// 质检出库单
    /// </summary>
    public class QAOutOrderFaceModel
    {
        /// <summary>
        /// 1新增、2删除、3修改，Items只需包含需新增、需删除、需修改的项，修改时，Items为空表示修改主数据，删除时，Items为空表示删除整个单据主表和明细。WMS操作数据以SEQ为唯一标识
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// ERP单据编码
        /// </summary>
        public string RefOrderCode { get; set; }

        /// <summary>
        /// ERP入库单的来源单号
        /// </summary>
        public string RefInBoundCode { get; set; }
        /// <summary>
        /// 是否自动质检取样
        /// </summary>
        public string IsAuto { get; set; }
        /// <summary>
        /// 质检出库单明细
        /// </summary>
        public IList<QAOutOrderFaceCellModel> Items { get; set; }
    }

    public class QAOutOrderFaceCellModel
    {
        /// <summary>
        /// 项次
        /// </summary>
        public string SEQ { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 取样物料总数
        /// </summary>
        public string SampleSumNum { get; set; }
        /// <summary>
        /// 取样标签个数
        /// </summary>
        public string SampleSumCnt { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 是否破坏性质检（true：是，false：否）
        /// </summary>
        public string IsBroken { get; set; }
        /// <summary>
        /// 是否外观质检（true：是，false：否）
        /// </summary>
        public string IsAppearQA { get; set; }
    }

    /// <summary>
    /// 盘点单
    /// </summary>
    public class CountOrderFaceModel
    {
        /// <summary>
        /// 1新增、2删除、3修改，Items只需包含需新增、需删除、需修改的项，修改时，Items为空表示修改主数据，删除时，Items为空表示删除整个单据主表和明细。WMS操作数据以SEQ为唯一标识
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// ERP单据编码
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// ERP仓库编码（针对ERP仓进行盘点单作业）
        /// </summary>
        public string ERPWarehouseCode { get; set; }
        /// <summary>
        /// 盘点方法  ByItem 指定物料
        /// </summary>
        public string CountMethod { get; set; }
        /// <summary>
        /// 是否明盘 true明盘，false盲盘
        /// </summary>
        public string IsOpen { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 是否自动执行盘点单据
        /// </summary>
        public string IsAuto { get; set; }
        /// <summary>
        /// 盘点单明细
        /// </summary>
        public IList<CountOrderFaceCellModel> Items { get; set; }
    }

    public class CountOrderFaceCellModel
    {
        /// <summary>
        /// 项次
        /// </summary>
        public string SEQ { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }

        /// <summary>
		/// ERP账目数量
		/// </summary>
		public decimal? ERPQty { get; set; }
    }


    /// <summary>
    /// 质检结果反馈
    /// </summary>
    public class QAResultFaceModel
    {
        /// <summary>
        /// 1新增、2删除、3修改，Items只需包含需新增、需删除、需修改的项，修改时，Items为空表示修改主数据，删除时，Items为空表示删除整个单据主表和明细。WMS操作数据以SEQ为唯一标识
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// ERP质检出库单据编码（样品对应的ERP质检出库单）
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// 质检结果明细
        /// </summary>
        public IList<QAResultFaceCellModel> Items { get; set; }
    }

    public class QAResultFaceCellModel
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 质检结果（Qua：合格，UnQua：不合格）
        /// </summary>
        public string QAResult { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 对应质检单明细项次
        /// </summary>
        public string SEQ { get; set; }
    }

    /// <summary>
    /// 盘点审核反馈
    /// </summary>
    public class CountAuditResultFaceModel
    {
        /// <summary>
        /// 1新增、2删除、3修改，Items只需包含需新增、需删除、需修改的项，修改时，Items为空表示修改主数据，删除时，Items为空表示删除整个单据主表和明细。WMS操作数据以SEQ为唯一标识
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// ERP记录的WMS盘点单据编码
        /// </summary>
        public string CountOrderCode { get; set; }
        /// <summary>
        /// 来源单据
        /// </summary>
        public string RefOrderCode { get; set; }
        /// <summary>
        /// 盘点审核明细
        /// </summary>
        public IList<CountAuditResultFaceCellModel> Items { get; set; }
    }

    public class CountAuditResultFaceCellModel
    {
        /// <summary>
        /// 盘点单明细的项次
        /// </summary>
        public string SEQ { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierCode { get; set; }
        /// <summary>
        /// 对应的ERP仓库的仓库编码
        /// </summary>
        public string ERPWarehouseCode { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string Lot { get; set; }
        /// <summary>
        /// 盘点结果（Pass通过，UnPass 不通过）
        /// </summary>
        public string CountResult { get; set; }
    }


    /// <summary>
    /// 同步到货单与入库单单号
    /// </summary>
    public class SyncERPOrderCodeModel
    {
        /// <summary>
        /// 1：代表新增,2：代表删除,3：代表修改
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// ERP到货单编码
        /// </summary>
        public string ERPArriveDocCode { get; set; }
        /// <summary>
        /// ERP入库单编码
        /// </summary>
        public string ERPInDocCode { get; set; }
    }


    #endregion
}
