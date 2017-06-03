# Perforamce.Load.Test
Performance parallel load test tool, is an integrated tool with postman, the core idea is bulding an ecosystem is capable to run postman collections in parallel, given enviornment file, and collection unit tests. 
During the executing well log them all (capturing performance counters) to backend store, to get insights from api behaviour over load test, and unit tests results; Exporting all results to (local) data store (sql express). You can run this tool by integrated powershell script, parameters are postman collection file, environment file, how many process should run in parallel and how many iterations. 
This tool designed to consume APIs to figure, and log performance as well functionality if the postman script contains functional unit test, and log them all in data store. After executing the collection, a report shall be displyed to summarize the execution results, however all details during the execution are stored on the data source.

Self hosted web api server must be up & runing with adequte permission before running (executing), this api responsible to log results on data store, and later should summary report. (Performance.Logger.API)

Technology Stack
NodeJs, C# (Web API), Powershell, SQLExpress (C# code first).

To run 10 process in parallel for 10 times, the following is sampe run from powershell console:

.\runner.ps1 -unitTestScript "coll.postman_collection.json"-unitTestEnvironment "env.postman_environment.json" -parallelProcess 10 -iterations 10

