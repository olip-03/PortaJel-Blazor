function LockScroll() {
    const scrollY = window.scrollY;
    const body = document.body;
    body.style.top = `-${scrollY}px`
    body.style.position = 'fixed';
}
function UnlockScoll() {
    const body = document.body;
    const scrollY = body.style.top;
    body.style.position = '';
    body.style.top = '';
    window.scrollTo(0, parseInt(scrollY || '0') * -1);
}