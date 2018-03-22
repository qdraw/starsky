// Write your JavaScript code.


var allItems =   document.querySelectorAll(".halfitem .caption h3");
if(allItems.length >= 1) {
    for(var i = 0; i < allItems.length; i++) {
        document.querySelectorAll(".halfitem .caption h3")[i].innerHTML = allItems[i].innerHTML.replace(".jpg","");
    }
}
console.log();