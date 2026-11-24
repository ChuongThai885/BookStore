$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$image = New-Object System.Drawing.Bitmap(1600, 1400)
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

function Write-Label([string]$text, [float]$x, [float]$y, [float]$size = 18,
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
                    [string]$fill, [string]$border) {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml($border), 2)
    try {
        $graphics.FillRectangle((Get-Brush $fill), $x, $y, $width, $height)
        $graphics.DrawRectangle($pen, $x, $y, $width, $height)
    } finally { $pen.Dispose() }
}

function Draw-Line([float]$x1, [float]$y1, [float]$x2, [float]$y2,
                   [bool]$arrowEnd = $false, [bool]$arrowStart = $false,
                   [string]$color = '#6C7E98') {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml($color), 3)
    $startArrow = $null
    $endArrow = $null
    if ($arrowEnd) {
        $endArrow = New-Object System.Drawing.Drawing2D.AdjustableArrowCap(5, 6)
        $pen.CustomEndCap = $endArrow
    }
    if ($arrowStart) {
        $startArrow = New-Object System.Drawing.Drawing2D.AdjustableArrowCap(5, 6)
        $pen.CustomStartCap = $startArrow
    }
    try { $graphics.DrawLine($pen, $x1, $y1, $x2, $y2) }
    finally {
        $pen.Dispose()
        if ($null -ne $startArrow) { $startArrow.Dispose() }
        if ($null -ne $endArrow) { $endArrow.Dispose() }
    }
}

function Draw-Route([float[]]$coordinates, [bool]$arrowEnd = $false,
                    [bool]$arrowStart = $false,
                    [string]$color = '#6C7E98') {
    for ($i = 0; $i -lt $coordinates.Length - 2; $i += 2) {
        $isLastSegment = $i -eq ($coordinates.Length - 4)
        $isFirstSegment = $i -eq 0
        Draw-Line $coordinates[$i] $coordinates[$i + 1] `
            $coordinates[$i + 2] $coordinates[$i + 3] `
            ($arrowEnd -and $isLastSegment) ($arrowStart -and $isFirstSegment) $color
    }
}

function Draw-ServerIcon([float]$x, [float]$y) {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml('#7890AA'), 2)
    try {
        foreach ($offset in @(0, 24, 48)) {
            $graphics.FillRectangle((Get-Brush '#EAF0F6'), $x, $y + $offset, 72, 19)
            $graphics.DrawRectangle($pen, $x, $y + $offset, 72, 19)
            $graphics.FillEllipse((Get-Brush '#39A96B'), $x + 8, $y + $offset + 6, 7, 7)
            $graphics.DrawLine($pen, $x + 24, $y + $offset + 9, $x + 62, $y + $offset + 9)
        }
    } finally { $pen.Dispose() }
}

function Draw-UserIcon([float]$x, [float]$y) {
    $graphics.FillEllipse((Get-Brush '#397AA3'), $x + 24, $y, 32, 32)
    $graphics.FillEllipse((Get-Brush '#397AA3'), $x + 10, $y + 34, 60, 38)
}

function Draw-NginxIcon([float]$x, [float]$y) {
    $points = [System.Drawing.Point[]]@(
        [System.Drawing.Point]::new([int]($x + 36), [int]$y),
        [System.Drawing.Point]::new([int]($x + 68), [int]($y + 18)),
        [System.Drawing.Point]::new([int]($x + 68), [int]($y + 54)),
        [System.Drawing.Point]::new([int]($x + 36), [int]($y + 72)),
        [System.Drawing.Point]::new([int]$x, [int]($y + 54)),
        [System.Drawing.Point]::new([int]$x, [int]($y + 18))
    )
    $graphics.FillPolygon((Get-Brush '#009639'), $points)
    Write-Label 'N' ($x + 19) ($y + 10) 48 '#FFFFFF' $true
}

function Draw-MonitorIcon([float]$x, [float]$y) {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml('#397AA3'), 4)
    try {
        $graphics.FillRectangle((Get-Brush '#EAF4FB'), $x, $y, 82, 54)
        $graphics.DrawRectangle($pen, $x, $y, 82, 54)
        $graphics.DrawLine($pen, $x + 41, $y + 55, $x + 41, $y + 68)
        $graphics.DrawLine($pen, $x + 22, $y + 69, $x + 60, $y + 69)
    } finally { $pen.Dispose() }
}

function Draw-DatabaseIcon([float]$x, [float]$y) {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml('#247866'), 3)
    try {
        $graphics.FillRectangle((Get-Brush '#D6EEE8'), $x, $y + 15, 76, 62)
        $graphics.FillEllipse((Get-Brush '#D6EEE8'), $x, $y + 58, 76, 28)
        $graphics.FillEllipse((Get-Brush '#E8F7F2'), $x, $y + 7, 76, 28)
        $graphics.DrawLine($pen, $x, $y + 21, $x, $y + 71)
        $graphics.DrawLine($pen, $x + 76, $y + 21, $x + 76, $y + 71)
        $graphics.DrawEllipse($pen, $x, $y + 7, 76, 28)
        $graphics.DrawArc($pen, $x, $y + 49, 76, 28, 0, 180)
        $graphics.DrawArc($pen, $x, $y + 58, 76, 28, 0, 180)
    } finally { $pen.Dispose() }
}

