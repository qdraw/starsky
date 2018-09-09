

var sideBarDefaultWidth;
if (document.querySelectorAll(".sidebar").length === 1) {
    function toggleSideMenu(isStartup) {
        if (!isStartup) {
            if (document.querySelector(".sidebar").className.indexOf("enabled") === -1) {
                document.querySelector(".sidebar").classList.add("enabled")
            }
        }

        if (document.querySelector(".sidebar .close").className.indexOf("collapsed") === -1) {
            if (!isStartup) {
                var hash = window.location.hash.replace("#sidebar=" + GetSidebarWindowHash("sidebar"),"");
                hash = hash.replace(";sidebar=" + GetSidebarWindowHash("sidebar"),"");
                hash = hash.replace("#sidebar", "");
                window.location.hash = hash.replace(";sidebar", "");
            }
            updatePrevNextHash();

            document.querySelector(".sidebar .close").classList.add("collapsed");
            sideBarDefaultWidth = document.querySelector(".sidebar").style.width;
            document.querySelector(".sidebar").style.width = "0px";
            sideBarDefaultPadding = document.querySelector(".sidebar").style.padding;
            document.querySelector(".sidebar").style.padding = "0px";
            document.querySelector(".sidebar .content").classList.add("collapsed");

            if (document.querySelectorAll(".archive").length === 1) {
                document.querySelector(".archive").classList.remove("collapsed")
            }

            if (document.querySelectorAll(".main-image").length === 1) {
                document.querySelector(".main-image").classList.remove("collapsed")
            }

            if (document.querySelectorAll(".detailview").length === 1) {
                document.querySelector(".detailview").classList.remove("collapsed")
            }
            if (document.querySelectorAll(".body-content").length === 1) {
                document.querySelector(".body-content").classList.remove("collapsed")
            }
            document.querySelector("body").classList.remove("collapsed")

        }
        else {
            if (!isStartup) {
                if (window.location.hash.indexOf("sidebar") === -1 
                    && window.location.hash.length === 0) {
                    window.location.hash = '#sidebar';
                }
                else if (window.location.hash.indexOf("sidebar") === -1){
                    window.location.hash += ';sidebar=';
                }
            }
            updatePrevNextHash();

            document.querySelector(".sidebar .close").classList.remove("collapsed");
            document.querySelector(".sidebar").style.width = sideBarDefaultWidth;
            if (isStartup) {
                document.querySelector(".sidebar .content").classList.remove("collapsed");
            }
            else {
                setTimeout(function () {
                    document.querySelector(".sidebar .content").classList.remove("collapsed");
                }, 300);
            }
            if (document.querySelectorAll(".archive").length === 1) {
                document.querySelector(".archive").classList.add("collapsed")
            }
            if (document.querySelectorAll(".main-image").length === 1) {
                document.querySelector(".main-image").classList.add("collapsed")
            }
            if (document.querySelectorAll(".detailview").length === 1) {
                document.querySelector(".detailview").classList.add("collapsed")
            }
            if (document.querySelectorAll("body-content").length === 1) {
                document.querySelector(".body-content").classList.add("collapsed")
            }
            document.querySelector("body").classList.add("collapsed")

        }
    }
    // in detailview mode
    if(document.querySelectorAll(".main-image").length === 1) {
        // Default on mobile disabled
        // Default on desktop enabled
        toggleSideMenu(true);
        toggleSideMenu(true);
        if (window.location.hash.indexOf("sidebar") === -1 && window.innerWidth <= 650) {
            // only in detailview
            toggleSideMenu(true);
        }
    }
    // in archive mode
    else {
        toggleSideMenu(true);
        toggleSideMenu(true);

        if (window.location.hash.indexOf("sidebar") === -1) {
            toggleSideMenu(true);
        }
    }
}




// Add select part to next prev url

if (document.querySelectorAll(".nextprev").length >= 1) {

    for (var i = 0; i < document.querySelectorAll(".nextprev").length; i++) {

        var object = document.querySelectorAll(".nextprev")[i].children;
        var searchPosition = window.location.search.indexOf("colorclass") - 1;
        addcolorclassPart = window.location.search.substr(searchPosition, window.location.search.length);

        for (var j = 0; j < object.length; j++) {

            // var test = object[j].href.substr(0,object[j].href.indexOf("colorclass"));
            // console.log(test)

            if (window.location.search.indexOf("colorclass") >= 0) {
                object[j].href += addcolorclassPart;
            }

        }
    }
}

function updatePrevNextHash() {
    for (var i = 0; i < document.querySelectorAll(".nextprev").length; i++) {
        var object = document.querySelectorAll(".nextprev")[i].children;
        for (var j = 0; j < object.length; j++) {
            if (object[j].href === undefined) return;

            // clean up the url, this method runs 5 times
            // and add everyting behind the #; 
            // if not don't clean it
            var indexof = object[j].href.indexOf("#");
            if (indexof === -1) indexof = object[j].href.length;
            var href = object[j].href.substr(0, indexof);
            href += window.location.hash;
            object[j].href = href;
            // console.log(href)
        }
    }
}
