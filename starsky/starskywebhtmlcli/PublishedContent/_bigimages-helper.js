// To replace base64 images with 500 px images
var bigimages = document.querySelectorAll('a[class^=\'lightbox\'] img');
for (var i = 0; i < bigimages.length; i++) {
    var noscriptItem = bigimages[i].parentNode.getElementsByTagName('noscript')[0]
    if ( noscriptItem.innerHTML.length >= 10 ) {
        var url = noscriptItem.innerHTML.match(/\"(.*?)\"/);
        url = url[0].replace(/\"/ig, "");
        bigimages[i].src = url;
    }
    var bigimagesTitle = document.createElement("span");
    bigimagesTitle.className = "lightbox-title";
    bigimagesTitle.innerHTML = bigimages[i].title;
    bigimages[i].parentElement.appendChild(bigimagesTitle);
}
