# Disclaimer
```javascript
# This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment. 
# THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
# INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
# We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that. 
# You agree: 
# (i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded; 
# (ii) to include a valid copyright notice on Your software product in which the Sample Code is embedded; 
# and (iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, that arise or result from the use or distribution of the Sample Code 
```

# Summary
This repo showcases how to read the async operation table and deliver to Application Insights.

# Architecture
![alt text](./_artifacts/ArchitectureDiagram.JPG "Architecture Diagram")

# How to Deploy
1. Clone this repo.
2. Open solution in Visual Studio.
3. Right-click on Microsoft.Dynamics365.OrganizationScanner project.
4. Select Publish.
5. Go through steps to push to Azure Function.

# Azure Function Settings
"AZURE_STORAGE_CONNECTION_STRING": "<Azure Storage connection string>",
"FUNCTIONS_WORKER_RUNTIME": "dotnet",
"APPINSIGHTS_INSTRUMENTATIONKEY": "<Azure Application Insights connection string>",
"SQL_CONNECTION_STRING": "<Dataverse SQL replica connection string>",
"TENANT_ID": "<Azure AD Tenant Id>",
"ORG_URL": "https://<organization>.crm.dynamics.com",
"CLIENT_ID": "<S2S User Client Id>",
"CLIENT_SECRET": "<S2S User Client Secret>",
"POWER_PLATFORM_URL": "https://api.powerplatform.com/",
"POWER_PLATFORM_CLIENT_ID": "<Power Platform Admin API Client Id>",
"POWER_PLATFORM_CLIENT_SECRET": "<Power Platform Admin API Client Secret>",
"POWER_PLATFORM_USERNAME": "<Dataverse Admin username>@<Azure AD Tenant Name>.onmicrosoft.com",
"POWER_PLATFORM_PASSWORD": "<Dataverse Admin password>"

# Example Kusto Query for Async Operations
customEvents
|extend cd = parse_json(customDimensions)
|project JobName_Status = strcat(name , " - ", tostring(cd.StatusName)), toint(cd.StatusCount), timestamp
//|where JobName_Status contains "Retrieve of ayw_"
|render timechart

# Example Kusto Query to Monitor for Exceptions on SQL Query
dependencies 
| where type == 'SQL' and success == false

## Explanation of Virtual Table
The Solution History Recorder will poll the Dataverse API as a service principal to get all solution history records for today. Once retrieved, the data is written to a CSV file.
This CSV file is within a container that is used as an external table for a SERVERLESS Azure Synapse Analytics SQL Pool. Using queries across databases, we can join history to solution.

Alternatively you can write this data to Azure Log Analytics and be alerted when a new solution history appears.

## Why is this needed?
Virtual tables are not exposed within Power Automate natively requiring connecting to the API similar to this solution.
Low code can be used here and potentially will be added to this repo.

# Example SQL Query
The sample query is located here: Microsoft.Dynamics365.OrganizationScanner\CREATE_TABLE_SOLUTION_HISTORY.sql
![alt text](CREATE_TABLE_SOLUTION_HISTORY.sql "Solution History SQL Table Create")
```SQL 
SELECT TOP (1000) [msdyn_solutionhistoryid]
      ,[msdyn_solutionid]
      ,[msdyn_endtime]
      ,[msdyn_starttime]
      ,[msdyn_errorcode]
      ,[msdyn_exceptionmessage]
      ,[msdyn_exceptionstack]
      ,[msdyn_ismanaged]
      ,[msdyn_ispatch]
      ,[msdyn_maxretries]
      ,[msdyn_name]
      ,[msdyn_operation]
      ,[msdyn_packagename]
      ,[msdyn_packageversion]
      ,[msdyn_publisherid]
      ,[msdyn_publishername]
      ,[msdyn_result]
      ,[msdyn_retrycount]
      ,[msdyn_solutionversion]
      ,[msdyn_status]
      ,[msdyn_suboperation]
      ,[msdyn_totaltime]
  FROM [dbo].[SolutionHistory]
```
