/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.IRepository.SystemManage;
using MST.Domain.ViewModel;
using MST.Repository.SystemManage;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace MST.Repository.SystemManage
{
    public class ItemsDetailRepository : RepositoryBase<ItemsDetailEntity>, IItemsDetailRepository
    {
        public List<ItemsDetailEntity> GetItemList(string enCode)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT  d.*
                            FROM    Sys_ItemsDetail d
                                    INNER  JOIN Sys_Items i ON i.F_Id = d.F_ItemId
                            WHERE   1 = 1
                                    AND i.F_EnCode = @enCode
                                    AND d.F_EnabledMark = 1
                                    AND d.F_DeleteMark = 0
                            ORDER BY d.F_SortCode ASC");
            DbParameter[] parameter = 
            {
                 new SqlParameter("@enCode",enCode)
            };
            return this.FindList(strSql.ToString(), parameter);

            
        }

        /// <summary>
        /// 获取配置节点中，指定表和字段所对应的枚举列表
        /// </summary>
        /// <param name="tableCode"></param>
        /// <param name="fieldCode"></param>
        /// <returns></returns>
        public List<ItemsDetailEntity> GetEnumList(string tableCode, string fieldCode)
        {
            StringBuilder strSql = new StringBuilder();

            strSql.Append(@"SELECT  d.*
                            FROM    Sys_ItemsDetail d
                                    INNER  JOIN Sys_Items i ON i.F_Id = d.F_ItemId
                                    INNER  JOIN Sys_Items c ON c.F_Id = i.F_ParentID
                                    INNER  JOIN Sys_Items r ON r.F_Id=c.F_ParentID
                            WHERE   r.F_EnCode = @RootEnCode
                                    AND c.F_EnCode = @tableCode
                                    AND i.F_EnCode = @fieldCode
                            ORDER BY d.F_SortCode ASC");
            DbParameter[] parameter =
            {
                 new SqlParameter("@RootEnCode","TypeConfig"),
                 new SqlParameter("@tableCode",tableCode),
                 new SqlParameter("@fieldCode",fieldCode)
            };

            return this.FindList(strSql.ToString(), parameter);
        }


        /// <summary>
        /// 获取配置节点中，指定表所有枚举字段与值
        /// </summary>
        /// <param name="tableCode"></param>
        /// <param name="fieldCode"></param>
        /// <returns></returns>
        public IList<SysItemsModel> GetEnumList(string tableCode)
        {
            IList<SysItemsModel> modelList = new List<SysItemsModel>();

            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT  i.*
                            FROM    Sys_Items i 
                                    INNER  JOIN Sys_Items c ON c.F_Id = i.F_ParentID
                                    INNER  JOIN Sys_Items r ON r.F_Id=c.F_ParentID
                            WHERE   r.F_EnCode = @RootEnCode
                                    AND c.F_EnCode = @tableCode");
            DbParameter[] parameter =
            {
                 new SqlParameter("@RootEnCode","TypeConfig"),
                 new SqlParameter("@tableCode",tableCode)
            };

            ItemsRepository itemRep = new ItemsRepository();
            List <ItemsEntity> itemList = itemRep.FindList(strSql.ToString(), parameter);
            foreach(ItemsEntity item in itemList)
            {
                SysItemsModel fieldModel = new SysItemsModel();
                fieldModel.F_Id = item.F_Id;
                fieldModel.F_ParentId = item.F_ParentId;
                fieldModel.F_EnCode = item.F_EnCode;
                fieldModel.F_FullName = item.F_FullName;
                fieldModel.F_IsTree = item.F_IsTree;
                fieldModel.F_Layers = item.F_Layers;
                fieldModel.F_SortCode = item.F_SortCode;
                fieldModel.F_DeleteMark = item.F_DeleteMark;
                fieldModel.F_EnabledMark = item.F_EnabledMark;
                fieldModel.F_Description = item.F_Description;
                fieldModel.DetailList = new List<ItemsDetailEntity>();
                List<ItemsDetailEntity> itemsDetailEntityList = GetEnumList(tableCode, item.F_EnCode);
                if(itemsDetailEntityList.Count>0)
                {
                    fieldModel.DetailList = itemsDetailEntityList;
                }
                modelList.Add(fieldModel);
            }
            return modelList;
        }
    }
}
