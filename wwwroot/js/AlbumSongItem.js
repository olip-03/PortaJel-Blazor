function InitAllAlbumSongItem() {
    img = document.getElementsByClassName("albumSongItem");
    for (i = 0; i < img.length; i++) {
        const elmnt = img[i];
        if (elmnt) {
            dragElement(elmnt);
            function dragElement(elmnt) {
                var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
                elmnt.touchstart = dragMouseDown;
                function dragMouseDown(e) {
                    e = e || window.event;
                    e.preventDefault();
                    // get the mouse cursor position at startup:
                    pos3 = e.clientX;
                    pos4 = e.clientY;
                    document.touchend = closeDragElement;
                    // call a function whenever the cursor moves:
                    document.touchmove = elementDrag;
                }
                function elementDrag(e) {
                    e = e || window.event;
                    e.preventDefault();
                    // calculate the new cursor position:
                    pos1 = pos3 - e.clientX;
                    pos2 = pos4 - e.clientY;
                    pos3 = e.clientX;
                    pos4 = e.clientY;
                    // set the element's new position:
                    elmnt.style.top = (elmnt.offsetTop - pos2) + "px";
                    elmnt.style.left = (elmnt.offsetLeft - pos1) + "px";
                }
                function closeDragElement() {
                    // stop moving when mouse button is released:
                    document.touchend = null;
                    document.touchmove = null;
                }
            }
        }
    }
}

