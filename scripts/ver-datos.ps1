# Muestra lo que hay guardado en SQL Server. Se usa con el atajo "3 - Ver mis datos".
$ErrorActionPreference = 'Stop'
. 'C:\muro\secretos.ps1'

function Consultar([string]$sql) {
    $cn = New-Object System.Data.SqlClient.SqlConnection $env:ConnectionStrings__Muro
    $cmd = $cn.CreateCommand()
    $cmd.CommandText = $sql
    $da = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$da.Fill($dt)
    $cn.Close()
    return ,$dt
}

Write-Host 'Base de datos: Muro (SQL Server). Tabla: Mensajes' -ForegroundColor Cyan
Write-Host ''
$mensajes = Consultar 'SELECT TOP 10 Id, Nombre, Texto, Fecha FROM dbo.Mensajes ORDER BY Id DESC'
if ($mensajes.Rows.Count -eq 0) {
    Write-Host 'Todavía no hay mensajes. Mandá uno desde tu sitio y volvé a abrir este atajo.'
} else {
    $mensajes | Format-Table Id, Nombre, Texto, Fecha -AutoSize
}

$hayVisitas = Consultar "SELECT OBJECT_ID('dbo.Visitas') AS Existe"
if ($hayVisitas.Rows[0].Existe -isnot [System.DBNull]) {
    $visitas = Consultar 'SELECT Total FROM dbo.Visitas WHERE Id = 1'
    Write-Host "Visitas guardadas: $($visitas.Rows[0].Total)" -ForegroundColor Green
}
