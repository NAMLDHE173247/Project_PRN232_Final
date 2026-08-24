(() => {
  const isOffline = document.body.dataset.offline === "true";

  const ensureOfflineBanner = () => {
    if (document.querySelector(".offline-banner")) return;
    const banner = document.createElement("div");
    banner.className = "offline-banner";
    banner.setAttribute("role", "status");
    banner.innerHTML = "<strong>Offline Mode</strong><span>Không có kết nối mạng/API. Các thao tác thay đổi dữ liệu đã bị vô hiệu hóa.</span>";
    document.querySelector(".admin-container")?.prepend(banner);
  };

  const applyOfflineState = (offline) => {
    document.body.dataset.offline = offline ? "true" : "false";
    if (offline) ensureOfflineBanner();
    document.querySelectorAll('form[method="post"]:not([data-offline-allow="true"]) button, form[method="post"]:not([data-offline-allow="true"]) input[type="submit"]').forEach((element) => {
      element.disabled = offline;
      element.title = offline ? "Không khả dụng khi hệ thống ngoại tuyến" : "";
    });
  };

  applyOfflineState(isOffline || !navigator.onLine);
  window.addEventListener("offline", () => applyOfflineState(true));
  window.addEventListener("online", () => applyOfflineState(false));
})();

(() => {
  const root = document.documentElement;
  const saved = localStorage.getItem("admin-theme");
  const preferredDark = window.matchMedia?.("(prefers-color-scheme: dark)").matches;
  root.dataset.theme = saved ?? (preferredDark ? "dark" : "light");
  document.getElementById("theme-toggle")?.addEventListener("click", () => {
    root.dataset.theme = root.dataset.theme === "dark" ? "light" : "dark";
    localStorage.setItem("admin-theme", root.dataset.theme);
  });
})();

(() => {
  if (document.body.dataset.adminAuthenticated !== "true" || !window.signalR) return;

  const container = document.createElement("div");
  container.className = "admin-toast-container";
  container.setAttribute("aria-live", "polite");
  container.setAttribute("aria-atomic", "true");
  document.body.appendChild(container);

  const showToast = (notification) => {
    const toast = document.createElement("div");
    const type = notification?.type === "error" ? "danger" : (notification?.type ?? "success");
    toast.className = `toast align-items-center text-bg-${type} border-0`;
    toast.setAttribute("role", "status");
    toast.innerHTML = `<div class="d-flex"><div class="toast-body"></div><button type="button" class="btn-close btn-close-white me-2 m-auto" aria-label="Đóng thông báo"></button></div>`;
    toast.querySelector(".toast-body").textContent = notification?.message ?? "Có cập nhật mới.";
    toast.querySelector("button").addEventListener("click", () => toast.remove());
    container.appendChild(toast);

    if (window.bootstrap?.Toast) {
      const instance = bootstrap.Toast.getOrCreateInstance(toast, { autohide: true, delay: 8000 });
      instance.show();
      toast.addEventListener("hidden.bs.toast", () => toast.remove(), { once: true });
    } else {
      window.setTimeout(() => toast.remove(), 8000);
    }
  };

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/admin-notifications")
    .withAutomaticReconnect()
    .build();
  connection.on("toast", (notification) => {
    window.setTimeout(() => showToast(notification), 300);
  });
  connection.start().catch(() => {
    // Toast notifications are optional; the existing TempData alerts remain available.
  });
})();
