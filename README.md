# Summary
This repo showcases how to read the async operation table and deliver to Application Insights.

# Example Kusto Query
customEvents
|extend cd = parse_json(customDimensions)
|project JobName_Status = strcat(name , " - ", tostring(cd.StatusName)), toint(cd.StatusCount), timestamp
//|where JobName_Status contains "Retrieve of ayw_"
|render timechart