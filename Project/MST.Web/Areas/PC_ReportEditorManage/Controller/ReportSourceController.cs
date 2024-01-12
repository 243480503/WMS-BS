/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.WMSLogic;
using MST.Code;
using MST.Data;
using MST.Domain.ViewModel;
using System;
using System.Data;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.Entity.SystemSecurity;
using MST.Application.SystemSecurity;
using MST.Application;

namespace MST.Web.Areas.PC_ReportEditorManage.Controllers
{
    public class ReportSourceController : ControllerBase
    {

        /// <summary>
        /// 数据源列表页列表数据
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            var data = new T_ReportSourceApp().GetList(pagination, keyword);
            var resultList = new
            {
                rows = data,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }

        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "ReportSourceController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "数据源管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除数据源";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {                

                new T_ReportSourceApp().DeleteForm(keyValue);

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


        /// <summary>
        /// 选中查询类型为数据库时，则点击刷新按钮，根据配置获取表或视图下拉列表数据
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDBTabViewList(T_ReportSourceEntity source)
        {
            DataTable dt = new DataTable();
            IList<dynamic> dynList=null;
            if (source.IsTelnet == "false") //非网络数据
            {
                switch (source.SelectType)
                {
                    case "View":
                        {
                            dt = new RepositoryBase().GetDataTable("select Name id,Name text from SysObjects where Type='V' order by Name", null);
                            dynList = dt.ToDynamicList();

                        }
                        break;
                    case "Table":
                        {
                            dt = new RepositoryBase().GetDataTable("select Name id,Name text from SysObjects where Type='U' order by Name", null);
                            dynList = dt.ToDynamicList();
                        }
                        break;
                    case "Sql":
                        {
                            dt = new RepositoryBase().GetDataTable(source.Sql, null);
                            dynList = dt.ToDynamicList();
                        }
                        break;
                }
            }
            else
            {

            }

            return Content(dynList.ToJson());
        }


        /// <summary>
        /// 检查数据源连接配置，并返回字段列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult CheckConnection(T_ReportSourceEntity source)
        { 
            DataTable dt = new DataTable();
            if (source.IsDB == "false")  //接口
            {
                if (source.IsTelnet == "false") //非远程接口
                {
                    string responseStr = MST.Code.HttpMethods.HttpGet(source.URL);
                    object obj = JsonConvert.DeserializeAnonymousType<object>(responseStr, new { });
                }
                else //远程接口
                {

                }
            }
            else  //数据库 
            {

                switch (source.SelectType)
                {
                    case "Table":
                        {
                            if (source.IsTelnet == "false") //非远程
                            {
                                dt = new RepositoryBase().GetDataTable("select * from " + source.TableCode, null);
                            }
                            else //远程
                            {

                            }
                        }
                        break;
                    case "View":
                        {
                            if (source.IsTelnet == "false") //非远程
                            {
                                dt = new RepositoryBase().GetDataTable("select * from " + source.ViewCode, null);
                            }
                            else //远程
                            {

                            }
                        }
                        break;
                    case "Sql":
                        {
                            if (source.IsTelnet == "false") //非远程
                            {
                                dt = new RepositoryBase().GetDataTable(source.Sql, null);
                            }
                            else //远程
                            {

                            }
                        }
                        break;
                }
            }

           
            

            IList<ReportSourceColumn> colList = new List<ReportSourceColumn>();
            int Sort = 1;
            foreach (DataColumn item in dt.Columns)
            {
                ReportSourceColumn col = new ReportSourceColumn();
                col.FieldCode = item.ColumnName;
                colList.Add(col);
                Sort = Sort + 1;
            }
            return Content(colList.ToJson());
        }

        /// <summary>
        /// 保存数据源modelStr
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SubmitSourceModel(string SourceJson)
        {
            T_ReportSourceEntity Source = SourceJson.ToObject<T_ReportSourceEntity>();
            if(string.IsNullOrEmpty(Source.F_Id)) //新增数据
            {
                Source.Create();
                new T_ReportSourceApp().Insert(Source);
            }
            else //数据库已存在
            {
                new T_ReportSourceApp().SubmitForm(Source, Source.F_Id);
            }
            return Success("操作成功");
        }
    }
}
