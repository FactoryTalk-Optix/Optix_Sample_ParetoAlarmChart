#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.EventLogger;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.Alarm;
using System.Text;
using System.Net.Mail;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FTOptix.WebUI;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using FTOptix.ODBCStore;
using FTOptix.InfluxDBStore;
using FTOptix.OPCUAServer;
using FTOptix.OPCUAClient;
using FTOptix.S7TiaProfinet;
using FTOptix.S7TCP;
#endregion

public class ParetoChartLogic : BaseNetLogic
{
    public override void Start()
    {
        Owner.GetVariable("From").Value = DateTime.Now.AddDays(-7);
        Owner.GetVariable("To").Value = DateTime.Now;
        RefreshGraph();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void RefreshGraph()
    {
        var pathHtml = ResourceUri.FromProjectRelativePath("eCharts/pareto.html");
        var pathJs = ResourceUri.FromProjectRelativePath("eCharts/paretoData.js");
        // Need to generate the JavaScript with Pareto chart script, for avoid issue on WebPresentationEngine
        GenerateJavaScriptFile(pathJs.Uri, Owner.GetVariable("DarkMode").Value);
        GenerateHtmlFile(pathHtml.Uri);
        var mainWebBrowser = (WebBrowser) Owner;
        mainWebBrowser.URL = pathHtml;
        mainWebBrowser.Refresh();
    }

    private static void GenerateHtmlFile(string fullPath)
    {
        var newHtmlFile = new StringBuilder();
        _ = newHtmlFile.AppendLine("<!DOCTYPE html>");
        _ = newHtmlFile.AppendLine("<html lang=\"en\" style=\"height: 100%\">");
        _ = newHtmlFile.AppendLine("<head>");
        _ = newHtmlFile.AppendLine("<meta charset=\"utf-8\">");
        _ = newHtmlFile.AppendLine("</head>");
        _ = newHtmlFile.AppendLine("<body style=\"height: 100%; margin: 0\">");
        _ = newHtmlFile.AppendLine("<div id=\"container\" style=\"height: 100%; font: Share Tech Mono;\"></div>");
        _ = newHtmlFile.AppendLine("<script type=\"text/javascript\" src=\"./echarts.js\"></script>");
        _ = newHtmlFile.AppendLine("<script type=\"text/javascript\" src=\"./paretoData.js\"></script>");
        _ = newHtmlFile.AppendLine("</body>");
        _ = newHtmlFile.AppendLine("</html>");
        System.IO.File.WriteAllText(fullPath, newHtmlFile.ToString());
    }

    private void GenerateJavaScriptFile(string filePath, bool darkMode)
    {
        StringBuilder jsFileContent = new();
        GenerateData(out string alarmNames, out string barValues, out string lineValues, out int maxCount);
        string darkModeParameter = darkMode ? "'dark'" : "null";

        _ = jsFileContent.AppendLine("document.addEventListener('DOMContentLoaded', function() {");
        _ = jsFileContent.AppendLine("    var dom = document.getElementById('container');");
        _ = jsFileContent.AppendLine($"    var myChart = echarts.init(dom, {darkModeParameter}, {{ renderer: 'svg' }});");
        _ = jsFileContent.AppendLine("    var option = {");
        _ = jsFileContent.AppendLine("        tooltip: {");
        _ = jsFileContent.AppendLine("            trigger: 'axis',");
        _ = jsFileContent.AppendLine("            axisPointer: {");
        _ = jsFileContent.AppendLine("                type: 'cross',");
        _ = jsFileContent.AppendLine("                crossStyle: {");
        _ = jsFileContent.AppendLine("                    color: '#999'");
        _ = jsFileContent.AppendLine("                }");
        _ = jsFileContent.AppendLine("            }");
        _ = jsFileContent.AppendLine("        },");
        GenerateJavaParetoData(ref jsFileContent, alarmNames, barValues, lineValues, maxCount);
        _ = jsFileContent.AppendLine("    };");
        _ = jsFileContent.AppendLine("    myChart.setOption(option);");
        _ = jsFileContent.AppendLine("    window.addEventListener('resize', myChart.resize);");
        _ = jsFileContent.AppendLine("});");

        System.IO.File.WriteAllText(filePath, jsFileContent.ToString());
    }

    private static void GenerateJavaParetoData(ref StringBuilder jsFileContent, string alarmNames, string barValues, string lineValues, int maxCount)
    {
        _ = jsFileContent.AppendLine("        legend: {");
        _ = jsFileContent.AppendLine("            data: ['Count', 'Incidence']");
        _ = jsFileContent.AppendLine("        },");
        _ = jsFileContent.AppendLine("        xAxis: {");
        _ = jsFileContent.AppendLine("            type: 'category',");
        _ = jsFileContent.AppendLine("            data: " + alarmNames + ",");
        _ = jsFileContent.AppendLine("            axisPointer: {");
        _ = jsFileContent.AppendLine("                type: 'shadow'");
        _ = jsFileContent.AppendLine("            }");
        _ = jsFileContent.AppendLine("        },");
        _ = jsFileContent.AppendLine("        yAxis: [");
        _ = jsFileContent.AppendLine("            {");
        _ = jsFileContent.AppendLine("                type: 'value',");
        _ = jsFileContent.AppendLine("                name: 'Count',");
        _ = jsFileContent.AppendLine("                min: 0,");
        _ = jsFileContent.AppendLine($"                max: {maxCount + 10},");
        _ = jsFileContent.AppendLine("                interval: 10,");
        _ = jsFileContent.AppendLine("                axisLabel: {");
        _ = jsFileContent.AppendLine("                    formatter: '{value}'");
        _ = jsFileContent.AppendLine("                }");
        _ = jsFileContent.AppendLine("            },");
        _ = jsFileContent.AppendLine("            {");
        _ = jsFileContent.AppendLine("                type: 'value',");
        _ = jsFileContent.AppendLine("                name: 'Incidence',");
        _ = jsFileContent.AppendLine("                min: 0,");
        _ = jsFileContent.AppendLine("                max: 100,");
        _ = jsFileContent.AppendLine("                interval: 25,");
        _ = jsFileContent.AppendLine("                axisLabel: {");
        _ = jsFileContent.AppendLine("                    formatter: '{value} %'");
        _ = jsFileContent.AppendLine("                }");
        _ = jsFileContent.AppendLine("            }");
        _ = jsFileContent.AppendLine("        ],");
        _ = jsFileContent.AppendLine("        series: [");
        _ = jsFileContent.AppendLine("            {");
        _ = jsFileContent.AppendLine("                name: 'Count',");
        _ = jsFileContent.AppendLine("                type: 'bar',");
        _ = jsFileContent.AppendLine("                data: " + barValues);
        _ = jsFileContent.AppendLine("            },");
        _ = jsFileContent.AppendLine("            {");
        _ = jsFileContent.AppendLine("                name: 'Incidence',");
        _ = jsFileContent.AppendLine("                type: 'line',");
        _ = jsFileContent.AppendLine("                yAxisIndex: 1,");
        _ = jsFileContent.AppendLine("                tooltip: {");
        _ = jsFileContent.AppendLine("                    valueFormatter: function (value) {");
        _ = jsFileContent.AppendLine("                        return value + ' %';");
        _ = jsFileContent.AppendLine("                    }");
        _ = jsFileContent.AppendLine("                },");
        _ = jsFileContent.AppendLine("                data: " + lineValues);
        _ = jsFileContent.AppendLine("            }");
        _ = jsFileContent.AppendLine("        ]");
    }

    private static string GenerateNonce()
    {
        byte[] nonceBytes = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(nonceBytes);
        return Convert.ToBase64String(nonceBytes);
    }

    public void GenerateData(out string alarmNames, out string barValues, out string lineValues, out int maxCount)
    {
        alarmNames = string.Empty;
        barValues = string.Empty;
        lineValues = string.Empty;
        maxCount = 0;
        Store myStore = InformationModel.Get<Store>(Owner.GetVariable("AlarmHistoryStore").Value);
        string querySQL = Owner.GetVariable("QueryData").Value;
        if (myStore != null)
        {
            myStore.Query(querySQL, out _, out object[,] queryResult);
            if (queryResult.GetLength(0) > 0)
            {
                List<object> returnRange = Enumerable.Range(0, queryResult.GetLength(0)).Select(x => queryResult[x, 0]).ToList();
                List<AlarmData> alarmDataList = new List<AlarmData>();
                foreach (object distinctAlarms in returnRange.Distinct())
                {
                    int alarmCount = returnRange.Count(x => x.ToString().Equals(distinctAlarms.ToString()));
                    alarmDataList.Add(new AlarmData()
                    {
                        AlarmName = distinctAlarms.ToString(),
                        Count = alarmCount
                    });
                }
                alarmDataList = alarmDataList.OrderByDescending(a => a.Count).ToList();
                maxCount = alarmDataList[0].Count;
                int totalAlarms = alarmDataList.Sum(a => a.Count);
                int cumulativeCount = 0;
                StringBuilder alarmNamesBuilder = new StringBuilder();
                StringBuilder barValuesBuilder = new StringBuilder();
                StringBuilder lineValuesBuilder = new StringBuilder();
                _ = alarmNamesBuilder.Append('[');
                _ = barValuesBuilder.Append('[');
                _ = lineValuesBuilder.Append('[');
                foreach (AlarmData alarmData in alarmDataList)
                {
                    cumulativeCount += alarmData.Count;
                    alarmData.CumulativePercentage = Math.Round((double) cumulativeCount / totalAlarms * 100, 0);
                    _ = alarmNamesBuilder.Append($"'{alarmData.AlarmName}'");
                    _ = barValuesBuilder.Append($"'{alarmData.Count}'");
                    _ = lineValuesBuilder.Append($"'{alarmData.CumulativePercentage}'");
                    if (!alarmData.Equals(alarmDataList[^1]))
                    {
                        _ = alarmNamesBuilder.Append(", ");
                        _ = barValuesBuilder.Append(", ");
                        _ = lineValuesBuilder.Append(", ");
                    }
                }
                _ = alarmNamesBuilder.Append(']');
                _ = barValuesBuilder.Append(']');
                _ = lineValuesBuilder.Append(']');
                alarmNames = alarmNamesBuilder.ToString();
                barValues = barValuesBuilder.ToString();
                lineValues = lineValuesBuilder.ToString();
            }
        }
    }

    sealed class AlarmData
    {
        public string AlarmName { get; set; }
        public int Count { get; set; }
        public double CumulativePercentage { get; set; }
    }
}