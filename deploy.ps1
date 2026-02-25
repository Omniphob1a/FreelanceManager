<#
deploy.ps1 - полностью рабочая версия без ошибок синтаксиса и совместимая с minikube

Что делает:
1) Убеждается, что minikube запущен (docker driver)
2) Для каждого сервиса (Projects, Users, Tasks, Gateway, frontend) собирает образ в minikube через `minikube image build`
3) Применяет manifestsall.yaml
4) Ждёт готовности деплойментов в namespace 'app' и выводит полезную информацию
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# repo root
$repoRoot = Get-Location
Write-Host ("Repo root: " + $repoRoot)

$minikubeDriver = "docker"

# сервисы => теги
$services = @{
    "Projects" = "freelance/projects-api:dev"
    "Users"    = "freelance/users-api:dev"
    "Tasks"    = "freelance/tasks-api:dev"
    "Gateway"  = "freelance/gateway:dev"
    "frontend" = "freelance/frontend:dev"
}

# 1) Ensure minikube running
Write-Host "Ensuring minikube is running..."
try {
    $status = & minikube status --format '{{.Host}}' 2>$null
} catch {
    $status = ""
}

if ($status -ne "Running") {
    Write-Host ("Starting minikube with driver '" + $minikubeDriver + "'...")
    minikube start --driver=$minikubeDriver
} else {
    Write-Host "minikube already running."
}

# 2) Build images
foreach ($svc in $services.Keys) {
    $svcPath = Join-Path $repoRoot $svc
    if (-not (Test-Path $svcPath)) {
        Write-Warning ("Skipping " + $svc + ": folder not found (" + $svcPath + ")")
        continue
    }

    $dockerfilePath = Join-Path $svcPath "Dockerfile"
    if (-not (Test-Path $dockerfilePath)) {
        Write-Warning ("Skipping " + $svc + ": Dockerfile not found in " + $svcPath)
        continue
    }

    $tag = $services[$svc]

    Write-Host ""
    Write-Host ("=== Building " + $svc + " -> " + $tag + " ===")
    Push-Location $svcPath
    try {
        # убран --progress для совместимости
        & minikube image build -f Dockerfile -t $tag .
        if ($LASTEXITCODE -ne 0) {
            throw ("minikube image build returned exit code " + $LASTEXITCODE + " for " + $svc)
        }
        Write-Host ("Image built: " + $tag)
    } catch {
        Write-Error ("Ошибка сборки " + $svc + ": " + $_.Exception.Message)
        Pop-Location
        throw
    }
    Pop-Location
}

# 3) Apply manifests
$manifest = Join-Path $repoRoot "manifestsall.yaml"
if (-not (Test-Path $manifest)) {
    throw "manifestsall.yaml not found in " + $repoRoot
}

Write-Host ""
Write-Host ("Applying Kubernetes manifests: " + $manifest)
kubectl apply -f $manifest

# 4) Wait for deployments ready
Write-Host ""
Write-Host "Waiting for deployments to be ready in namespace 'app' (timeout 10m)..."
$deadline = (Get-Date).AddMinutes(10)
while ((Get-Date) -lt $deadline) {
    try {
        $items = kubectl -n app get deploy -o json 2>$null | ConvertFrom-Json
    } catch {
        Start-Sleep -Seconds 2
        continue
    }

    $notReady = @()
    foreach ($d in $items.items) {
        $name = $d.metadata.name
        $desired = if ($d.spec.replicas) { $d.spec.replicas } else { 1 }
        $available = if ($d.status.availableReplicas) { $d.status.availableReplicas } else { 0 }
        if ($available -lt $desired) {
            $notReady += $name
        }
    }

    if ($notReady.Count -eq 0) { break }

    Write-Host ("Waiting... not ready: " + ($notReady -join ', '))
    Start-Sleep -Seconds 5
}

Write-Host "Done waiting (или таймаут достигнут)."

# 5) Show info
Write-Host ""
Write-Host "Pods (namespace app):"
kubectl -n app get pods -o wide

Write-Host ""
Write-Host "Services (namespace app):"
kubectl -n app get svc

# Show URLs if available
try {
    $frontendUrl = & minikube service -n app frontend --url 2>$null
    if ($frontendUrl) { Write-Host ("`nFrontend URL: " + $frontendUrl) }
} catch { }

try {
    $gatewayUrl = & minikube service -n app gateway --url 2>$null
    if ($gatewayUrl) { Write-Host ("Gateway URL: " + $gatewayUrl) }
} catch { }

Write-Host "`nЕсли pods не Running: kubectl -n app describe pod <pod> и kubectl -n app logs <pod>"
Write-Host "`nDeploy script finished."
