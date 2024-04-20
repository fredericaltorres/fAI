<#
#>  

function trace([string]$msg)         { Write-Host $msg -ForegroundColor Gray }
function traceAction([string]$msg)   { Write-Host $msg -ForegroundColor Cyan }
function traceBanner([string]$msg)   { Write-Host $msg -ForegroundColor Yellow }
function assert($boolExp, $message)  { if($boolExp) { write-host "[PASSED] $message" -ForegroundColor green } else { write-host "[ERROR] $message" -ForegroundColor red }  return $boolExp }

function deleteFile([string]$fileName) {

    if(test-path -LiteralPath $fileName) {
        del -LiteralPath $fileName
    }
}


function checkHttpResponse($response, $url) {
  
    if(($response.statusCode -ge 200) -and ($response.statusCode -le 204)) {
        write-host "HTTP StatusCode: $($response.statusCode)"
        return $true
    }
    else {
        Write-Error "`r`nHTTP StatusCode: $($response.statusCode)"
        return $false
    }
}

function getHeaders() {

    return @{ 'Content-type' = 'application/json';  'FOO' = '' }
}

function TestGetUrl([string]$url, [string]$message = "", [bool]$isJson = $false) {

    traceAction "`r`n$message - get url: $url"
    try { 
        $response = Invoke-WebRequest -Uri $url -Method GET -Headers (getHeaders)
    }
    catch {
        $response = $_.Exception.Response
        traceError $_.ToString()
        return $null
    }
    if((checkHttpResponse $response $url) -eq $true ) {

        $response.content | Set-Content "c:\temp\last.json"
        # traceSuccess "Response.Content: $( $response.content)"
        $h = $response.headers
        if($isJson) {
            return ($response.Content | ConvertFrom-Json)
        }
        else {
            return $response.Content
        }
    }
    else {
        return $null
    }
}

function TestPostUrl($url, $message, $body) { 

    $headers = @{ 
        'Content-type' = 'application/json'; 
        'X-Known-B2B-Token' = "70e2bc37-ee59-4ada-a6f4-fd716f7a483f"; 
        'BSKApplicationKey' = 'foo';
    }
    traceAction "`r`n$message, post url: $url"
    $response = Invoke-WebRequest -Uri $url -Method POST -Headers $headers -body $body
    if((checkHttpResponse $response $url) -eq $true ) {
        "OK $( $response.content )"
        $h = $response.headers
    }
}




function TestUrlDownloadFile([string]$url, [string]$message, [string]$downloadFileName) {

    traceAction "`r`n$message - Downloading File - GET url: $url"
    $response = $null
    try {
        deleteFile $downloadFileName
        headers = getHeaders
        $response = Invoke-WebRequest -Uri $url -Method GET -Headers headers -OutFile $downloadFileName -PassThru 
    }
    catch {
        $response = $_.Exception.Response
        trace $_.ToString()
    }
    
    $r = test-path $downloadFileName
    trace "File downloaded exist on file system: $r"
    deleteFile $downloadFileName
    return $r
}

cls
traceBanner "fAI.WebAp.Test`r`n"

$hostName = "localhost:7009"
$hostName = "faiwebapi.azurewebsites.net"
$url = "https://$hostName/Embedding";

$headers = getHeaders

write-output $url
$response = Invoke-WebRequest -Uri $url -Method GET -Headers $headers
write-output $response.Content


write-output $url
$response = Invoke-WebRequest -Uri $url -Method POST -Headers $headers -body '"sailing"'
write-output $response.Content
