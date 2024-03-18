// Define the target element you want to observe
const targetElement = document.querySelector('.your-div-class');

// Define the options for the Intersection Observer
const options = {
    root: null, // use the viewport as the root
    rootMargin: '0px', // no margin around the root
    threshold: 0.5 // trigger when 50% of the target is visible
};

// Create a new Intersection Observer
const observer = new IntersectionObserver((entries, observer) => {
    entries.forEach(entry => {
        // Check if the target element is intersecting with the viewport
        if (entry.isIntersecting) {
            // Call your function when the target element is visible
            yourFunction();

            // Stop observing after the function is called if needed
            // observer.unobserve(entry.target);
        }
    });
}, options);

// Start observing the target element
observer.observe(targetElement);

// Define your function to be called when the div is visible
function yourFunction() {
    // Your function logic here
    console.log('Div is visible!');
}
