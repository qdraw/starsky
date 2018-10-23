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

    window.onkeydown = function(e) {
        switch (e.keyCode) {
            case 37:
                // left
                setPrev();
                if (prev != null 
                    && document.activeElement.className.indexOf("form-control") === -1
                    && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {
                    window.location.href = prev;
                }
                break;
            case 38:
                // // up
                break;
            case 39:
                // right
                setNext();
                if (next != null
                    && document.activeElement.className.indexOf("form-control") === -1
                    && document.activeElement.className.indexOf("leaflet-touch-drag") === -1) {
                    window.location.href = next;
                }
                break;
            case 40:
                // // down
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
                if (next != null && document.activeElement.className.indexOf("form-control") === -1) {
                    window.location.href = next;
                }
                
            } else {/* right swipe */
                console.log('right!');
                setNext();
                if (prev != null && document.activeElement.className.indexOf("form-control") === -1) {
                    window.location.href = prev;
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


