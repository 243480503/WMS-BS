
var sourceData = null;
(function () {
    $.ajax({
        url: "/PC_BoardManage/Board/GetGridJson",
        data: {},
        dataType: "json",
        type: "Get",
        async: false,
        success: function (data) {
            if (data.state == "success") {
                sourceData = data.data;
                $("#allInSum").text(sourceData.curInOutCountData.InNum);
                $("#allOutSum").text(sourceData.curInOutCountData.OutNum);
            }
            else {
                //$.modalMsg(data.message, "error");
                alert("获取数据失败");
            }
        },
        error: function (data) {
            //$.modalMsg(data.message, "error");
            alert("获取数据失败");
        }
    });
})();

// 当日作业汇总
(function () {
    //{\"InTaskCount\":0,\"OutTaskCount\":0,\"QATaskCount\":0,\"CountTaskCount\":0}
    var curDayWorkData = sourceData.curDayWorkData;
    var chartData = [curDayWorkData.InTaskCount, curDayWorkData.OutTaskCount, curDayWorkData.QATaskCount, curDayWorkData.CountTaskCount];
    // 1实例化对象
    var myChart = echarts.init(document.querySelector(".bar .chart"));

    // 2. 当日作业汇总
    var option = {
        color: ["#2f89cf"],
        tooltip: {
            trigger: "axis",
            axisPointer: {
                // 坐标轴指示器，坐标轴触发有效
                type: "shadow" // 默认为直线，可选为：'line' | 'shadow'
            }
        },
        // 修改图表的大小
        grid: {
            left: "0%",
            top: "10px",
            right: "0%",
            bottom: "4%",
            containLabel: true
        },
        xAxis: [
            {
                type: "category",
                data: [
                    "入库作业",
                    "出库作业",
                    "盘点作业",
                    "质检作业"
                ],
                axisTick: {
                    alignWithLabel: true
                },
                // 修改刻度标签 相关样式
                axisLabel: {
                    color: "rgba(255,255,255,.6) ",
                    fontSize: "12"
                },
                // 不显示x坐标轴的样式
                axisLine: {
                    show: false
                }
            }
        ],
        yAxis: [
            {
                type: "value",
                // 修改刻度标签 相关样式
                axisLabel: {
                    color: "rgba(255,255,255,.6) ",
                    fontSize: 12
                },
                // y轴的线条改为了 2像素
                axisLine: {
                    lineStyle: {
                        color: "rgba(255,255,255,.1)",
                        width: 2
                    }
                },
                // y轴分割线的颜色
                splitLine: {
                    lineStyle: {
                        color: "rgba(255,255,255,.1)"
                    }
                }
            }
        ],
        series: [
            {
                name: "次数",
                type: "bar",
                barWidth: "35%",
                data: chartData,
                itemStyle: {
                    // 修改柱子圆角
                    barBorderRadius: 5
                }
            }
        ]
    };
    // 3. 把配置项给实例对象
    myChart.setOption(option);
    // 4. 让图表跟随屏幕自动的去适应
    window.addEventListener("resize", function () {
        myChart.resize();
    });
})();