function Draw-RedisIcon([float]$x, [float]$y) {
    $red = Get-Brush '#D94343'
    $darkRed = Get-Brush '#B82F36'
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml('#A92C32'), 2)
    for ($row = 0; $row -lt 3; $row++) {
        $top = $y + ($row * 18)
        $points = [System.Drawing.Point[]]@(
            [System.Drawing.Point]::new([int]($x + 28), [int]$top),
            [System.Drawing.Point]::new([int]($x + 58), [int]($top + 13)),
            [System.Drawing.Point]::new([int]($x + 28), [int]($top + 26)),
            [System.Drawing.Point]::new([int]($x - 2), [int]($top + 13))
        )
        $graphics.FillPolygon($red, $points)
        $graphics.DrawLine($pen, $x + 28, $top + 26, $x + 28, $top + 34)
        $graphics.FillPolygon($darkRed, [System.Drawing.Point[]]@(
            [System.Drawing.Point]::new([int]($x - 2), [int]($top + 13)),
            [System.Drawing.Point]::new([int]($x + 28), [int]($top + 26)),
            [System.Drawing.Point]::new([int]($x + 28), [int]($top + 34)),
            [System.Drawing.Point]::new([int]($x - 2), [int]($top + 21))
        ))
    }
    $pen.Dispose()
}

function Draw-QueueIcon([float]$x, [float]$y) {
    $pen = New-Object System.Drawing.Pen(
        [System.Drawing.ColorTranslator]::FromHtml('#B77A1D'), 4)
    try {
        $graphics.DrawLine($pen, $x + 15, $y + 37, $x + 45, $y + 12)
        $graphics.DrawLine($pen, $x + 45, $y + 12, $x + 75, $y + 37)
        $graphics.DrawLine($pen, $x + 15, $y + 37, $x + 45, $y + 62)
        $graphics.DrawLine($pen, $x + 45, $y + 62, $x + 75, $y + 37)
        $graphics.FillEllipse((Get-Brush '#E6A23C'), $x + 4, $y + 26, 22, 22)
        $graphics.FillEllipse((Get-Brush '#E6A23C'), $x + 34, $y + 1, 22, 22)
        $graphics.FillEllipse((Get-Brush '#E6A23C'), $x + 64, $y + 26, 22, 22)
        $graphics.FillEllipse((Get-Brush '#E6A23C'), $x + 34, $y + 51, 22, 22)
    } finally { $pen.Dispose() }
}

