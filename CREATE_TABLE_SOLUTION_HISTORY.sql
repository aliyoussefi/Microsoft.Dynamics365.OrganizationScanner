CREATE EXTERNAL TABLE SolutionHistory
(
            msdyn_endtime			datetime                          ,
			msdyn_errorcode         varchar(50)                   ,
            msdyn_exceptionmessage  varchar(MAX) ,                  
			msdyn_exceptionstack    varchar(MAX) , 
			msdyn_ismanaged    varchar(10) , 
			msdyn_ispatch    varchar(10) , 
			msdyn_maxretries    varchar(10) , 
			msdyn_name    varchar(100) , 
			msdyn_operation    varchar(30) , 
			msdyn_packagename    varchar(100) , 
			msdyn_packageversion    varchar(36) , 
			msdyn_publisherid    varchar(36) , 
			msdyn_publishername    varchar(36) , 
			msdyn_result    varchar(36) , 
			msdyn_retrycount    varchar(36) , 
			msdyn_solutionhistoryid varchar(50),
            msdyn_solutionid        varchar(50),
			msdyn_solutionversion    varchar(36) , 
			msdyn_starttime         datetime                          ,
			msdyn_status    varchar(36) , 
			msdyn_suboperation    varchar(36) , 
			msdyn_totaltime    int


)
WITH
(
    LOCATION = 'dataverse-90alitestins-alyoussefnaos/solutionhistory',
    DATA_SOURCE = SqlOnDemandDemo
    ,FILE_FORMAT = QuotedCsvWithHeaderFormat
)
GO