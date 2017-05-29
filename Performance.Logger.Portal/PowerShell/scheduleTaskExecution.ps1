Write-Host "Executing Performance Test - schedule task"
$baselocartoin="C:\Performance-Test";
$serviceLocation="C:\Performance-Test\bin\Debug\Test.Performance.API.exe";
$location="$baselocartoin\PowerShell";
$testAPIprocess= (Get-Process | where-Object {$_.Name -eq "Test.Performance.API" });
if ($testAPIprocess.Count -eq 0){
Start-Process -FilePath $serviceLocation
}
$cmd="$location\runner.ps1 -unitTestScript ""$baselocartoin\UnitTest\Collections\PerformanceTest.postman_collection.json""-unitTestEnvironment ""$baselocartoin\UnitTest\Environments\GeexGate-Test.postman_environment.json"" -parallelProcess 10 -iterations 1 -unitTestScriptLocation ""$location""";
Write-Host "Executing $cmd"
Invoke-Expression $cmd;