# build-all.ps1
$repoRoot = Get-Location

# названия каталогов и теги образов (отредактируйте теги при необходимости)
$services = @{
    "Projects" = "freelance/projects-api:dev"
    "Users"    = "freelance/users-api:dev"
    "Tasks"    = "freelance/tasks-api:dev"
    "Gateway"  = "freelance/gateway:dev"
    "frontend" = "freelance/frontend:dev"
}

foreach ($svc in $services.Keys) {
    $svcPath = Join-Path $repoRoot $svc
    if (-not (Test-Path $svcPath)) {
        Write-Warning "$svcPath не найден, пропускаю..."
        continue
    }

    # убеждаемся, что в папке есть Dockerfile
    $dockerfile = Join-Path $svcPath "Dockerfile"
    if (-not (Test-Path $dockerfile)) {
        Write-Warning "Dockerfile в $svcPath не найден, пропускаю..."
        continue
    }

    Write-Host "Собираю $svc -> $($services[$svc]) (контекст: $svcPath)"
    Push-Location $svcPath
    & minikube image build -f Dockerfile -t $services[$svc] .
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Сборка $svc завершилась с кодом $LASTEXITCODE"
        Pop-Location
        break
    }
    Pop-Location
}
