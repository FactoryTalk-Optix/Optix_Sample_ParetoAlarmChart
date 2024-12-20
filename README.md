# Alarm History and Pareto Visualization

This project allows you to analyze and visualize alarm history by executing SQL queries on an event table, extracting data related to alarms with "Active" status, and creating a Pareto chart to highlight the frequency of active alarms over time. The project combines SQL, Netlogic and Apache ECharts to create an interactive dashboard that can also be used offline by loading local scripts.

## Features

- **Active Alarm Count**: The SQL query selects alarms with the "Active_Id" status and counts how many times each alarm occurred, grouped by alarm type.
- **Data Visualization**: The extracted data is displayed in a table and used to generate a Pareto chart representing the distribution of active alarms.
- **Pareto Chart with Apache ECharts**: The Pareto chart shows the incidence of active alarms' frequency, using Apache ECharts for a dynamic, interactive visualization.
- **Offline Mode**: The application loads the ECharts JavaScript files locally, allowing the chart to be generated even without an internet connection.

## How It Works

### 1. SQL Query

The project relies on the following SQL query to extract and count the active alarms:

```sql
SELECT ConditionName, COUNT(*) AS ActiveCount FROM {#tableName:sql_identifier} 
WHERE ActiveState_Id=1 AND LocalTime BETWEEN {#from:sql_literal} AND {#to:sql_literal} 
GROUP BY ConditionName 
ORDER BY ActiveCount DESC
```

### 2. Table View

The query results are displayed in a table that shows the number of active alarms for each `ConditionName`. Below is an example of how the output from the query might look.

| ConditionName | ActiveCount |
|---------------|-------------|
| 2             | 40          |
| 1             | 25          |
| 3             | 15          |

### 3. Pareto Chart

The Pareto chart is generated using Apache ECharts, showing the distribution of active alarms visually. The y-axis represents the frequency of alarms, while the x-axis displays the different alarm types.

>[!WARNING]
>*The ECharts JavaScript file is loaded locally from the project, not from an online resource. This ensures the chart can be generated even without an internet connection, but it also means that future versions of ECharts may not be included. Ensure the JS file is updated if necessary.*

---

### ParetoChartLogic Class

The `ParetoChartLogic` class is responsible for generating a Pareto chart using alarm data. It generates an HTML file with embedded JavaScript to render the chart using the ECharts library.

### `RefreshGraph()`

Refreshes the graph by generating a new HTML file and updating the web browser URL.

### `GenerateHtmlFile(string fullPath, bool darkMode)`

Generates the HTML file for the Pareto chart.

### `GenerateHtmlHead(ref StringBuilder newHtmlFile)`

Generates the HTML head section.

### `GenerateData(out string alarmNames, out string barValues, out string lineValues, out int maxCount)`

Generates the data for the Pareto chart read from the database.
