function SearchPage_FocusOnInput() {
    var searchInput = document.getElementById("SearchPage_SearchInput");

    // Focus on the input initially
    searchInput.focus();

    // Check if the input loses focus and refocus it if needed
    var intervalId = setInterval(function () {
        if (document.activeElement !== searchInput) {
            searchInput.focus();
        }
    }, 100); // Check every 100 milliseconds

    // Clear the interval when the input is focused
    searchInput.addEventListener("focus", function () {
        clearInterval(intervalId);
    });
}