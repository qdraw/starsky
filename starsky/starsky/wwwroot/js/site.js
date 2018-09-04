// Write your JavaScript code.



if (document.querySelectorAll(".nextprev").length >= 1) {
    var next = null;
    var prev = null;
    if (document.querySelectorAll(".nextprev .next").length >= 1) next = document.querySelector(".nextprev .next").href;
    if (document.querySelectorAll(".nextprev .prev").length >= 1) prev = document.querySelector(".nextprev .prev").href;

    window.onkeydown = function(e) {
        switch (e.keyCode) {
            case 37:
                // left
                if (prev != null && document.activeElement.className.indexOf("form-control") === -1) {
                    window.location.href = prev;
                }
                break;
            case 38:
                // // up
                break;
            case 39:
                // right
                if (next != null && document.activeElement.className.indexOf("form-control") === -1) {
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
                if (prev != null && document.activeElement.className.indexOf("form-control") === -1) {
                    window.location.href = prev;
                }
            } else {/* right swipe */
                console.log('right!');
                if (next != null && document.activeElement.className.indexOf("form-control") === -1) {
                    window.location.href = next;
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


