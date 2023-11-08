const isDragging = new Boolean(false);
function dragElement(itemName) {
    elmnt = document.getElementById(itemName);
    debugText = document.getElementById("dragBegugText");
    function dragMouseDown(e) {
        //isDragging = true;

        e = e || window.event;
        e.preventDefault();
        // get the mouse cursor position at startup:
        //pos3 = e.clientX;

        var touch = e.touches[0];
        //var x = touch.pageX;
        //var y = touch.pageY;
        // or taking offset into consideration
        //var x_2 = touch.pageX - canvas.offsetLeft;
        //var y_2 = touch.pageY - canvas.offsetTop;

        pos4 = touch.clientY;
        document.onmouseup = closeDragElement;
        // call a function whenever the cursor moves:
        document.ontouchmove = elementDrag;
    }

    function elementDrag(e) {
        e = e || window.event;
        e.preventDefault();
        var touch = e.touches[0];

        // calculate the new cursor position:
        //pos1 = pos3 - e.clientX;
        pos2 = pos4 - touch.clientY;
        //pos3 = e.clientX;
        pos4 = touch.clientY;
        // set the element's new position:
        elmnt.style.top = (elmnt.offsetTop - pos2) + "px";
        debugText.innerHTML = (elmnt.offsetTop - pos2) + "px";
    }

    function closeDragElement() {
        // stop moving when mouse button is released:
        document.ontouchend = null;
        document.ontouchmove = null;
    }

    var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
    elmnt.ontouchstart = dragMouseDown;
}
function stopDrag() {
    //isDragging = false;
    debugText = document.getElementById("dragBegugText");
    debugText.innerHTML = "Drag Cancelled."

    elmnt = document.getElementById("music-player");
    elmnt.style.top = "-60px";
    elmnt.style.bottom = "60px";

    document.ontouchend = null;
    document.ontouchmove = null;
}