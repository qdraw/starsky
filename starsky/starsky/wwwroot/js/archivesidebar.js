var prevURL = "";
var selectedFiles = [];

var portfolioData = document.querySelectorAll("#portfolio-data");
var archive = document.querySelectorAll(".archive");
if (portfolioData.length === 1 && archive.length === 1) {
    for (var i = 0; i < portfolioData[0].children.length; i++) {
        // console.log(portfolioData[0].children[i]);
        portfolioData[0].children[i].addEventListener("click", function(e){

            if(archive.length === 1) {
                if(archive[0].className.indexOf("collapsed") >= 0 
                    && this.className.indexOf("directory-false") >= 0) {
                    e.preventDefault();
                    
                    if (window.location.hash.indexOf("anchor=") >= 0) {
                        window.location.hash = window.location.hash.replace(GetSidebarWindowHash("anchor"),"");
                        
                    }


                    var url = "";
                    if (!this.classList.contains("on")) {
                        // This features add it;
                        // console.log(this.className.indexOf("on"));

                        this.classList.add("on");

                        selectedFiles.push(this.dataset.filename);
                        // console.log(selectedFiles);
                    }
                    else {
                        // console.log(this.className);

                        this.classList.remove("on");
                        // ES5
                        selectedFiles = selectedFiles.filter(item => item !== this.dataset.filename);
                    }
                    var prevWindowHash = GetSidebarWindowHash("sidebar");

                    if (prevWindowHash.length >= 1) {
                        url = window.location.hash.replace(prevWindowHash,"");
                    }
                    if (prevWindowHash.length === 0) {
                        url = window.location.hash = replaceSideBarString(window.location.hash);
                        // when using anchors
                        if (url === "#;") {
                            url = url.replace("#;","");
                        }
                        if (url === "#") {
                            url += "sidebar="
                        }
                        if (url.length === 0) {
                            url += "#sidebar="
                        }
                        
                        if (url !== "#;" && url !== "#" && url.length !== 0 && url.indexOf("sidebar") === -1) {
                            url += "sidebar="
                        }
            

                    }

                    // console.log(url);
                    url = appendArrayToString(url,selectedFiles);
                    // console.log(selectedFiles);
                    // console.log(url);

                    // var   
                    if (url !== prevURL) {
                        var stateObj = { url: url };
                        history.pushState(stateObj, "Qdraw", url);
                    }
                    prevURL = url;

                    updateDisplayList();
                    // console.log("0")
                }
            }

        }, false);
    }
        
}

function appendArrayToString(url,selectedFiles) {
    for (var i = 0; i < selectedFiles.length; i++) {

        if (i === selectedFiles.length-1) {
            url += selectedFiles[i];
        }
        else {
            url += selectedFiles[i] + ",";
        }
    }
    return url;
}



function buildSidebarPage() {
    selectedFiles = [];
    var prevWindowHash = GetSidebarWindowHash("sidebar");
    if (prevWindowHash.length >= 1) {
        var getHashList =  prevWindowHash.split(",");
        for (var i = 0; i < getHashList.length; i++) {
            selectedFiles.push(getHashList[i]);
        }
    }
    var halfitems = document.querySelectorAll(".halfitem");
    
    // reset
    for (var i = 0; i < halfitems.length; i++) {
        if (halfitems[i].classList.contains("on")) {
            halfitems[i].classList.remove("on");
        }
    }

    for (var i = 0; i < selectedFiles.length; i++) {
        var uri_dec = decodeURIComponent(selectedFiles[i]);
        
        var query = ".halfitem[data-filename=\""+ uri_dec +"\"]";
        if (document.querySelectorAll(query).length === 1 && !document.querySelector(query).classList.contains("on") ) {
            document.querySelector(query).classList.add("on");
        }
    }
}
window.addEventListener("hashchange", function (e) {
    buildSidebarPage();
    updateDisplayList();
}, false);

buildSidebarPage();
updateDisplayList();

function updateDisplayList() {
    console.log("sdf")
    if (document.querySelectorAll(".js-selectedimages").length === 1) {
        var html = "";
        for (var i = 0; i < selectedFiles.length; i++) {
            html += selectedFiles[i] + "<br />";
        }
        document.querySelector(".js-selectedimages").innerHTML = html;
    }
    
}