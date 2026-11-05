param([string]$Context = 'docker-desktop')
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)
function Run([string]$exe, [string[]]$arguments) {
    & $exe @arguments
    if ($LASTEXITCODE -ne 0) { throw "$exe falló con código $LASTEXITCODE" }
}
function K([string[]]$arguments) { Run kubectl (@('--context',$Context,'-n','market-rubik') + $arguments) }
# Comprueba si un recurso existe sin que stderr se convierta en error terminante (Windows PowerShell 5.1).
function Exists([string[]]$arguments) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & kubectl @arguments 2>&1 | Out-Null
    $found = ($LASTEXITCODE -eq 0)
    $ErrorActionPreference = $previous
    return $found
}
Run docker @('build','-t','market-rubik-web:parte1','Market Rubik_SB')
Run docker @('build','-t','market-rubik-ai:parte1','ai_service')
if (!(Exists @('--context',$Context,'get','namespace','market-rubik'))) {
    Run kubectl @('--context',$Context,'create','namespace','market-rubik')
}
if (!(Exists @('--context',$Context,'-n','market-rubik','get','secret','market-settings'))) {
    if (!$env:MARKET_ADMIN_PASSWORD) { throw 'Define MARKET_ADMIN_PASSWORD para la cuenta admin.' }
    $dbPassword = [guid]::NewGuid().ToString('N')
    $settings = @('create','secret','generic','market-settings',
        "--from-literal=POSTGRES_PASSWORD=$dbPassword",'--from-literal=POSTGRES_USER=market',
        '--from-literal=POSTGRES_DB=market_rubik_db',
        "--from-literal=ConnectionStrings__DefaultConnection=Host=postgres;Database=market_rubik_db;Username=market;Password=$dbPassword",
        '--from-literal=ConnectionStrings__Redis=redis:6379',
        "--from-literal=Bootstrap__AdminPassword=$env:MARKET_ADMIN_PASSWORD")
    if ($env:MARKET_DEMO_PASSWORD) { $settings += "--from-literal=Bootstrap__DemoPassword=$env:MARKET_DEMO_PASSWORD" }
    if ($env:GOOGLE_CLIENT_ID) { $settings += "--from-literal=Authentication__Google__ClientId=$env:GOOGLE_CLIENT_ID" }
    if ($env:GOOGLE_CLIENT_SECRET) { $settings += "--from-literal=Authentication__Google__ClientSecret=$env:GOOGLE_CLIENT_SECRET" }
    K $settings
}
K @('apply','-f','k8s/infra.json')
K @('rollout','status','deployment/postgres','--timeout=180s')
K @('rollout','status','deployment/redis','--timeout=180s')
K @('delete','job','market-init','--ignore-not-found')
K @('apply','-f','k8s/init.json')
K @('wait','--for=condition=complete','job/market-init','--timeout=240s')
K @('apply','-f','k8s/web.json')
K @('rollout','restart','deployment/web')
K @('rollout','status','deployment/web','--timeout=180s')
K @('get','pods,services')
