# deploy_all_minikube.ps1
param(
    [string]$Namespace = "lab"
)

# 0. Проверки
Write-Host "Context:" (kubectl config current-context)
Write-Host "Minikube status:"
minikube status

# 1. Create namespace
kubectl create ns $Namespace --dry-run=client -o yaml | kubectl apply -f -

# 2. Build Docker images locally (Docker Desktop) — adjust paths if needed
Write-Host "Building Docker images..."
docker build -t fm-users-api:local -f Users/Users.Api/Dockerfile Users
docker build -t fm-tasks-api:local -f Tasks/Tasks.Api/Dockerfile Tasks
docker build -t fm-projects-api:local -f Projects/Projects.Api/Dockerfile Projects
docker build -t fm-gateway:local -f Gateway/Gateway/Dockerfile Gateway
docker build -t fm-frontend:local -f frontend/Dockerfile frontend

# 3. Load images into Minikube (works on multi-node)
Write-Host "Loading images into Minikube..."
minikube image load fm-users-api:local
minikube image load fm-tasks-api:local
minikube image load fm-projects-api:local
minikube image load fm-gateway:local
minikube image load fm-frontend:local

# 4. Apply manifest
Write-Host "Applying all-in-one Kubernetes manifest..."
kubectl apply -n $Namespace -f all-infra-apps.yaml

# 5. Wait for key pods (quick check)
Write-Host "Waiting for core infra pods to become ready (zookeeper, kafka, redis, grafana, prometheus)..."
kubectl wait --for=condition=ready pod -l app=zookeeper -n $Namespace --timeout=120s || true
kubectl wait --for=condition=ready pod -l app=kafka -n $Namespace --timeout=120s || true
kubectl wait --for=condition=ready pod -l app=redis -n $Namespace --timeout=120s || true
kubectl wait --for=condition=ready deployment/grafana -n $Namespace --timeout=120s || true
kubectl wait --for=condition=ready deployment/prometheus -n $Namespace --timeout=120s || true

Write-Host "Done. Check status with: kubectl get pods -n $Namespace"
kubectl get pods -n $Namespace
