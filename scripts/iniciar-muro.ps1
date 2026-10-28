# Arranca el sitio. Lo ejecuta una tarea programada de Windows cada vez que la máquina se enciende.
$ErrorActionPreference = 'Stop'

# Cadena de conexión y nombre del servidor (los deja el script de preparación de la máquina).
. 'C:\muro\secretos.ps1'

$env:ASPNETCORE_URLS = 'http://0.0.0.0:80'
$env:EQUIPO_CONFIG = 'C:\muro\mi-equipo.json'

Set-Location 'C:\muro-publish'
& 'C:\muro-publish\Muro.exe'