// 单据进度
(function () {
    //{\"InOrderCount\":14,\"OutOrderCount\":7,\"QAOrderCount\":16,\"CountOrderCount\":5}
    var curOrderPerData = sourceData.curOrderPerData;
    var fullData = [curOrderPerData.InOrderCount_Sum, curOrderPerData.OutOrderCount_Sum, curOrderPerData.QAOrderCount_Sum, curOrderPerData.CountOrderCount_Sum];
    var curData = [(100 * curOrderPerData.InOrderCount / curOrderPerData.InOrderCount_Sum).toFixed(2),
                   (100 * curOrderPerData.OutOrderCount / curOrderPerData.OutOrderCount_Sum).toFixed(2),
                   (100 * curOrderPerData.QAOrderCount / curOrderPerData.QAOrderCount_Sum).toFixed(2),
                   (100 * curOrderPerData.CountOrderCount / curOrderPerData.CountOrderCount_Sum).toFixed(2)
                  ];
    var myColor = ["#1089E7", "#F57474", "#56D0E3", "#F8B448"];
    // 1. 实例化对象
    var myChart = echarts.init(document.querySelector(".bar2 .chart"));
    // 2. 指定配置和数据
    var option = {
        grid: {
            top: "10%",
            left: "22%",
            bottom: "10%"
            // containLabel: true
        },
        // 不显示x轴的相关信息
        xAxis: {
            show: false
        },
        yAxis: [
            {
                type: "category",
                inverse: true,
                data: ["入库", "出库", "质检", "盘点"],
                // 不显示y轴的线
                axisLine: {
                    show: false
                },
                // 不显示刻度
                axisTick: {
                    show: false
                },
                // 把刻度标签里面的文字颜色设置为白色
                axisLabel: {
                    color: "#fff"
                }
            },
            {
                data: fullData,
                inverse: true,
                // 不显示y轴的线
                axisLine: {
                    show: false
                },
                // 不显示刻度
                axisTick: {
                    show: false
                },
                // 把刻度标签里面的文字颜色设置为白色
                axisLabel: {
                    color: "#fff"
                }
            }
        ],
        series: [
            {
                name: "条",
                type: "bar",
                data: curData,
                yAxisIndex: 0,
                // 修改第一组柱子的圆角
                itemStyle: {
                    barBorderRadius: 5,
                    // 此时的color 可以修改柱子的颜色
                    color: function (params) {
                        // params 传进来的是柱子对象
                        // console.log(params);
                        // dataIndex 是当前柱子的索引号
                        return myColor[params.dataIndex];
                    }
                },
                // 柱子之间的距离
                barCategoryGap: 50,
                //柱子的宽度
                barWidth: 18,
                // 显示柱子内的文字
                label: {
                    show: true,
                    position: "outside",
                    // {c} 会自动的解析为 数据  data里面的数据
                    formatter: "{c}%"
                }
            },
            {
                name: "框",
                type: "bar",
                barCategoryGap: 50,
                barWidth: 20,
                yAxisIndex: 1,
                data: [100, 100, 100, 100],
                itemStyle: {
                    color: "none",
                    borderColor: "#eee",
                    borderWidth: 0.2,
                    barBorderRadius: 5
                }
            }
        ]
    };

    // 3. 把配置给实例对象
    myChart.setOption(option);
    // 4. 让图表跟随屏幕自动的去适应
    window.addEventListener("resize", function () {
        myChart.resize();
    });
})();


