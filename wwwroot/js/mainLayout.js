// Define variables to store previous scroll position
let wScrollPrev = window.scrollY || document.documentElement.scrollTop;
window.addEventListener('scroll', function () {
    let toolbar = document.getElementById("mainLayoutToolbar");
    let libToolbar = this.document.getElementById("libraryToolbar");
    if (toolbar) {
        let wScrollCurrent = window.scrollY || document.documentElement.scrollTop;
        let wScrollDiff = parseFloat(wScrollPrev) - parseFloat(wScrollCurrent);
        let toolbarTop = parseFloat(toolbar.style.top);

        if (!toolbarTop) {
            // Fix NaN
            toolbarTop = 0;
        }

        if (wScrollDiff > 0) {
            // Scrolling Up
            if ((toolbarTop + wScrollDiff) >= 0) {
                toolbar.style.top = "0px";
            }
            else {
                toolbar.style.top = (toolbarTop + wScrollDiff) + 'px';
            }
        }
        else if (wScrollDiff < 0) {
            // Scrolling Down
            if (toolbarTop <= (toolbar.offsetHeight * -1)) {
                toolbar.style.top = "-" + toolbar.offsetHeight + 'px';
            }
            else {
                toolbar.style.top = (toolbarTop + wScrollDiff) + 'px';
            }
        }

        if (libToolbar)
        {
            var height = toolbar.offsetHeight;
            libToolbar.style.top = parseFloat(toolbar.style.top) + height + 'px';
        }

        wScrollPrev = wScrollCurrent;
    }
});
