/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.SystemManage;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.SystemManage;
using MST.Domain.IRepository.WMSLogic;
using MST.Domain.ViewModel;
using MST.Repository.SystemManage;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_LocationApp
    {
        private IT_LocationRepository service = new T_LocationRepository();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private static object lockObj = new object();

        public IQueryable<T_LocationEntity> FindList(Expression<Func<T_LocationEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_LocationEntity FindEntity(Expression<Func<T_LocationEntity, bool>> predicate)
        {
            T_LocationEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public void Delete(Expression<Func<T_LocationEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }

        public void Insert(T_LocationEntity entity)
        {
            service.Insert(entity);
        }

        public List<T_LocationEntity> GetList(string keyword = "")
        {
            var expression = ExtLinq.True<T_LocationEntity>();
            if (!string.IsNullOrEmpty(keyword))
            {
                expression = expression.And(t => t.LocationCode.Contains(keyword));
            }
            return service.IQueryable(expression).OrderBy(t => t.F_CreatorTime).ToList();
        }

        public List<T_LocationEntity> GetListByAreaID(Pagination pagination, string areaID ,string keyword)
        {
            var expression = ExtLinq.True<T_LocationEntity>();
            expression = expression.And(t => t.State == "Stored" && t.AreaID == areaID);
            if (!string.IsNullOrEmpty(keyword)) expression = expression.And(t => t.LocationCode.Contains(keyword));
            return service.FindList(expression, pagination);
        }

        public List<T_LocationEntity> GetList(Pagination pagination, string AreaId = "", string queryJson = "")
        {
            var expression = ExtLinq.True<T_LocationEntity>();
            if (!string.IsNullOrEmpty(AreaId))
            {
                expression = expression.And(t => t.AreaID == AreaId);
            }
            var param = queryJson.ToJObject();
            if (!param["keyword"].IsEmpty())
            {
                string keyword = param["keyword"].ToString();
                expression = expression.And(t => t.LocationCode.Contains(keyword));
            }

            if (!param["locationType"].IsEmpty())
            {
                string locationType = param["locationType"].ToString();
                if (locationType == "1")    /// 立库
                {
                    expression = expression.And(t => t.LocationType == "Cube");
                }
                else if (locationType == "2")   /// 平库
                {
                    expression = expression.And(t => t.LocationType == "Flat");
                }
            }

            if (!param["state"].IsEmpty())
            {
                string state = param["state"].ToString();
                if (state == "0")    /// 全部
                {

                }
                else if (state == "1")    /// 已存储
                {
                    expression = expression.And(t => t.State == "Stored");
                }
                else if (state == "2")   /// 空
                {
                    expression = expression.And(t => t.State == "Empty");
                }
                else if (state == "3")    /// 待入库
                {
                    expression = expression.And(t => t.State == "In");
                }
                else if (state == "4")   /// 待出库
                {
                    expression = expression.And(t => t.State == "Out");
                }
            }

            if (!param["forb"].IsEmpty())
            {
                string forb = param["forb"].ToString();
                if (forb == "0")    /// 全部
                {

                }
                else if (forb == "1")   /// 禁用
                {
                    expression = expression.And(t => t.ForbiddenState == "Lock");
                }
                else if (forb == "2")   /// 正常
                {
                    expression = expression.And(t => t.ForbiddenState == "Normal");
                }
            }

            return service.FindList(expression, pagination).ToList();
        }

        public List<T_LocationEntity> GetCountLocList(Pagination pagination, string AreaCode, string keyword)
        {
            var expression = ExtLinq.True<T_LocationEntity>();
            if (!string.IsNullOrEmpty(AreaCode))
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    expression = expression.And(t => t.AreaCode == AreaCode && t.LocationCode.Contains(keyword) && t.F_DeleteMark == false);
                }
                else expression = expression.And(t => t.AreaCode == AreaCode && t.F_DeleteMark == false);
            }
            else
            {
                expression = expression.And(t => t.LocationCode.Contains(keyword) && t.F_DeleteMark == false);
            }
            return service.FindList(expression, pagination).ToList();
        }


        public List<TreeViewModel> GetTreeJson()
        {
            var data = areaApp.GetList();

            var treeList = new List<TreeViewModel>();
            foreach (T_AreaEntity item in data)
            {
                TreeViewModel tree = new TreeViewModel();
                bool hasChildren = data.Count(t => t.ParentID == item.F_Id) == 0 ? false : true;
                tree.id = item.F_Id;
                tree.text = item.AreaName;
                tree.value = item.AreaCode;
                tree.parentId = item.ParentID;
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = hasChildren;
                treeList.Add(tree);
            }
            return treeList;
        }
        public List<T_LocationEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public T_LocationEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_LocationEntity itemsEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                itemsEntity.Modify(keyValue);
                service.Update(itemsEntity);
            }
            else
            {
                itemsEntity.Create();
                service.Insert(itemsEntity);
            }
        }

        #region 生成新货位时，判断货位是否已经存在
        public List<T_LocationEntity> GetExistsLoc(IList<T_LocationEntity> newLocList)
        {
            List<T_LocationEntity> existsList = service.IQueryable().ToList().Join(newLocList, i => new { i.Line, i.ColNum, i.Layer, i.AreaID }, j => new { j.Line, j.ColNum, j.Layer, j.AreaID }, (j, k) => j).ToList();
            return existsList;
        }
        #endregion

        #region 获取库位所有的行
        public List<int> GetAllRow()
        {
            List<int> rowList = service.IQueryable(o => o.LocationType == "Cube").Select(o => (int)o.Line).Distinct().OrderBy(o => o).ToList();
            return rowList;
        }
        #endregion

        #region 检查平库货位编码是否已存在
        public List<T_LocationEntity> GetExistsPlatLoc(string LocationCode)
        {
            List<T_LocationEntity> existsList = service.IQueryable(o => o.LocationCode == LocationCode).ToList();
            return existsList;
        }
        #endregion

        #region  获取可选用的货位队列,必须满足的限制(在货位分配，或手动下拉分配时使用)
        /// <summary>
        /// 获取可选用的货位队列,必须满足的限制(在货位分配，或手动下拉分配时使用)
        /// </summary>
        /// <param name="errMsg"></param>
        /// <param name="pathMsg"></param>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="containerType"></param>
        /// <param name="isEmptyIn"></param>
        /// <param name="CheckState"></param>
        /// <param name="pointLocCode"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public IList<T_LocationEntity> EnableUseLocList(ref string errMsg, ref string pathMsg, IRepositoryBase db, T_AreaEntity area, T_ContainerTypeEntity containerType, bool isEmptyIn, string CheckState, T_ItemEntity item)
        {
            IList<T_LocationEntity> enableLocList = new List<T_LocationEntity>();

            int?[] allCanUserLine = db.IQueryable<T_RowLineEntity>(o => o.IsEnable == "true").Join(db.IQueryable<T_DevRowEntity>(o => o.IsEnable == "true"), m => m.DevRowID, n => n.F_Id, (m, n) => m.Line).ToArray();
            if (allCanUserLine.Length < 1)
            {
                errMsg = "没有启用的巷道或行";
                return enableLocList;
            }

            Expression<Func<T_LocationEntity, bool>> exp = EnableUseLocExpression(ref errMsg, ref pathMsg, area, allCanUserLine, containerType, isEmptyIn, CheckState, item);

            if (exp != null)
            {
                // 所有可用货位
                enableLocList = db.IQueryable<T_LocationEntity>(exp).ToList();
            }
            return enableLocList;

        }
        #endregion

        #region  获取可选用的货位队列,必须满足的限制(在货位分配，或手动下拉分配时使用),此处用于移库优化
        /// <summary>
        /// 获取可选用的货位队列,必须满足的限制(在货位分配，或手动下拉分配时使用),此处用于移库优化
        /// </summary>
        /// <param name="errMsg"></param>
        /// <param name="pathMsg"></param>
        /// <param name="db"></param>
        /// <param name="area"></param>
        /// <param name="containerType"></param>
        /// <param name="isEmptyIn"></param>
        /// <param name="CheckState"></param>
        /// <param name="pointLocCode"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public Expression<Func<T_LocationEntity, bool>> EnableUseLocExpression(ref string errMsg, ref string pathMsg, T_AreaEntity area, int?[] allCanUserLine, T_ContainerTypeEntity containerType, bool isEmptyIn, string CheckState, T_ItemEntity item)
        {

            if (area.IsEnable != "true")
            {
                errMsg = "区域未启用";
                return null;
            }

            var expression = ExtLinq.True<T_LocationEntity>();

            if (area.AreaType == "Tunnel") //巷道库
            {
                pathMsg = pathMsg + "[巷道库、区域货位、货位状态、非禁用、深高宽限制]";


                // 对应区域的货位
                expression = expression.And(o => o.AreaID == area.F_Id);

                /// 货位状态
                expression = expression.And(o => o.State == "Empty" && o.IsAreaVir == "false");
                /// 非禁用状态
                expression = expression.And(o => o.ForbiddenState == "Normal"
                                            || o.ForbiddenState == "OnlyIn");

                //深宽高限制，容器没有设置长宽高范围，分配的货位也必须没有设置长宽高，否则分配范围之内的货位
                if ((containerType.LocMinWidth ?? 0) == 0 && (containerType.LocMaxWidth ?? 0) == 0)
                {
                    expression = expression.And(o => o.Width == null || o.Width == 0);
                }
                else
                {
                    if ((containerType.LocMinWidth ?? 0) != 0) //最小宽
                    {
                        expression = expression.And(o => o.Width != null && o.Width >= containerType.LocMinWidth);
                    }

                    if ((containerType.LocMaxWidth ?? 0) != 0) //限大宽
                    {
                        expression = expression.And(o => o.Width != null && o.Width <= containerType.LocMaxWidth);
                    }
                }

                if ((containerType.LocMinHeight ?? 0) == 0 && (containerType.LocMaxHeight ?? 0) == 0)
                {
                    expression = expression.And(o => o.High == null || o.High == 0);
                }
                else
                {
                    if ((containerType.LocMinHeight ?? 0) != 0) //最小高
                    {
                        expression = expression.And(o => o.High != null && o.High >= containerType.LocMinHeight);
                    }

                    if ((containerType.LocMaxHeight ?? 0) != 0) //限大高
                    {
                        expression = expression.And(o => o.High != null && o.High <= containerType.LocMaxHeight);
                    }
                }

                if ((containerType.LocMinLong ?? 0) == 0 && (containerType.LocMaxLong ?? 0) == 0)
                {
                    expression = expression.And(o => o.Long == null || o.Long == 0);
                }
                else
                {
                    if ((containerType.LocMinLong ?? 0) != 0) //最小深
                    {
                        expression = expression.And(o => o.Long != null && o.Long >= containerType.LocMinLong);
                    }

                    if ((containerType.LocMaxLong ?? 0) != 0) //限大深
                    {
                        expression = expression.And(o => o.Long != null && o.Long <= containerType.LocMaxLong);
                    }
                }



                if (area.IsERPPy == "true") //ERP区域做物理区域
                {
                    if (!isEmptyIn) //非空容器回库限制ERP区域
                    {
                        pathMsg = pathMsg + "[ERP物理区域]";
                        if (string.IsNullOrEmpty(item.ERPWarehouseCode))
                        {
                            errMsg = "物料未设置ERP区域";
                            return null;
                        }
                        expression = expression.And(o => o.ERPHouseCode == item.ERPWarehouseCode);
                    }
                }
                else
                {
                    pathMsg = pathMsg + "[无ERP物理区域]";
                }

                if (area.IsCheckPy == "true") //质检区域做物理区域
                {
                    if (!isEmptyIn) //非空容器回库限制质检区域
                    {
                        pathMsg = pathMsg + "[质检物理区域]";
                        if (string.IsNullOrEmpty(CheckState))
                        {
                            errMsg = "容器物料没有质检状态";
                            return null;
                        }
                        string locCheckState = "";
                        try
                        {
                            locCheckState = CheckState;
                        }
                        catch (Exception exParse)
                        {
                            errMsg = "质检状态不存在";
                            return null;
                        }
                        expression = expression.And(o => o.CheckPyType == locCheckState || o.CheckPyType == "All");
                    }
                }
                else
                {
                    pathMsg = pathMsg + "[无质检物理区域]";
                }

                if (containerType.ContainerKind == "Box") //纸箱不能放顶层
                {
                    expression = expression.And(o => o.IsLocTop != "true");
                }

                if (allCanUserLine.Length < 1)
                {
                    errMsg = "没有启用的巷道或行";
                    return null;
                }
                expression = expression.And(o => allCanUserLine.Contains(o.Line));

                if (item.IsDampproof == "true")
                {
                    pathMsg = pathMsg + "[物料防潮]";
                    expression = expression.And(o => o.IsDampproof == "true");
                }
            }

            return expression;

        }
        #endregion



        #region 柔性过滤货位队列
        /// <summary>
        /// 根据可用货位队列，柔性选取最佳货位队列
        /// </summary>
        /// <param name="errMsg"></param>
        /// <param name="pathMsg"></param>
        /// <param name="locationList"></param>
        /// <param name="containerType"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public IList<T_LocationEntity> FlexLocList(ref string errMsg, ref string pathMsg, IList<T_LocationEntity> locationList, T_ContainerTypeEntity containerType, T_ItemEntity item)
        {
            //物料在区域内，需要遵循货位优先级，此处按靠窗优先
            if (item.IsPriority == "true")
            {
                pathMsg = pathMsg + "[靠窗优先]";
                bool isHavePri = locationList.Any(o => o.IsItemPriority == "true");
                if (isHavePri)
                {
                    locationList = locationList.Where(o => o.IsItemPriority == "true").ToList();
                }
            }
            else
            {
                pathMsg = pathMsg + "[靠窗不优先]";
                bool isHavePri = locationList.Any(o => o.IsItemPriority != "true");
                if (isHavePri)
                {
                    locationList = locationList.Where(o => o.IsItemPriority != "true").ToList();
                }
            }

            //防潮
            if (item.IsDampproof == "true")
            {
                //防潮是硬性条件，已在硬性条件处理，此处不再做处理，注释
                //pathMsg = pathMsg + "[防潮优先]";
                //bool isHaveDampp = locationList.Any(o => o.IsDampproof == "true");
                //if (isHaveDampp)
                //{
                //    locationList = locationList.Where(o => o.IsDampproof == "true").ToList();
                //}
            }
            else
            {
                pathMsg = pathMsg + "[不防潮优先]";
                bool isHaveDampp = locationList.Any(o => o.IsDampproof == "false");
                if (isHaveDampp)
                {
                    locationList = locationList.Where(o => o.IsDampproof == "false").ToList();
                }
            }


            //容器为纸箱的，优先选择小尺寸货位
            if (containerType.ContainerKind == "Box")
            {
                pathMsg = pathMsg + "[纸箱优先小货位]";
                decimal? minHeight = locationList.Min(o => o.High);
                locationList = locationList.Where(o => o.High == minHeight).ToList();
            }
            else if (containerType.ContainerKind == "Plastic") //料箱优先顶层货位
            {
                pathMsg = pathMsg + "[料箱优先顶部货位]";
                bool isHaveTop = locationList.Any(o => o.IsLocTop == "true");
                if (isHaveTop)
                {
                    locationList = locationList.Where(o => o.IsLocTop == "true").ToList();
                }
            }

            return locationList;
        }
        #endregion

        #region 入库货位分配
        /// <summary>
        /// 入库货位分配
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <param name="logObj">文本日志</param>
        /// <param name="db">数据库上下文</param>
        /// <param name="containerType">容器类型</param>
        /// <param name="areaID">分配区域</param>
        /// <param name="isEmptyIn">是否空料箱(是的话入库侧优先)</param>
        /// <param name="ERPCode">ERP物理分区，无的话填空字符串或null</param>
        /// <param name="CheckState">当前容器质检状态，（质检物理分区，空托盘不做质检限制，所以可传入空字符串或null）</param>
        /// <param name="isSetInState">分配的货位是否变更货位状态为待入库</param>
        /// <param name="pointLocCode">手动指定的货位，无的话填空字符串或null</param>
        /// <returns></returns>
        public T_LocationEntity GetLocIn(ref string errMsg, ref LogObj logObj, IRepositoryBase db, T_ContainerTypeEntity containerType, string areaID, bool isEmptyIn, string ERPCode, string CheckState, bool isSetInState, string pointLocCode, T_ItemEntity item)
        {
            lock (lockObj)
            {
                string pathMsg = "";
                isEmptyIn = false;
                T_LocationEntity loc = null;

                T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == areaID);

                if (area.AreaType == "Tunnel") //巷道库
                {
                    IList<T_LocationEntity> locationList = EnableUseLocList(ref errMsg, ref pathMsg, db, area, containerType, isEmptyIn, CheckState, item);

                    if (!string.IsNullOrEmpty(errMsg)) //分配时产生的错误
                    {
                        return loc;
                    }

                    if (locationList.Count < 1)
                    {
                        errMsg = "区域已无可用货位";
                        return loc;
                    }

                    if (!string.IsNullOrEmpty(pointLocCode)) //按手动指定的货位分配
                    {
                        loc = locationList.FirstOrDefault(o => o.LocationCode == pointLocCode);
                        if (loc == null)
                        {
                            errMsg = "手动指定的货位无效";
                            return loc;
                        }
                    }
                    else  //自动分配
                    {
                        locationList = FlexLocList(ref errMsg, ref pathMsg, locationList, containerType, item);

                        /// 可用货位的行
                        IList<int?> lineList = locationList.Select(o => o.Line).Distinct().ToArray();
                        pathMsg = pathMsg + "[可用行" + string.Join(",", lineList) + "]";

                        /// 可用行的巷道ID
                        IList<string> rowLineList = db.FindList<T_RowLineEntity>(o => lineList.Contains(o.Line) && o.IsEnable == "true").Select(o => o.DevRowID).Distinct().ToArray();
                        if (rowLineList.Count < 1)
                        {
                            errMsg = "巷道对应的行设置不正确";
                            return loc;
                        }

                        //所有巷道(包含不可分配巷道)
                        IList<T_DevRowEntity> allTunnelList = db.FindList<T_DevRowEntity>(o => o.AreaID == areaID).ToList();
                        // 可用巷道
                        IList<T_DevRowEntity> canUseTunnelList = allTunnelList.Where(o => rowLineList.Contains(o.F_Id) && o.IsEnable == "true").ToList();
                        if (canUseTunnelList.Count < 1)
                        {
                            errMsg = "存在可用货位，但巷道不可用";
                            return loc;
                        }
                        pathMsg = pathMsg + "[可用巷道" + string.Join(",", canUseTunnelList.OrderBy(o => o.Num).Select(o => o.Num).ToArray()) + "]";


                        //可用巷道第一巷道
                        T_DevRowEntity canUseTunnelFirst = null;
                        if (area.IsTunnelDesc == "true")
                        {
                            pathMsg = pathMsg + "[巷道倒序]";
                            canUseTunnelFirst = canUseTunnelList.OrderByDescending(o => o.Num).FirstOrDefault();
                        }
                        else
                        {
                            pathMsg = pathMsg + "[巷道顺序]";
                            canUseTunnelFirst = canUseTunnelList.OrderBy(o => o.Num).FirstOrDefault();
                        }

                        T_DevRowEntity curTunnel = allTunnelList.FirstOrDefault(o => o.IsCursor == "true");
                        T_DevRowEntity nextTunnel = null;
                        if (curTunnel == null) // 所有巷道没有游标巷道，取可用巷道的第一个巷道
                        {
                            pathMsg = pathMsg + "[巷道无游标]";
                            nextTunnel = canUseTunnelFirst;
                            nextTunnel.IsCursor = "true";
                            db.Update<T_DevRowEntity>(nextTunnel);
                        }
                        else // 有游标巷道
                        {
                            pathMsg = pathMsg + "[巷道有游标]";

                            IList<T_RowLineEntity> curTunnelLineList = db.FindListAsNoTracking<T_RowLineEntity>(o => lineList.Contains(o.Line) && o.DevRowID == curTunnel.F_Id && o.IsEnable == "true").ToList();
                            if (curTunnel.IsEnable == "true" && curTunnelLineList != null && curTunnelLineList.Where(o => o.IsCursor == "true" && o.ContinuityCount_In > 0 && (o.ContinuityCount_In ?? 0) > (o.CurOverCount_In ?? 0)).Count() > 0) //当前游标巷道需要连续分配
                            {
                                pathMsg = pathMsg + "[原游标巷道连续分配]";
                                nextTunnel = curTunnel;
                            }
                            else
                            {
                                if (area.IsEvenTunnel == "true") //巷道均分
                                {
                                    pathMsg = pathMsg + "[巷道均分]";
                                    if (area.IsTunnelDesc == "true") //巷道倒序
                                    {
                                        pathMsg = pathMsg + "[巷道倒序]";
                                        nextTunnel = canUseTunnelList.Where(o => o.Num < curTunnel.Num).OrderByDescending(o => o.Num).FirstOrDefault();
                                    }
                                    else
                                    {
                                        pathMsg = pathMsg + "[巷道顺序]";
                                        nextTunnel = canUseTunnelList.Where(o => o.Num > curTunnel.Num).OrderBy(o => o.Num).FirstOrDefault();
                                    }
                                }
                                else
                                {
                                    pathMsg = pathMsg + "[巷道不均分]";
                                    if (area.IsTunnelDesc == "true")//巷道倒序
                                    {
                                        pathMsg = pathMsg + "[巷道倒序]";
                                        nextTunnel = canUseTunnelList.Where(o => o.Num <= curTunnel.Num).OrderByDescending(o => o.Num).FirstOrDefault();
                                    }
                                    else
                                    {
                                        pathMsg = pathMsg + "[巷道顺序]";
                                        nextTunnel = canUseTunnelList.Where(o => o.Num >= curTunnel.Num).OrderBy(o => o.Num).FirstOrDefault();
                                    }
                                }

                                if (nextTunnel == null) // 可用巷道没有下一巷道，取可用巷道第一巷道
                                {
                                    pathMsg = pathMsg + "[可用巷道无下一个]";
                                    curTunnel.IsCursor = "false";
                                    db.Update<T_DevRowEntity>(curTunnel);

                                    nextTunnel = canUseTunnelFirst;
                                    nextTunnel.IsCursor = "true";
                                    db.Update<T_DevRowEntity>(nextTunnel);
                                }
                                else
                                {
                                    pathMsg = pathMsg + "[可用巷道有下一个]";
                                    curTunnel.IsCursor = "false";
                                    db.Update<T_DevRowEntity>(curTunnel);

                                    nextTunnel.IsCursor = "true";
                                    db.Update<T_DevRowEntity>(nextTunnel);
                                }
                            }

                        }

                        // 确定巷道后，进行行均分

                        //所有行(包含不可分配行)
                        IList<T_RowLineEntity> allLineList = db.FindList<T_RowLineEntity>(o => o.DevRowID == nextTunnel.F_Id).ToList();
                        IList<T_RowLineEntity> canUseLineList = allLineList.Where(o => lineList.Contains(o.Line) && o.IsEnable == "true").ToList();
                        if (canUseLineList.Count < 1)
                        {
                            errMsg = "存在可用货位，但行不可用";
                            return loc;
                        }

                        T_RowLineEntity canUseLineFirst = null;
                        if (nextTunnel.IsLineDesc == "true") //行倒序
                        {
                            pathMsg = pathMsg + "[行倒序]";
                            canUseLineFirst = canUseLineList.OrderByDescending(o => o.Line).FirstOrDefault();
                        }
                        else
                        {
                            pathMsg = pathMsg + "[行顺序]";
                            canUseLineFirst = canUseLineList.OrderBy(o => o.Line).FirstOrDefault();
                        }

                        T_RowLineEntity curRowLine = allLineList.FirstOrDefault(o => o.IsCursor == "true");
                        T_RowLineEntity nextRowLine = null;
                        if (curRowLine == null) /// 没有游标行，取可用行第一行
                        {
                            pathMsg = pathMsg + "[无游标行]";
                            nextRowLine = canUseLineFirst;
                            nextRowLine.IsCursor = "true";
                            db.Update<T_RowLineEntity>(nextRowLine);
                        }
                        else // 有游标行
                        {
                            pathMsg = pathMsg + "[有游标行]";
                            if (nextTunnel.IsEvenInnerTunnel == "true") //巷道内均分
                            {
                                pathMsg = pathMsg + "[巷道内均分]";
                                if ((curRowLine.ContinuityCount_In ?? 1) <= (curRowLine.CurOverCount_In ?? 1)) //连续分配已满足，达到均分条件
                                {
                                    pathMsg = pathMsg + "[连续分配已满足]";
                                    if (nextTunnel.IsLineDesc == "true") // 行倒序
                                    {
                                        pathMsg = pathMsg + "[行倒序]";
                                        nextRowLine = canUseLineList.Where(o => o.Line < curRowLine.Line).OrderByDescending(o => o.Line).FirstOrDefault();
                                    }
                                    else
                                    {
                                        pathMsg = pathMsg + "[行顺序]";
                                        nextRowLine = canUseLineList.Where(o => o.Line > curRowLine.Line).OrderBy(o => o.Line).FirstOrDefault();
                                    }

                                    if (nextRowLine == null) // 没有下一行，取第一行
                                    {
                                        pathMsg = pathMsg + "[没有下一行]";
                                        nextRowLine = canUseLineFirst;
                                    }

                                    curRowLine.IsCursor = "false";
                                    curRowLine.CurOverCount_In = 0;
                                    db.Update<T_RowLineEntity>(curRowLine);

                                    nextRowLine.IsCursor = "true";
                                    nextRowLine.CurOverCount_In = 1; //重新计数
                                    db.Update<T_RowLineEntity>(nextRowLine);



                                }
                                else  //否则暂不均分
                                {
                                    pathMsg = pathMsg + "[连续分配不满足]";
                                    if (nextTunnel.IsLineDesc == "true") // 行倒序
                                    {
                                        pathMsg = pathMsg + "[行倒序]";
                                        nextRowLine = canUseLineList.Where(o => o.Line <= curRowLine.Line).OrderByDescending(o => o.Line).FirstOrDefault();
                                    }
                                    else
                                    {
                                        pathMsg = pathMsg + "[行顺序]";
                                        nextRowLine = canUseLineList.Where(o => o.Line >= curRowLine.Line).OrderBy(o => o.Line).FirstOrDefault();
                                    }

                                    if (nextRowLine == null) // 没有下一行或当前行，取第一行
                                    {
                                        pathMsg = pathMsg + "[没有下一行或当前行]";
                                        nextRowLine = canUseLineFirst;
                                    }

                                    if (curRowLine.F_Id != nextRowLine.F_Id) //切换了行
                                    {
                                        pathMsg = pathMsg + "[切换了行]";
                                        curRowLine.IsCursor = "false";
                                        curRowLine.CurOverCount_In = 0;
                                        db.Update<T_RowLineEntity>(curRowLine);
                                    }

                                    nextRowLine.IsCursor = "true";
                                    nextRowLine.CurOverCount_In = (nextRowLine.CurOverCount_In ?? 0) + 1; //计数
                                    db.Update<T_RowLineEntity>(nextRowLine);
                                }
                            }
                            else //不均分
                            {
                                pathMsg = pathMsg + "[巷道内不均分]";
                                if (nextTunnel.IsLineDesc == "true") // 行倒序
                                {
                                    pathMsg = pathMsg + "[行倒序]";
                                    nextRowLine = canUseLineList.Where(o => o.Line <= curRowLine.Line).OrderByDescending(o => o.Line).FirstOrDefault();
                                }
                                else
                                {
                                    pathMsg = pathMsg + "[行顺序]";
                                    nextRowLine = canUseLineList.Where(o => o.Line >= curRowLine.Line).OrderBy(o => o.Line).FirstOrDefault();
                                }

                                if (nextRowLine == null) // 没有下一行或当前行，取第一行
                                {
                                    pathMsg = pathMsg + "[没有下一行或当前行]";
                                    nextRowLine = canUseLineFirst;
                                }

                                if (curRowLine.F_Id != nextRowLine.F_Id || (curRowLine.ContinuityCount_In ?? 1) <= (curRowLine.CurOverCount_In ?? 1))//切换了行,或连续分配已满足
                                {
                                    pathMsg = pathMsg + "[切换了行]";
                                    curRowLine.IsCursor = "false";
                                    curRowLine.CurOverCount_In = 0;
                                    db.Update<T_RowLineEntity>(curRowLine);
                                }

                                nextRowLine.IsCursor = "true";
                                nextRowLine.CurOverCount_In = (nextRowLine.CurOverCount_In ?? 0) + 1; //计数
                                db.Update<T_RowLineEntity>(nextRowLine);
                            }
                        }

                        locationList = locationList.Where(o => o.Line == nextRowLine.Line).ToList();

                        //高低分层，0为不分层，否则为低货位的最高层
                        int SplitLayer = nextRowLine.SplitLowLayer_In ?? 0;

                        if (SplitLayer > 0) //分层，先判断低层是否无可用货位
                        {
                            pathMsg = pathMsg + "[高低分层]";
                            if (nextRowLine.IsLowPriority_In == "true") //低层优先
                            {
                                pathMsg = pathMsg + "[低层优先]";
                                int lowLayerCount = locationList.Where(o => o.Layer <= SplitLayer).Count();
                                if (lowLayerCount > 0) //低层有可用货位,取低层
                                {
                                    pathMsg = pathMsg + "[低层有货位取低层]";
                                    locationList = locationList.Where(o => o.Layer <= SplitLayer).ToList();
                                }
                                else
                                {
                                    pathMsg = pathMsg + "[低层无货位取高层]";
                                    locationList = locationList.Where(o => o.Layer > SplitLayer).ToList();
                                }
                            }
                            else
                            {
                                pathMsg = pathMsg + "[非低层优先]";
                                int highLayerCount = locationList.Where(o => o.Layer > SplitLayer).Count();
                                if (highLayerCount > 0) //高层有可用货位,取高层
                                {
                                    pathMsg = pathMsg + "[高层有货位取高层]";
                                    locationList = locationList.Where(o => o.Layer > SplitLayer).ToList();
                                }
                                else
                                {
                                    pathMsg = pathMsg + "[高层无货位取低层]";
                                    locationList = locationList.Where(o => o.Layer <= SplitLayer).ToList();
                                }
                            }
                        }

                        if (nextRowLine.IsColumnDesc_In == "true") //列倒序
                        {
                            pathMsg = pathMsg + "[列倒序]";
                            if (nextRowLine.IsLayerDesc_In == "true") //层倒序
                            {
                                pathMsg = pathMsg + "[层倒序]";
                                locationList = locationList.OrderByDescending(o => o.ColNum).ThenByDescending(o => o.Layer).ToList();
                            }
                            else
                            {
                                pathMsg = pathMsg + "[层顺序]";
                                locationList = locationList.OrderByDescending(o => o.ColNum).ThenBy(o => o.Layer).ToList();
                            }
                        }
                        else
                        {
                            pathMsg = pathMsg + "[列顺序]";
                            if (nextRowLine.IsLayerDesc_In == "true")//层倒序
                            {
                                pathMsg = pathMsg + "[层倒序]";
                                locationList = locationList.OrderBy(o => o.ColNum).ThenByDescending(o => o.Layer).ToList();
                            }
                            else
                            {
                                pathMsg = pathMsg + "[层顺序]";
                                locationList = locationList.OrderBy(o => o.ColNum).ThenBy(o => o.Layer).ToList();
                            }
                        }

                        loc = locationList.FirstOrDefault();
                    }

                    if (isSetInState)
                    {
                        pathMsg = pathMsg + "[更新]";
                        loc.State = "In";
                        db.Update<T_LocationEntity>(loc);
                    }

                    pathMsg = "货位分配：[" + loc.LocationCode + "]" + pathMsg;

                    //日志
                    logObj = new LogObj() { Message = pathMsg, Parms = new { containerType = containerType, areaID = areaID, isEmptyIn = isEmptyIn, ERPCode = ERPCode, isSetInState = isSetInState, pointLocCode = pointLocCode }, Path = "MST.Application.WMSLogic.T_LocationApp.GetLocIn" };

                    LogFactory.GetLogger().Info(logObj);
                }
                else if (area.AreaType == "Concentrate") //密集库
                {

                }
                else if (area.AreaType == "AGV") //AGV库
                {

                }
                else if (area.AreaType == "Flat") //平库
                {

                }
                return loc;
            }
        }
        #endregion

        #region 检查指定入库货位是否可用
        public T_LocationEntity CheckLocIn(ref string errMsg, IRepositoryBase db, T_ContainerTypeEntity containerType, string areaID, bool isEmptyIn, string erpWarehouseCode, string itemID, string CheckState, string pointLocCode, bool isSetInState)
        {
            string pathMsg = "";
            T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == areaID);
            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == itemID);
            IList<T_LocationEntity> enableUseList = EnableUseLocList(ref errMsg, ref pathMsg, db, area, containerType, isEmptyIn, CheckState, item);
            if (!string.IsNullOrEmpty(errMsg)) //分配时产生的错误
            {
                return null;
            }

            if (enableUseList.Count < 1)
            {
                errMsg = "区域已无可用货位";
                return null;
            }

            T_LocationEntity loc = enableUseList.FirstOrDefault(o => o.LocationCode == pointLocCode);
            if (loc == null)
            {
                errMsg = "货位不可用";
                return null;
            }

            if (isSetInState)
            {
                loc.State = "In";
                db.Update<T_LocationEntity>(loc);
            }

            return loc;
        }
        #endregion
    }
}