// 物料库存变化
(function () {
    //[{\"YearNum\":2020,\"ItemList\":[{\"ItemCode\":\"Pen\",\"ItemName\":\"圆珠笔\",\"MonthData\":[{\"Month\":1,\"SumQty\":0},{\"Month\":2,\"SumQty\":0},{\"Month\":3,\"SumQty\":0},{\"Month\":4,\"SumQty\":0},{\"Month\":5,\"SumQty\":0},{\"Month\":6,\"SumQty\":0},{\"Month\":7,\"SumQty\":0},{\"Month\":8,\"SumQty\":0},{\"Month\":9,\"SumQty\":0},{\"Month\":10,\"SumQty\":0},{\"Month\":11,\"SumQty\":0},{\"Month\":12,\"SumQty\":0}]},{\"ItemCode\":\"Mermory\",\"ItemName\":\"内存\",\"MonthData\":[{\"Month\":1,\"SumQty\":0},{\"Month\":2,\"SumQty\":0},{\"Month\":3,\"SumQty\":0},{\"Month\":4,\"SumQty\":0},{\"Month\":5,\"SumQty\":0},{\"Month\":6,\"SumQty\":0},{\"Month\":7,\"SumQty\":0},{\"Month\":8,\"SumQty\":0},{\"Month\":9,\"SumQty\":0},{\"Month\":10,\"SumQty\":0},{\"Month\":11,\"SumQty\":0},{\"Month\":12,\"SumQty\":0}]}]},{\"YearNum\":2021,\"ItemList\":[{\"ItemCode\":\"Pen\",\"ItemName\":\"圆珠笔\",\"MonthData\":[{\"Month\":1,\"SumQty\":0},{\"Month\":2,\"SumQty\":0},{\"Month\":3,\"SumQty\":0},{\"Month\":4,\"SumQty\":0},{\"Month\":5,\"SumQty\":0},{\"Month\":6,\"SumQty\":0},{\"Month\":7,\"SumQty\":0},{\"Month\":8,\"SumQty\":0},{\"Month\":9,\"SumQty\":0},{\"Month\":10,\"SumQty\":300},{\"Month\":11,\"SumQty\":0},{\"Month\":12,\"SumQty\":0}]},{\"ItemCode\":\"Mermory\",\"ItemName\":\"内存\",\"MonthData\":[{\"Month\":1,\"SumQty\":0},{\"Month\":2,\"SumQty\":0},{\"Month\":3,\"SumQty\":0},{\"Month\":4,\"SumQty\":0},{\"Month\":5,\"SumQty\":0},{\"Month\":6,\"SumQty\":0},{\"Month\":7,\"SumQty\":0},{\"Month\":8,\"SumQty\":0},{\"Month\":9,\"SumQty\":0},{\"Month\":10,\"SumQty\":150},{\"Month\":11,\"SumQty\":0},{\"Month\":12,\"SumQty\":0}]}]}]
    var curItemChangeData = sourceData.curItemChangeData;

    //获取图表要显示的物料名称和编码
    //var firstItemCode1 = firstYearData.ItemList[0].ItemCode;
    var firstItemName1 = curItemChangeData[0].ItemList[0].ItemName;

    //var firstItemCode2 = firstYearData.ItemList[1].ItemCode;
    var firstItemName2 = curItemChangeData[0].ItemList[1].ItemName;
    //var firstYearData = curItemChangeData[0];
    //var firstYear = firstYearData.YearNum;

    var yearData = [];
    $.each(curItemChangeData, function (i, n) {
        var parent = {};
        parent.year = n.YearNum;
        parent.data = [];
        $.each(n.ItemList, function (j, k) {
            var monthArray = [];
            $.each(k.MonthData, function (a, b) {
                monthArray.push(b.SumQty);
            });
            parent.data.push(monthArray);
        });
        yearData.push(parent);
        $(".line h2 a").eq(i).text(n.YearNum);
    });


    //var yearData = [
    //    {
    //        year: firstYear.YearNum, // 年份
    //        data: [
    //            // 两个数组是因为有两条线
    //            //[   firstYear.MonthList[0].SumQty,
    //            //    firstYear.MonthList[1].SumQty,
    //            //    firstYear.MonthList[2].SumQty,
    //            //    firstYear.MonthList[3].SumQty,
    //            //    firstYear.MonthList[4].SumQty,
    //            //    firstYear.MonthList[5].SumQty,
    //            //    firstYear.MonthList[6].SumQty,
    //            //    firstYear.MonthList[7].SumQty,
    //            //    firstYear.MonthList[8].SumQty,
    //            //    firstYear.MonthList[9].SumQty,
    //            //    firstYear.MonthList[10].SumQty,
    //            //    firstYear.MonthList[11].SumQty,
    //            //    firstYear.MonthList[12].SumQty
    //            //],
    //            //[40, 64, 191, 324, 290, 330, 310, 213, 180, 200, 180, 79]
    //        ]
    //    },
    //    {
    //        year: secondYear.YearNum, // 年份
    //        data: [
    //            // 两个数组是因为有两条线
    //            [123, 175, 112, 197, 121, 67, 98, 21, 43, 64, 76, 38],
    //            [143, 131, 165, 123, 178, 21, 82, 64, 43, 60, 19, 34]
    //        ]
    //    }
    //];
    // 1. 实例化对象
    var myChart = echarts.init(document.querySelector(".line .chart"));
    // 2.指定配置
    var option = {
        // 通过这个color修改两条线的颜色
        color: ["#00f2f1", "#ed3f35"],
        tooltip: {
            trigger: "axis"
        },
        legend: {
            // 如果series 对象有name 值，则 legend可以不用写data
            // 修改图例组件 文字颜色
            textStyle: {
                color: "#4c9bfd"
            },
            // 这个10% 必须加引号
            right: "10%"
        },
        grid: {
            top: "20%",
            left: "3%",
            right: "4%",
            bottom: "3%",
            show: true, // 显示边框
            borderColor: "#012f4a", // 边框颜色
            containLabel: true // 包含刻度文字在内
        },

        xAxis: {
            type: "category",
            boundaryGap: false,
            data: [
                "1月",
                "2月",
                "3月",
                "4月",
                "5月",
                "6月",
                "7月",
                "8月",
                "9月",
                "10月",
                "11月",
                "12月"
            ],
            axisTick: {
                show: false // 去除刻度线
            },
            axisLabel: {
                color: "#4c9bfd" // 文本颜色
            },
            axisLine: {
                show: false // 去除轴线
            }
        },
        yAxis: {
            type: "value",
            axisTick: {
                show: false // 去除刻度线
            },
            axisLabel: {
                color: "#4c9bfd" // 文本颜色
            },
            axisLine: {
                show: false // 去除轴线
            },
            splitLine: {
                lineStyle: {
                    color: "#012f4a" // 分割线颜色
                }
            }
        },
        series: [
            {
                name: firstItemName1,
                type: "line",
                // true 可以让我们的折线显示带有弧度
                smooth: true,
                data: yearData[1].data[0]
            },
            {
                name: firstItemName2,
                type: "line",
                smooth: true,
                data: yearData[1].data[1]
            }
            
            
        ]
    };

    // 3. 把配置给实例对象
    myChart.setOption(option);
    // 4. 让图表跟随屏幕自动的去适应
    window.addEventListener("resize", function () {
        myChart.resize();
    });

    // 5.点击切换效果
    $(".line h2").on("click", "a", function () {
        // alert(1);
        // console.log($(this).index());
        // 点击 a 之后 根据当前a的索引号 找到对应的 yearData的相关对象
        // console.log(yearData[$(this).index()]);
        var obj = yearData[$(this).index()];
        option.series[0].data = obj.data[0];
        option.series[1].data = obj.data[1];
        // 需要重新渲染
        myChart.setOption(option);
    });
})();


