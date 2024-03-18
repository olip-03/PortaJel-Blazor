function InitImageLoader(itemId) {
    var image = document.getElementById(itemId);
    try {
        if (image.complete) {
            image.style.opacity = "1"; // Make the full image visible
        } else {
            image.addEventListener("load", function () {
                image.style.opacity = "1"; // Make the full image visible
            }, false);
        }
    } catch (e) {
        console.log(e);
    }
}
function imgOK(img) {
    if (!img.complete) {
        return false;
    }
    if (typeof img.naturalWidth != "undefined" && img.naturalWidth == 0) {
        return false;
    }
    return true;
}
function InitAllImages() {
    img = document.getElementsByTagName("img");
    for (i = 0; i < img.length; i++) {
        if (!img[i].src.includes("data:image/png;base64") &&
            img[i].classList.contains("album-cover-img")) {

            try {
                if (imgOK(img[i])) {
                    img[i].style.opacity = "1"; // Make the full image visible
                }
                else {
                    img[i].addEventListener("load", (event) => {
                        const image = event.target;
                        image.style.opacity = "1"; // Make the full image visible
                    }, false);
                }
            } catch (e) {
                console.log(e);
            }
            
        }  
    }
}
