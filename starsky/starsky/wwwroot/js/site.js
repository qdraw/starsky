// Write your JavaScript code.


var FileItems =   document.querySelectorAll(".halfitem .caption h2");
if(FileItems.length >= 1) {
    for(var i = 0; i < FileItems.length; i++) {
        FileItems[i].innerHTML = FileItems[i].innerHTML.replace(".jpg","");
        if(FileItems[i].innerHTML.length >= 23){
            FileItems[i].innerHTML = FileItems[i].innerHTML.substr(0,23) + "…";
        }
    }
}
var tagItems =   document.querySelectorAll(".halfitem .caption p");
if(tagItems.length >= 1) {
    for(var i = 0; i < tagItems.length; i++) {
        if(tagItems[i].innerHTML.length >= 36){
            tagItems[i].innerHTML = tagItems[i].innerHTML.substr(0,36) + "…";
        }
    }
}

// var breadcrumbItem =   document.querySelectorAll(".breadcrumb");
// if (breadcrumbItem.length === 1){
//     for(var i = 0; i < breadcrumbItem[0].children.length; i++) {
//         console.log(breadcrumbItem[0].children[i].innerHTML.length);
//         console.log(breadcrumbItem[0].children[i].innerHTML);
//         if(breadcrumbItem[0].children[i].innerHTML.length >= 36){
//             breadcrumbItem[0].children[i].innerHTML = breadcrumbItem[0].children[i].innerHTML.substr(0,36) + "...";
//         }
//     }
// }
