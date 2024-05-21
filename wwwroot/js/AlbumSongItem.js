function InitAllAlbumSongItem() {
    img = document.getElementsByClassName("albumSongItem");
    for (i = 0; i < img.length; i++) {
        const elmnt = img[i];
        if (elmnt) {
            elmnt.scroll({
                top: 0,
                left: 20000,
                behavior: "instant",
            });
        }
        elmnt.addEventListener("scroll", (event) => {

        });
        elmnt.addEventListener("scrollend", (event) => {
            elmnt.scroll({
                top: 0,
                left: 20000,
                behavior: "smooth",
            });
        });
    }
}

