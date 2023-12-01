var elmnt = null;
var musicElmnt = null;

var startTime = -1;
var endTime = 0;

var fistPos = -1;
var lastPos = 0;

var vh = null;
var openElement = false;

function dragElement(itemName) {
    if (elmnt == null) { elmnt = document.getElementById("music-player"); }
    if (musicElmnt == null) { musicElmnt = document.getElementById("music-screen"); }

    vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);

    if (itemName == "open") {
        openElement = true;
        closeDragElement();
        return;
    }
    if (itemName == "close") {
        openElement = false;
        closeDragElement();
        return;
    }
    if (itemName == "init") {
        return;
    }

    function dragMouseDown(e) {
        elmnt.style.transitionDuration = "0s";

        e = e || window.event;
        e.preventDefault();

        var touch = e.touches[0];

        pos4 = touch.clientY;
        fistPos = pos4;

        document.ontouchend = closeDragElement;
        // call a function whenever the cursor moves:
        document.ontouchmove = elementDrag;
    }

    function elementDrag(e) {
        elmnt.style.transitionDuration = "0s";
        musicElmnt.style.transitionDuration = "0s";

        //e = e || window.event;
        //e.preventDefault();
        var touch = e.touches[0];

        // calculate the new cursor position:
        //pos1 = pos3 - e.clientX;
        pos2 = pos4 - touch.clientY;
        //pos3 = e.clientX;
        pos4 = touch.clientY;

        if (startTime == -1) {
            startTime = Date.now();
        }

        if (fistPos == -1) {
            fistPos = pos4;
        }
        lastPos = pos4;

        setVis = ((vh - pos4) / vh) * 100;
        setText = fistPos + " : " + lastPos;

        if (pos4 <= vh / 2) {
            openElement = true;
        }
        else {
            openElement = false;
        }
        // set the element's new position:
        elmnt.style.top = (elmnt.offsetTop - pos2) + "px";
        musicElmnt.style.top = (elmnt.offsetTop - pos2) + "px";
        musicElmnt.style.display = "flex";
        musicElmnt.style.zIndex = "29";
        musicElmnt.style.opacity = setVis + "%";
    }

    function closeDragElement() {
        // stop moving when mouse button is released:
        document.ontouchend = null;
        document.ontouchmove = null;
        endTime = Date.now();

        transactionTime = endTime - startTime;

        if (transactionTime <= 100) {
            // Open if the element was 'tapped'
            if (fistPos != -1) {
                distance = fistPos - lastPos;
                if (distance < 0) { distance *= -1; }
                if (distance <= 20) {
                    openElement = true;
                }
            }
            // Open element if swiped up
            if (lastPos < fistPos) {
                openElement = true;
            }
        }

        // Set position
        elmnt.style.transitionDuration = "0.4s";
        musicElmnt.style.transitionDuration = "0.4s";
        fistPos = -1;
        startTime = -1;
        if (!openElement) {
            // Element is closed
            enableScroll();
            DotNet.invokeMethodAsync("PortaJel-Blazor", "SetPlayerClosed", "true");

            elmnt.style.top = "calc(100vh - 132px)";

            musicElmnt.style.top = "calc(100vh - 132px)";
            musicElmnt.style.opacity = "0%";
            musicElmnt.style.zIndex = "19";
        }
        else {
            // Element is open
            disableScroll();
            DotNet.invokeMethodAsync("PortaJel-Blazor", "SetPlayerOpen", "true");

            elmnt.style.top = "-70px";

            musicElmnt.style.top = "0px";
            musicElmnt.style.opacity = "100%";
            musicElmnt.style.zIndex = "29";
        }
    }

    function disableScroll() {
        // Get the current page scroll position
        scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        scrollLeft = window.pageXOffset || document.documentElement.scrollLeft,

            // if any scroll is attempted, set this to the previous value
            window.onscroll = function () {
                window.scrollTo(scrollLeft, scrollTop);
            };
    }

    function enableScroll() {
        window.onscroll = function () { };
    }

    var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
    elmnt.ontouchstart = dragMouseDown;
}
