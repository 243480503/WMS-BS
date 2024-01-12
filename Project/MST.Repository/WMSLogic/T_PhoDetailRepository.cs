/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Data;
using MST.Data.Extensions;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Repository.WMSLogic;

namespace MST.Repository.WMSLogic
{
    public class T_PhoDetailRepository : RepositoryBase<T_PhoDetailEntity>, IT_PhoDetailRepository
    {
        /// <summary>
        /// 库存快照
        /// </summary>
        /// <param name="phoEntity"></param>
        public void ExecutePhoDetailBackup(T_PhoEntity phoEntity)
        {
            DbHelper.ExecuteSqlCommand(string.Format(@"insert into T_PhoDetail(F_ID,PhoID,F_Id_Detail, [ItemID], 
                                                        [ContainerID], [ContainerType], [ContainerKind], [LocationID], [LocationNo], [AreaID], [AreaCode], [AreaName],
                                                        [KindCode], [KindName], [ItemName], [ItemCode], [ItemBarCode], 
                                                        [BarCode],[Factory], [Qty], [OutQty], [CheckQty], [ItemUnitText], [CheckState], 
                                                        [CheckDetailID], [CheckID], [State],[IsItemMark], [IsCheckFreeze], [IsCountFreeze], 
                                                        [Lot], [Spec], [ERPWarehouseCode],[ValidityDayNum], [ProductDate], [OverdueDate], [SupplierID], 
                                                        [SupplierCode], [SupplierName], [ReceiveRecordID], [IsSpecial], 
                                                        [IsVirItemBarCode], [InBoundID], [RefInBoundCode], [InBoundDetailID], [SEQ], [Remark], [F_DeleteMark], [F_CreatorTime], 
                                                        [F_CreatorUserId], [CreatorUserName], [F_DeleteUserId], 
                                                        [F_DeleteTime], [DeleteUserName], [F_LastModifyTime], 
                                                        [F_LastModifyUserId], [ModifyUserName]) 
                                                        select  NEWID() as F_ID,'{0}' as PhoID,[F_Id] as F_Id_Detail, [ItemID], 
                                                        [ContainerID], [ContainerType], [ContainerKind], [LocationID], [LocationNo], [AreaID], [AreaCode], [AreaName],
                                                        [KindCode], [KindName], [ItemName], [ItemCode], [ItemBarCode], 
                                                        [BarCode],[Factory], [Qty], [OutQty], [CheckQty], [ItemUnitText], [CheckState], 
                                                        [CheckDetailID], [CheckID], [State],[IsItemMark], [IsCheckFreeze], [IsCountFreeze], 
                                                        [Lot], [Spec], [ERPWarehouseCode],[ValidityDayNum], [ProductDate], [OverdueDate], [SupplierID], 
                                                        [SupplierCode], [SupplierName], [ReceiveRecordID], [IsSpecial], 
                                                        [IsVirItemBarCode], [InBoundID], [RefInBoundCode], [InBoundDetailID], [SEQ], [Remark], [F_DeleteMark], [F_CreatorTime], 
                                                        [F_CreatorUserId], [CreatorUserName], [F_DeleteUserId], 
                                                        [F_DeleteTime], [DeleteUserName], [F_LastModifyTime], 
                                                        [F_LastModifyUserId], [ModifyUserName] from T_ContainerDetail"
                                                        , phoEntity.F_Id));
        }
    }
}
