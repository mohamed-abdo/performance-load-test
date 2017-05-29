[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True,Position=1)]
   [string]$newmanScript,
	
   [Parameter(Mandatory=$False)]
   [string]$parallelProcess=50,

   [Parameter(Mandatory=$False)]
   [string]$iterations=10000000000
)
function paymentApiRunner ($newmanScript ="Authorize-OrderAPI-UnitTest.js", $parallelProcess, $iterations) {
    Add-Type -AssemblyName System.Web
    $format="yyyy-MM-dd hh:mm:ss:fff";

    $initalizeUrl="http://localhost:7070/api/Tracer/initialize";
    $reportUrl= "http://localhost:7070/api/tracer/report";
    $APIPerformanceUrl="http://localhost:7070/api/tracer/performance";
    $finalizeUrl="http://localhost:7070/api/tracer/finalize";

    $runId= (New-Guid).ToString();
    $msg= "Run-Id: {$runId}, Running $newmanScript, with parallel: $parallelProcess, the maximum iterations: $iterations";
    Write-Host $msg;
    $startDate=Get-Date -Format $format;

     ## call service once start to initalize!
        $machine=(get-childitem env:computerName).value;
        $completedAt=Get-Date -Format $format;
        $startDate=$startDate;
        $completedAt=$completedAt;
        $params =@{ 
            "RunId"= $runId;
            "ParallelProcess" = $parallelProcess; 
            "Iterations"= $iterations;
            "Specs"= $newmanScript;
            "StartingAt"= $startDate; 
            "CompletedAt"= $null;
            "ExecutingMachine"= $machine
            };
        Write-Host "Calling Performace Service ....";
        $result = curl -Method POST  -header @{"Accept"="application/json"}  -Body ($params | ConvertTo-Json) -Uri "http://localhost:7070/api/Tracer/initialize"  -ContentType "application/json"

    $idleSec=1;
    $totalIdleSec=0;
    $counter=0;
    Write-Host "running command";
    while($counter -le $iterations){
        $p=Get-Process | where-Object {$_.Name -eq "node" };
        if ($p.count -le $parallelProcess){
            1..($parallelProcess-$p.count) | ForEach-Object {node $newmanScript "$runId http://localhost:7070/api/tracer/performance"  -Verbose -ErrorAction SilentlyContinue };
            Write-Host "Running index is:$counter.          Press CTR + C to trerminate execution!";
            $counter= $counter +1;
        }
        else {
            $totalIdleSec= $totalIdleSec +$idleSec;            SSSS
            Write-Host "Waitng for $idleSec second(s),     total waiting  second(s) : $totalIdleSec before running new process!"
            Start-sleep -Seconds $idleSec
        }
    }
  if($counter -gt $iterations){
        ## call service once done!
        $completedAt=Get-Date -Format $format;    
        $finalizeParams= "runId=$runId&completedAt=$completedAt";
        Write-Host "Calling Service .... with $params";
        $result = curl -Method POST  -header @{"Accept"="application/json"} -Uri "http://localhost:7070/api/tracer/finalize?$finalizeParams"  -ContentType "application/json"
        Write-Host $result | Format-Table;
        Write-Host "Opennig performance test report ...";
        Start-Process "http://localhost:7070/api/tracer/report?runId=$runId";
  }
}
paymentApiRunner $newmanScript  -parallelProcess $parallelProcess -iterations $iterations;