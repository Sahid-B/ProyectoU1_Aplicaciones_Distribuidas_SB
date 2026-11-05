Write-Host "1. Inicializando Docker Swarm..." -ForegroundColor Cyan
docker swarm init 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker Swarm ya estaba inicializado o falló." -ForegroundColor Yellow
}

Write-Host "2. Configurando secretos de Docker..." -ForegroundColor Cyan
# Limpiar el secreto si ya existe
docker secret inspect db_password *> $null
if ($LASTEXITCODE -ne 0) {
    if (!$env:MARKET_DB_PASSWORD) { throw "Define MARKET_DB_PASSWORD" }
    $env:MARKET_DB_PASSWORD | docker secret create db_password -
}

Write-Host "3. Construyendo imagen de la IA..." -ForegroundColor Cyan
docker build -t market_rubik_ai:latest ./ai_service

Write-Host "4. Construyendo imagen de la aplicación web..." -ForegroundColor Cyan
docker build -t market_rubik_web:latest "./Market Rubik_SB"

Write-Host "5. Desplegando el stack en Swarm..." -ForegroundColor Cyan
docker stack deploy -c docker-stack.yml proyecto

Write-Host "--------------------------------------------------------" -ForegroundColor Green
Write-Host "Despliegue finalizado." -ForegroundColor Green
Write-Host "Revisa los servicios con: docker service ls" -ForegroundColor Yellow
Write-Host "Revisa los contenedores con: docker stack ps proyecto" -ForegroundColor Yellow
