using ChartJS.Helpers.MVC;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PraiseCMS.Web.Helpers
{
    public static class ReportGenerator
    {
        public const string borderColorZero = ChartColors.Red;
        public const string borderColorOne = ChartColors.Blue;
        public const string borderColorTwo = ChartColors.Green;
        public const string borderColorThree = ChartColors.Yellow;
        public const string borderColorFour = ChartColors.Purple;
        public const string borderColorFive = ChartColors.Black;

        public static MvcHtmlString GenerateChart(List<Campus> campuses, List<ReportViewModel> reportData, string reportType)
        {
            var count = 0;

            var reportBuilder = new ReportBuilder
            {
                XAxisTitle = string.Empty,
                YAxisTitle = string.Empty
            };

            foreach (var item in reportData)
            {
                var dataSet = new ReportDataSet
                {
                    BorderWidth = "2"
                };

                switch (count)
                {
                    case 0:
                        dataSet.BorderColor = borderColorZero;
                        dataSet.BackgroundColor = borderColorZero;
                        break;
                    case 1:
                        dataSet.BorderColor = borderColorOne;
                        dataSet.BackgroundColor = borderColorOne;
                        break;
                    case 2:
                        dataSet.BorderColor = borderColorTwo;
                        dataSet.BackgroundColor = borderColorTwo;
                        break;
                    case 3:
                        dataSet.BorderColor = borderColorThree;
                        dataSet.BackgroundColor = borderColorThree;
                        break;
                    case 4:
                        dataSet.BorderColor = borderColorFour;
                        dataSet.BackgroundColor = borderColorFour;
                        break;
                    case 5:
                        dataSet.BorderColor = borderColorFive;
                        dataSet.BackgroundColor = borderColorFive;
                        break;
                    default:
                        dataSet.BorderColor = borderColorZero;
                        dataSet.BackgroundColor = borderColorZero;
                        break;
                }
                count++;

                reportBuilder.XAxisLabels.Add(item.Title);
                reportBuilder.YAxisLabels.Add(item.Total.ToString());

                dataSet.LinearData.Add(item.Total);
                reportBuilder.ReportDataSets.Add(dataSet);

                reportBuilder.Title = reportType;
            }

            var pieChart = ReportHelper.GetWidgetPieChart(reportBuilder);

            return new MvcHtmlString(pieChart.Draw("PieChart_" + Utilities.GenerateUniqueId()));
        }

        public static MvcHtmlString GenerateChartForCustomReport(CustomReportBuilder data)
        {
            var Label = data.Tab;
            var count = 0;

            var reportBuilder = new ReportBuilder();
            var dataSetList = new List<LineDataSets>();

            var dateRage = Utilities.GetDateRange(data.StartDate, data.EndDate);
            var date = dateRage.ToList().ToShortDate().Distinct();

            reportBuilder.XAxisLabels = data.XAxisColumns;
            reportBuilder.YAxisLabels = data.YAxisColumns;
            reportBuilder.XAxisTitle = data.XAxisTitle;
            reportBuilder.YAxisTitle = data.YAxisTitle;
            reportBuilder.YMultiAxisTitle = data.YMultiAxisTitle;

            foreach (var label in data.DataSetLabels)
            {
                var dataSet = new ReportDataSet
                {
                    Label = label,
                    BorderWidth = "2"
                };

                switch (count)
                {
                    case 0:
                        dataSet.BorderColor = borderColorZero;
                        dataSet.BackgroundColor = borderColorZero;
                        break;
                    case 1:
                        dataSet.BorderColor = borderColorOne;
                        dataSet.BackgroundColor = borderColorOne;
                        break;
                    case 2:
                        dataSet.BorderColor = borderColorTwo;
                        dataSet.BackgroundColor = borderColorTwo;
                        break;
                    case 3:
                        dataSet.BorderColor = borderColorThree;
                        dataSet.BackgroundColor = borderColorThree;
                        break;
                    case 4:
                        dataSet.BorderColor = borderColorFour;
                        dataSet.BackgroundColor = borderColorFour;
                        break;
                    case 5:
                        dataSet.BorderColor = borderColorFive;
                        dataSet.BackgroundColor = borderColorFive;
                        break;
                    default:
                        dataSet.BorderColor = borderColorZero;
                        dataSet.BackgroundColor = borderColorZero;
                        break;
                }
                count++;

                foreach (var dr in dateRage.Select(x => new { x.Month, x.Year }).Distinct().ToList())
                {
                    var linearData = data.Record.Where(x => x.Title == label && x.CreatedDate.Month == dr.Month && x.CreatedDate.Year == dr.Year).ToList();

                    if (linearData.IsNotNull() && linearData.Count > 0)
                    {
                        dataSet.LinearData.Add(linearData.Sum(x => x.Total));
                        dataSet.GivingCounts.Add(linearData.Count);
                    }
                    else
                    {
                        dataSet.LinearData.Add(0);
                        dataSet.GivingCounts.Add(0);
                    }
                }

                reportBuilder.ReportDataSets.Add(dataSet);
            }

            if (data.GraphType == GraphTypes.LineGraph)
            {
                var lineGraph = ReportHelper.GetLineChart(reportBuilder);
                return new MvcHtmlString(lineGraph.Draw("canvas", "100%", "100%"));
            }

            if (data.GraphType == GraphTypes.MultiAxisLineGraph)
            {
                var multiAxisLineGraph = ReportHelper.GetMultiAxisLineChart(reportBuilder);
                return new MvcHtmlString(multiAxisLineGraph.Draw("canvas", "100%", "100%"));
            }

            if (data.GraphType == GraphTypes.BarGraph)
            {
                var barGraph = ReportHelper.GetVerticalBarChart(reportBuilder);
                return new MvcHtmlString(barGraph.Draw("canvas", "100%", "100%"));
            }

            if (data.GraphType == GraphTypes.PieChart)
            {
                var pieChart = ReportHelper.GetPieChart(reportBuilder);
                return new MvcHtmlString(pieChart.Draw("canvas", "100%", "100%"));
            }

            if (data.GraphType == GraphTypes.DoughnutChart)
            {
                var doughnutChart = ReportHelper.GetDoughnutChart(reportBuilder);
                return new MvcHtmlString(doughnutChart.Draw("canvas", "100%", "100%"));
            }

            if (data.GraphType == GraphTypes.MixLineBarChart)
            {
                var mixLineBarChart = ReportHelper.GetMixLineBarChart(reportBuilder);
                return new MvcHtmlString(mixLineBarChart.Draw("canvas", "100%", "100%"));
            }

            if (data.GraphType == GraphTypes.AreaGraph)
            {
                var areaChart = ReportHelper.GetAreaChart(reportBuilder);
                return new MvcHtmlString(areaChart.Draw("canvas", "100%", "100%"));
            }

            var chart = ReportHelper.GetVerticalBarChart(reportBuilder);

            return new MvcHtmlString(chart.Draw("canvas", "100%", "100%"));
        }
    }

    public static class ReportHelper
    {
        public static ChartTypeLine GetMultiAxisLineChart(ReportBuilder reportBuilder)
        {
            var dataSetList = new List<LineDataSets>();

            foreach (var item in reportBuilder.ReportDataSets)
            {
                var lineData = new LineDataSets()
                {
                    Label = item.Label,
                    LinearData = item.LinearData.ToArray(),
                    BorderColor = item.BorderColor,
                    BorderWidth = Convert.ToInt32(item.BorderWidth),
                    YAxisID = "y-axis-1",
                };
                dataSetList.Add(lineData);
            }

            var data = new LineData()
            {
                Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : Utilities.GetMonthForChart(),
                YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                Datasets = dataSetList.ToArray()
            };

            return new ChartTypeLine()
            {
                Data = data,
                Options = new LineOptions()
                {
                    Scales = new ChartOptionsScales()
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes()
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes()
                            {
                                Position = ConstantPosition.LEFT,
                                Id = "y-axis-1",
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            },
                            new ChartOptionsScalesAxes()
                            {
                               Position = ConstantPosition.RIGHT,
                                Id = "y-axis-2",
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YMultiAxisTitle
                                },
                                GridLines = new ChartOptionsScaleGridLines(){ DrawOnChartArea = false}
                            }
                        }
                    },
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title }
                    },
                    Legend = new ChartOptionsLegend()
                    {
                        Position = ConstantPosition.TOP,
                    },
                    Tooltips = new ChartOptionsTooltip()
                    {
                        Mode = ConstantMode.INDEX,
                        Intersect = false
                    },
                    Hover = new ChartOptionsHover()
                    {
                        Mode = ConstantMode.NEAREST,
                        Intersect = true
                    },
                    ShowLines = true
                },
            };
        }
        public static ChartTypePolarArea GetAreaChart(ReportBuilder reportBuilder)
        {
            var dataSetList = new List<PolarAreaDataSets>();

            foreach (var item in reportBuilder.ReportDataSets)
            {
                var lineData = new PolarAreaDataSets()
                {
                    Label = item.Label,
                    BackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    BorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    HoverBackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    HoverBorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    HoverBorderWidth = new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3 },
                    LinearData = item.LinearData.ToArray()
                };
                dataSetList.Add(lineData);
            }

            return new ChartTypePolarArea()
            {
                Data = new PolarAreaData()
                {
                    Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : Utilities.GetMonthForChart(),
                    YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                    Datasets = dataSetList.ToArray(),
                },
                Options = new PolarAreaOptions()
                {
                    Scales = new ChartOptionsScales()
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            }
                        }
                    },
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title }
                    },
                    Scale = new ChartOptionsScale()
                    {
                        Ticks = new ChartOptionsScaleTicks() { BeginAtZero = true }
                    }
                }
            };
        }

        public static ChartTypeLine GetLineChart(ReportBuilder reportBuilder)
        {
            var dataSetList = new List<LineDataSets>();

            foreach (var item in reportBuilder.ReportDataSets)
            {
                var lineData = new LineDataSets()
                {
                    Label = item.Label,
                    LinearData = item.LinearData.ToArray(),
                    BorderColor = item.BorderColor,
                    BorderWidth = Convert.ToInt32(item.BorderWidth)
                };
                dataSetList.Add(lineData);
            }

            var data = new LineData()
            {
                Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : Utilities.GetMonthForChart(),
                YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                Datasets = dataSetList.ToArray()
            };

            return new ChartTypeLine()
            {
                Data = data,
                Options = new LineOptions()
                {
                    Scales = new ChartOptionsScales()
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes()
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes()
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            }
                        }
                    },
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title }
                    },
                    Legend = new ChartOptionsLegend()
                    {
                        Position = ConstantPosition.TOP,
                    },
                    Tooltips = new ChartOptionsTooltip()
                    {
                        Mode = ConstantMode.INDEX,
                        Intersect = false
                    },
                    Hover = new ChartOptionsHover()
                    {
                        Mode = ConstantMode.NEAREST,
                        Intersect = true
                    },
                    ShowLines = true
                },
            };
        }

        public static ChartTypeBar GetVerticalBarChart(ReportBuilder reportBuilder)
        {
            var dataSetList = new List<BarDataSets>();

            foreach (var item in reportBuilder.ReportDataSets)
            {
                var barData = new BarDataSets()
                {
                    Label = item.Label,
                    LinearData = item.LinearData.ToArray(),
                    BorderColor = item.BorderColor,
                    BackgroundColor = item.BorderColor,
                    BorderWidth = Convert.ToInt32(item.BorderWidth)
                };
                dataSetList.Add(barData);
            }

            return new ChartTypeBar()
            {
                Data = new BarData()
                {
                    Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : Utilities.GetMonthForChart(),
                    YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                    Datasets = dataSetList.ToArray()
                },
                Options = new BarOptions()
                {
                    Scales = new ChartOptionsScales()
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            }
                        }
                    },
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title }
                    },
                    Legend = new ChartOptionsLegend()
                    {
                        Position = ConstantPosition.TOP,
                    }
                }
            };
        }

        public static ChartTypePie GetPieChart(ReportBuilder reportBuilder)
        {
            var linearData = new List<int>();
            foreach (var item in reportBuilder.ReportDataSets)
            {
                if (item.LinearData.Count > 0)
                {
                    linearData.Add(Convert.ToInt32(item.LinearData[0].ToString()));
                }
            }

            return new ChartTypePie()
            {
                Data = new PieData()
                {
                    Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : Utilities.GetMonthForChart(),
                    YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                    Datasets = new[]
                    {
                            new PieDataSets()
                            {
                                Label = "My First dataset",
                                BackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                BorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                HoverBackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                HoverBorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                LinearData = linearData.ToArray()
                            }
                    }
                },
                Options = new PieOptions()
                {
                    Scales = new ChartOptionsScales()
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = false,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = false,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            }
                        }
                    },
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title }
                    }
                }
            };
        }

        public static ChartTypeDoughnut GetDoughnutChart(ReportBuilder reportBuilder)
        {
            var dataSetList = new List<DoughnutDataSets>();

            foreach (var item in reportBuilder.ReportDataSets)
            {
                var lineData = new DoughnutDataSets()
                {
                    Label = item.Label,
                    BackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    BorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    HoverBackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    HoverBorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                    HoverBorderWidth = new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3 },
                    LinearData = item.LinearData.ToArray(),
                };
                dataSetList.Add(lineData);
            }

            return new ChartTypeDoughnut()
            {
                Data = new DoughnutData()
                {
                    Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : Utilities.GetMonthForChart(),
                    YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                    Datasets = dataSetList.ToArray()
                },
                Options = new DoughnutOptions()
                {
                    Scales = new ChartOptionsScales()
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            }
                        }
                    },
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title }
                    },
                    Legend = new ChartOptionsLegend()
                    {
                        Position = ConstantPosition.TOP
                    }
                }
            };
        }

        public static ChartTypeMix GetMixLineBarChart(ReportBuilder reportBuilder)
        {
            var dataSetList = new List<MixDataSets>();

            foreach (var item in reportBuilder.ReportDataSets)
            {
                var lineData = new MixDataSets()
                {
                    Label = item.Label,
                    BackgroundColor = item.BorderColor,
                    BorderColor = item.BorderColor,
                    BorderWidth = 1,
                    LinearData = item.LinearData.ToArray()
                };
                dataSetList.Add(lineData);
            }

            return new ChartTypeMix()
            {
                Data = new MixData()
                {
                    Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : Utilities.GetMonthForChart(),
                    YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                    Datasets = dataSetList.ToArray()
                },
                Options = new BarOptions()
                {
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title }
                    },
                    Legend = new ChartOptionsLegend()
                    {
                        Position = ConstantPosition.TOP,
                    },
                    Tooltips = new ChartOptionsTooltip()
                    {
                        Mode = ConstantMode.INDEX,
                        Intersect = true
                    },
                    Scales = new ChartOptionsScales
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                 Display = true,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = true,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            }
                        }
                    }
                }
            };
        }

        public static ChartTypePie GetWidgetPieChart(ReportBuilder reportBuilder)
        {
            var linearData = new List<int>();

            foreach (var item in reportBuilder.ReportDataSets)
            {
                if (item.LinearData.Count > 0)
                {
                    linearData.Add(Convert.ToInt32(item.LinearData[0].ToString()));
                }
            }

            return new ChartTypePie()
            {
                Data = new PieData()
                {
                    Labels = reportBuilder.XAxisLabels.Count > 0 ? reportBuilder.XAxisLabels.ToArray() : new string[] { },
                    YLabels = reportBuilder.YAxisLabels.Count > 0 ? reportBuilder.YAxisLabels.ToArray() : null,
                    Datasets = new[]
                    {
                            new PieDataSets()
                            {
                                Label = "My First dataset",
                                BackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                BorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                HoverBackgroundColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                HoverBorderColor = new[] { ChartColors.Red, ChartColors.Blue, ChartColors.Orange, ChartColors.Yellow, ChartColors.Green, ChartColors.Purple, ChartColors.Black, ChartColors.Grey, ChartColors.Pink },
                                LinearData = linearData.ToArray()
                            }
                    }
                },
                Options = new PieOptions()
                {
                    Scales = new ChartOptionsScales()
                    {
                        XAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = false,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = false,
                                    LabelString = reportBuilder.XAxisTitle
                                }
                            }
                        },
                        YAxes = new[]
                        {
                            new ChartOptionsScalesAxes
                            {
                                Display = false,
                                ScaleLabel = new ChartScaleLabel()
                                {
                                    Display = false,
                                    LabelString = reportBuilder.YAxisTitle
                                }
                            }
                        }
                    },
                    Title = new ChartOptionsTitle()
                    {
                        Display = true,
                        Text = new[] { reportBuilder.Title },
                        FontColor = "#3f4254",
                        FontSize = 16
                    }
                }
            };
        }
    }
}