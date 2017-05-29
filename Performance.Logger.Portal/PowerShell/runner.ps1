[CmdletBinding()]
Param(
   [Parameter(Mandatory=$True,Position=1)]
   [string]$unitTestScript,

   [Parameter(Mandatory=$True,Position=2)]
   [string]$unitTestEnvironment,

   [Parameter(Mandatory=$True)]
   [string]$parallelProcess=50,

   [Parameter(Mandatory=$True)]
   [string]$iterations=1000000,

   [Parameter(Mandatory=$False)]
   [switch]$OpenProcessWindow=$False,

   [Parameter(Mandatory=$False)]
   [string]$unitTestScriptLocation=".\"
	
)

function paymentApiRunner ($unitTestScript, $unitTestEnvironment, $parallelProcess, $iterations, $unitTestScriptLocation) {
	$runId= ([guid]::NewGuid()).ToString();
	$parallelProcess= $parallelProcess -as [int];
	$iterations=$iterations -as [int];
	$format="yyyy-MM-dd HH:mm:ss.fff";
	$logDir= "c:\\SoftIdeas\\Perfomance-Test\\Log";
	$logFile= "$logDir\\$runId.log";
	New-Item $logDir -Type Directory -Force;
	New-Item $logFile -Type File;
	$initalizeUrl=     "http://localhost:7070/api/PreformanceTracer/initialize";
	$reportUrl=        "http://localhost:7070/api/PreformanceTracer/report";
	$finalizeUrl=      "http://localhost:7070/api/PreformanceTracer/finalize";
	$APIPerformanceUrl="http://localhost:7070/api/PreformanceTracer/APICalls";
	$APITestCaseUrl	  ="http://localhost:7070/api/PreformanceTracer/APITestCase";
	$callBackErrUrl   ="http://localhost:7070/api/PreformanceTracer/APIErrorLogger";
	$UnitTestExecuter="$unitTestScriptLocation\UnitTestExecuter.js";
	$InitialNodePCount=0;
	$InitialNodePCountSeed=(Get-Process | where-Object {$_.Name -eq "node" }).Count;
	$msg= "Run-Id: $runId | Running $unitTestScript | using $unitTestEnvironment | Parallel: $parallelProcess | Iterations: $iterations";
	Write-Host $msg;
	$msg | Out-File $logFile -Append -Encoding utf8;
	$startDate=Get-Date -Format $format;

	 ## call service once start to initialize!
		$machine=(get-childitem env:computerName).value;
		$programFiles=(get-childitem env:"ProgramFiles(x86)").value; 
		$completedAt=Get-Date -Format $format;
		$startDate=$startDate;
		$completedAt=$completedAt;
		$params =@{ 
			"RunId"= $runId;
			"ParallelProcess" = $parallelProcess; 
			"Iterations"= $iterations;
			"Specs"= $unitTestScript;
			"Environment"=$unitTestEnvironment;
			"StartingAt"= $startDate; 
			"CompletedAt"= $null;
			"ExecutingMachine"= $machine
			};
	Write-Host "Calling Performance Service ....";
	"Calling Performance Service .... |" | Out-File $logFile -Append -Encoding utf8;
	$result = Invoke-WebRequest -Method POST  -Headers @{"Accept"="application/json"}  -Body ($params | ConvertTo-Json) -Uri  $initalizeUrl  -ContentType "application/json"

	$idleSec=1;
	$counter=0;
	Write-Host "running command";
	$nodejs="$programFiles\nodejs\";
	$currentLocation=(Get-Location).Path;
	Write-Host "Executing: from location: $currentLocation";
	$cmdArgs="$UnitTestExecuter $unitTestScript $unitTestEnvironment $runId $APIPerformanceUrl $APITestCaseUrl $callBackErrUrl";
	$totalExecutions=($parallelProcess * $iterations);
	Write-Host "Executing: $totalExecutions# times";
	"Executing: $totalExecutions# times | Command: $cmdArgs |" | Out-File $logFile -Append -Encoding utf8;
	Write-Host $cmdArgs
	while($counter -lt $totalExecutions){
		$InitialNodePCount=(Get-Process | where-Object {$_.Name -eq "node" }).Count - $InitialNodePCountSeed;
		if ($InitialNodePCount -lt $parallelProcess){
		
			if( ($parallelProcess - $InitialNodePCount) -ge ($totalExecutions - $counter)){
				$maxRuninParallel=($totalExecutions - $counter);
				}
				else{
					$maxRuninParallel=($parallelProcess - $InitialNodePCount);
				}
			Write-Host "Executing:$maxRuninParallel, Command: $cmdArgs";
			"Executing: at $(Get-Date -Format $format) | Execution Index: $counter | Processes: $maxRuninParallel# |" | Out-File $logFile -Append -Encoding utf8;
			if($OpenProcessWindow -eq $True){
			1..($maxRuninParallel) | ForEach-Object { Start-Process node $cmdArgs  -Verbose -ErrorAction SilentlyContinue };}
			else{
			1..($maxRuninParallel) | ForEach-Object { Start-Process node $cmdArgs -NoNewWindow -Verbose -ErrorAction SilentlyContinue };}
			Write-Host "Running index is:$counter.         Press CTR + C to terminate execution!";
			$counter= $counter + $maxRuninParallel;
		}
		else {
			Write-Host "Waiting for completed execution process, before running a new process!"
		}

	}
	$inprogress=$true;
	while($inprogress){
		## call service once done!
		$InitialNodePCount=(Get-Process | where-Object {$_.Name -eq "node" }).Count - $InitialNodePCountSeed;
		if($InitialNodePCount -eq 0){
		$completedAt=Get-Date -Format $format;    
		$finalizeParams= "$runId/$completedAt";
		Write-Host "Calling Service .... with  '$finalizeUrl/$finalizeParams'";
		$result = Invoke-WebRequest -Method POST  -Headers @{"Accept"="application/json"} -Uri "$finalizeUrl/$finalizeParams"  -ContentType "application/json"
		Write-Host $result | Format-Table;
		Write-Host "Opening performance test report ...";
		"Opening performance test report ... |" | Out-File $logFile -Append -Encoding utf8;
		Start-Process ("$reportUrl/$runId");
		$inprogress=$false;
		}
		else{
			Write-Host "Waiting for current process to be completed, before running the report!"
			Start-sleep -Seconds $idleSec
		}
  }
 
}
paymentApiRunner -unitTestScript  $unitTestScript  -unitTestEnvironment $unitTestEnvironment -parallelProcess $parallelProcess -iterations $iterations -unitTestScriptLocation $unitTestScriptLocation;