// Write your JavaScript code.

function setNext() {
    if (document.querySelectorAll(".nextprev .next").length >= 1) next = document.querySelector(".nextprev .next").href;
}

function setPrev() {
    if (document.querySelectorAll(".nextprev .prev").length >= 1) prev = document.querySelector(".nextprev .prev").href;
}
if (document.querySelectorAll(".nextprev").length >= 1) {
    var next = null;
    var prev = null;
    setPrev();
    setNext();

    var previousKey = [];
    window.onkeydown = function(e) {
        console.log(e.keyCode);
        switch (e.keyCode) {
            case 37:
                // left
                setPrev();
                if (prev != null 
                    && (document.activeElement.className.indexOf("form-control") === -1
                    && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) 
                    ) {
                    window.location.href = prev;
                }
                else if( (prev != null && previousKey.indexOf(18) >= 0) ) {
                    document.activeElement.blur();
                    setInterval(function(){
                        if (formSubmitting === undefined) {
                            window.location.href = prev;
                            formSubmitting = false;
                        }
                        else if(formSubmitting) {
                            console.log("<");
                            window.location.href = prev;
                            formSubmitting = false;
                        }
                    }, 200);
                }
                else if (document.activeElement.className.indexOf("form-control") === -1
                && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {
                    showPopupDialog("<p>Je bent al bij het eerste item, je kunt niet verder terug</p>\n" +
                        "<p>\n" +
                        "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oke</a>\n" +
                        "</p>\n", 3000);
                }
                break;
            case 38:
                // // up
                break;
            case 39:
                // right
                setNext();
                if (next != null
                && (document.activeElement.className.indexOf("form-control") === -1
                && document.activeElement.className.indexOf("leaflet-touch-drag") === -1)) {
                    window.location.href = next;
                }
                else if( (next != null && previousKey.indexOf(18) >= 0) ) {
                    document.activeElement.blur();
                    setInterval(function(){
                        if (formSubmitting === undefined) {
                            window.location.href = next;
                            formSubmitting = false;
                        }
                        else if(formSubmitting) {
                            console.log(">");
                            window.location.href = next;
                            formSubmitting = false;
                        }
                    }, 200);
                }
                else if (document.activeElement.className.indexOf("form-control") === -1
                && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {
                    showPopupDialog( "<p>Je bent al bij het laatste item, je kunt niet verder</p>\n" +
                        "<p>\n" +
                        "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oke</a>\n" +
                        "</p>\n",3000); 
                }
                break;
            case 40:
                // // down
                break;
            case 27: // escape
                hidePopupDialog();
                break;
            case 13: // enter
                if (document.querySelector("#popup").className.indexOf("on") >= 0) {
                    if (document.querySelectorAll("#popup .btn-secondary").length >= 1) {
                        document.querySelector("#popup .btn-secondary").click();
                        break;
                    }
                    if (document.querySelectorAll("#popup .btn-default").length >= 1) {
                        document.querySelector("#popup .btn-default").click();
                        break;
                    }
                }
                break;
            case 224: // apple cmd
                // previousKey.push(17);
                break;
            case 17: // ctrl
                // previousKey.push(17);
                break;
            case 18: // alt
                previousKey.push(18);
                break;
            default:
                previousKey = [];
                break;

        }
    };
    
    // swipes

    document.addEventListener('touchstart', handleTouchStart, false);
    document.addEventListener('touchmove', handleTouchMove, false);
    var xDown = null;
    var yDown = null;
}

function handleTouchStart(evt) {
    xDown = evt.touches[0].clientX;
    yDown = evt.touches[0].clientY;
}//e/handleTouchStart


function handleTouchMove(evt) {
    if (!xDown || !yDown) {
        return;
    }

    var xUp = evt.touches[0].clientX;
    var yUp = evt.touches[0].clientY;

    var xDiff = xDown - xUp;
    var yDiff = yDown - yUp;
    if (Math.abs(xDiff) + Math.abs(yDiff) > 150) { //to deal with to short swipes

        if (Math.abs(xDiff) > Math.abs(yDiff)) {/*most significant*/
            if (xDiff > 0) {/* left swipe */
                console.log('left!');
                setNext();
                if (next != null && document.activeElement.className.indexOf("form-control") === -1
                    && document.activeElement.className.indexOf("leaflet-touch-drag") === -1)
                {
                    window.location.href = next;
                }
                else if (
                    document.activeElement.className.indexOf("form-control") === -1
                    && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) 
                {
                    showPopupDialog( "<p>Je bent al bij het eerste item, je kunt niet verder terug.</p>\n" +
                        "<p>\n" +
                        "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oke</a>\n" +
                        "</p>\n",3000);
                }
            } else {/* right swipe */
                console.log('right!');
                setNext();
                if (prev != null && document.activeElement.className.indexOf("form-control") === -1
                    && document.activeElement.className.indexOf("leaflet-touch-drag") === -1)
                {
                    window.location.href = prev;
                }
                else if (
                document.activeElement.className.indexOf("form-control") === -1
                    && document.activeElement.className.indexOf("leaflet-touch-drag") === -1)
                {
                    showPopupDialog( "<p>Je bent al bij het laatste item, je kunt niet verder</p>\n" +
                        "<p>\n" +
                        "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oké</a>\n" +
                        "</p>\n",3000);
                }
            }
        } else {
            if (yDiff > 0) {/* up swipe */
                console.log('Up!');
            } else { /* down swipe */
                console.log('Down!');
            }
        }
        /* reset values */
        xDown = null;
        yDown = null;
    }
}//e/handleTouchMove





/* menu.js from qdraw */


document.querySelector(".head #hamburger").addEventListener("click", function(e){ hamburger(e); }, false);

var isHamburgerActive = true;

function hamburger(e) {

    if (e !== undefined) {
        e.preventDefault();
    }
    document.querySelector(".head #hamburger").innerHTML = "";

    if (isHamburgerActive === false) {
        document.querySelector(".head #menu ul").className = "moveto";
        document.querySelector(".head #hamburger").className = "close";
    }

    if (isHamburgerActive === true) {
        document.querySelector(".head #menu ul").className = "moveaway";
        document.querySelector(".head #hamburger").className = "hamburger";
    }
    if (isHamburgerActive === true && e === undefined) {
        document.querySelector(".head #menu ul").classList.add("moveawayinitial");
    }

    isHamburgerActive = !isHamburgerActive;
    // console.log("hamburger" + isHamburgerActive);

}

var widthMobile = 900; 
if (window.innerWidth <= widthMobile) {
    hamburger();
}

window.addEventListener('resize', function() {

    // Close or open menu if change in resolution
    if (window.innerWidth >= widthMobile) {
        isHamburgerActive = false;
        hamburger();
    }

    if (window.innerWidth <= widthMobile) {
        isHamburgerActive = true;
        hamburger();
    }


}, true);

if (document.querySelectorAll("#popup").length === 1) {
    document.querySelector("#popup")
        .addEventListener("click", function(e){ 
            hidePopupDialog(e); 
        }, false);
}
function hidePopupDialog(those) {
    
    // use also direct  
    if (those !== undefined) {
        if (those.target.id !== "popup") return;
    }
    
    if (document.querySelectorAll("#popup").length === 1) {
        document.querySelector("#popup").classList.remove("on");
    }
}

function showPopupDialog(content,timeout) {
    if (document.querySelectorAll("#popup").length === 1) {
        document.querySelector("#popup").classList.add("on");
        if (content !== undefined) {
            document.querySelector("#popup .content").innerHTML = content;
            
            var links = document.querySelectorAll("#popup .content a");
            for (var i = 0; i < links.length; i++) {

                document.querySelectorAll("#popup .content a")[i].addEventListener("click",
                    function (e) {
                        var target = e.target.getAttribute("data-onclick");
                        if (target === "hidePopupDialog()") {
                            hidePopupDialog()
                        }
                        else if (target === "location.reload()") {
                            location.reload()
                        }
                    }, false);
            }
        }
        if (!isNaN(timeout)) {
            console.log(timeout);
            setTimeout(function(){
                hidePopupDialog()
            }, timeout);
        }
    }
}
