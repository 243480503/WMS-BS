/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.WMSLogic;
using MST.Code;
using MST.Domain.Entity.WMSLogic;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace MST.Web.Areas.PC_ReportManage.Controllers
{
    /// <summary>
    /// 数据预测
    /// </summary>
    public class DataPredictionController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType">1按年月,2按年,3按月份</param>
        /// <param name="dataVal">所选月份（按年无用）</param>
        /// <param name="forecastTypeVal">预测方式：1回归，2智能</param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetData(string dataType, int dataVal, int forecastTypeVal, string keyword)
        {
            string msgAll = null;
            //AI预测值
            //IList<Dictionary<string, float>> forecastRes = new List<Dictionary<string, float>>();
            IList<object> forecastRes = new List<object>();
            int curYear = DateTime.Now.Year;
            int curMonth = DateTime.Now.Month;
            DateTime curMonthFirstDay = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date;
            //所有原始数据 
            IList<T_InOutDetailEntity> inOutList = new T_InOutDetailApp().FindList(o => o.OrderType == "PurchaseIn").ToList();

            if (!string.IsNullOrEmpty(keyword))
            {
                inOutList = inOutList.Where(o => o.ItemCode.Contains(keyword) || o.ItemName.Contains(keyword)).ToList();
            }

            //所有物料
            IList<string> itemList = inOutList.Select(o => o.ItemCode).Distinct().ToList();

            if (dataType == "1") //按年月
            {


                //需要显示的年月
                IList<string> xList = new List<string>();
                if (dataVal < curMonth) //
                {
                    for (int i = -8; i <= 1; i++)
                    {
                        string str = (curYear + i) + dataVal.ToString().PadLeft(2, '0');
                        xList.Add(str);
                    }
                }
                else
                {
                    for (int i = -9; i <= 0; i++)
                    {
                        string str = (curYear + i) + dataVal.ToString().PadLeft(2, '0');
                        xList.Add(str);
                    }
                }

                int ForecastX = Convert.ToInt32(xList.Last()); //需要预测的X轴的值


                inOutList = inOutList.Where(o => (o.F_CreatorTime.HasValue ? o.F_CreatorTime.Value < curMonthFirstDay : false) && (o.F_CreatorTime.HasValue ? o.F_CreatorTime.Value.Month == dataVal : false)).ToList(); //按月份过滤

                IList<YearMonthModel> detailList = inOutList.GroupBy(o => new
                {
                    Year = o.F_CreatorTime.HasValue ? o.F_CreatorTime.Value.Year : 0
                })
                                        .Select(o => new YearMonthModel
                                        {
                                            Year = o.Key.Year,
                                            MonthDetail = o.GroupBy(m =>
                                                   new
                                                   {
                                                       Month = m.F_CreatorTime.HasValue ? m.F_CreatorTime.Value.Month : 0
                                                   })
                                        .Select(n => new MonthDetail
                                        {
                                            Month = n.Key.Month,
                                            ItemDetail = n.GroupBy(i => new { ItemCode = i.ItemCode, ItemName = i.ItemName })
                                                          .Select(k => new ItemDetail { ItemName = k.Key.ItemName, ItemCode = k.Key.ItemCode, QtySum = k.Sum(j => j.ChangeQty) }).ToList()
                                        }).OrderBy(k => k.Month).ToList()
                                        }).OrderBy(o => o.Year).ToList();

                if (forecastTypeVal == 2)
                {
                    //数据加工
                    foreach (string item in itemList)
                    {
                        List<TimeSeriesData> macData = new List<TimeSeriesData>();
                        foreach (YearMonthModel cell in detailList)
                        {
                            bool hasInList = false;
                            foreach (MonthDetail monthDetailCell in cell.MonthDetail)
                            {

                                foreach (ItemDetail detail in monthDetailCell.ItemDetail)
                                {
                                    if (item == detail.ItemCode)
                                    {
                                        hasInList = true;
                                        macData.Add(new TimeSeriesData(detail.QtySum == null ? 0 : (float)detail.QtySum.Value));
                                    }
                                }

                            }
                            if (hasInList == false)
                            {
                                macData.Add(new TimeSeriesData(0));
                            }
                        }

                        string msg;
                        float[] Forecast = MacLearn(item, macData, dataType, out msg);
                        if (msg == null)
                        {
                            forecastRes.Add(new { ItemCode = item, Qty = Forecast[0] });
                        }
                        else
                        {
                            msgAll = msgAll + "[" + msg + "]";
                            forecastRes.Add(new { ItemCode = item, Qty = 0 });
                        }
                    }
                }

                var data = new
                {
                    xList = xList,//历年当前月份，不包含需预测的下年同月份
                    month = dataVal, //月份
                    curYear = curYear, //当前年份
                    itemList = itemList, //所有物料类型
                    Data = detailList, // 所有数据，
                    Forecast = forecastRes, //智能预测的值
                    dataType = dataType, //查询类型，此处为1表示历年同月份查询,
                    msg = msgAll, //有错误则预测失败
                    ForecastX = ForecastX
                };
                return Content(data.ToJson());
            }
            else if (dataType == "2")//按年
            {
                int ForecastX = (curYear); //需要预测的X轴的值

                //需要显示的年份
                IList<string> xList = new List<string>();// { curYear - 10, curYear - 9, curYear - 8, curYear - 7, curYear - 6, curYear - 5, curYear - 4, curYear - 3, curYear - 2, curYear - 1 };
                for (int i = -10; i <= 0; i++)
                {
                    xList.Add((curYear + i).ToString());
                }

                inOutList = inOutList.Where(o => o.F_CreatorTime.HasValue ? o.F_CreatorTime.Value.Year < curYear : false).ToList(); //按月份过滤
                List<YearMondel> detailList = inOutList.GroupBy(o => new
                {
                    Year = o.F_CreatorTime.HasValue ? o.F_CreatorTime.Value.Year : 0
                })
                                        .Select(o => new YearMondel
                                        {
                                            Year = o.Key.Year,
                                            ItemDetail = o.GroupBy(i => new { ItemCode = i.ItemCode, ItemName = i.ItemName })
                                                          .Select(k => new ItemDetail { ItemName = k.Key.ItemName, ItemCode = k.Key.ItemCode, QtySum = k.Sum(j => j.ChangeQty) }).ToList()
                                        }).ToList();


                //数据加工
                if (forecastTypeVal == 2)
                {
                    foreach (string item in itemList)
                    {

                        List<TimeSeriesData> macData = new List<TimeSeriesData>();

                        detailList.ForEach(o =>
                        {
                            var temp = o.ItemDetail.Where(j => j.ItemCode == item).FirstOrDefault();
                            if (temp != null)
                            {
                                macData.Add(new TimeSeriesData(temp.QtySum == null ? 0 : (float)temp.QtySum.Value));
                            }
                            else
                            {
                                macData.Add(new TimeSeriesData(0));
                            }
                        });

                        string msg;
                        float[] Forecast = MacLearn(item, macData, dataType, out msg);
                        if (msg == null)
                        {
                            forecastRes.Add(new { ItemCode = item, Qty = Forecast[0] });
                        }
                        else
                        {
                            msgAll = msgAll + "[" + msg + "]";
                            forecastRes.Add(new { ItemCode = item, Qty = 0 });

                        }
                    }
                }

                var data = new
                {
                    xList = xList, //所有年份，不包含需预测的年份
                    month = dataVal, //月份(此处无用)
                    curYear = curYear, //当前年份
                    itemList = itemList, //所有物料类型
                    Data = detailList, //所有数据
                    Forecast = forecastRes, //智能预测的值
                    dataType = dataType, //查询类型,此处为2表示年份查询
                    msg = msgAll, //有错误则预测失败
                    ForecastX = ForecastX //需要预测的X轴的值
                };
                return Content(data.ToJson());
            }
            else if (dataType == "3")//按月
            {
                int ForecastX = -1;
                IList<string> xList = new List<string>();
                for (int i = 0; i < 12; i++)
                {
                    xList.Add((i + 1).ToString());
                }
                if (curYear == dataVal)
                {
                    ForecastX = curMonth;
                }

                //查询月份往前再推12个月

                inOutList = inOutList.Where(o => (o.F_CreatorTime.HasValue ? o.F_CreatorTime.Value < curMonthFirstDay : false) && (o.F_CreatorTime.HasValue ? (o.F_CreatorTime.Value.Year == dataVal || (o.F_CreatorTime.Value.Year == dataVal - 1)) : false)).ToList(); //按月份过滤

                List<YearMonthModel> detailList = inOutList.GroupBy(o => new
                {
                    Year = o.F_CreatorTime.HasValue ? o.F_CreatorTime.Value.Year : 0
                })
                                        .Select(o => new YearMonthModel
                                        {
                                            Year = o.Key.Year,
                                            MonthDetail = o.GroupBy(m =>
                                                   new
                                                   {
                                                       Month = m.F_CreatorTime.HasValue ? m.F_CreatorTime.Value.Month : 0
                                                   })
                                        .Select(n => new MonthDetail
                                        {
                                            Month = n.Key.Month,
                                            ItemDetail = n.GroupBy(i => new { ItemCode = i.ItemCode, ItemName = i.ItemName })
                                                          .Select(k => new ItemDetail { ItemName = k.Key.ItemName, ItemCode = k.Key.ItemCode, QtySum = k.Sum(j => j.ChangeQty) }).ToList()
                                        }).OrderBy(k => k.Month).ToList()
                                        }).ToList();

                //补全本年的年份
                if (detailList.Where(o => o.Year == (dataVal)).Count() < 1) //不存在上一年数据
                {
                    YearMonthModel year = new YearMonthModel();
                    year.Year = dataVal;
                    year.MonthDetail = new List<MonthDetail>();
                    detailList.Add(year);
                }

                //补全上一年的年份
                if (detailList.Where(o => o.Year == (dataVal - 1)).Count() < 1) //不存在上一年数据
                {
                    YearMonthModel year = new YearMonthModel();
                    year.Year = dataVal - 1;
                    year.MonthDetail = new List<MonthDetail>();
                    detailList.Add(year);
                }

                //补全年份中没有数据的月份
                for (int i = 0; i < detailList.Count; i++)
                {
                    IList<MonthDetail> mDetailList = detailList[i].MonthDetail;
                    int? lastMonth = (detailList[i].Year == curYear)?(curMonth-1):12;
                    
                    for (int j = 0; j < lastMonth; j++)
                    {
                        if (mDetailList.Where(o => o.Month == (j + 1)).Count() < 1)
                        {
                            MonthDetail detail = new MonthDetail();
                            detail.Month = (j + 1);
                            detail.ItemDetail = new List<ItemDetail>();
                            detailList[i].MonthDetail.Add(detail);
                        }
                    }
                }

               

                if (forecastTypeVal == 2)
                {
                    //数据加工
                    foreach (string item in itemList)
                    {
                        List<TimeSeriesData> macData = new List<TimeSeriesData>();
                        foreach (YearMonthModel cell in detailList)
                        {

                            foreach (MonthDetail monthDetailCell in cell.MonthDetail)
                            {
                                bool hasInList = false;
                                foreach (ItemDetail detail in monthDetailCell.ItemDetail)
                                {
                                    if (item == detail.ItemCode)
                                    {
                                        hasInList = true;
                                        macData.Add(new TimeSeriesData(detail.QtySum == null ? 0 : (float)detail.QtySum.Value));
                                    }
                                }
                                if (hasInList == false)
                                {
                                    macData.Add(new TimeSeriesData(0));
                                }
                            }
                        }

                        string msg;
                        float[] Forecast = MacLearn(item, macData, dataType, out msg);
                        if (msg == null)
                        {
                            forecastRes.Add(new { ItemCode = item, Qty = Forecast[0] });
                        }
                        else
                        {
                            msgAll = msgAll + "[" + msg + "]";
                            forecastRes.Add(new { ItemCode = item, Qty = 0 });
                        }
                    }
                }

                //排序
                detailList = detailList.OrderBy(o => o.Year).ToList();
                detailList.ForEach(o =>o.MonthDetail = o.MonthDetail.OrderBy(j => j.Month).ToList());


                var data = new
                {
                    xList = xList,//历年当前月份，不包含需预测的下年同月份
                    year = dataVal, //年份
                    curYear = curYear, //当前年份
                    itemList = itemList, //所有物料类型
                    Data = detailList, // 所有数据，
                    Forecast = forecastRes, //智能预测的值
                    dataType = dataType, //查询类型，此处为1表示历年同月份查询,
                    msg = msgAll, //有错误则预测失败
                    ForecastX = ForecastX //需要预测的X轴的值
                };
                return Content(data.ToJson());
            }
            return null;
        }

        //按年月模型
        //[{"Year":2021,"MonthDetail":[{"Month":10,"ItemDetail":[{"ItemName":"纸箱物品0001","ItemCode":"纸箱","QtySum":50.00},{"ItemName":"圆珠笔","ItemCode":"Pen","QtySum":300.00},{"ItemName":"料箱LXZ","ItemCode":"FRP","QtySum":4.00},{"ItemName":"内存","ItemCode":"Mermory","QtySum":150.00},{"ItemName":"呼吸机","ItemCode":"Hxj","QtySum":100.00},{"ItemName":"麻将桌","ItemCode":"Mjz","QtySum":2.00}]}]}]
        private class YearMonthModel
        {
            public int? Year { get; set; }
            public IList<MonthDetail> MonthDetail { get; set; }
        }

        private class MonthDetail
        {
            public int? Month { get; set; }
            public IList<ItemDetail> ItemDetail { get; set; }

        }

        private class ItemDetail
        {
            public string ItemName { get; set; }
            public string ItemCode { get; set; }
            public decimal? QtySum { get; set; }
        }


        //按年模型
        private class YearMondel
        {
            public int? Year { get; set; }
            public IList<ItemDetail> ItemDetail { get; set; }
        }

        /// <summary>
        /// 用于训练的数据集模型
        /// </summary>
        private class MacDataModel
        {
            public decimal? value { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType">1按年月,2按年</param>
        /// <returns></returns>
        private float[] MacLearn(string item, IList<TimeSeriesData> data, string dataType, out string msg)
        {
            msg = null;
            if (data.Count < 1)
            {
                msg = "无原始数据";
                return null;
            }
            MLContext ml = new MLContext();
            var dataView = ml.Data.LoadFromEnumerable(data);

            var inputColumnName = nameof(TimeSeriesData.Value);
            var outputColumnName = nameof(ForecastResult.Forecast);

            // 单变量时序预测
            SsaForecastingEstimator model = null;
            if (dataType == "1")//按年月
            {
                if (data.Count < 5)
                {
                    msg = item + "原始数据太少(应大于2年)";
                    return null;
                }
                model = ml.Forecasting.ForecastBySsa(outputColumnName, //转换 inputColumnName 后产生的列的名称
                    inputColumnName,  //要转换的列的名称。 如果设置为 null ，则的 outputColumnName 值将用作源。 向量包含警报、原始分数、P 值作为前三个值。
                    2, //windowSize 周期，用于生成轨迹矩阵的系列窗口的长度(参数 L)
                    data.Count, //seriesLength 此参数指定执行预测时使用的数据点数，保留在緩衝區中的數列長度，用於模型化 (參數 N) ，应大于周期，11 用于建模 (参数 N) 的序列的长度。
                    data.Count, //trainSize 数据总数，开始用于定型的序列的长度。
                    1, //要预测的值的数目
                    confidenceLevel: 0.95f, //预测的置信度。置信水平越高（最高为1），置信区间的范围就越宽。相反，置信水平越低，置信区间界限越窄
                    confidenceLowerBoundColumn: "ConfidenceLowerBound", //置信区间下限列的名称。 如果未指定，则不会计算置信区间。
                    confidenceUpperBoundColumn: "ConfidenceUpperBound"  //置信区间上限列的名称。 如果未指定，则不会计算置信区间。
                    );
            }
            else //按年
            {
                if (data.Count < 5)
                {
                    msg = item + "原始数据太少(应大于等于5年)";
                    return null;
                }
                model = ml.Forecasting.ForecastBySsa(outputColumnName, //转换 inputColumnName 后产生的列的名称
                   inputColumnName,  //要转换的列的名称。 如果设置为 null ，则的 outputColumnName 值将用作源。 向量包含警报、原始分数、P 值作为前三个值。
                   2, //windowSize 周期，用于生成轨迹矩阵的系列窗口的长度(参数 L)
                   data.Count, //seriesLength 此参数指定执行预测时使用的数据点数，保留在緩衝區中的數列長度，用於模型化 (參數 N) ，应大于周期，11 用于建模 (参数 N) 的序列的长度。
                   data.Count, //trainSize 数据总数，开始用于定型的序列的长度。
                   1, //要预测的值的数目
                   confidenceLevel: 0.95f, //预测的置信度。置信水平越高（最高为1），置信区间的范围就越宽。相反，置信水平越低，置信区间界限越窄
                   confidenceLowerBoundColumn: "ConfidenceLowerBound", //置信区间下限列的名称。 如果未指定，则不会计算置信区间。
                   confidenceUpperBoundColumn: "ConfidenceUpperBound"  //置信区间上限列的名称。 如果未指定，则不会计算置信区间。
                   );
            }
            var transformer = model.Fit(dataView);

            var forecastEngine = transformer.CreateTimeSeriesEngine<TimeSeriesData,
                ForecastResult>(ml);

            var forecast = forecastEngine.Predict();
            return forecast.Forecast;
        }

        class ForecastResult
        {
            public float[] Forecast { get; set; }
            public float[] ConfidenceLowerBound { get; set; }
            public float[] ConfidenceUpperBound { get; set; }
        }

        class TimeSeriesData
        {
            public float Value;

            public TimeSeriesData(float value)
            {
                Value = value;
            }
        }
    }


}
