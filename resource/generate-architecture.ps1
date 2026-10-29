$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$image = New-Object System.Drawing.Bitmap(1600, 1220)
$graphics = [System.Drawing.Graphics]::FromImage($image)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#F4F7FB'))
$fonts = @{}
$brushes = @{}

function Get-Brush([string]$color) {
    if (-not $brushes.ContainsKey($color)) {
        $brushes[$color] = New-Object System.Drawing.SolidBrush(
            [System.Drawing.ColorTranslator]::FromHtml($color))
    }
    return $brushes[$color]
}

function Write-Label([string]$text, [float]$x, [float]$y, [float]$size = 20,
                     [string]$color = '#526178', [bool]$bold = $false) {
    $key = "$size-$bold"
    if (-not $fonts.ContainsKey($key)) {
        $style = [System.Drawing.FontStyle]::Regular
        if ($bold) { $style = [System.Drawing.FontStyle]::Bold }
        $fonts[$key] = New-Object System.Drawing.Font(
            'Segoe UI', $size, $style, [System.Drawing.GraphicsUnit]::Pixel)
    }
    $graphics.DrawString($text, $fonts[$key], (Get-Brush $color), $x, $y)
}

function Draw-Panel([float]$x, [float]$y, [float]$width, [float]$height,
                    [string]$fill, [string]$border, [bool]$dashed = $false) {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml($border), 2)
    if ($dashed) { $pen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash }
    try {
        $graphics.FillRectangle((Get-Brush $fill), $x, $y, $width, $height)
        $graphics.DrawRectangle($pen, $x, $y, $width, $height)
    } finally { $pen.Dispose() }
}

function Draw-Link([float]$x1, [float]$y1, [float]$x2, [float]$y2,
                   [bool]$dashed = $false) {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml('#6C7E98'), 3)
    $arrow = New-Object System.Drawing.Drawing2D.AdjustableArrowCap(5, 6)
    $pen.CustomEndCap = $arrow
    if ($dashed) { $pen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash }
    try { $graphics.DrawLine($pen, $x1, $y1, $x2, $y2) }
    finally { $pen.Dispose(); $arrow.Dispose() }
}

try {
    Write-Label 'BOOK STORE' 70 35 38 '#14263F' $true
    Write-Label 'Repository architecture | implemented paths and work in progress' 72 88 22

    Draw-Panel 70 145 1460 140 '#FFFFFF' '#CDD8E6'
    Write-Label 'BROWSER / UI' 100 165 17 '#176B90' $true
    Write-Label 'React 19 + TypeScript' 100 197 29 '#14263F' $true
    Write-Label 'Vite 8 | Tailwind CSS 4 | Axios | TanStack Query' 100 242 21
    Write-Label 'Product listing wired to API' 1050 200 23 '#176B90' $true
    Write-Label 'Auth flow not yet wired in UI' 1050 241 20

    Draw-Link 800 285 800 345
    Write-Label 'HTTP / JSON' 825 300 18
    Draw-Panel 350 350 900 110 '#EAF3FF' '#A8C6ED'
    Write-Label 'Vite development proxy' 380 366 25 '#14263F' $true
    Write-Label '/api/product/* -> Product /api/*' 380 407 20
    Write-Label '/api/auth/* -> Auth /api/*' 840 407 20

    Draw-Link 565 460 435 535
    Write-Label 'Product requests' 190 485 19 '#176B90'
    Draw-Link 1035 460 1165 535 $true
    Write-Label 'Auth UI integration pending' 1180 485 18

    Draw-Panel 70 540 700 220 '#FFFFFF' '#65A9C6'
    Write-Label 'PRODUCT SERVICE' 100 558 17 '#176B90' $true
    Write-Label 'ASP.NET Core Web API (.NET 10)' 100 591 28 '#14263F' $true
    Write-Label 'Book / Author / Genre controllers' 100 637 22
    Write-Label 'Application services -> Repositories -> EF Core' 100 678 21
    Write-Label 'Product API called by the UI' 100 721 18 '#176B90'

    Draw-Panel 830 540 700 220 '#FFFFFF' '#C9A257'
    Write-Label 'AUTHENTICATION SERVICE | IN PROGRESS' 860 558 17 '#92691C' $true
    Write-Label 'ASP.NET Core Identity (.NET 10)' 860 591 28 '#14263F' $true
    Write-Label 'Guid users / roles | Refresh-token repository' 860 637 21
    Write-Label 'JWT key manager | JWKS / discovery endpoints' 860 678 21
    Write-Label 'Auth controller has no login actions yet' 860 721 18 '#92691C'

    Draw-Link 420 760 420 820
    Write-Label 'EF Core / SQL Server' 445 777 18
    Draw-Link 1180 760 1180 820
    Write-Label 'EF Core / SQL Server' 1205 777 18
    Draw-Panel 70 825 700 105 '#E8F4F2' '#89B6AA'
    Write-Label 'BookStore.Products' 100 840 27 '#175D50' $true
    Write-Label 'SQL Server | ProductConnection' 100 883 21 '#175D50'
    Draw-Panel 830 825 700 105 '#E8F4F2' '#89B6AA'
    Write-Label 'BookStore.Authentication' 860 840 27 '#175D50' $true
    Write-Label 'SQL Server | AuthenticationConnection' 860 883 21 '#175D50'

    Draw-Panel 70 965 1460 125 '#F0F1F4' '#AAB1BD' $true
    Write-Label 'PLANNED / NOT IMPLEMENTED' 100 980 17 '#687386' $true
    Write-Label 'Order service' 100 1013 24 '#39475B' $true
    Write-Label 'Empty backend folder; UI proxy prefix exists.' 100 1052 19
    Write-Label 'RabbitMQ / event messaging' 830 1013 24 '#39475B' $true
    Write-Label 'No broker integration or service-to-service events yet.' 830 1052 19

    Write-Label 'Solid arrows: implemented code paths   |   Dashed arrow: configured proxy only' 70 1113 19
    Write-Label 'Scope: local development. Vite proxy is not a production API gateway.' 70 1146 19
    Write-Label 'Database boxes describe configured targets, not verified deployed databases.' 70 1177 18

    $image.Save((Join-Path $PSScriptRoot 'bookstore_architecture.png'),
        [System.Drawing.Imaging.ImageFormat]::Png)
} finally {
    foreach ($font in $fonts.Values) { $font.Dispose() }
    foreach ($brush in $brushes.Values) { $brush.Dispose() }
    $graphics.Dispose()
    $image.Dispose()
}
