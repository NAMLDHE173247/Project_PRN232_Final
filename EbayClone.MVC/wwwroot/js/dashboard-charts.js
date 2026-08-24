(() => {
  if (!window.Chart) return;

  const css = getComputedStyle(document.documentElement);
  const colors = [
    css.getPropertyValue("--primary").trim() || "#2457d6",
    css.getPropertyValue("--warning").trim() || "#a15c00"
  ];

  const createChart = (id) => {
    const canvas = document.getElementById(id);
    if (!canvas) return;

    const labels = JSON.parse(canvas.dataset.labels ?? "[]");
    const values = JSON.parse(canvas.dataset.values ?? "[]");
    new Chart(canvas, {
      type: "doughnut",
      data: {
        labels,
        datasets: [{ data: values, backgroundColor: colors, borderWidth: 0 }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { position: "bottom", labels: { color: css.getPropertyValue("--text").trim() } } }
      }
    });
  };

  createChart("user-status-chart");
  createChart("product-status-chart");
})();
