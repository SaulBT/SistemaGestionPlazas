[CmdletBinding()]
param(
    [ValidateSet('Iniciar', 'Detener', 'Volver', 'Estado', 'Validar')]
    [string]$Accion = 'Iniciar',
    [string]$CorreoAdministrador
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$raizProyecto = Split-Path -Parent $PSScriptRoot
$archivoCompose = Join-Path $raizProyecto 'docker-compose.pruebas.yml'
$carpetaLocal = Join-Path $raizProyecto '.local/sgpla-pruebas'
$archivoEntorno = Join-Path $carpetaLocal 'entorno.env'
$nombreProyecto = 'sgpla-pruebas'

function Invoke-Compose {
    param([string[]]$Argumentos)
    & docker compose --project-directory $raizProyecto --project-name $nombreProyecto --env-file $archivoEntorno -f $archivoCompose @Argumentos
    if ($LASTEXITCODE -ne 0) { throw "Docker no pudo completar: $($Argumentos[0]). No se ha ejecutado ninguna acción contra el proyecto habitual." }
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'No se encontró Docker. Instale o abra Docker Desktop y vuelva a ejecutar este script.'
}

if ($Accion -eq 'Validar') {
    # No crea archivos privados, contenedores ni volúmenes; no requiere el daemon.
    $passwordAnterior = $env:SGPLA_PRUEBAS_SQL_PASSWORD
    $correoAnterior = $env:SGPLA_PRUEBAS_ADMIN_EMAIL
    try {
        $env:SGPLA_PRUEBAS_SQL_PASSWORD = 'SoloValidacion_2026!NoSeUsa'
        $env:SGPLA_PRUEBAS_ADMIN_EMAIL = 'validacion@example.invalid'
        & docker compose --project-directory $raizProyecto --project-name $nombreProyecto -f $archivoCompose config --quiet
        if ($LASTEXITCODE -ne 0) { throw 'La configuración de Compose no es válida.' }
        Write-Host 'Configuración válida. No se inició ni modificó ningún entorno.'
    }
    finally {
        $env:SGPLA_PRUEBAS_SQL_PASSWORD = $passwordAnterior
        $env:SGPLA_PRUEBAS_ADMIN_EMAIL = $correoAnterior
    }
    return
}

if (-not (Test-Path -LiteralPath $archivoEntorno)) {
    if ($Accion -ne 'Iniciar') {
        Write-Host 'El entorno de pruebas todavía no está configurado.'
        Write-Host 'Su aplicación habitual sigue en http://localhost:8080'
        return
    }
    if ([string]::IsNullOrWhiteSpace($CorreoAdministrador)) {
        $CorreoAdministrador = Read-Host 'Correo institucional del administrador de pruebas (sin contraseña)'
    }
    $CorreoAdministrador = $CorreoAdministrador.Trim().ToLowerInvariant()
    # Impide caracteres interpretables por SQLCMD, SQL o el formato de archivo env.
    if ($CorreoAdministrador.Length -gt 255 -or $CorreoAdministrador -notmatch '^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$') {
        throw 'El correo no es válido. Use su correo institucional completo.'
    }
    New-Item -ItemType Directory -Path $carpetaLocal -Force | Out-Null
    $passwordPruebas = 'Sgpla_' + [Guid]::NewGuid().ToString('N') + '!9a'
    $configuracion = "SGPLA_PRUEBAS_SQL_PASSWORD=$passwordPruebas`nSGPLA_PRUEBAS_ADMIN_EMAIL=$CorreoAdministrador`n"
    [IO.File]::WriteAllText($archivoEntorno, $configuracion, [Text.UTF8Encoding]::new($false))
    Write-Host 'Configuración privada creada. Conserve .local/sgpla-pruebas/entorno.env para reutilizar este entorno.'
}
elseif (-not [string]::IsNullOrWhiteSpace($CorreoAdministrador)) {
    $lineaGuardada = Get-Content -LiteralPath $archivoEntorno | Where-Object { $_ -like 'SGPLA_PRUEBAS_ADMIN_EMAIL=*' }
    if ($lineaGuardada -ne ('SGPLA_PRUEBAS_ADMIN_EMAIL=' + $CorreoAdministrador.Trim().ToLowerInvariant())) {
        throw 'El entorno ya tiene otro administrador configurado. No se modificó; gestione las cuentas desde la aplicación de pruebas.'
    }
}

# Evita que variables heredadas sustituyan silenciosamente la configuración privada.
$passwordPrevio = $env:SGPLA_PRUEBAS_SQL_PASSWORD
$correoPrevio = $env:SGPLA_PRUEBAS_ADMIN_EMAIL
try {
    $env:SGPLA_PRUEBAS_SQL_PASSWORD = $null
    $env:SGPLA_PRUEBAS_ADMIN_EMAIL = $null
    Invoke-Compose -Argumentos @('config', '--quiet')
    if ($Accion -eq 'Estado') {
        Invoke-Compose -Argumentos @('ps', '--all')
        Write-Host 'Pruebas: http://localhost:8081 | Habitual: http://localhost:8080'
        return
    }
    if ($Accion -in @('Detener', 'Volver')) {
        Invoke-Compose -Argumentos @('stop')
        Write-Host 'Pruebas detenido. Sus datos y documentos de pruebas se conservaron.'
        if ($Accion -eq 'Volver') {
            # Recupera también el caso en que se eliminaron contenedores con down,
            # pero se conservó el volumen. No ejecuta db-init ni reconstruye imágenes.
            $composeHabitual = Join-Path $raizProyecto 'docker-compose.yml'
            $volumenHabitual = Join-Path $raizProyecto 'docker-compose.volver.yml'
            & docker compose --project-directory $raizProyecto -p sistema_sgpla -f $composeHabitual -f $volumenHabitual up -d --no-deps --no-build --wait --wait-timeout 240 db
            if ($LASTEXITCODE -ne 0) { throw 'No se pudo iniciar SQL habitual con su volumen existente. No se crea una base vacía como alternativa.' }
            & docker compose --project-directory $raizProyecto -p sistema_sgpla -f $composeHabitual -f $volumenHabitual up -d --no-deps --no-build backend
            if ($LASTEXITCODE -ne 0) { throw 'SQL habitual inició, pero no se pudo iniciar su aplicación con la imagen existente.' }
        }
        Write-Host 'Sus datos habituales: http://localhost:8080'
        return
    }

    Write-Host 'Iniciando SOLO sgpla-pruebas. La aplicación y base habituales no se modifican.'
    Invoke-Compose -Argumentos @('build', 'db-init', 'backend')
    Invoke-Compose -Argumentos @('up', '-d', '--wait', '--wait-timeout', '240', 'db')
    Invoke-Compose -Argumentos @('run', '--rm', '--no-deps', 'db-init')
    # Lee el secreto dentro del contenedor: no se pasa como argumento ni se imprime.
    $comandoSql = 'SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b -d GestionDePlazasBD -v Administrador="$SGPLA_TEST_ADMIN_EMAIL" -i /bootstrap/inicializar-acceso.sql'
    Invoke-Compose -Argumentos @('exec', '-T', 'db', '/bin/bash', '-c', $comandoSql)
    Invoke-Compose -Argumentos @('up', '-d', '--no-deps', 'backend')

    $disponible = $false
    for ($intento = 0; $intento -lt 20; $intento++) {
        try {
            $respuesta = Invoke-WebRequest -Uri 'http://localhost:8081/Login' -UseBasicParsing -TimeoutSec 3
            if ($respuesta.StatusCode -eq 200 -and $respuesta.Content -match 'Entorno de pruebas') {
                $disponible = $true
                break
            }
        } catch { }
        Start-Sleep -Seconds 2
    }
    if (-not $disponible) {
        throw 'El contenedor inició, pero no se pudo verificar la página de pruebas. Revise Estado y los logs del proyecto sgpla-pruebas; el entorno habitual sigue intacto.'
    }
    Write-Host ''
    Write-Host 'LISTO: http://localhost:8081/Login'
    Write-Host 'Entre con el correo institucional configurado y su contraseña habitual de LDAP.'
    Write-Host 'Base nueva sin registros operativos. Cree áreas, entidades, cuentas y catálogos desde la aplicación.'
    Write-Host 'Sus registros anteriores siguen en http://localhost:8080'
    Write-Host 'No se reinicia ni vacía esta base al volver a ejecutar Iniciar.'
}
finally {
    $env:SGPLA_PRUEBAS_SQL_PASSWORD = $passwordPrevio
    $env:SGPLA_PRUEBAS_ADMIN_EMAIL = $correoPrevio
}
