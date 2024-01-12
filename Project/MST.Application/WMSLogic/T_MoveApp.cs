/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)_WMS
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Code.Extend;
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.IRepository.WMSLogic;
using MST.Domain.ViewModel;
using MST.Repository.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MST.Application.WMSLogic
{
    public class T_MoveApp
    {
        private IT_MoveRepository service = new T_MoveRepository();
        private T_ItemApp itemApp = new T_ItemApp();

        public IQueryable<T_MoveEntity> FindList(Expression<Func<T_MoveEntity, bool>> predicate)
        {
            return service.IQueryable(predicate);
        }

        public T_MoveEntity FindEntity(Expression<Func<T_MoveEntity, bool>> predicate)
        {
            T_MoveEntity entity = service.FindEntity(predicate);
            return entity;
        }

        public List<T_MoveEntity> GetList()
        {
            return service.IQueryable().ToList();
        }
        public List<T_MoveEntity> GetList(Pagination pagination, string queryJson)
        {
            var expression = ExtLinq.True<T_MoveEntity>();
            var queryParam = queryJson.ToJObject();
            if (!queryParam["keyword"].IsEmpty())
            {
                string keyword = queryParam["keyword"].ToString();
                expression = expression.And(t => t.MoveCode.Contains(keyword));
            }
            if (!queryParam["resultType"].IsEmpty())
            {
                string resultType = queryParam["resultType"].ToString();
                switch (resultType)
                {
                    case "1":
                        break;
                    case "2":
                        expression = expression.And(t => t.State != "Over");
                        break;
                    case "3":
                        expression = expression.And(t => t.State == "Over");
                        break;
                    default:
                        break;
                }
            }
            return service.FindList(expression, pagination).ToList();
        }
        public List<T_MoveEntity> GetList(string keyValue)
        {
            IQueryable<T_MoveEntity> query = service.IQueryable();
            if (!string.IsNullOrEmpty(keyValue))
            {
                query = query.Where(o => o.MoveCode.Contains(keyValue));
            }
            return query.ToList();
        }

        public T_MoveEntity GetForm(string keyValue)
        {
            return service.FindEntity(keyValue);
        }

        public void Delete(Expression<Func<T_MoveEntity, bool>> predicate)
        {
            service.Delete(predicate);
        }
        public void DeleteForm(string keyValue)
        {
            service.Delete(t => t.F_Id == keyValue);
        }
        public void SubmitForm(T_MoveEntity moveEntity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                moveEntity.Modify(keyValue);
                service.Update(moveEntity);
            }
            else
            {
                moveEntity.Create();
                service.Insert(moveEntity);
            }
        }

        public void Insert(T_MoveEntity entity)
        {
            service.Insert(entity);
        }

        #region 产生移库任务并返回
        /// <summary>
        /// 产生移库任务并返回
        /// </summary>
        /// <param name="db"></param>
        /// <param name="moveID"></param>
        /// <param name="genNum">要产生的移库数量</param>
        /// <param name="taskNoList"></param>
        /// <param name="isRandom"></param>
        /// <returns></returns>
        public AjaxResult GenMoveRecord(IRepositoryBase db, string moveID, int genNum, ref IList<string> taskNoList)
        {
            T_MoveEntity moveEntity = db.FindEntity<T_MoveEntity>(o => o.F_Id == moveID);

            if (moveEntity.IsAuto == "true") /// 自动：整理货架货位
            {
                bool isSameRowLine = RuleConfig.MoveRule.RowLineRule.IsSameRowLine;
                bool isSameLine = RuleConfig.MoveRule.RowLineRule.IsSameLine;
                AjaxResult moveLocRst = GetMoveFromLoc(db, moveEntity.IsReverse, genNum, moveEntity.AreaCode, isSameRowLine, isSameLine, null);

                Dictionary<string, string> moveLocList = new Dictionary<string, string>();
                if ((ResultType)moveLocRst.state == ResultType.success)
                {
                    moveLocList = (Dictionary<string, string>)moveLocRst.data;
                }
                else
                {
                    return moveLocRst;
                }

                if (moveLocList.Count > 0)
                {
                    IList<T_MoveRecordEntity> moveRecordList = new List<T_MoveRecordEntity>();
                    IList<T_TaskEntity> taskList = new List<T_TaskEntity>();
                    IList<T_LocationEntity> srcLocList = new List<T_LocationEntity>();
                    IList<T_LocationEntity> tagLocList = new List<T_LocationEntity>();

                    IList<T_TaskEntity> taskInDBList = db.FindListAsNoTracking<T_TaskEntity>(o => true);
                    IList<T_ContainerEntity> conList = db.FindListAsNoTracking<T_ContainerEntity>(o => true);
                    IList<T_ContainerDetailEntity> conDetailList = db.FindListAsNoTracking<T_ContainerDetailEntity>(o => true);
                    IList<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => o.AreaCode == moveEntity.AreaCode);
                    foreach (KeyValuePair<string, string> loc in moveLocList)
                    {
                        T_ContainerEntity container = conList.FirstOrDefault(o => o.LocationID == loc.Key && o.F_DeleteMark == false);
                        IList<T_ContainerDetailEntity> containerDetailList = conDetailList.Where(o => o.BarCode == container.BarCode).ToList();
                        T_ContainerDetailEntity firstcontainerDetail = containerDetailList.FirstOrDefault();
                        if (firstcontainerDetail == null)
                        {
                            return new AjaxResult() { state = ResultType.error, message = "容器" + container.BarCode + "没有库存" };
                        }
                        T_MoveRecordEntity moveRecord = new T_MoveRecordEntity();
                        moveRecord.MoveID = moveID;
                        T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == firstcontainerDetail.ItemID);
                        moveRecord.ItemCode = item.ItemCode;
                        moveRecord.ItemName = item.ItemName;
                        moveRecord.Factory = item.Factory;
                        moveRecord.ExpireDate = null;
                        moveRecord.ItemID = item.F_Id;
                        moveRecord.Qty = containerDetailList.Sum(o => o.Qty);
                        moveRecord.Lot = firstcontainerDetail.Lot;
                        moveRecord.Spec = firstcontainerDetail.Spec;
                        moveRecord.ItemUnitText = firstcontainerDetail.ItemUnitText;
                        moveRecord.OverdueDate = firstcontainerDetail.OverdueDate;
                        moveRecord.ContainerID = container.F_Id;
                        moveRecord.BarCode = container.BarCode;

                        moveRecord.ContainerKind = container.ContainerKind;

                        moveRecord.F_Id = Guid.NewGuid().ToString();
                        moveRecord.State = "New";
                        moveRecord.GenType = "Auto";



                        T_LocationEntity srcLocEntity = locList.FirstOrDefault(o => o.F_Id == loc.Key);
                        moveRecord.SrcLocationID = srcLocEntity.F_Id;
                        moveRecord.SrcLocationCode = srcLocEntity.LocationCode;
                        //分配货位

                        T_LocationEntity tagLocEntity = locList.FirstOrDefault(o => o.F_Id == loc.Value);
                        moveRecord.TagLocationID = loc.Value;
                        moveRecord.TagLocationCode = tagLocEntity.LocationCode;
                        moveRecord.State = "Moving";




                        //产生任务
                        T_TaskEntity taskInDB = taskInDBList.FirstOrDefault(o => o.BarCode == moveRecord.BarCode);
                        if (taskInDB != null) //容器任务存在
                        {
                            continue;
                        }

                        moveRecordList.Add(moveRecord);

                        //变更货位状态
                        srcLocEntity.State = "Out";
                        srcLocList.Add(srcLocEntity);

                        tagLocEntity.State = "In";
                        tagLocList.Add(tagLocEntity);

                        T_TaskEntity task = new T_TaskEntity();
                        task.F_Id = Guid.NewGuid().ToString();
                        task.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                        task.TaskInOutType = "MoveType";
                        task.TaskType = "TaskType_Move";
                        task.OrderType = "MoveType";
                        task.ContainerID = moveRecord.ContainerID;
                        task.BarCode = moveRecord.BarCode;
                        task.ContainerType = container.ContainerType;
                        task.SrcLocationID = moveRecord.SrcLocationID;
                        task.SrcLocationCode = moveRecord.SrcLocationCode;
                        task.TagAreaID = "";
                        task.TagLocationID = moveRecord.TagLocationID;
                        task.TagLocationCode = moveRecord.TagLocationCode;
                        task.ApplyStationID = null;
                        task.Level = 30;
                        task.State = "New";
                        task.IsWcsTask = "true";
                        task.ExecEquID = null;
                        task.IsCanExec = "true";
                        task.SendWCSTime = null;
                        task.WaveCode = null;
                        task.WaveID = null;
                        task.SEQ = null;
                        task.WaveDetailID = null;
                        task.OrderID = moveRecord.MoveID;
                        task.OrderDetailID = moveRecord.F_Id;
                        task.OrderCode = moveEntity.MoveCode;
                        task.OverTime = null;
                        taskList.Add(task);
                        taskNoList.Add(task.TaskNo);
                    }
                    db.BulkInsert<T_MoveRecordEntity>(moveRecordList);
                    db.BulkInsert<T_TaskEntity>(taskList);
                    db.BulkUpdate<T_LocationEntity>(srcLocList);
                    db.BulkUpdate<T_LocationEntity>(tagLocList);
                    db.BulkSaveChanges();
                }
            }
            else
            {
                IList<T_MoveRecordEntity> MoveDetailList = db.FindList<T_MoveRecordEntity>(o => o.MoveID == moveID);
                if (MoveDetailList.Count < 1) //没有数据
                {
                    return new AjaxResult() { state = ResultType.error, message = "请先添加移库数据" };
                }

                IList<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => true);
                IList<T_ContainerEntity> conList = db.FindList<T_ContainerEntity>(o => true);
                IList<T_MoveRecordEntity> updMoveRecordList = new List<T_MoveRecordEntity>();
                IList<T_MoveRecordEntity> existsTaskMoveRecordList = new List<T_MoveRecordEntity>();
                IList<T_LocationEntity> updLocList = new List<T_LocationEntity>();
                IList<T_TaskEntity> taskDBList = db.FindList<T_TaskEntity>(o => true);
                IList<T_TaskEntity> insTaskList = new List<T_TaskEntity>();
                foreach (T_MoveRecordEntity cell in MoveDetailList)
                {
                    if (cell.GenType == "PointTag") //指定源货位与目标货位
                    {
                        cell.State = "Moving";
                        updMoveRecordList.Add(cell);
                    }
                    else //仅指定源货位
                    {
                        bool isSameRowLine = RuleConfig.MoveRule.RowLineRule.IsSameRowLine;
                        bool isSameLine = RuleConfig.MoveRule.RowLineRule.IsSameLine;
                        AjaxResult ajaxRst = GetMoveFromLoc(db, moveEntity.IsReverse, 1, moveEntity.AreaCode, isSameRowLine, isSameLine, cell.SrcLocationID);
                        if ((ResultType)ajaxRst.state == ResultType.success)
                        {
                            Dictionary<string, string> tagLocDic = (Dictionary<string, string>)ajaxRst.data;
                            string tagLocID = tagLocDic[cell.SrcLocationID];
                            T_LocationEntity tagLoc = locList.FirstOrDefault(o => o.F_Id == tagLocID);// db.FindEntity<T_LocationEntity>(o => o.F_Id == tagLocID);
                            cell.TagLocationID = tagLoc.F_Id;
                            cell.TagLocationCode = tagLoc.LocationCode;
                            cell.State = "Moving";
                            updMoveRecordList.Add(cell);
                        }
                        else // false仅表示没有可用货位，不代表错误
                        {
                            cell.Remark = ajaxRst.message;
                            cell.State = "Over";
                            updMoveRecordList.Add(cell);
                            continue;
                        }
                    }


                    T_ContainerEntity container = conList.FirstOrDefault(o => o.LocationID == cell.SrcLocationID && o.F_DeleteMark == false);// db.FindEntity<T_ContainerEntity>(o => o.LocationID == cell.SrcLocationID && o.F_DeleteMark == false);

                    //产生任务
                    T_TaskEntity taskInDB = taskDBList.FirstOrDefault(o => o.BarCode == cell.BarCode);// db.FindEntity<T_TaskEntity>(o => o.BarCode == cell.BarCode);
                    if (taskInDB != null) //容器任务存在
                    {
                        cell.Remark = "容器任务已存在:" + cell.BarCode;
                        cell.State = "Over";
                        existsTaskMoveRecordList.Add(cell);
                        continue;
                    }

                    //变更货位状态
                    T_LocationEntity locSrc = locList.FirstOrDefault(o => o.F_Id == cell.SrcLocationID);// db.FindEntity<T_LocationEntity>(o => o.F_Id == cell.SrcLocationID);
                    locSrc.State = "Out";
                    updLocList.Add(locSrc);

                    T_LocationEntity tagSrc = locList.FirstOrDefault(o => o.F_Id == cell.TagLocationID);// db.FindEntity<T_LocationEntity>(o => o.F_Id == cell.TagLocationID);
                    tagSrc.State = "In";
                    updLocList.Add(tagSrc);

                    T_TaskEntity task = new T_TaskEntity();
                    task.F_Id = Guid.NewGuid().ToString();
                    task.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                    task.TaskInOutType = "MoveType";
                    task.TaskType = "TaskType_Move";
                    task.OrderType = "MoveType";
                    task.ContainerID = cell.ContainerID;
                    task.BarCode = cell.BarCode;
                    task.ContainerType = container.ContainerType;
                    task.SrcLocationID = cell.SrcLocationID;
                    task.SrcLocationCode = cell.SrcLocationCode;
                    task.TagAreaID = "";
                    task.TagLocationID = cell.TagLocationID;
                    task.TagLocationCode = cell.TagLocationCode;
                    task.ApplyStationID = null;
                    task.Level = 30;
                    task.State = "New";
                    task.IsWcsTask = "true";
                    task.ExecEquID = null;
                    task.IsCanExec = "true";
                    task.SendWCSTime = null;
                    task.WaveCode = null;
                    task.WaveID = null;
                    task.SEQ = null;
                    task.WaveDetailID = null;
                    task.OrderID = cell.MoveID;
                    task.OrderDetailID = cell.F_Id;
                    task.OrderCode = moveEntity.MoveCode;
                    task.OverTime = null;
                    insTaskList.Add(task);
                    taskNoList.Add(task.TaskNo);
                }

                db.BulkUpdate<T_MoveRecordEntity>(updMoveRecordList);
                db.BulkUpdate<T_LocationEntity>(updLocList);
                db.BulkUpdate<T_MoveRecordEntity>(existsTaskMoveRecordList);
                db.BulkInsert<T_TaskEntity>(insTaskList);
                db.BulkSaveChanges();

            }

            db.SaveChanges();

            return new AjaxResult() { state = ResultType.success };
        }
        #endregion


        #region 获取要移库的源货位与目标货位，按巷道已存储位越靠后越优先（指定源货位，返回1个，不指定源货位，返回多个）
        /// <summary>
        /// 获取要移库的源货位与目标货位，按巷道已存储位越靠后越优先（指定源货位，返回1个，不指定源货位，返回多个）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="isReverse">是否逆向整理</param>
        /// <param name="genNum">总共需要产生的任务数量</param>
        /// <param name="areaCode"></param>
        /// <param name="isSameRowLine"></param>
        /// <param name="isSameLine"></param>
        /// <param name="srcLocID">手动指定的需要移库的存储货位(为空则视为不指定)</param>
        /// <param name="isRandom">是否新任务产生时使用随机行或随机巷道(任务完成时产生下一组任务需要随机，新建时可不随机)</param>
        /// <returns></returns>
        private AjaxResult GetMoveFromLoc(IRepositoryBase db, string isReverse, int genNum, string areaCode, bool isSameRowLine, bool isSameLine, string srcLocID)
        {
            try
            {
                AjaxResult rst = new AjaxResult();
                Dictionary<string, string> dic = new Dictionary<string, string>();//<源货位ID,目标货位ID>,从一个已存储货位，移动到另一个目标空货位


                T_AreaEntity area = db.FindEntityAsNoTracking<T_AreaEntity>(o => o.AreaCode == areaCode);
                string errMsg = "";
                string pathMsg = "";

                IList<T_LocationEntity> areaLocList = db.FindListAsNoTracking<T_LocationEntity>(o => o.AreaCode == areaCode);
                string[] devRowList = db.FindListAsNoTracking<T_DevRowEntity>(o => o.AreaID == area.F_Id).Select(o => o.F_Id).ToArray();
                IList<T_RowLineEntity> lineList = db.FindListAsNoTracking<T_RowLineEntity>(o => devRowList.Contains(o.DevRowID)).ToList();

                //该区域所有货位，包含空货位、已存储货位
                IList<LocTempModel> locTempModelList = areaLocList.Join(lineList, m => m.Line, n => n.Line, (m, n) => new LocTempModel
                {
                    DevRowID = n.DevRowID,
                    Line = m.Line,
                    Layer = m.Layer,
                    State = m.State,
                    ColNum = m.ColNum,
                    ForbiddenState = m.ForbiddenState,
                    LocationID = m.F_Id
                }).ToList();

                IList<ColNumTempModel> colNumTempList = locTempModelList.GroupBy(o => new { ColNum = o.ColNum }).Select(o => new ColNumTempModel
                {
                    ColNum = o.Key.ColNum,
                    DevRowList = o.GroupBy(k => new { DevRowID = k.DevRowID }).Select(j => new DevRowTempModel
                    {
                        DevRowID = j.Key.DevRowID,
                        LineList = j.GroupBy(u => new { Line = u.Line }).Select(b => new LineTempModel
                        {
                            Line = b.Key.Line,
                            LocList = b.ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();



                IList<T_ItemEntity> itemList = db.FindListAsNoTracking<T_ItemEntity>(o => true).ToList();
                IList<T_ContainerDetailEntity> containerDetalList = db.FindListAsNoTracking<T_ContainerDetailEntity>(o => true).ToList();
                IList<T_ContainerTypeEntity> containerTypeList = db.FindListAsNoTracking<T_ContainerTypeEntity>(o => true).ToList();

                int?[] allCanUserLine = db.IQueryable<T_RowLineEntity>(o => o.IsEnable == "true").Join(db.IQueryable<T_DevRowEntity>(o => o.IsEnable == "true"), m => m.DevRowID, n => n.F_Id, (m, n) => m.Line).ToArray();
                if (!string.IsNullOrEmpty(srcLocID)) //不为空，视为指定源货位(只处理一个)
                {
                    T_LocationEntity locEntity = areaLocList.FirstOrDefault(o => o.F_Id == srcLocID);
                    T_RowLineEntity rowLineEntity = lineList.FirstOrDefault(o => o.Line == locEntity.Line);
                    string tagLocID = FindTagLoc(ref errMsg, ref pathMsg, areaLocList, allCanUserLine, itemList, containerTypeList, containerDetalList, lineList, dic, isReverse, area, isSameRowLine, isSameLine, locEntity.F_Id, rowLineEntity.DevRowID, locEntity.ColNum, locEntity.Line);
                    if (!string.IsNullOrEmpty(tagLocID))
                    {
                        dic.Add(srcLocID, tagLocID);
                    }
                }
                else //为空，视为自动选取源货位(需处理多个)
                {
                    IList<string> stroedList = new List<string>();

                    //排序，从那边开始循环已存储货位
                    IList<ColNumTempModel> loopColNumList = new List<ColNumTempModel>();
                    if (isReverse == "false") //从后往前靠拢
                    {
                        loopColNumList = colNumTempList.OrderByDescending(o => o.ColNum).ToList();
                    }
                    else
                    {
                        loopColNumList = colNumTempList.OrderBy(o => o.ColNum).ToList();
                    }

                    foreach (var ColNum in loopColNumList) //循环列
                    {
                        foreach (var DevRow in ColNum.DevRowList) //循环巷道
                        {
                            foreach (var Line in DevRow.LineList) //循环行
                            {
                                foreach (var loc in Line.LocList) //循环该列的层
                                {
                                    if (loc.State != "Stored") //储位不为已存储，不需要移动
                                    {
                                        continue;
                                    }

                                    if (loc.ForbiddenState == "Lock" || loc.ForbiddenState == "OnlyIn") //储位已锁定或不可出
                                    {
                                        continue;
                                    }

                                    //为加快效率，没有空位的，直接跳过
                                    string[] hasUsedLocArray = dic.Values.ToArray();
                                    if (isSameRowLine) //同巷道
                                    {
                                        if (isSameLine) //同行
                                        {
                                            if (isReverse == "false") //从后往前靠拢
                                            {
                                                int emptyCount = locTempModelList.Count(o => o.DevRowID == loc.DevRowID && o.Line == loc.Line && o.ColNum < loc.ColNum && (!hasUsedLocArray.Contains(o.LocationID)) && o.State == "Empty" && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyIn"));
                                                if (emptyCount == 0)
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                int emptyCount = locTempModelList.Count(o => o.DevRowID == loc.DevRowID && o.Line == loc.Line && o.ColNum > loc.ColNum && (!hasUsedLocArray.Contains(o.LocationID)) && o.State == "Empty" && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyIn"));
                                                if (emptyCount == 0)
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (isReverse == "false") //从后往前靠拢
                                            {
                                                int emptyCount = locTempModelList.Count(o => o.DevRowID == loc.DevRowID && o.ColNum < loc.ColNum && (!hasUsedLocArray.Contains(o.LocationID)) && o.State == "Empty" && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyIn"));
                                                if (emptyCount == 0)
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                int emptyCount = locTempModelList.Count(o => o.DevRowID == loc.DevRowID && o.ColNum > loc.ColNum && (!hasUsedLocArray.Contains(o.LocationID)) && o.State == "Empty" && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyIn"));
                                                if (emptyCount == 0)
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (isReverse == "false") //从后往前靠拢
                                        {
                                            int emptyCount = locTempModelList.Count(o => o.ColNum < loc.ColNum && (!hasUsedLocArray.Contains(o.LocationID)) && o.State == "Empty" && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyIn"));
                                            if (emptyCount == 0)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            int emptyCount = locTempModelList.Count(o => o.ColNum > loc.ColNum && (!hasUsedLocArray.Contains(o.LocationID)) && o.State == "Empty" && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyIn"));
                                            if (emptyCount == 0)
                                            {
                                                continue;
                                            }
                                        }
                                    }


                                    string tagLocID = FindTagLoc(ref errMsg, ref pathMsg, areaLocList, allCanUserLine, itemList, containerTypeList, containerDetalList, lineList, dic, isReverse, area, isSameRowLine, isSameLine, loc.LocationID, loc.DevRowID, loc.ColNum, loc.Line);

                                    if (!string.IsNullOrEmpty(tagLocID))
                                    {
                                        dic.Add(loc.LocationID, tagLocID);//从一个已存储货位，移动到另一个目标空货位

                                        if (dic.Count == genNum) //数量已满
                                        {
                                            rst.state = ResultType.success;
                                            rst.data = dic;
                                            return rst;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (dic.Count == 0)
                {
                    rst.state = ResultType.warning;
                    rst.message = "没有需要整理的货位";
                    return rst;
                }
                else
                {
                    rst.state = ResultType.success;
                    rst.data = dic;
                    return rst;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private string FindTagLoc(ref string errMsg, ref string pathMsg, IList<T_LocationEntity> areaLocList, int?[] allCanUserLine, IList<T_ItemEntity> itemList, IList<T_ContainerTypeEntity> conTypeList, IList<T_ContainerDetailEntity> conDetailList, IList<T_RowLineEntity> lineList, Dictionary<string, string> dic, string isReverse, T_AreaEntity area, bool isSameRowLine, bool isSameLine, string srcLocID, string srcDevID, int? srcColNum, int? srcLine)
        {
            try
            {
                T_LocationApp locApp = new T_LocationApp();
                T_ContainerTypeEntity containerType;
                bool isEmptyIn = false;

                //查找该货位可移动到的所有可用空货位
                T_ContainerDetailEntity oneContainerDetail = conDetailList.FirstOrDefault(o => o.LocationID == srcLocID);
                T_ItemEntity item = itemList.FirstOrDefault(o => o.F_Id == oneContainerDetail.ItemID);
                containerType = conTypeList.FirstOrDefault(o => o.ContainerTypeCode == oneContainerDetail.ContainerType);
                if (item.ItemCode == FixType.Item.EmptyRack.ToString() || item.ItemCode == FixType.Item.EmptyPlastic.ToString())
                {
                    isEmptyIn = true;
                }
                Expression<Func<T_LocationEntity, bool>> exp = locApp.EnableUseLocExpression(ref errMsg, ref pathMsg, area, allCanUserLine, containerType, isEmptyIn, oneContainerDetail.CheckState, item);
                var expLamda = exp.Compile();
                IList<T_LocationEntity> enableUseList = areaLocList.Where(expLamda).ToList();
                if (!string.IsNullOrEmpty(errMsg)) //分配时产生的错误
                {
                    return null;
                }
                if (enableUseList.Count < 1) //没有合适货位
                {
                    errMsg = "没有合适货位";
                    return null;
                }



                string[] alreadyUseArray = dic.Select(o => o.Value).ToArray(); //已被占用的空货位
                enableUseList = enableUseList.Where(o => !alreadyUseArray.Contains(o.F_Id)).ToList();
                if (enableUseList.Count < 1) //合适货位已被其它占用
                {
                    errMsg = "合适货位已被其它占用";
                    return null;
                }



                //包含巷道信息的可以货位列表
                var enableUseDevList = enableUseList.Join(lineList, m => m.Line, n => n.Line, (m, n) => new
                {
                    DevRowID = n.DevRowID,
                    Line = m.Line,
                    Layer = m.Layer,
                    State = m.State,
                    ColNum = m.ColNum,
                    ForbiddenState = m.ForbiddenState,
                    LocationID = m.F_Id
                }).ToList();


                string tagLocID = null;

                if (isSameRowLine) //同巷道
                {
                    if (isSameLine) //同行
                    {
                        if (isReverse == "false") //从后往前靠拢
                        {
                            var locList = enableUseDevList.Where(o => o.DevRowID == srcDevID && o.Line == srcLine && o.ColNum < srcColNum);
                            var tagLoc = locList.FirstOrDefault(o => o.ColNum == locList.Min(k => k.ColNum));

                            if (tagLoc != null)
                            {
                                tagLocID = tagLoc.LocationID;
                            }
                        }
                        else
                        {
                            var locList = enableUseDevList.Where(o => o.DevRowID == srcDevID && o.Line == srcLine && o.ColNum > srcColNum);
                            var tagLoc = locList.FirstOrDefault(o => o.ColNum == locList.Max(k => k.ColNum));
                            if (tagLoc != null)
                            {
                                tagLocID = tagLoc.LocationID;
                            }
                        }
                    }
                    else //同巷道,跨行
                    {
                        if (isReverse == "false") //从后往前靠拢
                        {

                            var locList = enableUseDevList.Where(o => o.DevRowID == srcDevID && o.ColNum < srcColNum);
                            var tagLoc = locList.FirstOrDefault(o => o.ColNum == locList.Min(k => k.ColNum));
                            if (tagLoc != null)
                            {
                                tagLocID = tagLoc.LocationID;
                            }
                        }
                        else
                        {
                            var locList = enableUseDevList.Where(o => o.DevRowID == srcDevID && o.ColNum > srcColNum);
                            var tagLoc = locList.FirstOrDefault(o => o.ColNum == locList.Max(k => k.ColNum));
                            if (tagLoc != null)
                            {
                                tagLocID = tagLoc.LocationID;
                            }
                        }
                    }
                }
                else //可跨巷道（同时代表可跨行）
                {
                    if (isReverse == "false") //从后往前靠拢
                    {

                        var locList = enableUseDevList.Where(o => o.ColNum < srcColNum);
                        var tagLoc = locList.FirstOrDefault(o => o.ColNum == locList.Min(k => k.ColNum));

                        if (tagLoc != null)
                        {
                            tagLocID = tagLoc.LocationID;
                        }
                    }
                    else
                    {
                        var locList = enableUseDevList.Where(o => o.ColNum > srcColNum);
                        var tagLoc = locList.FirstOrDefault(o => o.ColNum == locList.Max(k => k.ColNum));

                        if (tagLoc != null)
                        {
                            tagLocID = tagLoc.LocationID;
                        }
                    }
                }

                return tagLocID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private class ColNumTempModel
        {
            public int? ColNum { get; set; }
            public IList<DevRowTempModel> DevRowList { get; set; }
        }

        private class DevRowTempModel
        {
            public string DevRowID { get; set; }
            public IList<LineTempModel> LineList { get; set; }
        }

        private class LineTempModel
        {
            public int? Line { get; set; }
            public IList<LocTempModel> LocList { get; set; }
        }

        private class LocTempModel
        {
            public string DevRowID { get; set; }
            public int? Line { get; set; }
            public int? Layer { get; set; }
            public string State { get; set; }
            public int? ColNum { get; set; }
            public string ForbiddenState { get; set; }
            public string LocationID { get; set; }
        }
        #endregion

        #region 产生移库单(用于内部自定义调整)
        /// <summary>
        /// 产生移库单(用于内部自定义调整)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dicLoc">移库的源货位与目标货位</param>
        /// <param name="remark">移库备注原因</param>
        /// <param name="moveID">产生的移库ID</param>
        /// <returns></returns>
        public AjaxResult GenMoveOrder(IRepositoryBase db, string areaID, Dictionary<string, string> dicLoc, string remark, ref string moveID)
        {
            AjaxResult res = new AjaxResult();
            try
            {
                T_MoveEntity move = new T_MoveEntity();
                move.Create();

                T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.F_Id == areaID);
                move.AreaCode = area.AreaCode;
                move.AreaID = area.F_Id;
                move.GenType = "Sys";
                move.IsAuto = "false";
                move.IsReverse = "false";
                move.MoveCode = T_CodeGenApp.GenNum("MoveRule");
                move.Remark = remark;
                move.State = "New";
                move.TaskCellNum = 0;
                db.Insert<T_MoveEntity>(move);

                foreach (var cell in dicLoc)
                {
                    T_ContainerEntity con = db.FindEntity<T_ContainerEntity>(o => o.LocationID == cell.Key && o.F_DeleteMark == false);
                    if (con == null)
                    {
                        res.state = ResultType.error.ToString();
                        res.message = "货位不存在容器：货位ID：" + cell.Key;
                        return res;
                    }

                    T_LocationEntity locSource = db.FindEntity<T_LocationEntity>(o => o.F_Id == cell.Key);
                    if (locSource.State != "Stored")
                    {
                        res.state = ResultType.error.ToString();
                        res.message = "货位不为已存储：货位编码：" + locSource.LocationCode;
                        return res;
                    }


                    T_LocationEntity locTag = db.FindEntity<T_LocationEntity>(o => o.F_Id == cell.Value);
                    if (locTag.State != "Empty")
                    {
                        res.state = ResultType.error.ToString();
                        res.message = "货位状态不为空：货位编码：" + locTag.LocationCode;
                        return res;
                    }

                    IList<T_ContainerDetailEntity> conDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ContainerID == con.F_Id).ToList();
                    if (conDetailList.Count < 0)
                    {
                        res.state = ResultType.error.ToString();
                        res.message = "货位无库存：货位ID：" + cell.Key;
                        return res;
                    }

                    T_ContainerDetailEntity conDetailFirst = conDetailList[0];  //仅记录同容器中一个物料

                    T_MoveRecordEntity record = new T_MoveRecordEntity();
                    record.Create();
                    record.BarCode = con.BarCode;
                    record.ContainerID = con.F_Id;
                    record.ExpireDate = null;
                    record.GenType = "PointSource";
                    record.InBoundID = conDetailFirst.InBoundID;
                    record.InBoundDetailID = conDetailFirst.InBoundDetailID;
                    record.ItemCode = conDetailFirst.ItemCode;
                    record.ItemID = conDetailFirst.ItemID;
                    record.ItemName = conDetailFirst.ItemName;
                    record.Factory = conDetailFirst.Factory;
                    record.Lot = conDetailFirst.Lot;
                    record.Spec = conDetailFirst.Spec;
                    record.ItemUnitText = conDetailFirst.ItemUnitText;
                    record.OverdueDate = conDetailFirst.OverdueDate;
                    record.MoveID = move.F_Id;
                    record.Qty = conDetailFirst.Qty;
                    record.ReceiveRecordID = conDetailFirst.ReceiveRecordID;
                    record.Remark = null;
                    record.SrcLocationCode = locSource.LocationCode;
                    record.SrcLocationID = locSource.F_Id;
                    record.State = "New";
                    record.SupplierCode = conDetailFirst.SupplierCode;
                    record.SupplierID = conDetailFirst.SupplierID;
                    record.SupplierName = conDetailFirst.SupplierName;
                    record.TagLocationCode = locTag.LocationCode;
                    record.TagLocationID = locTag.F_Id;

                    db.Insert<T_MoveRecordEntity>(record);
                }

                moveID = move.F_Id;

                db.SaveChanges();

                res.state = ResultType.success.ToString();
                return res;
            }
            catch (Exception ex)
            {
                db.RollBack();
                res.state = ResultType.error.ToString();
                res.message = ex.Message;
                return res;
            }
        }
        #endregion
    }
}
