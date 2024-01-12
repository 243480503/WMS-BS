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
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_BaseDataManage.Controllers
{
    public class RowLineController : ControllerBase
    {
        private T_EquApp equApp = new T_EquApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_DevRowApp devRowApp = new T_DevRowApp();
        private T_RowLineApp rowLineApp = new T_RowLineApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_AreaApp areaApp = new T_AreaApp();

        /// <summary>
        /// 获取行列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyValue">当前巷道ID</param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyValue, string keyword)
        {
            IList<T_RowLineEntity> data = rowLineApp.GetList(pagination, keyValue, keyword);
            IList<RowLineModel> modelList = new List<RowLineModel>();
            foreach (T_RowLineEntity dr in data)
            {
                RowLineModel model = dr.ToObject<RowLineModel>();
                modelList.Add(model);
            }

            modelList = modelList.OrderBy(o => o.Line).ToList();

            var resultList = new
            {
                rows = modelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }

        #region 获取设备字典列表
        /// <summary>
        /// 获取设备字典列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDevDicList()
        {
            List<T_EquEntity> equList = equApp.FindList(o => o.EquType == "Stacker").ToList();
            //equList.Add(new T_EquEntity() { F_Id = "AGV", EquName = "AGV" }); 
            return Content(equList.ToJson());
        }
        #endregion

        #region 获取行字典列表
        public class LineModel
        {
            public int? Line { get; set; }
            public string F_Id { get; set; }
        }

        /// <summary>
        /// 获取行字典列表
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <param name="devRowID">当前巷道ID</param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLineDicList(string areaID, string devRowID)
        {
            List<LineModel> lineList = new T_LocationApp().FindList(o => o.AreaID == areaID && o.LocationType == "Cube").Select(o => new LineModel { Line = o.Line, F_Id = o.Line.ToString() }).Distinct().OrderBy(o => o.Line).ToList();
            var list = rowLineApp.FindList(o => o.DevRowID == devRowID).ToList();

            foreach (var line in lineList) //获取当前行，用于绑定行的下拉列表数据 
            {
                T_RowLineEntity lineTemp = list.FirstOrDefault(o => o.Line == line.Line);
                if (lineTemp != null)
                {
                    line.F_Id = lineTemp.F_Id;
                }
            }

            if (string.IsNullOrEmpty(devRowID)) //新增
            {
                int?[] lineArray = list.Select(o => o.Line).Distinct().ToArray();
                lineList = lineList.Where(o => !lineArray.Contains(o.Line)).OrderBy(o => o.Line).ToList();
            }

            return Content(lineList.ToJson());
        }

        /// <summary>
        /// 获取行字典列表
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <param name="devRowID">当前巷道ID</param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLayerDicList(string areaID, int line)
        {
            IList<int> layer = new T_LocationApp().FindList(o => o.AreaID == areaID && o.Line == line).Select(o => o.Layer ?? 0).Distinct().OrderBy(o => o).ToList();

            layer.Add(0);
            layer = layer.OrderBy(o => o).ToList();

            var data = layer.Select(o => new {F_Id = o ,Layer = o });

            return Content(data.ToJson());
        }
        #endregion


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = rowLineApp.GetForm(keyValue);
            RowLineModel model = data.ToObject<RowLineModel>();
            return Content(model.ToJson());
        }

        #region 新增/修改行
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_RowLineEntity rowLineEntity, string areaID, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DevRowController.SubmitForm";
            logObj.Parms = new { rowLineEntity = rowLineEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "巷道";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "新增/修改行";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    if (!user.IsSystem)
                    {
                        T_RowLineEntity entity = rowLineApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                        if (entity.IsBase == "true")
                        {
                            return Error("系统数据不允许修改。", "");
                        }
                    }
                }

                string[] devRowIDList = new T_DevRowApp().FindList(o => o.AreaID == areaID).Select(o => o.F_Id).ToArray();
                IList<T_RowLineEntity> existsList = rowLineApp.FindList(o => devRowIDList.Contains(o.DevRowID) && o.Line == rowLineEntity.Line && o.DevRowID == rowLineEntity.DevRowID && o.F_Id != keyValue).ToList();
                if (existsList.Count > 0)
                {
                    return Error("行号已被指定。", "");
                }

                rowLineApp.SubmitForm(rowLineEntity, keyValue);

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


        #region 删除行
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "DevRowController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "巷道";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除行";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    T_RowLineEntity entity = rowLineApp.FindEntity(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                rowLineApp.Delete(o => o.F_Id == keyValue);

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
    }
}
