using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

// Muro del hackathon: una app mínima en C# que guarda mensajes en SQL Server.
//
// Configuración (la deja lista el script de la máquina, no hace falta tocarla):
//   ConnectionStrings__Muro  cadena de conexión a SQL Server
//   EQUIPO_CONFIG            ruta del archivo con el nombre del equipo (por defecto C:\muro\mi-equipo.json)
//   SERVER_LABEL             nombre del servidor que se muestra en pantalla

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var cs = app.Configuration.GetConnectionString("Muro")
    ?? "Server=localhost\\SQLEXPRESS;Database=Muro;Integrated Security=true;TrustServerCertificate=true";
var archivoEquipo = app.Configuration["EQUIPO_CONFIG"] ?? @"C:\muro\mi-equipo.json";
var servidor = app.Configuration["SERVER_LABEL"] ?? Environment.MachineName;
const string version = "Versión 2";

// Crea la tabla si no existe. Si falla, el sitio avisa en pantalla y sigue arrancando.
string? errorInicio = null;
try
{
    await using var cn = new SqlConnection(cs);
    await cn.OpenAsync();
    await using var cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Mensajes') IS NULL
CREATE TABLE dbo.Mensajes (
    Id     INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(40)  NOT NULL,
    Texto  NVARCHAR(200) NOT NULL,
    Fecha  DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);
IF OBJECT_ID('dbo.Visitas') IS NULL
BEGIN
    CREATE TABLE dbo.Visitas (Id INT PRIMARY KEY, Total INT NOT NULL);
    INSERT INTO dbo.Visitas (Id, Total) VALUES (1, 0);
END", cn);
    await cmd.ExecuteNonQueryAsync();
}
catch (Exception ex)
{
    errorInicio = ex.Message;
    Console.WriteLine("No pude conectar con SQL Server: " + ex.Message);
}

// Lee el nombre, el emoji y el color del equipo cada vez que se abre la página:
// editás el archivo, guardás y recargás, sin volver a publicar.
(string Nombre, string Emoji, string Color) LeerEquipo()
{
    var nombre = "Equipo sin nombre";
    var emoji = "☁️";
    var color = "#5b6cff";
    try
    {
        if (File.Exists(archivoEquipo))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(archivoEquipo));
            var raiz = doc.RootElement;
            if (raiz.TryGetProperty("nombre", out var n) && n.ValueKind == JsonValueKind.String)
            {
                var t = (n.GetString() ?? "").Trim();
                if (t.Length is > 0 and <= 40) nombre = t;
            }
            if (raiz.TryGetProperty("emoji", out var e) && e.ValueKind == JsonValueKind.String)
            {
                var t = (e.GetString() ?? "").Trim();
                if (t.Length is > 0 and <= 8) emoji = t;
            }
            if (raiz.TryGetProperty("color", out var c) && c.ValueKind == JsonValueKind.String)
            {
                var t = (c.GetString() ?? "").Trim();
                if (Regex.IsMatch(t, "^#[0-9a-fA-F]{6}$")) color = t;
            }
        }
    }
    catch
    {
        // Si el archivo tiene un error de escritura, se usan los valores por defecto.
    }
    return (nombre, emoji, color);
}

// Para chequeos de disponibilidad.
app.MapGet("/salud", () => Results.Text($"ok {servidor}"));

app.MapGet("/", async () =>
{
    var (nombreEquipo, emoji, color) = LeerEquipo();
    var filas = new StringBuilder();
    string? error = errorInicio;
    var visitas = 0;
    try
    {
        await using var cn = new SqlConnection(cs);
        await cn.OpenAsync();

        // Novedad de la versión 2: cada visita se suma en la base de datos.
        await using (var contar = new SqlCommand(
            "UPDATE dbo.Visitas SET Total = Total + 1 OUTPUT inserted.Total WHERE Id = 1", cn))
        {
            visitas = Convert.ToInt32(await contar.ExecuteScalarAsync());
        }

        await using var cmd = new SqlCommand(
            "SELECT TOP 20 Nombre, Texto, Fecha FROM dbo.Mensajes ORDER BY Id DESC", cn);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync())
        {
            var hora = rd.GetDateTime(2).ToLocalTime().ToString("HH:mm");
            filas.Append("<li><b>").Append(WebUtility.HtmlEncode(rd.GetString(0))).Append("</b> ")
                 .Append("<span>").Append(hora).Append("</span><br>")
                 .Append(WebUtility.HtmlEncode(rd.GetString(1))).Append("</li>");
        }
        error = null;
    }
    catch (Exception ex)
    {
        error = ex.Message;
    }

    var aviso = error is null ? "" :
        "<p class=err>La base de datos no responde: " + WebUtility.HtmlEncode(error) + "</p>";

    var html = $@"<!doctype html>
<html lang=es><head><meta charset=utf-8>
<meta name=viewport content='width=device-width,initial-scale=1'>
<title>{WebUtility.HtmlEncode(nombreEquipo)}</title>
<style>
 body{{font-family:system-ui,sans-serif;background:#0f1220;color:#e8eaf6;max-width:560px;margin:0 auto;padding:20px 16px}}
 h1{{margin:0 0 4px}} .srv{{color:{color};font-size:14px;margin-bottom:20px}}
 form{{display:grid;gap:8px;margin-bottom:24px}}
 input,button{{font:inherit;padding:10px;border-radius:8px;border:1px solid #333a5c;background:#171b30;color:inherit}}
 button{{background:{color};border:0;font-weight:600;cursor:pointer}}
 ul{{list-style:none;padding:0;margin:0;display:grid;gap:10px}}
 li{{background:#171b30;border-radius:10px;padding:10px 12px;border-left:4px solid {color}}} li span{{color:#8a90b0;font-size:12px}}
 .err{{background:#3a1d28;border-radius:8px;padding:10px}}
</style></head><body>
<h1>{WebUtility.HtmlEncode(emoji)} {WebUtility.HtmlEncode(nombreEquipo)}</h1>
<div class=srv>{version} &middot; Servidor: {WebUtility.HtmlEncode(servidor)} &middot; Visitas: {visitas}</div>
{aviso}
<form method=post action=/mensajes>
 <input name=nombre maxlength=40 placeholder='Tu nombre' required>
 <input name=texto maxlength=200 placeholder='Dejá un mensaje' required>
 <button>Publicar</button>
</form>
<ul>{filas}</ul>
</body></html>";
    return Results.Content(html, "text/html; charset=utf-8");
});

app.MapPost("/mensajes", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var nombre = form["nombre"].ToString().Trim();
    var texto = form["texto"].ToString().Trim();

    if (nombre.Length is 0 or > 40 || texto.Length is 0 or > 200)
        return Results.Redirect("/");

    try
    {
        await using var cn = new SqlConnection(cs);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(
            "INSERT INTO dbo.Mensajes (Nombre, Texto) VALUES (@n, @t)", cn);
        cmd.Parameters.AddWithValue("@n", nombre);
        cmd.Parameters.AddWithValue("@t", texto);
        await cmd.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al guardar: " + ex.Message);
    }
    return Results.Redirect("/");
});

app.Run();
