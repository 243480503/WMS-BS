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
using MST.Application.WMSLogic;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.RF_EmptyContainerManage.Controllers
{
    [HandlerLogin]
    public class EmptyContainerController : ControllerBase
    {
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_LocationApp locationApp = new T_LocationApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_TaskApp taskApp = new T_TaskApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private static object lockObj = new object();

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult CallEmpty()
        {
            return View();
        }

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
            stationList = stationApp.FindList(o => o.UseCode.Contains("EmptyOut")).ToList();
            return Content(stationList.ToJson());
        }

        /// <summary>
        /// 站台对应的空容器库存
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetEmptyCountInWare(string stationID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == stationID);
                IList<T_ContainerDetailEntity> empList = new List<T_ContainerDetailEntity>();
                if (station.StationCode == FixType.Station.StationEmpty.ToString()) //空料箱站台
                {
                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.NormalArea.ToString());
                    empList = db.FindList<T_ContainerDetailEntity>(o => o.AreaCode == area.AreaCode && o.ItemCode == FixType.Item.EmptyPlastic.ToString())
                             .Join(db.FindList<T_LocationEntity>(o => o.State == "Stored" && (o.ForbiddenState == "Normal"
                                                           || o.ForbiddenState == "OnlyOut"))
                             , m => m.LocationID, n => n.F_Id, (m, n) => m).ToList();
                }
                else if (station.StationCode == FixType.Station.StationIn_BigItem.ToString()) //大件站台
                {
                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.BigItemArea.ToString());
                    empList = db.FindList<T_ContainerDetailEntity>(o => o.AreaCode == area.AreaCode && o.ItemCode == FixType.Item.EmptyRack.ToString())
                             .Join(db.FindList<T_LocationEntity>(o => o.State == "Stored" && (o.ForbiddenState == "Normal"
                                                           || o.ForbiddenState == "OnlyOut"))
                             , m => m.LocationID, n => n.F_Id, (m, n) => m).ToList();
                }
                else
                {
                    return Error("未知站台类型", "");
                }
                int count = empList.Count;
                return Content(count.ToJson());
            }
        }
        #endregion

        #region RF呼叫空容器

        /// <summary>
        /// RF呼叫空容器
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult CallOut(string stationID, int num)
        {
            AjaxResult res = EmptyCallOut(stationID, num);
            if ((ResultType)res.state == ResultType.success)
            {
                return Success(res.message);
            }
            else
            {
                return Error(res.message, "");
            }
        }

        /// <summary>
        /// 呼叫空容器(RF呼叫、WCS呼叫 公用)
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="num"></param>
        /// <param name="locationNo">可选，若不填写，则随机存放空料箱货位，若填写，则指定要存放的空料箱货位地址，且num参数应固定为1</param>
        /// <returns></returns>
        public AjaxResult EmptyCallOut(string stationID, int num, string locationNo = null)
        {
            lock (lockObj)
            {
                using (var db = new RepositoryBase().BeginTrans())
                {
                    AjaxResult res = new AjaxResult();
                    T_StationEntity station = stationApp.FindEntity(o => o.F_Id == stationID);

                    int lastNum = num;
                    if (station.StationCode == FixType.Station.StationEmpty.ToString()) //空料箱站台需统计空位数量
                    {
                        if (!string.IsNullOrEmpty(locationNo))
                        {
                            T_AreaEntity emptyArea = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.EmptyArea.ToString());
                            IList<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => o.AreaID == emptyArea.F_Id && o.State == "Empty" && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyIn"));
                            if (locList.Count < 1)
                            {
                                res.state = ResultType.error;
                                res.message = "空料箱存储位已满";
                                return res;
                            }
                            if (num > locList.Count)
                            {
                                lastNum = locList.Count;
                            }
                        }
                        else
                        {
                            lastNum = 1;
                        }
                    }
                    else //其它站台只可一次叫一个
                    {
                        IList<T_TaskEntity> inWayTask = db.FindList<T_TaskEntity>(o => o.TaskType == "TaskType_EmptyOut" && o.TagLocationCode == station.TagAddress);
                        if (inWayTask.Count > 0)
                        {
                            res.state = ResultType.error;
                            res.message = "已存在空容器出库任务";
                            return res;
                        }
                    }

                    List<string> taskNoList = new List<string>();

                    IList<T_ContainerDetailEntity> empList = new List<T_ContainerDetailEntity>();
                    if (station.StationCode == FixType.Station.StationEmpty.ToString()) //空料箱站台,统计空容器
                    {
                        T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.NormalArea.ToString());
                        empList = db.FindList<T_ContainerDetailEntity>(o => o.AreaCode == area.AreaCode && o.ItemCode == FixType.Item.EmptyPlastic.ToString())
                                 .Join(db.FindList<T_LocationEntity>(o => o.State == "Stored" && (o.ForbiddenState == "Normal"
                                                               || o.ForbiddenState == "OnlyOut"))
                                 , m => m.LocationID, n => n.F_Id, (m, n) => m).OrderByDescending(o => o.LocationNo).ToList();
                    }
                    else if (station.StationCode == FixType.Station.StationIn_BigItem.ToString()) //大件站台,统计空容器
                    {
                        T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.BigItemArea.ToString());
                        empList = db.FindList<T_ContainerDetailEntity>(o => o.AreaCode == area.AreaCode && o.ItemCode == FixType.Item.EmptyRack.ToString())
                                 .Join(db.FindList<T_LocationEntity>(o => o.State == "Stored" && (o.ForbiddenState == "Normal"
                                                               || o.ForbiddenState == "OnlyOut"))
                                 , m => m.LocationID, n => n.F_Id, (m, n) => m).ToList();
                    }
                    else
                    {
                        res.state = ResultType.error;
                        res.message = "未知站台类型";
                        return res;
                    }

                    if (empList.Count < 1)
                    {
                        res.state = ResultType.error;
                        res.message = "空容器不足";
                        return res;
                    }

                    for (int i = 0; i < lastNum; i++)
                    {
                        T_ContainerDetailEntity emp = empList[i];
                        T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == emp.BarCode && o.F_DeleteMark == false);
                        T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == containerEntity.LocationID);
                        loc.State = "Out";
                        db.Update<T_LocationEntity>(loc);



                        T_TaskEntity taskBack = new T_TaskEntity();
                        taskBack.F_Id = Guid.NewGuid().ToString();
                        taskBack.TaskNo = T_CodeGenApp.GenNum("TaskRule");

                        taskBack.ContainerID = containerEntity.F_Id;
                        taskBack.BarCode = containerEntity.BarCode;
                        taskBack.ContainerType = containerEntity.ContainerType;
                        taskBack.SrcLocationID = containerEntity.LocationID;
                        taskBack.SrcLocationCode = containerEntity.LocationNo;
                        taskBack.SrcWCSLocCode = loc.WCSLocCode;
                        taskBack.ApplyStationID = station.F_Id;

                        if (station.StationCode == FixType.Station.StationEmpty.ToString()) //移库类型
                        {
                            string errMsg = "";
                            T_LocationApp locationApp = new T_LocationApp();
                            T_AreaEntity empArea = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.EmptyArea.ToString());
                            T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == containerEntity.ContainerType);
                            LogObj log = null;
                            T_LocationEntity locIn = null;
                            if (string.IsNullOrEmpty(locationNo))
                            {
                                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                locIn = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, empArea.F_Id, true, null, null, true, null, item);
                            }
                            else
                            {
                                locIn = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locationNo);
                                if (locIn.State != "Empty")
                                {
                                    res.state = ResultType.error;
                                    res.message = "当前呼叫空容器的货位状态不为空";
                                    return res;
                                }
                                if (locIn.ForbiddenState == "Lock" || locIn.ForbiddenState == "OnlyOut")
                                {
                                    res.state = ResultType.error;
                                    res.message = "当前呼叫空容器的货位禁用状态为锁定或可出不可入";
                                    return res;
                                }
                            }
                            if (locIn == null)
                            {
                                res.state = ResultType.error;
                                res.message = "货位分配失败：" + errMsg;
                                return res;
                            }

                            taskBack.TaskInOutType = "MoveType";
                            taskBack.TaskType = "TaskType_Move";
                            taskBack.OrderType = "MoveType";

                            taskBack.TagAreaID = empArea.F_Id;
                            taskBack.TagLocationID = locIn.F_Id;
                            taskBack.TagLocationCode = locIn.LocationCode;
                            taskBack.TagWCSLocCode = locIn.WCSLocCode;

                            locIn.State = "In";
                            db.Update<T_LocationEntity>(locIn);
                        }
                        else  //出库类型
                        {
                            taskBack.TaskInOutType = "OutType";
                            taskBack.TaskType = "TaskType_EmptyOut";
                            taskBack.OrderType = "EmptyOut";

                            taskBack.TagAreaID = "";
                            taskBack.TagLocationID = station.F_Id;
                            taskBack.TagLocationCode = station.TagAddress;
                            taskBack.TagWCSLocCode = station.TagAddress;
                        }
                        taskBack.WaveID = "";
                        taskBack.WaveCode = "";
                        taskBack.SEQ = 0;
                        taskBack.Level = 20;
                        taskBack.State = "New";
                        taskBack.IsWcsTask = "true";
                        taskBack.SendWCSTime = null;

                        taskBack.OrderID = "";
                        taskBack.OrderDetailID = "";
                        taskBack.OrderCode = "";
                        taskBack.OverTime = null;

                        taskBack.IsCanExec = "true";
                        db.Insert<T_TaskEntity>(taskBack);
                        taskNoList.Add(taskBack.TaskNo);
                    }
                    db.SaveChanges();

                    WCSPost post = new WCSPost();
                    WCSResult wcsRes = post.SendTask(db, taskNoList);
                    if (wcsRes.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                        res.state = ResultType.success;
                        res.message = "呼叫成功";
                        return res;
                    }
                    else
                    {
                        db.RollBack();
                        res.state = ResultType.error;
                        res.message = "失败:" + wcsRes.FailMsg;
                        return res;
                    }
                }
            }
        }
        #endregion

        #region  获取所有空容器出库任务
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetEmptyTask()
        {

            var data = taskApp.FindList(o => o.TaskType == "TaskType_EmptyOut").ToList();
            IList<ItemsDetailEntity> enumStatelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumInOutTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.TaskInOutType).ToList();
            IList<ItemsDetailEntity> enumOrderTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.OrderType).ToList();
            IList<ItemsDetailEntity> enumTaskTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.TaskType).ToList();
            IList<TaskModel> taskModelList = new List<TaskModel>();
            foreach (T_TaskEntity task in data)
            {
                TaskModel model = task.ToObject<TaskModel>();
                model.TaskInOutTypeName = enumInOutTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskInOutType).F_ItemName;
                model.StateName = enumStatelist.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.TaskTypeName = enumTaskTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskType).F_ItemName;
                model.OrderTypeName = enumOrderTypelist.FirstOrDefault(o => o.F_ItemCode == model.OrderType).F_ItemName;
                model.TagLocationCodeName = task.TagLocationCode;
                taskModelList.Add(model);
            }
            return Content(taskModelList.ToJson());
        }
        #endregion

        /**********************************************************************/

        #region 空容器入库（仅料架）
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult PutEmpty()
        {
            return View();
        }

        /// <summary>
        /// 获取默认入库点列表（仅料架）
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetInStationDicList()
        {
            List<T_StationEntity> stationList = new List<T_StationEntity>();
            stationList = stationApp.FindList(o => o.CurModel == "Empty" && o.StationCode == FixType.Station.StationIn_BigItem.ToString() && o.UseCode.Contains("EmptyIn")).ToList();
            return Content(stationList.ToJson());
        }
        #endregion


        #region 产生空容器入库任务并发送（仅料架）

        /// <summary>
        /// 产生空容器入库任务并发送（仅料架）
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SendEmptyTask(string barCode, string stationID, string pointLoc)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                T_TaskEntity inDBTask = db.FindEntity<T_TaskEntity>(o => o.BarCode == barCode && o.ApplyStationID == stationID);
                if (inDBTask != null)
                {
                    return Error("不可重复提交", "");
                }

                T_StationEntity stationEntity = db.FindEntity<T_StationEntity>(o => o.F_Id == stationID);
                T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.AreaCode == FixType.Area.BigItemArea.ToString());
                T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == barCode && o.F_DeleteMark == false);
                T_ContainerTypeEntity containerTypeEntity = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == FixType.ContainerType.InnerRack.ToString());
                if (containerEntity == null) /// 如果是新容器
                {
                    containerEntity = new T_ContainerEntity();
                    containerEntity.F_Id = Guid.NewGuid().ToString();
                    containerEntity.BarCode = barCode;
                    containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                    containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                    containerEntity.IsContainerVir = "0";
                    containerEntity.AreaID = areaEntnty.F_Id;
                    containerEntity.AreaCode = areaEntnty.AreaCode;
                    containerEntity.AreaName = areaEntnty.AreaName;
                    containerEntity.F_DeleteMark = false;
                    db.Insert<T_ContainerEntity>(containerEntity);
                    db.SaveChanges();
                }
                else
                {
                    if (containerEntity.F_DeleteMark == true)
                    {
                        containerEntity.BarCode = barCode;
                        containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                        containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                        containerEntity.IsContainerVir = "0";
                        containerEntity.AreaID = areaEntnty.F_Id;
                        containerEntity.AreaCode = areaEntnty.AreaCode;
                        containerEntity.AreaName = areaEntnty.AreaName;
                        containerEntity.F_DeleteMark = false;
                        db.Update<T_ContainerEntity>(containerEntity);
                        db.SaveChanges();
                    }
                    else
                    {
                        return Error("该容器条码已存在", "");
                    }

                }

                string errMsg = "";
                T_LocationApp locationApp = new T_LocationApp();
                LogObj log = null;

                T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                T_LocationEntity loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerTypeEntity, areaEntnty.F_Id, true, null, null, true, pointLoc, item);
                if (loc == null)
                {
                    return Error("入库货位分配失败" + errMsg, "");
                }

                T_TaskEntity taskEntity = new T_TaskEntity();
                taskEntity.Create();
                taskEntity.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                taskEntity.TaskInOutType = "InType";

                taskEntity.TaskType = "TaskType_EmptyIn";
                taskEntity.OrderType = "EmptyIn";


                taskEntity.ContainerID = containerEntity.F_Id;
                taskEntity.BarCode = barCode;
                taskEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                taskEntity.SrcLocationID = stationEntity.F_Id;
                taskEntity.SrcLocationCode = stationEntity.LeaveAddress;
                taskEntity.SrcWCSLocCode = stationEntity.LeaveAddress;
                taskEntity.TagAreaID = areaEntnty.F_Id;
                taskEntity.ApplyStationID = stationEntity.F_Id;
                taskEntity.SEQ = 1;
                taskEntity.Level = 10;
                taskEntity.State = "New";
                taskEntity.IsWcsTask = "true";
                taskEntity.ExecEquID = null;
                taskEntity.IsCanExec = "true";
                taskEntity.SendWCSTime = DateTime.Now;
                taskEntity.OverTime = null;
                taskEntity.F_DeleteMark = false;
                taskEntity.OrderCode = "";
                taskEntity.OrderDetailID = "";
                taskEntity.OrderID = "";
                taskEntity.TagLocationCode = loc.LocationCode;
                taskEntity.TagLocationID = loc.F_Id;
                taskEntity.TagWCSLocCode = loc.WCSLocCode;
                db.Insert<T_TaskEntity>(taskEntity);
                db.SaveChanges();

                string[] taskNoList = new string[] { taskEntity.TaskNo };

                WCSPost post = new WCSPost();
                WCSResult wcsRes = post.SendTask(db, taskNoList);
                if (wcsRes.IsSuccess)
                {
                    db.CommitWithOutRollBack();
                    return Success("操作成功");
                }
                else
                {
                    db.RollBack();
                    return Error("失败:" + wcsRes.FailMsg, "");
                }
            }
        }
        #endregion

        #region  获取所有空容器入库任务
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetInEmptyTask()
        {
            var data = taskApp.FindList(o => o.TaskType == "TaskType_EmptyIn").ToList();
            IList<ItemsDetailEntity> enumStatelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumInOutTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.TaskInOutType).ToList();
            IList<ItemsDetailEntity> enumOrderTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.OrderType).ToList();
            IList<ItemsDetailEntity> enumTaskTypelist = itemsDetailApp.FindEnum<T_TaskEntity>(o => o.TaskType).ToList();
            IList<TaskModel> taskModelList = new List<TaskModel>();
            foreach (T_TaskEntity task in data)
            {
                T_StationEntity station = stationApp.FindEntity(o => o.F_Id == task.SrcLocationID);

                TaskModel model = task.ToObject<TaskModel>();
                model.TaskInOutTypeName = enumInOutTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskInOutType).F_ItemName;
                model.StateName = enumStatelist.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                model.TaskTypeName = enumTaskTypelist.FirstOrDefault(o => o.F_ItemCode == model.TaskType).F_ItemName;
                model.OrderTypeName = enumOrderTypelist.FirstOrDefault(o => o.F_ItemCode == model.OrderType).F_ItemName;
                model.TagLocationCodeName = task.TagLocationCode;
                taskModelList.Add(model);
            }
            return Content(taskModelList.ToJson());
        }
        #endregion
    }
}
