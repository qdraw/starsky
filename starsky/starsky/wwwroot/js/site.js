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
        // console.log(e.keyCode);
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
                }
                else if (document.activeElement.className.indexOf("form-control") === -1
                && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {
                    showUpdateDialog("Je bent al bij het laatste item, je kunt niet verder", 3000);

                    // showPopupDialog("<p>Je bent al bij het eerste item, je kunt niet verder terug</p>\n" +
                    //     "<p>\n" +
                    //     "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oke</a>\n" +
                    //     "</p>\n", 3000);
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
                }
                else if (document.activeElement.className.indexOf("form-control") === -1
                && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {
                    showUpdateDialog("Je bent al bij het eerste item, je kunt niet verder",3000);

                    // showPopupDialog( "<p>Je bent al bij het laatste item, je kunt niet verder</p>\n" +
                    //     "<p>\n" +
                    //     "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oke</a>\n" +
                    //     "</p>\n",3000); 
                }
                break;
            case 40:
                // // down
                break;
            case 27: // escape
                hidePopupDialog();
                break;
            case 13: // enter
                if (document.querySelectorAll("#popup").length === 1) {
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
                }
                break;
            case 224: // apple cmd
                previousKey.push(17);
                break;
            case 17: // ctrl
                previousKey.push(17);
                break;
            case 18: // alt
                previousKey.push(18);
                break;
            case 16: // shift
                previousKey.push(16);
                break;
            case 67: // C
                previousKey.push(67);
                break;
            case 86: // V
                previousKey.push(86);
                break;
            default:
                previousKey = [];
                break;

        }
        
        
        // auto copy
        if(document.querySelectorAll(".archive").length === 1 || document.querySelectorAll(".detailview").length === 1) {
            // copy
            if (previousKey.indexOf(17) >= 0 && previousKey.indexOf(16) >= 0 && previousKey.indexOf(67) >= 0) {
                console.log("copy");
                console.log(previousKey)

                var formControl = document.querySelectorAll(".form-control.js-allow-auto-copy");
                for (var i = 0; i < formControl.length; i++) {

                    var name = "copy_#" + formControl[i].parentElement.id + " - " + formControl[i].className;
                    var contentCopy = document.querySelectorAll(".form-control.js-allow-auto-copy")[i].textContent;
                    window.localStorage.setItem(name, contentCopy);

                    showUpdateDialog("Je hebt nu de items gekopieerd",3000);

                }
                previousKey = [];
            }
            // paste
            if (previousKey.indexOf(17) >= 0 && previousKey.indexOf(16) >= 0 && previousKey.indexOf(86) >= 0) {
                console.log("paste");
                // console.log(previousKey)

                var formControlPaste = document.querySelectorAll(".form-control.js-allow-auto-copy");
                for (var i = 0; i < formControlPaste.length; i++) {

                    var nameCopy = "copy_#" + formControlPaste[i].parentElement.id + " - " + formControlPaste[i].className;
                    var content = window.localStorage.getItem(nameCopy);
                    if(content !== undefined && content !== null && content !== "") {
                        formControlPaste[i].textContent = content;
                        formControlPaste[i].focus(true);
                        formControlPaste[i].blur();
                    }
                    // console.log("paste +> " + content);
                }
                showUpdateDialog("Je hebt nu de items geplakt",3000);

                previousKey = [];
            }
        }
        
    };
    
    // swipes

    // document.addEventListener('touchstart', handleTouchStart, false);
    // document.addEventListener('touchmove', handleTouchMove, false);
    // var xDown = null;
    // var yDown = null;

    // document.addEventListener('gestureend', function(e) {
    //     if (e.scale < 1.0) {
    //         // User moved fingers closer together
    //         console.log("User moved fingers closer together");
    //     } else if (e.scale > 1.0) {
    //         // User moved fingers further apart
    //         console.log(" User moved fingers further apart");
    //     }
    // }, false);
    
}

// function handleTouchStart(evt) {
//     xDown = evt.touches[0].clientX;
//     yDown = evt.touches[0].clientY;
// }//e/handleTouchStart

