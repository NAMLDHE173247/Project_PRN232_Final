#!/usr/bin/env bash
set -Eeuo pipefail

if [[ ! -f .env && ( -z "${SA_PASSWORD:-}" || -z "${JWT_KEY:-}" || -z "${ADMIN_PASSWORD:-}" ) ]]; then
  echo "Missing deployment secrets. Set SA_PASSWORD, JWT_KEY and ADMIN_PASSWORD in .env or the environment." >&2
  exit 1
fi

timeout_seconds="${1:-120}"
services=(api1 api2)

for service in "${services[@]}"; do
  echo "Updating ${service}..."
  docker compose up -d --no-deps --force-recreate "${service}"

  deadline=$(( $(date +%s) + timeout_seconds ))
  while true; do
    container_id="$(docker compose ps -q "${service}")"
    if [[ -z "${container_id}" ]]; then
      docker compose logs --tail=80 "${service}" || true
      echo "${service} did not start." >&2
      exit 1
    fi

    health="$(docker inspect --format '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' "${container_id}")"
    echo "${service} health: ${health}"
    [[ "${health}" == "healthy" ]] && break
    if [[ "${health}" == "unhealthy" || $(date +%s) -ge ${deadline} ]]; then
      docker compose logs --tail=80 "${service}" || true
      echo "${service} failed its health check." >&2
      exit 1
    fi
    sleep 3
  done
done

curl --fail --silent --show-error --max-time 15 http://localhost/health >/dev/null
echo "Rolling update completed successfully."
