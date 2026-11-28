window.addEventListener("error", function (event) {
    const image = event.target;
    if (image instanceof HTMLImageElement &&
        !image.dataset.fallbackApplied &&
        !image.src.endsWith("/images/products/nike_air_force_1.png")) {
        image.dataset.fallbackApplied = "true";
        image.src = "/images/products/nike_air_force_1.png";
    }
}, true);
