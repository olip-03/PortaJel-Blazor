function imgloaded(element) {
    element.style.opacity = "1"; // Make the full image visible
    
}
function InitImageLoader(itemId) {
    var image = document.getElementById(itemId);

    if (image.complete) {
        imaage.style.opacity = "1"; // Make the full image visible
    } else {
        image.addEventListener("load", function () {
            image.style.opacity = "1"; // Make the full image visible
        }, false);
    }
}
function InitAllImages() {
    img = document.getElementsByTagName("img");
    for (i = 0; i < img.length; i++) {
        if (!img[i].src.includes("data:image/png;base64") &&
            img[i].classList.contains("album-cover-img")) {
            console.log(img[i].src);

            if (img[i].complete) {
                img[i].style.opacity = "1"; // Make the full image visible
            } else {
                // img[i].addEventListener("load", imgloaded);
                img[i].addEventListener("load", (event) => {
                    const image = event.target;
                    image.style.opacity = "1"; // Make the full image visible
                }, false);
            }
        }  
    }
}
