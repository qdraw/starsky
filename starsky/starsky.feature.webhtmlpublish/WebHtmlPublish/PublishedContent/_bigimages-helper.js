// To replace base64 images with 500 px images
var bigimages = document.querySelectorAll('a[class^=\'lightbox\'] img');

for (var image of bigimages) {
    var noscriptItem = image.parentNode.getElementsByTagName('noscript')[0]
    if ( noscriptItem.innerHTML.length >= 10 ) {
        var url = noscriptItem.innerHTML.match(/\"(.*?)\"/);
        url = url[0].replace(/\"/ig, "");
		image.src = url;
    }
    var bigimagesTitle = document.createElement("span");
    bigimagesTitle.className = "lightbox-title";
    bigimagesTitle.innerHTML = image.title;
	image.parentElement.appendChild(bigimagesTitle);
}
