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
        ResourceUri pathHtml = ResourceUri.FromProjectRelativePath(@"eCharts\pareto.html");
        GenerateHtmlFile(pathHtml.Uri, Owner.GetVariable("DarkMode").Value);
        WebBrowser mainWebBrowser = (WebBrowser) Owner;
        mainWebBrowser.URL = pathHtml;
        mainWebBrowser.Refresh();
    }

    private void GenerateHtmlFile(string fullPath, bool darkMode)
    {
        StringBuilder newHtmlFile = new StringBuilder();
        GenerateHtmlHead(ref newHtmlFile);
        GenerateData(out string alarmNames, out string barValues, out string lineValues, out int maxCount);
        string darkModeParameter = darkMode ? "'dark'" : "null";
        _ = newHtmlFile.AppendLine("<body style=\"height: 100%; margin: 0\">");
        _ = newHtmlFile.AppendLine("<div id=\"container\" style=\"height: 100%\"></div>");
        _ = newHtmlFile.AppendLine("<script type=\"text/javascript\">");
        // Javascript
        _ = newHtmlFile.AppendLine("var dom = document.getElementById('container');");
        _ = newHtmlFile.AppendLine($"var myChart = echarts.init(dom, {darkModeParameter}, {{ renderer: 'svg' }});");
        _ = newHtmlFile.AppendLine("// Specify the configuration items and data for the chart");
        _ = newHtmlFile.AppendLine("var option = {");
        _ = newHtmlFile.AppendLine("tooltip: {");
        _ = newHtmlFile.AppendLine("trigger: 'axis',");
        _ = newHtmlFile.AppendLine("axisPointer: {");
        _ = newHtmlFile.AppendLine("type: 'cross',");
        _ = newHtmlFile.AppendLine("crossStyle: {");
        _ = newHtmlFile.AppendLine("color: '#999'");
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("},");
        _ = newHtmlFile.AppendLine("legend: {");
        _ = newHtmlFile.AppendLine("data: ['Count', 'Incidence']");
        _ = newHtmlFile.AppendLine("},");
        _ = newHtmlFile.AppendLine("xAxis: {");
        _ = newHtmlFile.AppendLine("type: 'category',");
        _ = newHtmlFile.AppendLine("data: " + alarmNames + ",");
        _ = newHtmlFile.AppendLine("axisPointer: {");
        _ = newHtmlFile.AppendLine("type: 'shadow'");
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("},");
        _ = newHtmlFile.AppendLine("yAxis: [");
        _ = newHtmlFile.AppendLine("{");
        _ = newHtmlFile.AppendLine("type: 'value',");
        _ = newHtmlFile.AppendLine("name: 'Count',");
        _ = newHtmlFile.AppendLine("min: 0,");
        _ = newHtmlFile.AppendLine($"max: {maxCount + 10},");
        _ = newHtmlFile.AppendLine("interval: 10,");
        _ = newHtmlFile.AppendLine("axisLabel: {");
        _ = newHtmlFile.AppendLine("formatter: '{value}'");
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("},");
        _ = newHtmlFile.AppendLine("{");
        _ = newHtmlFile.AppendLine("type: 'value',");
        _ = newHtmlFile.AppendLine("name: 'Incidence',");
        _ = newHtmlFile.AppendLine("min: 0,");
        _ = newHtmlFile.AppendLine("max: 100,");
        _ = newHtmlFile.AppendLine("interval: 25,");
        _ = newHtmlFile.AppendLine("axisLabel: {");
        _ = newHtmlFile.AppendLine("formatter: '{value} %'");
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("],");
        _ = newHtmlFile.AppendLine("series: [");
        _ = newHtmlFile.AppendLine("{");
        _ = newHtmlFile.AppendLine("name: 'Count',");
        _ = newHtmlFile.AppendLine("type: 'bar',");
        _ = newHtmlFile.AppendLine("data: " + barValues);
        _ = newHtmlFile.AppendLine("},");
        _ = newHtmlFile.AppendLine("{");
        _ = newHtmlFile.AppendLine("name: 'Incidence',");
        _ = newHtmlFile.AppendLine("type: 'line',");
        _ = newHtmlFile.AppendLine("yAxisIndex: 1,");
        _ = newHtmlFile.AppendLine("tooltip: {");
        _ = newHtmlFile.AppendLine("valueFormatter: function (value) {");
        _ = newHtmlFile.AppendLine("return value + ' %';");
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("},");
        _ = newHtmlFile.AppendLine("data: " + lineValues);
        _ = newHtmlFile.AppendLine("}");
        _ = newHtmlFile.AppendLine("]");
        _ = newHtmlFile.AppendLine("};");
        _ = newHtmlFile.AppendLine("myChart.setOption(option);");
        _ = newHtmlFile.AppendLine("window.addEventListener('resize', myChart.resize);");
        _ = newHtmlFile.AppendLine("</script>");
        _ = newHtmlFile.AppendLine("</body>");
        _ = newHtmlFile.AppendLine("</html>");
        System.IO.File.WriteAllText(fullPath, newHtmlFile.ToString());
    }

    private void GenerateHtmlHead(ref StringBuilder newHtmlFile)
    {
        _ = newHtmlFile.AppendLine("<!DOCTYPE html>");
        _ = newHtmlFile.AppendLine("<html lang=\"en\" style=\"height: 100%\">");
        _ = newHtmlFile.AppendLine("<head>");
        _ = newHtmlFile.AppendLine("<meta charset=\"utf-8\" />");
        _ = newHtmlFile.AppendLine("<script src=\"echarts.js\"></script>");
        _ = newHtmlFile.AppendLine("</head>");
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
