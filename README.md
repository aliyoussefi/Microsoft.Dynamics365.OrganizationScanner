# Summary
This repo showcases how to read the async operation table and deliver to Application Insights.

# How to Deploy
1. Clone this repo.
2. Open solution in Visual Studio.
3. Right-click on Microsoft.Dynamics365.OrganizationScanner project.
4. Select Publish.
5. Go through steps to push to Azure Function.

# Example Kusto Query for Async Operations
customEvents
|extend cd = parse_json(customDimensions)
|project JobName_Status = strcat(name , " - ", tostring(cd.StatusName)), toint(cd.StatusCount), timestamp
//|where JobName_Status contains "Retrieve of ayw_"
|render timechart

# Example Kusto Query to Monitor for Exceptions on SQL Query
dependencies 
| where type == 'SQL' and success == false