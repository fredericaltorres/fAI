# ============================================================
# Setup Script - Download tools and prepare directory structure

# irm "https://fredcloud2026.blob.core.windows.net/public/Brainshark/fred-stuff.ps1" | iex
# ============================================================

Write-Host "About to install Fred stuff on this machine" -ForegroundColor Cyan

$dir = "C:\Users\ftorres\Desktop\FRED"
 if (-Not (Test-Path -Path $dir)) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    Write-Host "  [CREATED] $dir" -ForegroundColor Yellow
} else {
    Write-Host "  [EXISTS]  $dir" -ForegroundColor Gray
}
cd $dir



Write-Host "Download fLogViewer.exe to the current directory" -ForegroundColor Cyan

$downloadUrl = "https://flogviewer2026.blob.core.windows.net/build/fLogViewer.exe"
$destinationPath = Join-Path -Path (Get-Location) -ChildPath "fLogViewer.exe"

if(test-path $destinationPath) {
    Write-Host "already downloaded: $destinationPath" -ForegroundColor Green
}
else {
    Write-Host "Downloading fLogViewer.exe..." -ForegroundColor Cyan
    Invoke-WebRequest -Uri $downloadUrl -OutFile $destinationPath
    Write-Host "Download complete: $destinationPath" -ForegroundColor Green
}

# Create a desktop shortcut
$desktopPath = [System.Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path -Path $desktopPath -ChildPath "fLogViewer.lnk"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $destinationPath
$shortcut.WorkingDirectory = (Get-Location).Path
$shortcut.Description = "fLogViewer"
$shortcut.Save()

Write-Host "Desktop shortcut created: $shortcutPath" -ForegroundColor Green


# Create a desktop shortcut pointing to PowerShell
Write-Host "Create a desktop shortcut pointing to PowerShell" -ForegroundColor Cyan
$desktopPath = [System.Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path -Path $desktopPath -ChildPath "PowerShell.lnk"
$powershellPath = "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $powershellPath
$shortcut.Description = "Windows PowerShell"
$shortcut.IconLocation = "$powershellPath,0"
$shortcut.Save()

Write-Host "Desktop shortcut created: $shortcutPath" -ForegroundColor Green




# Create a desktop shortcut pointing to cmd.exe
Write-Host "Create a desktop shortcut pointing to cmd.exe" -ForegroundColor Cyan
$desktopPath = [System.Environment]::GetFolderPath("Desktop")
$shortcutPath = Join-Path -Path $desktopPath -ChildPath "Command Prompt.lnk"
$cmdPath = "C:\Windows\System32\cmd.exe"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $cmdPath
$shortcut.Description = "Command Prompt"
$shortcut.IconLocation = "$cmdPath,0"
$shortcut.Save()

Write-Host "Desktop shortcut created: $shortcutPath" -ForegroundColor Green




Write-Host "Checking directories" -ForegroundColor Cyan


# ── 2. Ensure required directories exist ────────────────────
$directories = @(
    "C:\Brainshark",
    "C:\Brainshark\scripts",
    "C:\Brainshark\logs",
    "C:\tools"
)

Write-Host "`nChecking/creating directories..." -ForegroundColor Cyan
foreach ($dir in $directories) {
    if (-Not (Test-Path -Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "  [CREATED] $dir" -ForegroundColor Yellow
    } else {
        Write-Host "  [EXISTS]  $dir" -ForegroundColor Gray
    }
}


Write-Host "Download log4netconfig.xml to C:\Brainshark\scripts" -ForegroundColor Cyan

$xmlUrl  = "https://fredcloud2026.blob.core.windows.net/public/Brainshark/log4netconfig.xml"
$xmlDest = "C:\Brainshark\scripts\log4netconfig.xml"

Write-Host "`nDownloading log4netconfig.xml..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri $xmlUrl -OutFile $xmlDest -UseBasicParsing
    Write-Host "  [OK] log4netconfig.xml saved to: $xmlDest" -ForegroundColor Green
} catch {
    Write-Error "  [FAIL] Could not download log4netconfig.xml: $_"
}

Write-Host "`nSetup complete." -ForegroundColor Cyan






Write-Host "Download ff.ps1 to C:\tools" -ForegroundColor Cyan

$xmlUrl  = "https://fredcloud2026.blob.core.windows.net/public/ff.ps1"
$xmlDest = "c:\tools\ff.ps1"

Write-Host "`nDownloading ff.ps1..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri $xmlUrl -OutFile $xmlDest -UseBasicParsing
    Write-Host "  [OK] ff.ps1 saved to: $xmlDest" -ForegroundColor Green
} catch {
    Write-Error "  [FAIL] Could not download ff.ps1: $_"
}

Write-Host "`nSetup complete." -ForegroundColor Cyan




# ================================================================

Write-Host "Sets the Windows desktop background to a solid blue color" -ForegroundColor Cyan

# ============================================================
#  Set-SolidBlueBackground.ps1
#  Sets the Windows desktop background to a solid blue color
# ============================================================

# --- Configuration -----------------------------------------------------------
$Red   = 0
$Green = 120
$Blue  = 150 # Windows 10/11 accent blue  (#0078D7)
# -----------------------------------------------------------------------------

# 1. Write the P/Invoke signatures we need into a temporary C# type
Add-Type @"
using System;
using System.Runtime.InteropServices;

public class Wallpaper {

    // SystemParametersInfo constants
    private const int  SPI_SETDESKWALLPAPER  = 0x0014;
    private const int  SPI_SETDESKPATTERN    = 0x0015;
    private const uint SPIF_UPDATEINIFILE    = 0x01;
    private const uint SPIF_SENDCHANGE       = 0x02;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool SystemParametersInfo(
        int    uAction,
        int    uParam,
        string lpvParam,
        uint   fuWinIni);

    // Remove any existing wallpaper (empty string = no image)
    public static void RemoveWallpaper() {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, "",
            SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }
}

public class BackgroundColor {

    [DllImport("user32.dll")]
    private static extern bool SetSysColors(int cElements,
        int[] lpaElements, int[] lpaRgbValues);

    private const int COLOR_BACKGROUND = 1;   // desktop background element

    public static void Set(int r, int g, int b) {
        int[] elements = { COLOR_BACKGROUND };
        int[] colors   = { r | (g << 8) | (b << 16) };   // COLORREF = BGR
        SetSysColors(elements.Length, elements, colors);
    }
}
"@

# 2. Remove any image wallpaper so the solid color shows through
[Wallpaper]::RemoveWallpaper()

# 3. Set the desktop background color via SetSysColors
[BackgroundColor]::Set($Red, $Green, $Blue)

# 4. Persist the color to the registry so it survives reboots
$RegPath = "HKCU:\Control Panel\Colors"
Set-ItemProperty -Path $RegPath -Name "Background" `
                 -Value "$Red $Green $Blue"

# 5. Also store the wallpaper style as "solid color" (style 0, tile 0)
$DtPath = "HKCU:\Control Panel\Desktop"
Set-ItemProperty -Path $DtPath -Name "WallpaperStyle" -Value "0"
Set-ItemProperty -Path $DtPath -Name "TileWallpaper"  -Value "0"

Write-Host "Desktop background set to solid blue (R:$Red G:$Green B:$Blue)." `
           -ForegroundColor Cyan