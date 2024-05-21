function InitAllAlbumSongItem() {
    img = document.getElementsByClassName("albumSongItem");
    function ScrollTo(elmnt, instant)
    {
        let overscrollCont = document.getElementById(elmnt.id + "-overscroll");
        if (overscrollCont) {
            if (instant == true) {
                elmnt.scroll({
                    top: 0,
                    left: overscrollCont.offsetWidth + 1,
                    behavior: "instant",
                });
                return;
            }
            elmnt.scroll({
                top: 0,
                left: overscrollCont.offsetWidth + 1,
                behavior: "smooth",
            });
            return;
        }

        console.error("Could not determine overscroll container for " + elmnt.id + "-overscroll");
        if (instant == true) {
            elmnt.scroll({
                top: 0,
                left: 20000,
                behavior: "instant",
            });
            return;
        }
        elmnt.scroll({
            top: 0,
            left: 20000,
            behavior: "smooth",
        });
        return;
    }
    for (i = 0; i < img.length; i++) {
        const elmnt = img[i];
        if (elmnt) {
            ScrollTo(elmnt, true)
        }
        elmnt.addEventListener("scroll", (event) => {

        });
        elmnt.addEventListener("scrollend", (event) => {
            ScrollTo(elmnt, false);
        });
    }
}

