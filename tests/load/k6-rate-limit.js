import http from "k6/http";
import { check } from "k6";

const baseUrl = __ENV.BASE_URL || "http://localhost";
const email = __ENV.ADMIN_EMAIL || "admin@gmail.com";
const password = __ENV.ADMIN_PASSWORD || "Admin@123";

export const options = {
  scenarios: {
    login_burst: {
      executor: "constant-arrival-rate",
      rate: 20,
      timeUnit: "1s",
      duration: "20s",
      preAllocatedVUs: 10,
      maxVUs: 30,
    },
  },
  thresholds: {
    checks: ["rate>0.95"],
  },
};

export default function () {
  const response = http.post(
    `${baseUrl}/api/auth/login`,
    JSON.stringify({ email, password }),
    { headers: { "Content-Type": "application/json" } },
  );

  check(response, {
    "login is accepted or rate limited": (result) =>
      result.status === 200 || result.status === 429,
    "rate limit is observable": (result) => result.status === 429,
  });
}
