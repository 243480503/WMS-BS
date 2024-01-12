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
using MST.Mapping.SystemManage;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_BaseDataManage.Controllers
{
    public class ItemController : ControllerBase
    {
        private ItemsDetailApp itemsDetailEnum = new ItemsDetailApp();
        private T_ItemKindApp itemKindApp = new T_ItemKindApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private T_ItemAreaApp itemAreaApp = new T_ItemAreaApp();
        private T_ItemInStationApp itemInStationApp = new T_ItemInStationApp();
        private T_ERPWarehouseApp erpApp = new T_ERPWarehouseApp();

        [HttpGet]
        [HandlerAuthorize]
        public ActionResult ItemUpload()
        {
            return View();
        }

        #region 物料Excel导入
        [HttpPost]
        public ActionResult UploadAndCheckItemExcel()
        {
            try
            {
                if (HttpContext.Request.Files.Count < 1)
                {
                    return Error("文件不能为空", "");
                }
                else
                {
                    HttpPostedFileBase file = HttpContext.Request.Files[0];

                    string ext = Path.GetExtension(file.FileName);
                    if (string.IsNullOrEmpty(ext) || (ext.ToLower() != ".xls" && ext.ToLower() != ".xlsx"))
                    {
                        return Error("非Excel文件", "");
                    }

                    string datePath = "ItemExcel\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
                    string rootPath = Server.MapPath("~");
                    string fullPath = rootPath + datePath;
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    string fileName = datePath + Guid.NewGuid().ToString() + "_" + file.FileName;
                    string lastPath = rootPath + fileName;
                    file.SaveAs(lastPath);
                    return Success("上传成功", fileName);
                }
            }
            catch (Exception ex)
            {
                return Error(ex.Message, "");
            }
        }

        private AjaxResult CheckDataTable(DataTable dt)
        {
            try
            {
                List<ExcelColModel> colNumList = new List<ExcelColModel>();
                //物料编码 物料名称    品类编码    单位           预警数量     预警天数    最大库存  保留库存    规格   需贴标      有效期天数   生产厂家    单位数量  容器类型       ERP仓库编码      是否需质检   是否破坏性质检 是否批次控制
                //ItemCode ItemName    KindCode ItemUnitText    WarningQty ValidityWarning   MaxQty  MinQty      Spec IsItemMark  ValidityDayNum Factory     UnitQty  ContainerType   ERPWarehouseCode IsNeedCheck       IsBroken    IsMustLot

                colNumList.Add(new ExcelColModel() { ColName = "物料编码", ColCode = "ItemCode", NotEmpty = true, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "物料名称", ColCode = "ItemName", NotEmpty = true, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "品类编码", ColCode = "KindCode", NotEmpty = true, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "单位名称", ColCode = "ItemUnitText", NotEmpty = true, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "预警数量", ColCode = "WarningQty", NotEmpty = true, ValueType = "decimal", IsCanZero = true });
                colNumList.Add(new ExcelColModel() { ColName = "预警天数", ColCode = "ValidityWarning", NotEmpty = true, ValueType = "int", IsCanZero = true });
                colNumList.Add(new ExcelColModel() { ColName = "最大库存", ColCode = "MaxQty", NotEmpty = true, ValueType = "decimal", IsCanZero = true });
                colNumList.Add(new ExcelColModel() { ColName = "保留库存", ColCode = "MinQty", NotEmpty = true, ValueType = "decimal", IsCanZero = true });
                colNumList.Add(new ExcelColModel() { ColName = "规格", ColCode = "Spec", NotEmpty = false, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "是否需贴标", ColCode = "IsItemMark", NotEmpty = true, ValueType = "bool" });
                colNumList.Add(new ExcelColModel() { ColName = "有效期时长", ColCode = "ValidityDayNum", NotEmpty = true, ValueType = "int", IsCanZero = true });
                colNumList.Add(new ExcelColModel() { ColName = "时长单位", ColCode = "ValidityUnitType", NotEmpty = true, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "生产厂家", ColCode = "Factory", NotEmpty = false, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "单位数量", ColCode = "UnitQty", NotEmpty = true, ValueType = "decimal", IsCanZero = false });
                colNumList.Add(new ExcelColModel() { ColName = "容器类型", ColCode = "ContainerType", NotEmpty = true, ValueType = "string" });
                //colNumList.Add(new ExcelColModel() { ColName = "ERP仓库编码", ColCode = "ERPWarehouseCode", NotEmpty = true, ValueType = "string" });
                colNumList.Add(new ExcelColModel() { ColName = "是否需质检", ColCode = "IsNeedCheck", NotEmpty = true, ValueType = "bool" });
                //colNumList.Add(new ExcelColModel() { ColName = "是否破坏性质检", ColCode = "IsBroken", NotEmpty = true, ValueType = "bool" });
                colNumList.Add(new ExcelColModel() { ColName = "是否批次控制", ColCode = "IsMustLot", NotEmpty = true, ValueType = "bool" });

                List<ExcelCheckModel> checkList = new List<ExcelCheckModel>();

                //列名校验
                foreach (ExcelColModel tempCol in colNumList)
                {
                    bool isInBook = false;
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (tempCol.ColName == col.ColumnName)
                        {
                            isInBook = true;
                        }
                    }
                    if (!isInBook) //Excel列名不匹配
                    {
                        checkList.Add(new ExcelCheckModel() { Type = 1, ColName = tempCol.ColName });
                    }
                }

                if (checkList.Count > 0)
                {
                    return new AjaxResult() { state = ResultType.error.ToString(), message = "缺少列:" + string.Join(",", checkList.Where(o => o.Type == 1).Select(o => o.ColName).Distinct().ToList()) };
                }

                //重复值校验
                var repItemCode = dt.AsEnumerable().Select(o => new { ItemCode = o.Field<string>("物料编码") }).GroupBy(o => new { ItemCode = o.ItemCode }).Select(o => new { ItemCode = o.Key.ItemCode, Count = o.Count() }).Where(o => o.Count > 1).Select(o => new { ItemCode = o.ItemCode }).Distinct().ToList();
                if (repItemCode.Count > 0)
                {
                    return new AjaxResult() { state = ResultType.error.ToString(), message = "物料编码重复:" + string.Join(",", repItemCode) };
                }


                IList<T_ItemEntity> itemList = new T_ItemApp().FindList(o => true).ToList();
                IList<T_ContainerTypeEntity> containerTypeList = new T_ContainerTypeApp().FindList(o => o.F_DeleteMark != true).ToList();
                IList<T_ItemKindEntity> itemKindList = new T_ItemKindApp().FindList(o => o.F_DeleteMark != true).ToList();


                int rowCount = 0;
                foreach (DataRow r in dt.Rows)
                {
                    rowCount = rowCount + 1;
                    foreach (ExcelColModel m in colNumList)
                    {
                        string cellVal = r[m.ColName] == null ? null : r[m.ColName].ToString();

                        if (m.NotEmpty) //为空类型验证
                        {
                            if (string.IsNullOrEmpty(cellVal))
                            {
                                checkList.Add(new ExcelCheckModel() { Type = 3, RowNum = rowCount, ColName = m.ColName });
                                continue;
                            }
                        }

                        switch (m.ValueType)
                        {
                            case "string":
                                {
                                    //什么也不做
                                }
                                break;
                            case "int":
                                {
                                    int outVal = 0;
                                    if (int.TryParse(cellVal, out outVal))
                                    {
                                        if ((!m.IsCanZero) && outVal == 0)
                                        {
                                            checkList.Add(new ExcelCheckModel() { Type = 5, RowNum = rowCount, ColName = m.ColName });
                                            continue;
                                        }

                                        if (outVal > int.MaxValue || outVal < 0)
                                        {
                                            checkList.Add(new ExcelCheckModel() { Type = 6, RowNum = rowCount, ColName = m.ColName });
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        checkList.Add(new ExcelCheckModel() { Type = 4, RowNum = rowCount, ColName = m.ColName });
                                        continue;
                                    }
                                }
                                break;
                            case "decimal":
                                {
                                    decimal outVal = 0;
                                    if (decimal.TryParse(cellVal, out outVal))
                                    {
                                        if ((!m.IsCanZero) && outVal == 0)
                                        {
                                            checkList.Add(new ExcelCheckModel() { Type = 5, RowNum = rowCount, ColName = m.ColName });
                                            continue;
                                        }

                                        if (outVal > decimal.MaxValue || outVal < 0)
                                        {
                                            checkList.Add(new ExcelCheckModel() { Type = 6, RowNum = rowCount, ColName = m.ColName });
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        checkList.Add(new ExcelCheckModel() { Type = 4, RowNum = rowCount, ColName = m.ColName });
                                        continue;
                                    }
                                }
                                break;
                            case "bool":
                                {
                                    if (cellVal != "是" && cellVal != "否")
                                    {
                                        checkList.Add(new ExcelCheckModel() { Type = 4, RowNum = rowCount, ColName = m.ColName });
                                        continue;
                                    }
                                }
                                break;
                        }
                    }


                    //物料编码 物料名称    品类编码    单位           预警数量     预警天数    最大库存  保留库存    规格   需贴标      有效期天数   生产厂家    单位数量  容器类型       ERP仓库编码      是否需质检   是否破坏性质检 是否批次控制
                    //ItemCode ItemName    KindCode ItemUnitText    WarningQty ValidityWarning   MaxQty  MinQty      Spec IsItemMark  ValidityDayNum Factory     UnitQty  ContainerType   ERPWarehouseCode IsNeedCheck       IsBroken    IsMustLot



                    T_ContainerTypeEntity containerType = containerTypeList.FirstOrDefault(o => o.ContainerTypeCode == r["容器类型"].ToString());
                    if (containerType == null)
                    {
                        checkList.Add(new ExcelCheckModel() { Type = 7, RowNum = rowCount, ColName = "容器类型", CellVal = r["容器类型"].ToString() });
                        continue;
                    }
                    T_ItemKindEntity itemKind = itemKindList.FirstOrDefault(o => o.KindCode == r["品类编码"].ToString());
                    if (itemKind == null)
                    {
                        checkList.Add(new ExcelCheckModel() { Type = 8, RowNum = rowCount, ColName = "品类编码", CellVal = r["品类编码"].ToString() });
                        continue;
                    }

                }

                if (checkList.Count > 0)
                {
                    IList<ExcelCheckModel> msg3 = checkList.Where(o => o.Type == 3).ToList(); //为空
                    IList<ExcelCheckModel> msg4 = checkList.Where(o => o.Type == 4).ToList(); //值非法
                    IList<ExcelCheckModel> msg5 = checkList.Where(o => o.Type == 5).ToList(); //不可为0
                    IList<ExcelCheckModel> msg6 = checkList.Where(o => o.Type == 6).ToList(); //超过数值范围
                    IList<ExcelCheckModel> msg7 = checkList.Where(o => o.Type == 7).ToList(); //非有效容器编码
                    IList<ExcelCheckModel> msg8 = checkList.Where(o => o.Type == 8).ToList(); //非有效品类

                    string msg = "";

                    if (msg3 != null && msg3.Count > 0)
                    {
                        msg = "值不可为空:";
                        foreach (ExcelCheckModel c in msg3)
                        {
                            msg = msg + "[第" + c.RowNum + "行+" + c.ColName + "]";
                        }
                        msg = msg + "\r\n";
                    }

                    if (msg4 != null && msg4.Count > 0)
                    {
                        msg = "值不正确:";
                        foreach (ExcelCheckModel c in msg4)
                        {
                            msg = msg + "[第" + c.RowNum + "行+" + c.ColName + "]";
                        }
                        msg = msg + "\r\n";
                    }

                    if (msg5 != null && msg5.Count > 0)
                    {
                        msg = "值不可为0:";
                        foreach (ExcelCheckModel c in msg5)
                        {
                            msg = msg + "[第" + c.RowNum + "行+" + c.ColName + "]";
                        }
                        msg = msg + "\r\n";
                    }

                    if (msg6 != null && msg6.Count > 0)
                    {
                        msg = "值范围不正确:";
                        foreach (ExcelCheckModel c in msg6)
                        {
                            msg = msg + "[第" + c.RowNum + "行+" + c.ColName + "]";
                        }
                        msg = msg + "\r\n";
                    }

                    if (msg7 != null && msg7.Count > 0)
                    {
                        msg = "非有效容器类型:";
                        foreach (ExcelCheckModel c in msg7)
                        {
                            msg = msg + "[第" + c.RowNum + "行+" + c.ColName + "值" + c.CellVal + "]";
                        }
                        msg = msg + "\r\n";
                    }

                    if (msg8 != null && msg8.Count > 0)
                    {
                        msg = "非有效品类编码:";
                        foreach (ExcelCheckModel c in msg8)
                        {
                            msg = msg + "[第" + c.RowNum + "行+" + c.ColName + "值" + c.CellVal + "]";
                        }
                        msg = msg + "\r\n";
                    }
                    return new AjaxResult() { state = ResultType.error.ToString(), message = "校验失败:" + msg };

                }
                else
                {
                    return new AjaxResult() { state = ResultType.success.ToString(), message = "校验成功" };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private class ExcelCheckModel
        {
            /// <summary>
            /// 1 缺少列,2 物料编码重复,3为空类型验证,4值类型验证(非法字符),5数值类型验证(是否可为0),6数值类型验证(大于最大值或负数),7非有效容器类型,8非品类
            /// </summary>
            public int Type { get; set; }
            public string ColName { get; set; }
            public int RowNum { get; set; }

            public string CellVal { get; set; }
        }

        private class ExcelColModel
        {
            public string ColName { get; set; }
            public string ColCode { get; set; }
            public bool NotEmpty { get; set; }
            public string ValueType { get; set; }
            /// <summary>
            /// 为数值时，是否可为0
            /// </summary>
            public bool IsCanZero { get; set; }
        }

        /// <summary>
        /// 导入Exce数据
        /// </summary>
        /// <param name="itemEntity"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult DoItemExcel(string upFileName)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ItemController.DoItemExcel";
            logObj.Parms = new { upFileName = upFileName };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "物料管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交文档/Excel导入";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                using (var db = new RepositoryBase().BeginTrans())
                {
                    OperatorModel user = OperatorProvider.Provider.GetCurrent();
                    string target = Server.MapPath("~");

                    //校验DataTable
                    DataTable dt = ExcelHelper.ExcelToDataTable(target + upFileName, "物料信息", true);
                    AjaxResult res = CheckDataTable(dt);
                    if (res.state.ToString() == ResultType.error.ToString())
                    {
                        return Error(res.message, "");
                    }

                    IList<T_ContainerTypeEntity> containerTypeList = db.FindList<T_ContainerTypeEntity>(o => o.F_DeleteMark != true).ToList();
                    IList<T_ItemKindEntity> itemKindList = db.FindList<T_ItemKindEntity>(o => o.F_DeleteMark != true).ToList();

                    foreach (DataRow r in dt.Rows)
                    {
                        string itemCodeStr = r["物料编码"].ToString();
                        T_ItemEntity entity = db.FindEntity<T_ItemEntity>(o => o.ItemCode == itemCodeStr);
                        if ((!user.IsSystem) && entity != null)
                        {
                            if (entity.IsBase == "true")
                            {
                                return Error("系统数据不允许修改:" + entity.ItemName, "");
                            }
                        }




                        string KindCodeStr = r["品类编码"].ToString();
                        T_ItemKindEntity itemKind = itemKindList.FirstOrDefault(o => o.KindCode == KindCodeStr);

                        int isExistsChild = itemKindList.Where(o => o.ParentID == itemKind.F_Id && o.F_DeleteMark == false).Count();
                        if (isExistsChild > 0)
                        {
                            return Error("品类不是终节点：" + itemKind.KindCode, "");
                        }

                        if (entity == null)
                        {
                            entity = new T_ItemEntity();
                        }
                        //物料编码 物料名称    品类编码    单位名称           预警数量     预警天数    最大库存  保留库存    规格   是否需贴标      有效期时长        时长单位               生产厂家    单位数量  容器类型       ERP仓库编码      是否需质检   是否破坏性质检 是否批次控制
                        //ItemCode ItemName    KindCode ItemUnitText    WarningQty ValidityWarning   MaxQty  MinQty      Spec IsItemMark           ValidityDayNum    ValidityUnitType         Factory     UnitQty  ContainerType   ERPWarehouseCode IsNeedCheck       IsBroken    IsMustLot

                        entity.ItemCode = r["物料编码"].ToString();
                        entity.ItemName = r["物料名称"].ToString();
                        entity.KindCode = r["品类编码"].ToString();
                        entity.ItemKindID = itemKind.F_Id;
                        entity.KindName = itemKind.KindName;
                        entity.ItemUnitText = r["单位名称"].ToString();
                        entity.WarningQty = Convert.ToDecimal(r["预警数量"].ToString());
                        entity.ValidityWarning = Convert.ToInt32(r["预警天数"].ToString());
                        entity.MaxQty = Convert.ToDecimal(r["最大库存"].ToString());
                        entity.MinQty = Convert.ToDecimal(r["保留库存"].ToString());
                        entity.Spec = r["规格"].ToString() == null ? "" : r["规格"].ToString();

                        entity.ValidityDayNum = Convert.ToInt32(r["有效期时长"].ToString());
                        string valUnitType = r["时长单位"].ToString();

                        if (valUnitType == "年")
                        {
                            entity.ValidityUnitType = "Year";
                        }
                        else if (valUnitType == "月")
                        {
                            entity.ValidityUnitType = "Month";
                        }
                        else if (valUnitType == "日")
                        {
                            entity.ValidityUnitType = "Day";
                        }

                        entity.Factory = r["生产厂家"].ToString() == null ? "" : r["生产厂家"].ToString();
                        entity.UnitQty = Convert.ToDecimal(r["单位数量"].ToString());
                        entity.CheckPerc = 0;
                        entity.CheckBoxPerc = 0;
                        entity.ContainerType = r["容器类型"].ToString();
                        entity.IsNeedCheck = r["是否需质检"].ToString() == "是" ? "true" : "false";
                        entity.IsMustLot = r["是否批次控制"].ToString() == "是" ? "true" : "false";

                        T_ContainerTypeEntity cType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == entity.ContainerType);
                        if (cType.ContainerKind == "Box")
                        {
                            entity.IsItemMark = "false";
                        }
                        else
                        {
                            entity.IsItemMark = r["是否需贴标"].ToString() == "是" ? "true" : "false";
                        }
                        entity.IsSpecial = "false";
                        entity.IsBroken = "false";
                        entity.IsBase = "false";
                        entity.F_DeleteMark = false;

                        ItemModel model = entity.ToObject<ItemModel>();
                        model.InStationList = new List<T_ItemInStationEntity>();
                        model.StoredAreaList = new List<T_ItemAreaEntity>();


                        if (cType.ContainerKind == "Box" || cType.ContainerKind == "Plastic")
                        {
                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_Normal.ToString());
                            model.InStationList.Add(new T_ItemInStationEntity() { StationID = station.F_Id });

                            T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.NormalArea.ToString());
                            model.StoredAreaList.Add(new T_ItemAreaEntity() { AreaID = area.F_Id });
                        }
                        else if (cType.ContainerKind == "Rack")
                        {

                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_BigItem.ToString());
                            model.InStationList.Add(new T_ItemInStationEntity() { StationID = station.F_Id });

                            T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.BigItemArea.ToString());
                            model.StoredAreaList.Add(new T_ItemAreaEntity() { AreaID = area.F_Id });
                        }
                        itemApp.SubmitForm(db, model, model.F_Id);
                        db.SaveChanges();
                        //根据容器类型，修改入库地点

                        ChangeOrderItem(db, entity);
                        db.SaveChanges();
                    }

                    db.CommitWithOutRollBack();

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
            }
            catch (Exception ex)
            {

                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 变更物料数据时，同时变更新建状态下的入库明细
        private void ChangeOrderItem(IRepositoryBase db, T_ItemEntity item)
        {
            IList<T_InBoundDetailEntity> inboundDetailList = db.FindList<T_InBoundDetailEntity>(o => o.ItemID == item.F_Id && o.State == "New");
            foreach (T_InBoundDetailEntity detail in inboundDetailList)
            {
                T_ItemInStationEntity itemInStation = db.FindEntity<T_ItemInStationEntity>(o => o.ItemID == item.F_Id);
                T_ItemAreaEntity itemArea = db.FindEntity<T_ItemAreaEntity>(o => o.ItemID == item.F_Id);

                detail.ItemName = item.ItemName;
                detail.Factory = item.Factory;
                detail.Spec = item.Spec;
                detail.ItemUnitText = item.ItemUnitText;
                detail.StationID = itemInStation.StationID;
                detail.StoreAreaID = itemArea.AreaID;
                detail.ValidityDayNum = item.ValidityDayNum;
                if (detail.ValidityDayNum != 0)
                {
                    if (item.ValidityUnitType == "Year")
                    {
                        detail.ProductDate = detail.OverdueDate.Value.AddYears(-1 * (detail.ValidityDayNum ?? 0));
                    }
                    else if (item.ValidityUnitType == "Month")
                    {
                        detail.ProductDate = detail.OverdueDate.Value.AddMonths(-1 * (detail.ValidityDayNum ?? 0));
                    }
                    else if (item.ValidityUnitType == "Day")
                    {
                        detail.ProductDate = detail.OverdueDate.Value.AddDays(-1 * (detail.ValidityDayNum ?? 0));
                    }
                }
                else
                {
                    detail.ProductDate = null;
                }
                db.Update<T_InBoundDetailEntity>(detail);
            }

            IList<T_OutBoundDetailEntity> outboundDetailList = db.FindList<T_OutBoundDetailEntity>(o => o.ItemID == item.F_Id && o.State == "New");
            foreach (T_OutBoundDetailEntity detail in outboundDetailList)
            {
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

                detail.ItemName = item.ItemName;
                db.Update<T_OutBoundDetailEntity>(detail);
            }
        }
        #endregion

        #region 物料种类-查询按钮
        /// <summary>
        /// 物料种类-查询按钮
        /// </summary>
        /// <param name="kindId">种类ID</param>
        /// <param name="keyword">关键字</param>
        /// <param name="delType">1全部，2正常，3已删除</param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string delType, string kindId, string keyword)
        {
            var data = itemApp.GetList(pagination, delType, kindId, keyword);
            List<ItemModel> itemEntityList = new List<ItemModel>();
            IList<T_ERPWarehouseEntity> erpList = erpApp.FindList(o => true).ToList();
            IList<ItemsDetailEntity> validityUnitTypeDic = itemsDetailEnum.FindEnum<T_ItemEntity>(o => o.ValidityUnitType).ToList();
            IList<ItemsDetailEntity> conKindDic = itemsDetailEnum.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();
            foreach (T_ItemEntity item in data)
            {
                ItemModel itemModel = item.ToObject<ItemModel>();
                IList<T_ItemAreaEntity> areaList = itemAreaApp.FindList(o => o.ItemID == item.F_Id).ToList();
                itemModel.StoredAreaList = areaList;
                IList<T_ItemInStationEntity> inStationList = itemInStationApp.FindList(o => o.ItemID == item.F_Id).ToList();
                itemModel.InStationList = inStationList;
                if (!string.IsNullOrEmpty(item.ERPWarehouseCode))
                {
                    itemModel.ERPWarehouseName = erpList.FirstOrDefault(o => o.ERPHouseCode == item.ERPWarehouseCode).ERPHouseName;
                }
                T_ContainerTypeEntity cType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == itemModel.ContainerType);
                if (cType != null)
                {
                    itemModel.ContainerKindName = conKindDic.FirstOrDefault(o => o.F_ItemCode == cType.ContainerKind).F_ItemName;
                }

                if (!string.IsNullOrEmpty(itemModel.ValidityUnitType))
                {
                    itemModel.ValidityUnitTypeName = validityUnitTypeDic.FirstOrDefault(o => o.F_ItemCode == itemModel.ValidityUnitType).F_ItemName;
                }
                else
                {
                    itemModel.ValidityUnitTypeName = "";
                }

                itemEntityList.Add(itemModel);
            }

            var resultList = new
            {
                rows = itemEntityList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }
        #endregion

        #region 获取左侧种类树
        /// <summary>
        /// 获取左侧种类树
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeJson()
        {
            List<TreeViewModel> data = itemApp.GetTreeJson();
            return Content(data.TreeViewJson("-1"));
        }
        #endregion

        #region 根据ID获取物料明细
        /// <summary>
        /// 根据ID获取物料明细
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = itemApp.GetForm(keyValue);
            ItemModel model = data.ToObject<ItemModel>();
            model.StoredAreaList = new T_ItemAreaApp().FindList(o => o.ItemID == model.F_Id).ToList();
            model.InStationList = new T_ItemInStationApp().FindList(o => o.ItemID == model.F_Id).ToList();
            model.InStationList = new T_ItemInStationApp().FindList(o => o.ItemID == model.F_Id).ToList();
            model.ContainerTypeName = new T_ContainerTypeApp().FindEntity(o => o.F_Id == model.ContainerType)?.ContainerTypeName;
            if (!string.IsNullOrEmpty(data.ValidityUnitType))
            {
                model.ValidityUnitTypeName = new ItemsDetailApp().FindEnum<T_ItemEntity>(o => o.ValidityUnitType).FirstOrDefault(o => o.F_ItemCode == data.ValidityUnitType).F_ItemName;
            }

            return Content(model.ToJson());
        }
        #endregion

        #region 根据ID新增或修改物料明细
        /// <summary>
        /// 根据ID新增或修改物料明细
        /// </summary>
        /// <param name="itemEntity"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(ItemModel itemModel, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ItemController.SubmitForm";
            logObj.Parms = new { itemModel = itemModel, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "物料管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新建/修改物料";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                using (var db = new RepositoryBase().BeginTrans())
                {
                    OperatorModel user = OperatorProvider.Provider.GetCurrent();
                    if (!string.IsNullOrEmpty(keyValue))
                    {
                        if (!user.IsSystem)
                        {
                            T_ItemEntity entity = db.FindEntity<T_ItemEntity>(o => o.F_Id == keyValue);
                            if (entity.IsBase == "true")
                            {
                                return Error("系统数据不允许修改。", "");
                            }
                        }
                    }

                    int isExistsCode = db.FindList<T_ItemEntity>(o => o.ItemCode == itemModel.ItemCode && o.F_Id != keyValue && o.F_DeleteMark == false).Count();
                    if (isExistsCode > 0)
                    {
                        return Error("编码已存在", "");
                    }

                    T_ItemKindEntity itemKind = db.FindEntity<T_ItemKindEntity>(o => o.F_Id == itemModel.ItemKindID);
                    int isExistsChild = db.FindList<T_ItemKindEntity>(o => o.ParentID == itemKind.F_Id && o.F_DeleteMark == false).Count();
                    if (isExistsChild > 0)
                    {
                        return Error("所属品类不能有子品类", "");
                    }

                    itemApp.SubmitForm(db, itemModel, keyValue);
                    db.SaveChanges();
                    T_ItemEntity itemEntity = db.FindEntity<T_ItemEntity>(o => o.ItemCode == itemModel.ItemCode);
                    ChangeOrderItem(db, itemEntity);

                    db.CommitWithOutRollBack();
                }


                logObj.Message = "操作成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("操作成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 根据ID删除/启用物料明细
        /// <summary>
        /// 根据ID删除/启用物料明细
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ItemController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "物料管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除物料";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_ItemEntity entity = itemApp.FindEntity(o => o.F_Id == keyValue);

                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                entity.F_DeleteMark = !(entity.F_DeleteMark ?? false);
                itemApp.Update(entity);

                logObj.Message = "删除成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("删除成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("删除失败。", ex.ToJson());
            }
        }
        #endregion

        #region 根据容器类型，返回默认入库点和默认存储区域
        /// <summary>
        /// 根据容器类型，返回默认入库点和默认存储区域
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDefInStationAndStoredAreaList(string containerTypeCode)
        {
            T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == containerTypeCode);
            T_StationEntity inStation = stationApp.FindEntity(o => o.F_Id == containerType.InStationID);

            T_AreaEntity areaEntity = new T_AreaEntity();
            if (inStation.StationCode == FixType.Station.StationIn_BigItem.ToString())
            {
                areaEntity = areaApp.FindEntity(o => o.AreaCode == FixType.Area.BigItemArea.ToString());
            }
            else if (inStation.StationCode == FixType.Station.StationIn_Normal.ToString())
            {
                areaEntity = areaApp.FindEntity(o => o.AreaCode == FixType.Area.NormalArea.ToString());
            }
            else
            {
                return Error("未知的入库站台", "");
            }
            var data = new { InStation = inStation, AreaEntity = areaEntity };
            return Content(data.ToJson());
        }
        #endregion

        #region 获取默认入库点列表
        /// <summary>
        /// 获取默认入库点列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetStationDicList()
        {
            List<T_StationEntity> stationList = new List<T_StationEntity>();
            stationList = stationApp.FindList(o => o.UseCode.Contains("PurchaseIn")).ToList();
            return Content(stationList.ToJson());
        }
        #endregion

        #region 获取存放区域列表
        /// <summary>
        /// 获取存放区域列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetAreaDicList()
        {
            List<T_AreaEntity> areaList = new List<T_AreaEntity>();
            areaList = areaApp.FindList(o => true).ToList();
            return Content(areaList.ToJson());
        }
        #endregion

        #region 获取存放区域列表(主要功能区域)
        /// <summary>
        /// 获取存放区域列表(主要功能区域)
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMainAreaDicList()
        {
            List<T_AreaEntity> areaList = new List<T_AreaEntity>();
            areaList = areaApp.FindList(o => o.IsMain == "true").ToList();
            return Content(areaList.ToJson());
        }
        #endregion

        #region 获取容器类型
        /// <summary>
        /// 获取默认入库点列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetContainerDicList()
        {
            List<T_ContainerTypeEntity> enumContainerTypeList = containerTypeApp.FindList(o => o.F_DeleteMark != true).ToList();
            return Content(enumContainerTypeList.ToJson());
        }
        #endregion
    }
}
