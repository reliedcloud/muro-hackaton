# Trae la última versión del proyecto desde GitHub y la publica. Se usa con el atajo "4 - Traer novedades".
$ErrorActionPreference = 'Stop'
Set-Location 'C:\muro'

Write-Host 'Buscando novedades en GitHub...' -ForegroundColor Cyan
git pull --ff-only
if ($LASTEXITCODE -ne 0) {
    Write-Host 'No se pudieron traer las novedades. Levantá la mano y avisá.' -ForegroundColor Red
    exit 1
}

& 'C:\muro\scripts\publicar.ps1'
