/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.SystemManage;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_ReportEditorManage.Controllers
{
    public class ReportSetController : ControllerBase
    {
        /// <summary>
        /// 列表页列表数据
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            var data = new T_ReportSetApp().GetList(pagination, keyword);
            var resultList = new
            {
                rows = data,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }

        /// <summary>
        /// 编辑页获取配置数据
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = new T_ReportSetApp().FindEntity(o => o.F_Id == keyValue);
            ReportSetModel model = data.ToObject<ReportSetModel>();
            return Content(model.ToJson());
        }

        /// <summary>
        /// 编辑页获取权限下拉框
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetAuthJson(string keyValue)
        {
            var data = new RoleApp().FindList(o => o.F_DeleteMark == false);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 编辑页获取权限下拉框
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetSetSourceInfo(string modelStr)
        {
            ReportSetModel modelUI = modelStr.ToObject<ReportSetModel>();

            

            if (!string.IsNullOrEmpty(modelUI.F_Id)) //修改
            {
                //后台所有角色的报表配置
                T_ReportSetEntity setInDb = new T_ReportSetApp().FindEntity(o => o.F_Id == modelUI.F_Id);
                ReportSetConfigModel setConfigModelInDb = setInDb.ReportSetConfig.ToObject<ReportSetConfigModel>(); //后台数据库中的报表配置

                //获取后端报表配置赋值到前端
                for (int i = 0; i < modelUI.ReportSetConfigModel.ReportOwnerList.Count; i++)
                {
                    ReportOwner reportOwner = modelUI.ReportSetConfigModel.ReportOwnerList[i];
                    ReportOwner ownerInDB = setConfigModelInDb.ReportOwnerList.FirstOrDefault(o => o.Role.F_Id == reportOwner.Role.F_Id);
                    if (ownerInDB != null)
                    {
                        reportOwner = ownerInDB;
                    }
                }
                modelUI.ReportSetConfigModel.DefOwner = setConfigModelInDb.DefOwner;
            }
            else  //新增时，初始化通用字段
            {
                //初始通用
                modelUI.ReportSetConfigModel.DefOwner = new ReportOwner();
                RoleEntity nomalRole = new RoleEntity();
                nomalRole.F_Id = "0";
                nomalRole.F_FullName = "通用";
                nomalRole.F_EnCode = "0";
                modelUI.ReportSetConfigModel.DefOwner.Role=nomalRole;//通用

                //初始选中角色
                foreach (ReportOwner owner in modelUI.ReportSetConfigModel.ReportOwnerList)
                {
                    RoleEntity role = new RoleApp().FindEntity(o => o.F_Id == owner.Role.F_Id);
                    owner.Role.F_FullName = role.F_FullName;
                    owner.Role.F_EnCode = role.F_EnCode;
                    owner.Role.F_Id = role.F_Id;
                }

                //合并角色后，初始化数据源字段
                List<ReportOwner> ownerAll = new List<ReportOwner>();
                ownerAll.Add(modelUI.ReportSetConfigModel.DefOwner);
                ownerAll = ownerAll.Concat(modelUI.ReportSetConfigModel.ReportOwnerList).ToList();
                T_ReportSourceEntity source = new T_ReportSourceApp().FindEntity(o => o.F_Id == modelUI.DataSourceID);
                foreach (ReportOwner owner in ownerAll) //通用角色+所选角色
                {
                    owner.ReportHeadColList = new List<ReportHeadCol>();
                    IList<ReportSourceColumn> colList = source.ColumnJson.ToObject<IList<ReportSourceColumn>>();
                    foreach (ReportSourceColumn sourceCol in colList)
                    {
                        ReportHeadCol col = new ReportHeadCol();
                        col.FieldCode = sourceCol.FieldCode;
                        col.DataType = sourceCol.DataType;
                        owner.ReportHeadColList.Add(col);
                    }
                }
            }

            return Content(modelUI.ToJson());
        }
    }
}
