import http from "k6/http";
import { check, fail } from "k6";
import { sleep } from "k6";

const baseUrl = __ENV.BASE_URL || "http://localhost";
const email = __ENV.ADMIN_EMAIL || "admin@gmail.com";
const password = __ENV.ADMIN_PASSWORD || "Admin@123";

export const options = {
  stages: [
    { duration: "15s", target: 5 },
    { duration: "30s", target: 20 },
    { duration: "15s", target: 0 },
  ],
  thresholds: {
    http_req_failed: ["rate<0.05"],
    http_req_duration: ["p(95)<1000"],
  },
};

export function setup() {
  const response = http.post(
    `${baseUrl}/api/auth/login`,
    JSON.stringify({ email, password }),
    { headers: { "Content-Type": "application/json" } },
  );

  const token = response.json("token");
  if (response.status !== 200 || !token) {
    fail(`Admin login failed with HTTP ${response.status}`);
  }
  return { token };
}

export default function (data) {
  const response = http.get(`${baseUrl}/api/admin/dashboard`, {
    headers: { Authorization: `Bearer ${data.token}` },
    tags: { endpoint: "admin-dashboard" },
  });

  check(response, {
    "dashboard returns 200": (result) => result.status === 200,
  });
  sleep(1);
}
