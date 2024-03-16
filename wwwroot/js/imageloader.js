function InitImageLoader(itemId) {
    var image = document.getElementById(itemId);

    function loaded() {
        image.style.opacity = "1"; // Make the full image visible
    }

    if (image.complete) {
        loaded();
    } else {
        image.addEventListener("load", loaded);
    }
};
