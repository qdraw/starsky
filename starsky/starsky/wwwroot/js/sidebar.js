

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
                hash = hash.replace(";sidebar", "");
                console.log(hash)
                
                var url = hash;
                if(url === "") url = "#";

                var stateObj = { url: url };
                history.replaceState(stateObj, "Qdraw", url);

                // window.location.hash = hash;


            }

            updatePrevNextHash();

            document.querySelector(".sidebar .close").classList.add("collapsed");
            document.querySelector(".sidebar .close").innerHTML = "Bewerken";

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
            document.querySelector(".sidebar .close").innerHTML = "Annuleer";

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

        window.onwheel = null;
    } //end toggleSideMenu()
    
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



function updatePrevNextHash() {
    for (var i = 0; i < document.querySelectorAll(".nextprev").length; i++) {
        var object = document.querySelectorAll(".nextprev")[i].children;
        for (var j = 0; j < object.length; j++) {
            if (object[j].href === undefined) continue;

            // clean up the url, this method runs 5 times
            // and add everyting behind the #; 
            // if not don't clean it
            var indexof = object[j].href.indexOf("#");
            if (indexof === -1) indexof = object[j].href.length;
            var href = object[j].href.substr(0, indexof);
            if (window.location.search.indexOf("collections=false") >= 0 &&
            object[j].href.indexOf("collections=false") === -1) {
                href += "&collections=false"
            }
            href += window.location.hash;
            object[j].href = href;
            // console.log(href)
        }
    }
}
