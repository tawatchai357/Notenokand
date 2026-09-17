# Reproduce PNG icons from the existing blue/white N brand (no external assets).
Add-Type -AssemblyName System.Drawing
$iconRoot = Join-Path $PSScriptRoot '../src/Notenokand.Web/wwwroot/icons'
[void][System.IO.Directory]::CreateDirectory($iconRoot)
foreach ($entry in @(@{Size=192;Name='icon-192.png'}, @{Size=512;Name='icon-512.png'}, @{Size=512;Name='icon-maskable-512.png'}, @{Size=180;Name='apple-touch-icon.png'})) {
    $size = $entry.Size
    $bitmap = [System.Drawing.Bitmap]::new($size, $size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $pen = [System.Drawing.Pen]::new([System.Drawing.Color]::White, [single]($size * .085))
    try {
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $graphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#1565d8'))
        $points = [System.Drawing.PointF[]]@(
            [System.Drawing.PointF]::new($size*.30, $size*.72),
            [System.Drawing.PointF]::new($size*.30, $size*.28),
            [System.Drawing.PointF]::new($size*.70, $size*.72),
            [System.Drawing.PointF]::new($size*.70, $size*.28)
        )
        $graphics.DrawLines($pen, $points)
        $bitmap.Save((Join-Path $iconRoot $entry.Name), [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $pen.Dispose(); $graphics.Dispose(); $bitmap.Dispose() }
}
