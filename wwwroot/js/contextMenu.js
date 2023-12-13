function contextMenu(state) {
    menuElmnt = document.getElementById("context-menu");
    function OpenContextMenu() {
        menuElmnt.style.top = "0vh";
        menuElmnt.style.opacity = "100%";
    }
    function CloseContextMenu() {
        menuElmnt.style.top = "100vh";
        menuElmnt.style.opacity = "0%";
    }

    if (state == "open") {
        OpenContextMenu();
    }
    else {
        CloseContextMenu();
    }
}