// 每日出入库数量（30天）
(function () {
    //[{\"InOutType\":\"In\",\"DataList\":[{\"Day\":0,\"DayQty\":0},{\"Day\":1,\"DayQty\":0},{\"Day\":2,\"DayQty\":0},{\"Day\":3,\"DayQty\":0},{\"Day\":4,\"DayQty\":0},{\"Day\":5,\"DayQty\":0},{\"Day\":6,\"DayQty\":0},{\"Day\":7,\"DayQty\":0},{\"Day\":8,\"DayQty\":0},{\"Day\":9,\"DayQty\":0},{\"Day\":10,\"DayQty\":0},{\"Day\":11,\"DayQty\":0},{\"Day\":12,\"DayQty\":0},{\"Day\":13,\"DayQty\":0},{\"Day\":14,\"DayQty\":0},{\"Day\":15,\"DayQty\":0},{\"Day\":16,\"DayQty\":0},{\"Day\":17,\"DayQty\":0},{\"Day\":18,\"DayQty\":0},{\"Day\":19,\"DayQty\":0},{\"Day\":20,\"DayQty\":0},{\"Day\":21,\"DayQty\":0},{\"Day\":22,\"DayQty\":0},{\"Day\":23,\"DayQty\":0},{\"Day\":24,\"DayQty\":0},{\"Day\":25,\"DayQty\":0},{\"Day\":26,\"DayQty\":0},{\"Day\":27,\"DayQty\":0},{\"Day\":28,\"DayQty\":0},{\"Day\":29,\"DayQty\":0}]},{\"InOutType\":\"Out\",\"DataList\":[{\"Day\":0,\"DayQty\":0},{\"Day\":1,\"DayQty\":0},{\"Day\":2,\"DayQty\":0},{\"Day\":3,\"DayQty\":0},{\"Day\":4,\"DayQty\":0},{\"Day\":5,\"DayQty\":0},{\"Day\":6,\"DayQty\":0},{\"Day\":7,\"DayQty\":0},{\"Day\":8,\"DayQty\":0},{\"Day\":9,\"DayQty\":0},{\"Day\":10,\"DayQty\":0},{\"Day\":11,\"DayQty\":0},{\"Day\":12,\"DayQty\":0},{\"Day\":13,\"DayQty\":0},{\"Day\":14,\"DayQty\":0},{\"Day\":15,\"DayQty\":0},{\"Day\":16,\"DayQty\":0},{\"Day\":17,\"DayQty\":0},{\"Day\":18,\"DayQty\":0},{\"Day\":19,\"DayQty\":0},{\"Day\":20,\"DayQty\":0},{\"Day\":21,\"DayQty\":0},{\"Day\":22,\"DayQty\":0},{\"Day\":23,\"DayQty\":0},{\"Day\":24,\"DayQty\":0},{\"Day\":25,\"DayQty\":0},{\"Day\":26,\"DayQty\":0},{\"Day\":27,\"DayQty\":0},{\"Day\":28,\"DayQty\":0},{\"Day\":29,\"DayQty\":0}]}]
    var curDayInOut = sourceData.curDayInOut;
    var xAxis = [];
    var InData = [];
    var OutData = [];
    $.each(curDayInOut, function (i, n) {
        if (n.InOutType == "In") {
            $.each(n.DataList, function (j, k) {
                xAxis.push(j + 1);
                InData.push(k.DayQty);
            });
        }
        else {
            $.each(n.DataList, function (j, k) {
                OutData.push(k.DayQty);
            });
        }
    });

    var myChart = echarts.init(document.querySelector(".line2 .chart"));
    var option = {
        tooltip: {
            trigger: "axis"
        },
        legend: {
            top: "0%",
            data: ["出库数量", "入库数量"],
            textStyle: {
                color: "rgba(255,255,255,.5)",
                fontSize: "12"
            }
        },

        grid: {
            left: "10",
            top: "30",
            right: "10",
            bottom: "10",
            containLabel: true
        },
        xAxis: [
            {
                type: "category",
                boundaryGap: false,
                // x轴更换数据
                data: xAxis,
                // 文本颜色为rgba(255,255,255,.6)  文字大小为 12
                axisLabel: {
                    textStyle: {
                        color: "rgba(255,255,255,.6)",
                        fontSize: 12
                    }
                },
                // x轴线的颜色为   rgba(255,255,255,.2)
                axisLine: {
                    lineStyle: {
                        color: "rgba(255,255,255,.2)"
                    }
                }
            }
        ],
        yAxis: [
            {
                type: "value",
                axisTick: { show: false },
                axisLine: {
                    lineStyle: {
                        color: "rgba(255,255,255,.1)"
                    }
                },
                axisLabel: {
                    textStyle: {
                        color: "rgba(255,255,255,.6)",
                        fontSize: 12
                    }
                },
                // 修改分割线的颜色
                splitLine: {
                    lineStyle: {
                        color: "rgba(255,255,255,.1)"
                    }
                }
            }
        ],
        series: [
            {
                name: "出库数量",
                type: "line",
                smooth: true,
                // 单独修改当前线条的样式
                lineStyle: {
                    color: "#0184d5",
                    width: "2"
                },
                // 填充颜色设置
                areaStyle: {
                    color: new echarts.graphic.LinearGradient(
                        0,
                        0,
                        0,
                        1,
                        [
                            {
                                offset: 0,
                                color: "rgba(1, 132, 213, 0.4)" // 渐变色的起始颜色
                            },
                            {
                                offset: 0.8,
                                color: "rgba(1, 132, 213, 0.1)" // 渐变线的结束颜色
                            }
                        ],
                        false
                    ),
                    shadowColor: "rgba(0, 0, 0, 0.1)"
                },
                // 设置拐点
                symbol: "circle",
                // 拐点大小
                symbolSize: 8,
                // 开始不显示拐点， 鼠标经过显示
                showSymbol: false,
                // 设置拐点颜色以及边框
                itemStyle: {
                    color: "#0184d5",
                    borderColor: "rgba(221, 220, 107, .1)",
                    borderWidth: 12
                },
                data: OutData
            },
            {
                name: "入库数量",
                type: "line",
                smooth: true,
                lineStyle: {
                    normal: {
                        color: "#00d887",
                        width: 2
                    }
                },
                areaStyle: {
                    normal: {
                        color: new echarts.graphic.LinearGradient(
                            0,
                            0,
                            0,
                            1,
                            [
                                {
                                    offset: 0,
                                    color: "rgba(0, 216, 135, 0.4)"
                                },
                                {
                                    offset: 0.8,
                                    color: "rgba(0, 216, 135, 0.1)"
                                }
                            ],
                            false
                        ),
                        shadowColor: "rgba(0, 0, 0, 0.1)"
                    }
                },
                // 设置拐点 小圆点
                symbol: "circle",
                // 拐点大小
                symbolSize: 5,
                // 设置拐点颜色以及边框
                itemStyle: {
                    color: "#00d887",
                    borderColor: "rgba(221, 220, 107, .1)",
                    borderWidth: 12
                },
                // 开始不显示拐点， 鼠标经过显示
                showSymbol: false,
                data: InData
            }
        ]
    };
    myChart.setOption(option);
    // 4. 让图表跟随屏幕自动的去适应
    window.addEventListener("resize", function () {
        myChart.resize();
    });
})();


