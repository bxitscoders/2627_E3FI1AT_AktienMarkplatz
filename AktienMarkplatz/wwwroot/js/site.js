// Zeichnet alle Kursdiagramme auf der Seite (siehe Pages/Shared/_Kursdiagramm.cshtml).
// Jedes <canvas class="kursdiagramm__flaeche"> bringt seine Punkte als JSON in data-punkte mit.
document.querySelectorAll("canvas.kursdiagramm__flaeche").forEach(function (canvas) {
    if (typeof Chart === "undefined") return;

    const punkte = JSON.parse(canvas.dataset.punkte);
    const farbName = canvas.dataset.steigt === "true" ? "--gain" : "--loss";
    const farbe = getComputedStyle(document.documentElement).getPropertyValue(farbName).trim();

    new Chart(canvas, {
        type: "line",
        data: {
            labels: punkte.map(function (punkt) { return punkt.datum; }),
            datasets: [{
                data: punkte.map(function (punkt) { return punkt.wert; }),
                borderColor: farbe,
                borderWidth: 2,
                pointRadius: 0,
                tension: 0.2
            }]
        },
        options: {
            maintainAspectRatio: false,
            interaction: { mode: "index", intersect: false },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (eintrag) {
                            return eintrag.parsed.y.toFixed(2).replace(".", ",") + " $";
                        }
                    }
                }
            },
            scales: {
                x: { grid: { display: false }, ticks: { maxTicksLimit: 6 } }
            }
        }
    });
});
