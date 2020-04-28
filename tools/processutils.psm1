<#
.Synopsis
Utility module for working with processes,.
#>

<#
Downloads a file over http/https.
#>
function Get-DownloadFile($url, $output)
{
    Write-Host "Downloading $url => $output"
    $wc = New-Object System.Net.WebClient
    $wc.DownloadFile($url, $output)
}

<#
Downloads SysInternals handle.exe if not present beside the script.
#>
function Install-HandleUtil()
{
    $HandleUtilFilename = "handle.exe"
    $HandleUtilDir = (Join-Path -Path $PSScriptRoot -ChildPath "handle")
    if ((Test-Path -Path $HandleUtilDir) -eq $false)
    {
        $null = New-Item -ItemType Directory $HandleUtilDir  # suppress return value
    }
    $HandleUtilPath = (Join-Path -Path $HandleUtilDir -ChildPath $HandleUtilFilename)
    if ((Test-Path -Path $HandleUtilPath) -eq $false)
    {
        $ArchivePath = (Join-Path -Path $HandleUtilDir -ChildPath "handle.zip")
        $url = "https://download.sysinternals.com/files/Handle.zip"
        try
        {
            Get-DownloadFile -url $url -output $ArchivePath
            Expand-Archive $ArchivePath -Force -DestinationPath $HandleUtilDir
            Remove-Item -Force $ArchivePath
        }
        catch
        {
            throw
        }
        if ((Test-Path -Path $HandleUtilPath) -eq $false)
        {
            throw "Handle utility not found in downloaded archive"
        }
    }
    return $HandleUtilPath
}

<#
Get PIDs of all processes with open handles on given path.
#>
function Get-ProcessesWithHandle($path)
{
    $path = $path.TrimEnd("\\")  # path to a folder should not end with a separator
    $HandleUtil = (Install-HandleUtil)
    $HandleOutput = (. $HandleUtil -nobanner $path) | Out-String
    # Example of a matching line:
    # explorer.exe       pid: 9360   type: File          2F28: C:\code\MixedRealityUtils-UE
    $lines = $HandleOutput.Split([Environment]::NewLine, [System.StringSplitOptions]::RemoveEmptyEntries) `
                | Where-Object { $_  -match "\spid:\s" }
    if ($lines.Count -gt 0)
    {
        Write-Host "The following processes have open handles on $path"
        $lines | ForEach-Object { Write-Host $_ }
    }
    return ($lines `
        | ForEach-Object { Select-String -InputObject $_ -Pattern "pid: (\d+)" } `
        | Where-Object { $_ -ne $null } `
        | ForEach-Object { $_.Matches[0].Groups[1].Value })
}

<#
Terminate all processes with open handles on given path.
#>
function Stop-ProcessesWithHandle($path)
{
    $pids = Get-ProcessesWithHandle($path)
    $pids | ForEach-Object { 
        Write-Host "Terminating process $_..."
        try { Stop-Process -Force -Id $_ -PassThru -ErrorAction SilentlyContinue } catch {}
    }
    $pids | ForEach-Object {
        try { Wait-Process -Id $_  -ErrorAction SilentlyContinue } catch {}
    }
}