// 货位占用率
(function () {
    //{\"LocStored\":28,\"LocEmpty\":29}
    var curLocPerData = sourceData.curLocPerData;

    // 1. 实例化对象
    var myChart = echarts.init(document.querySelector(".pie .chart"));
    // 2.指定配置
    var option = {
        color: ["#065aab", "#066eab", "#0682ab", "#0696ab", "#06a0ab"],
        tooltip: {
            trigger: "item",
            formatter: "{a} <br/>{b}: {c} ({d}%)"
        },

        legend: {
            bottom: "0%",
            // 修改小图标的大小
            itemWidth: 10,
            itemHeight: 10,
            // 修改图例组件的文字为 12px
            textStyle: {
                color: "rgba(255,255,255,.5)",
                fontSize: "12"
            }
        },
        series: [
            {
                name: "货位占用率",
                type: "pie",
                // 这个radius可以修改饼形图的大小
                // radius 第一个值是内圆的半径 第二个值是外圆的半径
                radius: ["40%", "60%"],
                center: ["50%", "45%"],
                avoidLabelOverlap: false,
                // 图形上的文字
                label: {
                    show: false,
                    position: "center"
                },
                // 链接文字和图形的线是否显示
                labelLine: {
                    show: false
                },
                data: [
                    { value: curLocPerData.LocStored, name: "已存储" },
                    { value: curLocPerData.LocEmpty, name: "未存储" }
                ]
            }
        ]
    };

    // 3. 把配置给实例对象
    myChart.setOption(option);
    // 4. 让图表跟随屏幕自动的去适应
    window.addEventListener("resize", function () {
        myChart.resize();
    });
})();



