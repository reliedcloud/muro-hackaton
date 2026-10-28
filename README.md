# Muro del hackathon

Una aplicación en C# (.NET 8) que guarda mensajes en una base de datos SQL Server. La usamos en el workshop de nube del hackathon para mostrar, paso a paso, cómo se publica y se actualiza una aplicación en un servidor.

## Qué hace

- Muestra una página con el nombre de tu equipo.
- Cualquiera puede dejar un mensaje desde el celular.
- Los mensajes se guardan en SQL Server, no en la memoria del programa. Si reiniciás el sitio, siguen ahí.

## Qué hay en cada carpeta

| Carpeta | Contenido |
|---|---|
| `Muro/` | El proyecto en C# (`Program.cs`) |
| `scripts/` | Los scripts que compilan, publican y consultan la base |
| `escritorio/` | Los atajos que aparecen en el escritorio de cada servidor |
| `mi-equipo.ejemplo.json` | Modelo del archivo donde cada equipo escribe su nombre, emoji y color |

## En el workshop

Cada servidor ya tiene todo instalado y el sitio compilado y funcionando. Desde el escritorio:

1. **Ponerle nombre a mi equipo**: se abre `mi-equipo.json`. Cambiás el nombre, guardás y recargás el sitio.
2. **Publicar mi sitio**: compila el proyecto y reinicia el sitio.
3. **Ver mis datos en SQL Server**: muestra los mensajes guardados en la base.
4. **Traer novedades de GitHub**: hace `git pull` y publica la última versión.

El sitio se abre desde cualquier celular en `http://IP-DE-TU-SERVIDOR`.

## Para usarlo en tu propia computadora

Necesitás el SDK de .NET 8 y una instancia de SQL Server (por ejemplo, SQL Server Express).

```powershell
git clone https://github.com/REEMPLAZAR/muro-hackathon
cd muro-hackathon
copy mi-equipo.ejemplo.json mi-equipo.json
$env:EQUIPO_CONFIG = "$PWD\mi-equipo.json"
$env:ConnectionStrings__Muro = "Server=localhost\SQLEXPRESS;Database=Muro;Integrated Security=true;TrustServerCertificate=true"
dotnet run --project Muro
```

La base `Muro` tiene que existir antes. La tabla se crea sola.
