# setup.ps1
# PowerShell script: start minikube, build local images into minikube and apply manifests
# -------------- IMPORTANT --------------
# Save this file as UTF-8 with BOM (в VSCode: Save with Encoding -> "UTF-8 with BOM")
# Run in PowerShell (not cmd): .\setup.ps1

$ErrorActionPreference = 'Stop'

# Параметры
$minikubeProfile = "minikube"
$nodes = 3
$driver = "docker"
$memory = "8192"
$cpus = 4
$namespace = "fm-app"

function Ensure-CommandExists {
    param([string]$cmd)
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Error "Команда '$cmd' не найдена в PATH. Установите её и повторите."
        exit 1
    }
}

# проверки
Ensure-CommandExists -cmd "minikube"
Ensure-CommandExists -cmd "kubectl"

Write-Host "Starting minikube (driver=$driver, nodes=$nodes)..."
# старт minikube
& minikube start --driver=$driver --nodes=$nodes --memory=$memory --cpus=$cpus

# убедиться, что папка ./k8s есть
$k8sDir = Join-Path (Get-Location) "k8s"
if (-not (Test-Path -Path $k8sDir)) {
    New-Item -ItemType Directory -Path $k8sDir | Out-Null
    Write-Host "Папка ./k8s создана."
    Write-Host "Поместите в неё YAML-манифесты (namespace.yaml, projects-db.yaml, users-db.yaml, tasks-db.yaml, hangfire-db.yaml, redis.yaml, zookeeper-kafka.yaml, projects-api.yaml, users-api.yaml, tasks-api.yaml, gateway.yaml, frontend.yaml, monitoring.yaml, kafka-topics-job.yaml)."
    Read-Host -Prompt "Нажмите Enter когда файлы будут готовы..."
}

# Список собираемых образов: настройте пути если надо
$images = @(
    @{ path = "Users"; tag = "fm/users:local" },
    @{ path = "Projects"; tag = "fm/projects:local" },
    @{ path = "Tasks"; tag = "fm/tasks:local" },
    @{ path = "Gateway"; tag = "fm/gateway:local" },
    @{ path = "frontend"; tag = "fm/frontend:local" }
)

foreach ($img in $images) {
    $p = $img.path
    $t = $img.tag

    if (-not (Test-Path -Path $p)) {
        Write-Warning "Путь '$p' не найден — пропускаю образ $t. Если Dockerfile в другом месте, поправьте путь в скрипте."
        continue
    }

    $dockerfile = Join-Path $p "Dockerfile"
    if (-not (Test-Path -Path $dockerfile)) {
        Write-Warning "Dockerfile не найден по пути '$dockerfile' — пропускаю образ $t."
        continue
    }

    Write-Host "Building image '$t' from folder '$p' (Dockerfile: $dockerfile) ..."
    # Используем minikube image build — помещает образ прямо в minikube
    & minikube image build --tag $t --file $dockerfile $p
}

# Применение манифестов
Write-Host "Применяю манифесты из ./k8s ..."
# namespace сначала (если есть)
$nsFile = Join-Path $k8sDir "namespace.yaml"
if (Test-Path $nsFile) {
    & kubectl apply -f $nsFile
} else {
    Write-Warning "namespace.yaml не найден в ./k8s — предполагаю, что namespace уже есть или будет создан вручную."
}

$manifestFiles = @(
    "projects-db.yaml",
    "users-db.yaml",
    "tasks-db.yaml",
    "hangfire-db.yaml",
    "redis.yaml",
    "zookeeper-kafka.yaml",
    "projects-api.yaml",
    "users-api.yaml",
    "tasks-api.yaml",
    "gateway.yaml",
    "frontend.yaml",
    "monitoring.yaml",
    "kafka-topics-job.yaml"
)

foreach ($f in $manifestFiles) {
    $full = Join-Path $k8sDir $f
    if (Test-Path $full) {
        Write-Host "Applying $f ..."
        & kubectl apply -n $namespace -f $full
    } else {
        Write-Warning "Манифест '$f' не найден в ./k8s — пропускаю."
    }
}

Write-Host ""
Write-Host "Манифесты применены (частично или полностью). Состояние подов:"
& kubectl get pods -n $namespace

Write-Host ""
Write-Host "Примеры команд для доступа к сервисам (используйте в отдельных PowerShell окнах):"
Write-Host "kubectl port-forward svc/frontend -n $namespace 8080:80"
Write-Host "kubectl port-forward svc/gateway -n $namespace 5000:8080"
Write-Host "kubectl port-forward svc/projects-api -n $namespace 5001:80"
Write-Host "kubectl port-forward svc/users-api -n $namespace 5002:80"
Write-Host "kubectl port-forward svc/tasks-api -n $namespace 5003:8080"
Write-Host "kubectl port-forward svc/projects-db -n $namespace 5432:5432"
Write-Host "kubectl port-forward svc/redis -n $namespace 6379:6379"
Write-Host "kubectl port-forward svc/kafka -n $namespace 29092:9092  # локальный порт 29092 -> kafka:9092"
Write-Host ""
Write-Host "Если нужно — я могу добавить в скрипт автоматическое создание этих YAML-файлов перед apply (чтобы вам не копировать вручную)."