// 当前库存统计
(function () {
    //[{\"Qty\":299,\"ItemCode\":\"Disk\",\"ItemName\":\"硬盘\"},{\"Qty\":198,\"ItemCode\":\"Dsd\",\"ItemName\":\"订书钉\"}
    var curContainerQtyData = sourceData.curContainerQtyData;
    var data = [];
    $.each(curContainerQtyData, function (i, n) {
        data.push({ "name": n.ItemName,"value": n.Qty});
    });
    var myChart = echarts.init(document.querySelector(".pie2 .chart"));
    var option = {
        color: [
            "#006cff",
            "#60cda0",
            "#ed8884",
            "#ff9f7f",
            "#0096ff",
            "#9fe6b8",
            "#32c5e9",
            "#1d9dff"
        ],
        tooltip: {
            trigger: "item",
            formatter: "{a} <br/>{b} : {c} ({d}%)"
        },
        legend: {
            bottom: "0%",
            itemWidth: 10,
            itemHeight: 10,
            textStyle: {
                color: "rgba(255,255,255,.5)",
                fontSize: "12"
            }
        },
        series: [
            {
                name: "库存分布",
                type: "pie",
                radius: ["10%", "70%"],
                center: ["50%", "50%"],
                roseType: "radius",
                // 图形的文字标签
                label: {
                    fontSize: 10
                },
                // 链接图形和文字的线条
                labelLine: {
                    // length 链接图形的线条
                    length: 6,
                    // length2 链接文字的线条
                    length2: 8
                },
                data: data
            }
        ]
    };
    myChart.setOption(option);
    // 监听浏览器缩放，图表对象调用缩放resize函数
    window.addEventListener("resize", function () {
        myChart.resize();
    });
})();


