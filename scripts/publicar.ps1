# Compila el proyecto y reinicia el sitio. Se usa con el atajo "2 - Publicar mi sitio".
$ErrorActionPreference = 'Stop'
$repo = 'C:\muro'
$salida = 'C:\muro-publish'

Write-Host 'Publicando tu sitio...' -ForegroundColor Cyan

# Frena el sitio actual para poder reemplazar los archivos.
Stop-ScheduledTask -TaskName 'Muro' -ErrorAction SilentlyContinue
Get-Process -Name 'Muro' -ErrorAction SilentlyContinue | Stop-Process -Force

dotnet publish "$repo\Muro\Muro.csproj" -c Release -o $salida --nologo -v quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host 'No se pudo compilar. Levantá la mano y avisá.' -ForegroundColor Red
    exit 1
}

Start-ScheduledTask -TaskName 'Muro'
Start-Sleep -Seconds 6

$ip = (Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' } |
    Select-Object -First 1).IPAddress

try {
    $r = Invoke-WebRequest -Uri 'http://localhost/salud' -UseBasicParsing -TimeoutSec 10
    Write-Host "Listo: $($r.Content)" -ForegroundColor Green
    Write-Host ''
    Write-Host "Abrí esto desde tu celular:  http://$ip" -ForegroundColor Green
} catch {
    Write-Host 'El sitio todavía no responde. Esperá unos segundos y volvé a probar.' -ForegroundColor Yellow
}
