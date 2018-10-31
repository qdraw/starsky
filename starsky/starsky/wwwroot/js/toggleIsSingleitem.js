
function toggleIsSingleitem() {

    if (document.querySelectorAll(".js-toggle-issingleitem").length === 1) {

        var toAdd = "?issingleitem=False";

        if (document.querySelector(".js-toggle-issingleitem").className.indexOf(" on") === -1) {
            document.querySelector(".js-toggle-issingleitem").classList.add("on");
            toAdd = "?issingleitem=True";
            localStorage.setItem("issingleitem", "true");
        }
        else {
            document.querySelector(".js-toggle-issingleitem").classList.remove("on");
            localStorage.setItem("issingleitem", "false");
        }
        runIsSingleitem(toAdd);
    }
}


function runIsSingleitem(toAdd) {
    if (document.querySelectorAll(".js-toggle-issingleitem").length === 1) {
        if (toAdd === "?issingleitem=False") {
            document.querySelector(".js-toggle-issingleitem").classList.remove("on");
        }
        else if (toAdd  === "?issingleitem=True") {
            document.querySelector(".js-toggle-issingleitem").classList.add("on");
        }
    }

    console.log(localStorage.getItem("issingleitem"));


    var halfitems = document.querySelectorAll(".halfitem");

    for (var i = 0; i < halfitems.length; i++) {
        if (halfitems[i].className.indexOf("hide") === -1
            && halfitems[i].className.indexOf("directory-false") >= 1) {
            for (var j = 0; j < halfitems[i].children.length; j++) {

                if (halfitems[i].children[j].className.indexOf("lazyload") >= 0) {

                    halfitems[i].children[j].dataset.src = halfitems[i].children[j].dataset.src.replace("?issingleitem=False",toAdd);
                    halfitems[i].children[j].dataset.src = halfitems[i].children[j].dataset.src.replace("?issingleitem=True",toAdd);
                    halfitems[i].children[j].src = halfitems[i].children[j].src.replace("?issingleitem=False",toAdd);
                    halfitems[i].children[j].src = halfitems[i].children[j].src.replace("?issingleitem=True",toAdd);
                    // console.log(halfitems[i].children[j].dataset.src)

                }
            }

        }
    }
}