// 模拟飞行路线模块地图模块
(function () {
    var myChart = echarts.init(document.querySelector(".map .chart"));
    var geoCoordMap = {
        上海: [121.4648, 31.2891],
        东莞: [113.8953, 22.901],
        东营: [118.7073, 37.5513],
        中山: [113.4229, 22.478],
        临汾: [111.4783, 36.1615],
        临沂: [118.3118, 35.2936],
        丹东: [124.541, 40.4242],
        丽水: [119.5642, 28.1854],
        乌鲁木齐: [87.9236, 43.5883],
        佛山: [112.8955, 23.1097],
        保定: [115.0488, 39.0948],
        兰州: [103.5901, 36.3043],
        包头: [110.3467, 41.4899],
        北京: [116.4551, 40.2539],
        北海: [109.314, 21.6211],
        南京: [118.8062, 31.9208],
        南宁: [108.479, 23.1152],
        南昌: [116.0046, 28.6633],
        南通: [121.1023, 32.1625],
        厦门: [118.1689, 24.6478],
        台州: [121.1353, 28.6688],
        合肥: [117.29, 32.0581],
        呼和浩特: [111.4124, 40.4901],
        咸阳: [108.4131, 34.8706],
        哈尔滨: [127.9688, 45.368],
        唐山: [118.4766, 39.6826],
        嘉兴: [120.9155, 30.6354],
        大同: [113.7854, 39.8035],
        大连: [122.2229, 39.4409],
        天津: [117.4219, 39.4189],
        太原: [112.3352, 37.9413],
        威海: [121.9482, 37.1393],
        宁波: [121.5967, 29.6466],
        宝鸡: [107.1826, 34.3433],
        宿迁: [118.5535, 33.7775],
        常州: [119.4543, 31.5582],
        广州: [113.5107, 23.2196],
        廊坊: [116.521, 39.0509],
        延安: [109.1052, 36.4252],
        张家口: [115.1477, 40.8527],
        徐州: [117.5208, 34.3268],
        德州: [116.6858, 37.2107],
        惠州: [114.6204, 23.1647],
        成都: [103.9526, 30.7617],
        扬州: [119.4653, 32.8162],
        承德: [117.5757, 41.4075],
        拉萨: [91.1865, 30.1465],
        无锡: [120.3442, 31.5527],
        日照: [119.2786, 35.5023],
        昆明: [102.9199, 25.4663],
        杭州: [119.5313, 29.8773],
        枣庄: [117.323, 34.8926],
        柳州: [109.3799, 24.9774],
        株洲: [113.5327, 27.0319],
        武汉: [114.3896, 30.6628],
        汕头: [117.1692, 23.3405],
        江门: [112.6318, 22.1484],
        沈阳: [123.1238, 42.1216],
        沧州: [116.8286, 38.2104],
        河源: [114.917, 23.9722],
        泉州: [118.3228, 25.1147],
        泰安: [117.0264, 36.0516],
        泰州: [120.0586, 32.5525],
        济南: [117.1582, 36.8701],
        济宁: [116.8286, 35.3375],
        海口: [110.3893, 19.8516],
        淄博: [118.0371, 36.6064],
        淮安: [118.927, 33.4039],
        深圳: [114.5435, 22.5439],
        清远: [112.9175, 24.3292],
        温州: [120.498, 27.8119],
        渭南: [109.7864, 35.0299],
        湖州: [119.8608, 30.7782],
        湘潭: [112.5439, 27.7075],
        滨州: [117.8174, 37.4963],
        潍坊: [119.0918, 36.524],
        烟台: [120.7397, 37.5128],
        玉溪: [101.9312, 23.8898],
        珠海: [113.7305, 22.1155],
        盐城: [120.2234, 33.5577],
        盘锦: [121.9482, 41.0449],
        石家庄: [114.4995, 38.1006],
        福州: [119.4543, 25.9222],
        秦皇岛: [119.2126, 40.0232],
        绍兴: [120.564, 29.7565],
        聊城: [115.9167, 36.4032],
        肇庆: [112.1265, 23.5822],
        舟山: [122.2559, 30.2234],
        苏州: [120.6519, 31.3989],
        莱芜: [117.6526, 36.2714],
        菏泽: [115.6201, 35.2057],
        营口: [122.4316, 40.4297],
        葫芦岛: [120.1575, 40.578],
        衡水: [115.8838, 37.7161],
        衢州: [118.6853, 28.8666],
        西宁: [101.4038, 36.8207],
        西安: [109.1162, 34.2004],
        贵阳: [106.6992, 26.7682],
        连云港: [119.1248, 34.552],
        邢台: [114.8071, 37.2821],
        邯郸: [114.4775, 36.535],
        郑州: [113.4668, 34.6234],
        鄂尔多斯: [108.9734, 39.2487],
        重庆: [107.7539, 30.1904],
        金华: [120.0037, 29.1028],
        铜川: [109.0393, 35.1947],
        银川: [106.3586, 38.1775],
        镇江: [119.4763, 31.9702],
        长春: [125.8154, 44.2584],
        长沙: [113.0823, 28.2568],
        长治: [112.8625, 36.4746],
        阳泉: [113.4778, 38.0951],
        青岛: [120.4651, 36.3373],
        韶关: [113.7964, 24.7028]
    };

    var XAData = [
        [{ name: "西安" }, { name: "拉萨", value: 100 }],
        [{ name: "西安" }, { name: "上海", value: 100 }],
        [{ name: "西安" }, { name: "广州", value: 100 }],
        [{ name: "西安" }, { name: "西宁", value: 100 }],
        [{ name: "西安" }, { name: "银川", value: 100 }]
    ];

    var XNData = [
        [{ name: "西宁" }, { name: "北京", value: 100 }],
        [{ name: "西宁" }, { name: "上海", value: 100 }],
        [{ name: "西宁" }, { name: "广州", value: 100 }],
        [{ name: "西宁" }, { name: "西安", value: 100 }],
        [{ name: "西宁" }, { name: "银川", value: 100 }]
    ];

    var YCData = [
        [{ name: "拉萨" }, { name: "潍坊", value: 100 }],
        [{ name: "拉萨" }, { name: "哈尔滨", value: 100 }],
        [{ name: "银川" }, { name: "上海", value: 100 }],
        [{ name: "银川" }, { name: "西安", value: 100 }],
        [{ name: "银川" }, { name: "西宁", value: 100 }]
    ];

    var planePath =
        "path://M1705.06,1318.313v-89.254l-319.9-221.799l0.073-208.063c0.521-84.662-26.629-121.796-63.961-121.491c-37.332-0.305-64.482,36.829-63.961,121.491l0.073,208.063l-319.9,221.799v89.254l330.343-157.288l12.238,241.308l-134.449,92.931l0.531,42.034l175.125-42.917l175.125,42.917l0.531-42.034l-134.449-92.931l12.238-241.308L1705.06,1318.313z";
    //var planePath = 'arrow';
    var convertData = function (data) {
        var res = [];
        for (var i = 0; i < data.length; i++) {
            var dataItem = data[i];

            var fromCoord = geoCoordMap[dataItem[0].name];
            var toCoord = geoCoordMap[dataItem[1].name];
            if (fromCoord && toCoord) {
                res.push({
                    fromName: dataItem[0].name,
                    toName: dataItem[1].name,
                    coords: [fromCoord, toCoord],
                    value: dataItem[1].value
                });
            }
        }
        return res;
    };

    var color = ["#a6c84c", "#ffa022", "#46bee9"]; //航线的颜色
    var series = [];
    [
        ["西安", XAData],
        ["西宁", XNData],
        ["银川", YCData]
    ].forEach(function (item, i) {
        series.push(
            {
                name: item[0] + " Top3",
                type: "lines",
                zlevel: 1,
                effect: {
                    show: true,
                    period: 6,
                    trailLength: 0.7,
                    color: "red", //arrow箭头的颜色
                    symbolSize: 3
                },
                lineStyle: {
                    normal: {
                        color: color[i],
                        width: 0,
                        curveness: 0.2
                    }
                },
                data: convertData(item[1])
            },
            {
                name: item[0] + " Top3",
                type: "lines",
                zlevel: 2,
                symbol: ["none", "arrow"],
                symbolSize: 10,
                effect: {
                    show: true,
                    period: 6,
                    trailLength: 0,
                    symbol: planePath,
                    symbolSize: 15
                },
                lineStyle: {
                    normal: {
                        color: color[i],
                        width: 1,
                        opacity: 0.6,
                        curveness: 0.2
                    }
                },
                data: convertData(item[1])
            },
            {
                name: item[0] + " Top3",
                type: "effectScatter",
                coordinateSystem: "geo",
                zlevel: 2,
                rippleEffect: {
                    brushType: "stroke"
                },
                label: {
                    normal: {
                        show: true,
                        position: "right",
                        formatter: "{b}"
                    }
                },
                symbolSize: function (val) {
                    return val[2] / 8;
                },
                itemStyle: {
                    normal: {
                        color: color[i]
                    },
                    emphasis: {
                        areaColor: "#2B91B7"
                    }
                },
                data: item[1].map(function (dataItem) {
                    return {
                        name: dataItem[1].name,
                        value: geoCoordMap[dataItem[1].name].concat([dataItem[1].value])
                    };
                })
            }
        );
    });
    var option = {
        tooltip: {
            trigger: "item",
            formatter: function (params, ticket, callback) {
                if (params.seriesType == "effectScatter") {
                    return "线路：" + params.data.name + "" + params.data.value[2];
                } else if (params.seriesType == "lines") {
                    return (
                        params.data.fromName +
                        ">" +
                        params.data.toName +
                        "<br />" +
                        params.data.value
                    );
                } else {
                    return params.name;
                }
            }
        },
        legend: {
            orient: "vertical",
            top: "bottom",
            left: "right",
            data: ["西安 Top3", "西宁 Top3", "银川 Top3"],
            textStyle: {
                color: "#fff"
            },
            selectedMode: "multiple"
        },
        geo: {
            map: "china",
            label: {
                emphasis: {
                    show: true,
                    color: "#fff"
                }
            },
            // 把中国地图放大了1.2倍
            zoom: 1.2,
            roam: true,
            itemStyle: {
                normal: {
                    // 地图省份的背景颜色
                    areaColor: "rgba(20, 41, 87,0.6)",
                    borderColor: "#195BB9",
                    borderWidth: 1
                },
                emphasis: {
                    areaColor: "#2B91B7"
                }
            }
        },
        series: series
    };
    myChart.setOption(option);
    // 监听浏览器缩放，图表对象调用缩放resize函数
    window.addEventListener("resize", function () {
        myChart.resize();
    });
})();
