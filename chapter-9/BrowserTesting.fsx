#r "../packages/Selenium.WebDriver/lib/net40/WebDriver.dll"
#r "../packages/canopy/lib/canopy.dll"

open canopy

// Replace the string below with the path to the Chrome driver
// in your system, which can be downloaded from:
// http://chromedriver.storage.googleapis.com/index.html
chromeDir <- "/path/to/chromedriver/"

// Start an instance of the Google Chrome browser
start chrome

// This is how you define a test
"taking canopy for a spin" &&& fun _ ->

    // Go to url
    // This is a sample url available to showcase Canopy’s capabilities
    url "http://lefthandedgoat.github.io/canopy/testpages/"

    // Assert that the element with an id of 'welcome' has the text 'Welcome'
    "#welcome" == "Welcome"

    // Assert that the element with an id of 'firstName' has the value 'John'
    "#firstName" == "John"

    // Change the value of element with an id of 'firstName' to 'Something Else'
    "#firstName" << "Something Else"

    // Verify another element's value, click a button and verify the element is updated
    "#button_clicked" == "button not clicked"
    click "#button"
    "#button_clicked" == "button clicked"

//run all tests
run()

// close all browsers
quit()