try {
    Write-Label 'BOOK STORE' 70 35 40 '#14263F' $true
    Write-Label 'Current runtime components and request paths' 72 88 22

    Draw-Panel 70 155 260 210 '#FFFFFF' '#CDD8E6'
    Draw-UserIcon 95 220
    Write-Label 'USER / BROWSER' 180 190 17 '#176B90' $true
    Write-Label 'Book Store UI' 180 224 19 '#14263F' $true
    Write-Label 'HTTPS requests' 180 258 17
    Write-Label 'localhost:8000' 180 286 17

    Draw-Panel 460 155 440 210 '#FFFFFF' '#87B99C'
    Draw-NginxIcon 490 220
    Write-Label 'NGINX GATEWAY' 580 184 18 '#247247' $true
    Write-Label 'TLS reverse proxy :8000' 580 218 19 '#14263F' $true
    Write-Label '/  ->  Web UI :5173' 580 254 17
    Write-Label '/api/*  ->  backend APIs' 580 284 17

    Draw-Panel 1090 155 440 210 '#FFFFFF' '#65A9C6'
    Draw-MonitorIcon 1120 220
    Draw-ServerIcon 1126 292
    Write-Label 'WEB UI SERVER' 1225 184 18 '#176B90' $true
    Write-Label 'React + TypeScript' 1225 218 19 '#14263F' $true
    Write-Label 'Vite development server' 1225 252 17
    Write-Label 'localhost:5173' 1225 281 17

    Draw-Line 330 260 450 260 $true
    Write-Label 'HTTPS :8000' 340 226 15
    Draw-Line 900 260 1080 260 $true
    Write-Label '/  ->  :5173' 930 226 15
    Draw-Route @(1170, 365, 1170, 400, 920, 400, 905, 385) $true
    Write-Label 'UI API requests: /api/*' 948 372 15 '#526178' $true

    Draw-Line 680 365 680 445
    Draw-Line 280 445 1320 445
    Draw-Line 280 445 280 500 $true
    Draw-Line 800 445 800 500 $true
    Draw-Line 1320 445 1320 500 $true
    Write-Label 'NGINX routes API paths to the matching service' 570 414 16 '#526178' $true

    Draw-Panel 70 510 420 270 '#FFFFFF' '#65A9C6'
    Write-Label 'PRODUCT API SERVER' 95 528 17 '#176B90' $true
    Draw-ServerIcon 98 590
    Write-Label 'ASP.NET Core Web API' 190 578 20 '#14263F' $true
    Write-Label 'HTTP :5187' 190 612 17
    Write-Label 'Books / authors / genres' 190 644 17
    Write-Label 'EF Core -> Product SQL' 190 676 17
    Write-Label 'JWT middleware not configured' 190 716 15 '#687386'

    Draw-Panel 590 510 420 270 '#FFFFFF' '#C9A257'
    Write-Label 'AUTHENTICATION API SERVER' 615 528 17 '#92691C' $true
    Draw-ServerIcon 618 590
    Write-Label 'ASP.NET Core Identity' 710 578 20 '#14263F' $true
    Write-Label 'HTTP :5280' 710 612 17
    Write-Label 'Login / refresh / OpenID / JWKS' 710 644 16
    Write-Label 'EF Core -> Authentication SQL' 710 676 16
    Write-Label 'Redis token blacklist' 710 716 16 '#92691C'

    Draw-Panel 1110 510 420 270 '#FFFFFF' '#5E9B76'
    Write-Label 'CART API + SYNC WORKER' 1135 528 17 '#34764D' $true
    Draw-ServerIcon 1138 590
    Write-Label 'ASP.NET Core Web API' 1230 578 20 '#14263F' $true
    Write-Label 'HTTP :5142' 1230 612 17
    Write-Label 'JWT validation via Auth JWKS' 1230 644 16
    Write-Label 'Redis cart hashes' 1230 676 16
    Write-Label 'RabbitMQ publisher + worker' 1230 716 16 '#34764D'

    Draw-Line 1110 750 1010 750 $true
    Write-Label 'OIDC / JWKS' 1015 721 14 '#92691C' $true

    Draw-Line 280 780 280 830 $true
    Draw-Line 800 780 800 830 $true
    Draw-Line 1320 780 1320 830 $true

    Draw-Panel 70 840 420 140 '#E8F4F2' '#89B6AA'
    Draw-DatabaseIcon 95 865
    Write-Label 'PRODUCT SQL SERVER' 190 858 17 '#175D50' $true
    Write-Label 'ProductConnection' 190 894 18 '#175D50'

    Draw-Panel 590 840 420 140 '#E8F4F2' '#89B6AA'
    Draw-DatabaseIcon 615 865
    Write-Label 'AUTHENTICATION SQL SERVER' 710 858 16 '#175D50' $true
    Write-Label 'AuthenticationConnection' 710 894 17 '#175D50'

    Draw-Panel 1110 840 420 140 '#E8F4F2' '#89B6AA'
    Draw-DatabaseIcon 1135 865
    Write-Label 'CART SQL SERVER' 1230 858 17 '#175D50' $true
    Write-Label 'CartConnection' 1230 894 18 '#175D50'

    Draw-Route @(590, 750, 560, 750, 560, 1020, 415, 1020, 415, 1040) $true
    Write-Label 'JTI blacklist' 425 995 14 '#9E4040' $true
    Draw-Route @(1530, 745, 1550, 745, 1550, 1235, 760, 1235, 760, 1130) $true
    Draw-Route @(1530, 735, 1570, 735, 1570, 1015, 1185, 1015, 1185, 1040) $true $true

    Draw-Panel 70 1040 690 180 '#F9EEEE' '#C37E7E'
    Draw-RedisIcon 105 1080
    Write-Label 'REDIS SERVER' 190 1060 18 '#9E4040' $true
    Write-Label 'Shared by Authentication and Cart' 190 1095 20 '#14263F' $true
    Write-Label 'Authentication: revoked JWT JTI blacklist with TTL' 190 1130 16
    Write-Label 'Cart: user cart hashes and expiry' 190 1164 16

    Draw-Panel 840 1040 690 180 '#FFF7E8' '#C9A257'
    Draw-QueueIcon 875 1082
    Write-Label 'RABBITMQ BROKER' 980 1060 18 '#92691C' $true
    Write-Label 'Cart event transport' 980 1095 20 '#14263F' $true
    Write-Label 'Cart publishes cart.touched / cart.cleared' 980 1130 16
    Write-Label 'CartSyncBackgroundService consumes cart.sync.db' 980 1164 16
    Write-Label 'Batch sync reads Redis and writes SQL; DLQ configured' 980 1192 15

    Write-Label 'Solid arrows show configured local request paths through NGINX.' 70 1270 17
    Write-Label 'Cart validates JWTs using Authentication discovery/JWKS; Product API has no JWT middleware.' 70 1300 16
    Write-Label 'Redis is shared by Authentication and Cart; the Cart API publishes and consumes RabbitMQ events.' 70 1330 16

    $image.Save((Join-Path $PSScriptRoot 'bookstore_architecture.png'),
        [System.Drawing.Imaging.ImageFormat]::Png)
} finally {
    foreach ($font in $fonts.Values) { $font.Dispose() }
    foreach ($brush in $brushes.Values) { $brush.Dispose() }
    $graphics.Dispose()
    $image.Dispose()
}
