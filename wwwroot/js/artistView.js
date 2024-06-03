
var stickyLockviewEngagedHeight = -1;
var stickyLockviewEngagedScroll

function InitalizeArtistView() {
    // Get the header element
    const header = document.getElementById('header-backgroundPlaceholder');
    const headerBackground = document.getElementById('header-backgroundImg');
    const headerText = document.getElementById('header-text');
    const playbackButton = document.getElementById('playback-controls-btns');
    const favoriteButton = document.getElementById('playback-controls-fav');

    // Function to update the header position
    function updateHeaderPosition() {
        if (header) {
            if (headerBackground) {
                const scrollFraction = Math.min(Math.max(window.scrollY / 100, 0), 1);

                header.style.opacity = scrollFraction;
                headerBackground.style.opacity = scrollFraction;
                if (headerText) {
                    if (scrollFraction == 1) {
                        headerText.style.opacity = scrollFraction;
                    }
                    else {
                        headerText.style.opacity = 0;
                    }
                }
            }
        }
    }

    // Event listener for the scroll event
    window.addEventListener('scroll', updateHeaderPosition);

    // Initial call to position the header correctly
    updateHeaderPosition();
}
function ToggleExpandDescription() {
    const additionalInfo = document.querySelector('.additional-info');
    const additionalInfoText = document.querySelector('.additional-info-text');

    // Temporarily disable CSS properties
    if (additionalInfo.style.overflow == 'visible') {
        // Set to strict layout
        additionalInfo.style.overflow = 'hidden';
        additionalInfo.style.borderRadius = '0.5rem';
        additionalInfo.style.backgroundColor = 'white';
        additionalInfo.style.padding = '1rem';
        additionalInfo.style.textOverflow = 'ellipsis';
        additionalInfo.style.display = '-webkit-box';
        additionalInfo.style.webkitLineClamp = '3';
        additionalInfo.style.webkitBoxOrient = 'vertical';
        additionalInfo.style.margin = '1rem';
        additionalInfo.style.boxShadow = '0.2rem 0.2rem 0.5rem rgba(0, 0, 0, 0.5)';

        additionalInfoText.style.background = '-webkit-linear-gradient(#eee, #eee, #333)';
        additionalInfoText.style.webkitBackgroundClip = 'text';
        additionalInfoText.style.webkitTextFillColor = 'transparent';
    }
    else {
        additionalInfo.style.overflow = 'visible';
        additionalInfo.style.textOverflow = 'unset';
        additionalInfo.style.display = 'block';
        additionalInfo.style.webkitLineClamp = 'unset';
        additionalInfo.style.webkitBoxOrient = 'unset';

        additionalInfoText.style.background = 'unset';
        additionalInfoText.style.webkitBackgroundClip = 'unset';
        additionalInfoText.style.webkitTextFillColor = 'unset';
    }
}
function MoveFavoriteButtonUp() {


}
