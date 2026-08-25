[CmdletBinding()]
param(
    [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = "Stop"
$requiredVariables = @("SA_PASSWORD", "JWT_KEY", "ADMIN_PASSWORD")
$missingVariables = $requiredVariables | Where-Object { [string]::IsNullOrWhiteSpace((Get-Item "Env:$($_)" -ErrorAction SilentlyContinue).Value) }
if ($missingVariables.Count -gt 0 -and -not (Test-Path ".env")) {
    throw "Missing deployment secrets: $($missingVariables -join ', '). Set them in .env or the environment."
}

$services = @("api1", "api2")

Write-Host "Building the API image..."
docker compose build api1 api2

foreach ($service in $services) {
    Write-Host "Updating $service..."
    docker compose up -d --no-deps --force-recreate $service

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        Start-Sleep -Seconds 3
        $containerId = docker compose ps -q $service
        if ([string]::IsNullOrWhiteSpace($containerId)) {
            throw "$service did not start."
        }

        $health = docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' $containerId
        Write-Host "$service health: $health"
        if ($health -eq "healthy") { break }
        if ($health -eq "unhealthy" -or (Get-Date) -ge $deadline) {
            docker compose logs --tail=80 $service
            throw "$service failed its health check."
        }
    } while ($true)
}

Write-Host "Checking the public endpoint..."
try {
    Invoke-WebRequest -UseBasicParsing -Uri "http://localhost/health" -TimeoutSec 15 | Out-Null
    Write-Host "Rolling update completed successfully."
}
catch {
    docker compose logs --tail=80 nginx
    throw "The public health endpoint is unavailable after the update."
}
