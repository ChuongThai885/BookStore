$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$image = New-Object System.Drawing.Bitmap(1600, 1500)
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
    Write-Label 'Current runtime components and communication paths' 72 88 22

    Draw-Panel 70 130 1460 115 '#FFFFFF' '#CDD8E6'
    Write-Label 'WEB UI' 100 148 17 '#176B90' $true
    Write-Label 'React + TypeScript | Vite | Axios | TanStack Query' 100 181 26 '#14263F' $true
    Write-Label 'Product listing calls the Product API' 100 217 19

    Draw-Link 800 245 800 285
    Write-Label 'HTTP' 820 252 16
    Draw-Panel 250 290 1100 100 '#EAF3FF' '#A8C6ED'
    Write-Label 'VITE DEVELOPMENT PROXY' 280 302 17 '#176B90' $true
    Write-Label '/api/product -> Product API | /api/auth -> Authentication API' 280 339 23 '#14263F' $true

    Draw-Link 430 390 275 445
    Write-Label 'HTTP / JSON' 270 404 16

    Draw-Panel 70 450 430 245 '#FFFFFF' '#65A9C6'
    Write-Label 'PRODUCT API' 95 470 17 '#176B90' $true
    Write-Label 'ASP.NET Core Web API' 95 505 25 '#14263F' $true
    Write-Label 'Books / authors / genres' 95 548 20
    Write-Label 'EF Core repositories' 95 584 20
    Write-Label 'No JWT middleware configured' 95 642 17 '#687386'

    Draw-Panel 585 450 430 245 '#FFFFFF' '#C9A257'
    Write-Label 'AUTHENTICATION API' 610 470 17 '#92691C' $true
    Write-Label 'ASP.NET Core Identity' 610 505 25 '#14263F' $true
    Write-Label 'Login / register / refresh / logout' 610 548 19
    Write-Label 'JWT issuer | OpenID config | JWKS' 610 584 19
    Write-Label 'SQL + Redis token blacklist' 610 620 18 '#92691C'

    Draw-Panel 1100 450 430 245 '#FFFFFF' '#5E9B76'
    Write-Label 'CART API + WORKER' 1125 470 17 '#34764D' $true
    Write-Label 'JWT Bearer + [Authorize]' 1125 505 23 '#14263F' $true
    Write-Label 'Cart state in Redis hashes' 1125 548 19
    Write-Label 'RabbitMQ publisher + batch consumer' 1125 584 18
    Write-Label 'EF Core cart snapshot' 1125 620 18 '#34764D'

    Draw-Link 1100 650 1015 650

    Draw-Link 275 695 275 770
    Draw-Link 800 695 800 770
    Draw-Link 1325 695 1325 770
    Draw-Panel 70 775 430 100 '#E8F4F2' '#89B6AA'
    Write-Label 'Product SQL Server' 95 794 23 '#175D50' $true
    Write-Label 'ProductConnection' 95 833 18 '#175D50'
    Draw-Panel 585 775 430 100 '#E8F4F2' '#89B6AA'
    Write-Label 'Authentication SQL Server' 610 794 23 '#175D50' $true
    Write-Label 'AuthenticationConnection' 610 833 18 '#175D50'
    Draw-Panel 1100 775 430 100 '#E8F4F2' '#89B6AA'
    Write-Label 'Cart SQL Server' 1125 794 23 '#175D50' $true
    Write-Label 'CartConnection' 1125 833 18 '#175D50'

    Draw-Panel 70 930 690 185 '#F9EEEE' '#C37E7E'
    Write-Label 'REDIS SERVER' 100 950 17 '#9E4040' $true
    Write-Label 'Shared connection used by Authentication and Cart' 100 985 23 '#14263F' $true
    Write-Label 'Authentication: revoked JWT JTI blacklist with TTL' 100 1030 19
    Write-Label 'Cart: user cart hashes and expiry' 100 1068 19

    Draw-Panel 840 930 690 210 '#FFF7E8' '#C9A257'
    Write-Label 'RABBITMQ' 870 950 17 '#92691C' $true
    Write-Label 'Cart event transport' 870 985 23 '#14263F' $true
    Write-Label 'Cart publishes cart.touched / cart.cleared' 870 1030 18
    Write-Label 'CartSyncBackgroundService consumes cart.sync.db' 870 1065 18
    Write-Label 'Batch sync reads Redis and writes SQL; DLQ configured' 870 1100 17

    Write-Label 'Solid arrows show configured application communication. Vite proxy is for local development.' 70 1190 19
    Write-Label 'Cart validates JWTs using Authentication discovery/JWKS; Product API has no JWT middleware configured.' 70 1225 18
    Write-Label 'Cart event flow: CartService -> RabbitMQ -> CartSyncBackgroundService -> Redis snapshot -> SQL.' 70 1260 18
    Write-Label 'Redis and RabbitMQ are external services configured through shared application settings.' 70 1295 18

    $image.Save((Join-Path $PSScriptRoot 'bookstore_architecture.png'),
        [System.Drawing.Imaging.ImageFormat]::Png)
} finally {
    foreach ($font in $fonts.Values) { $font.Dispose() }
    foreach ($brush in $brushes.Values) { $brush.Dispose() }
    $graphics.Dispose()
    $image.Dispose()
}
