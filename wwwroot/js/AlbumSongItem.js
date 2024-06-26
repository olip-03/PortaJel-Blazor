﻿function InitAllAlbumSongItem() {
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
            // .scrollLeft();
            let queueContainer = document.getElementById(elmnt.id + "-overscroll");
            let favContainer = document.getElementById(elmnt.id + "-overscrollFav");

            if (event.target.scrollLeft <= 15) {
                // bump the haptics
                // DotNet.invokeMethodAsync("BumpHaptics");
            }
            if (event.target.scrollLeft >= favContainer.offsetWidth + queueContainer.offsetWidth - 15) {
                // bump the haptics
                // DotNet.invokeMethodAsync("BumpHaptics");
            }
        });
        elmnt.addEventListener("scrollend", (event) => {

            let queueContainer = document.getElementById(elmnt.id + "-overscroll");
            let favContainer = document.getElementById(elmnt.id + "-overscrollFav");

            if (event.target.scrollLeft <= 30) {
                // Invoke add to queue function
                DotNet.invokeMethodAsync("BumpHaptics");
            }
            if (event.target.scrollLeft >= favContainer.offsetWidth + queueContainer.offsetWidth - 30) {
                // Invoke favourite toggle funciton
                DotNet.invokeMethodAsync("BumpHaptics");
            }
            ScrollTo(elmnt, false);
        });
    }
}

function InitAlbumSongItem(pDotNetReference, targetId) {
    function ScrollTo(elmnt, instant) {
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

    const elmnt = document.getElementById(targetId);
    if (elmnt) {
        ScrollTo(elmnt, true);

        elmnt.addEventListener("scroll", (event) => {
            // .scrollLeft();
            let queueContainer = document.getElementById(elmnt.id + "-overscroll");
            let favContainer = document.getElementById(elmnt.id + "-overscrollFav");

            if (event.target.scrollLeft <= 15) {
                // bump the haptics
                // DotNet.invokeMethodAsync("BumpHaptics");
            }
            if (event.target.scrollLeft >= favContainer.offsetWidth + queueContainer.offsetWidth - 15) {
                // bump the haptics
                // DotNet.invokeMethodAsync("BumpHaptics");
            }
        });
        elmnt.addEventListener("scrollend", (event) => {

            let queueContainer = document.getElementById(elmnt.id + "-overscroll");
            let favContainer = document.getElementById(elmnt.id + "-overscrollFav");

            if (event.target.scrollLeft <= 30) {
                // Invoke add to queue function
                pDotNetReference.invokeMethodAsync("QueueSong");
            }
            if (event.target.scrollLeft >= favContainer.offsetWidth + queueContainer.offsetWidth - 30) {
                // Invoke favourite toggle funciton
                pDotNetReference.invokeMethodAsync("FavouriteSong");
            }
            ScrollTo(elmnt, false);
        });
    }

}