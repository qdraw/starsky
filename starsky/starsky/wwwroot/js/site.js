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
                if (prev != null) {
                    window.location.href = prev;
                }
                break;
            case 38:
                // // up
                // if (prev != null) {
                //     window.location.href = prev;
                // }
                break;
            case 39:
                // right
                if (next != null) {
                    window.location.href = next;
                }
                break;
            case 40:
                // // down
                // if (next != null) {
                //     window.location.href = next;
                // }
                break;
        }
    };
}