// credit: http://www.javascriptkit.com/javatutors/touchevents2.shtml
function swipedetect(el, callback){

    var touchsurface = el,
        swipedir,
        startX,
        startY,
        distX,
        distY,
        threshold = 170, //required min distance traveled to be considered swipe
        restraint = 100, // maximum distance allowed at the same time in perpendicular direction
        allowedTime = 300, // maximum time allowed to travel that distance
        elapsedTime,
        startTime,
        handleswipe = callback || function(swipedir){}

    touchsurface.addEventListener('touchstart', function(e){
        var touchobj = e.changedTouches[0];
        swipedir = 'none';
        dist = 0;
        startX = touchobj.pageX;
        startY = touchobj.pageY;
        startTime = new Date().getTime() // record time when finger first makes contact with surface
        // e.preventDefault()
    }, false)

    touchsurface.addEventListener('touchmove', function(e){
        // e.preventDefault() // prevent scrolling when inside DIV
    }, false);

    touchsurface.addEventListener('touchend', function(e){
        var touchobj = e.changedTouches[0];
        distX = touchobj.pageX - startX;// get horizontal dist traveled by finger while in contact with surface
        distY = touchobj.pageY - startY; // get vertical dist traveled by finger while in contact with surface
        elapsedTime = new Date().getTime() - startTime; // get time elapsed
        if (elapsedTime <= allowedTime){ // first condition for awipe met
            if (Math.abs(distX) >= threshold && Math.abs(distY) <= restraint){ // 2nd condition for horizontal swipe met
                swipedir = (distX < 0)? 'left' : 'right' // if dist traveled is negative, it indicates left swipe
            }
            else if (Math.abs(distY) >= threshold && Math.abs(distX) <= restraint){ // 2nd condition for vertical swipe met
                swipedir = (distY < 0)? 'up' : 'down' // if dist traveled is negative, it indicates up swipe
            }
        }
        handleswipe(swipedir)
        // e.preventDefault()
    }, false)
}

//USAGE:

function goRight() {
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
        showUpdateDialog("Je bent al bij het eerste item, je kunt niet verder",3000);

        // showPopupDialog("<p>Je bent al bij het eerste item, je kunt niet verder terug.</p>\n" +
        //     "<p>\n" +
        //     "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oke</a>\n" +
        //     "</p>\n", 3000);
       
    }
}

function goLeft() {
    console.log('left!');
    setNext();
    if (next != null && document.activeElement.className.indexOf("form-control") === -1
        && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {
        window.location.href = next;
    }
    else if (
        document.activeElement.className.indexOf("form-control") === -1
        && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {

        showUpdateDialog("Je bent al bij het laatste item, je kunt niet verder",3000);

        // showPopupDialog( "<p>Je bent al bij het laatste item, je kunt niet verder</p>\n" +
        //     "<p>\n" +
        //     "<a data-onclick=\"hidePopupDialog()\" class=\"btn-sm btn btn-secondary\">Oké</a>\n" +
        //     "</p>\n",3000);

    }
} 

var el = document.querySelector("body");
swipedetect(el, function(swipedir){

    if (swipedir === "left") 
    {
        goLeft();
    }
    if (swipedir === "right") {
        goRight();
    } 

    // swipedir contains either "none", "left", "right", "top", or "down"
});





//
// function handleTouchMove(evt) {
//     if (!xDown || !yDown) {
//         return;
//     }
//
//     var xUp = evt.touches[0].clientX;
//     var yUp = evt.touches[0].clientY;
//
//     var xDiff = xDown - xUp;
//     var yDiff = yDown - yUp;
//     if (Math.abs(xDiff) + Math.abs(yDiff) > 150) { //to deal with to short swipes
//
//         if (Math.abs(xDiff) > Math.abs(yDiff)) {/*most significant*/
//             if (xDiff > 0) {/* left swipe */
//                 goLeft()
//             }
//             } else {/* right swipe */
//                 goRight()
//             }
//         } else {
//             if (yDiff > 0) {/* up swipe */
//                 console.log('Up!');
//             } else { /* down swipe */
//                 console.log('Down!');
//             }
//         }
//         /* reset values */
//         xDown = null;
//         yDown = null;
//     }
// }
// }//e/handleTouchMove





/* menu.js from qdraw but changed */


document.querySelector(".head #hamburger").addEventListener("click", function(e){ hamburger(e); }, false);

var isHamburgerActive = false;

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

if (document.querySelectorAll("#popup").length === 1) {
    document.querySelector("#popup")
        .addEventListener("click", function(e){ 
            // on first button fail > reload entire page
            var statusFirstButton =  document.querySelectorAll("#popup .content a")[0].getAttribute("data-onclick");
            if (statusFirstButton === "location.reload()") {
                location.reload()
            }
            //else hide >
            hidePopupDialog(e);
            hidePreloader();
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

function showUpdateDialog(content,timeout) {
    if (document.querySelectorAll("#update").length === 1) {
        document.querySelector("#update").classList.add("on");

        document.querySelector("#update").innerHTML = content;
        
        if (!isNaN(timeout)) {
            setTimeout(function(){
                hideUpdateDialog()
            }, timeout);
        }
    }
}


function hideUpdateDialog() {
    if (document.querySelectorAll("#update").length === 1) {
        document.querySelector("#update").classList.remove("on");
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
                            hidePopupDialog();
                        }
                        else if (target === "location.reload()") {
                            location.reload()
                        }
                        else if (target === "queryDeleteApi()") {
                            queryDeleteApi();
                        }
                        else if (target === "queryUndoDeleteApi()") {
                            queryUndoDeleteApi();
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
