var prevURL = "";
var selectedFiles = [];

function buildArchiveSidebar() {
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
                            window.location.hash =
                                window.location.hash.replace(GetSidebarWindowHash("anchor"),"");
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
                            // ES5 > remove item from array
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

                        url = appendArrayToString(url,selectedFiles,",");

                        // var   
                        if (url !== prevURL) {
                            var stateObj = { url: url };
                            history.pushState(stateObj, "Qdraw", url);
                        }
                        prevURL = url;

                        updateDisplayList();
                        updateControls();
                        // console.log("0")
                    }
                }

            }, false);
        }
    }
}
buildArchiveSidebar();

function appendArrayToString(url,selectedFiles,splitter) {
    for (var i = 0; i < selectedFiles.length; i++) {

        if (i === selectedFiles.length-1) {
            url += selectedFiles[i];
        }
        else {
            url += selectedFiles[i] + splitter;
        }
    }
    return url;
}

function seletedItemsOn() {
    // reset
    var halfitems = document.querySelectorAll(".halfitem");

    for (var i = 0; i < halfitems.length; i++) {
        if (halfitems[i].classList.contains("on")) {
            halfitems[i].classList.remove("on");
        }
    }  
}

function buildSidebarPage() {
    selectedFiles = [];
    var prevWindowHash = GetSidebarWindowHash("sidebar");
    if (prevWindowHash.length >= 1) {
        var getHashList =  prevWindowHash.split(",");
        for (var i = 0; i < getHashList.length; i++) {
            var filenamefromquery = decodeURIComponent(getHashList[i]);
            var query = ".halfitem[data-filename=\"" + filenamefromquery + "\"]";
            if (document.querySelectorAll(query).length === 1) {
                selectedFiles.push(filenamefromquery);
            }
        }
    }

    seletedItemsOn();

    for (var i = 0; i < selectedFiles.length; i++) {

        var query1 = ".halfitem[data-filename=\""+ selectedFiles[i] +"\"]";
        console.log(document.querySelectorAll(query1).length)

        if (document.querySelectorAll(query1).length === 1 && 
            !document.querySelector(query1).classList.contains("on") ) {
            document.querySelector(query1).classList.add("on");
        }
    }
}

window.addEventListener("hashchange", function (e) {
    buildSidebarPage();
    updateDisplayList();
    updateControls();
}, false);

buildSidebarPage();
updateDisplayList();
updateControls();

function updateDisplayList() {
    if (document.querySelectorAll(".js-selectedimages").length === 1) {
        var html = "<h2>Geselecteerde bestanden</h2><ul>";
        for (var i = 0; i < selectedFiles.length; i++) {
            html += "<li><a class='close' onclick='removeThisItem(\""+ selectedFiles[i] +"\")'></a> "+ selectedFiles[i] + "</li>";
        }
        html += "</ul>";
        document.querySelector(".js-selectedimages").innerHTML = html;
    }
}

function updateControls() {
    console.log(selectedFiles);
    console.log(document.querySelectorAll(".js-controls"));

    if (selectedFiles.length === 0 && document.querySelectorAll(".js-controls").length === 1) {
        document.querySelector(".js-controls").classList.add("hidden");
    }

    if (selectedFiles.length >= 1 && document.querySelectorAll(".js-controls").length === 1) {
        document.querySelector(".js-controls").classList.remove("hidden");

    }
}

function updateKeywords() {
    if (document.querySelectorAll("#js-keywords-update").length === 1){
        var keywords = document.querySelector('.js-keywords');

        // check if content already is send to the server
        if (keywords.textContent !== keywords.dataset.previouscontent) {
            queryKeywords(keywords.textContent);
            keywords.dataset.previouscontent = keywords.textContent;
        }
    }
}

function updateCaptionAbstract() {
    if (document.querySelectorAll("#js-captionabstract-update").length === 1){
        var captionabstract = document.querySelector('.js-captionabstract');

        // check if content already is send to the server
        if (captionabstract.textContent !== captionabstract.dataset.previouscontent) {
            queryCaptionAbstract(captionabstract.textContent);
            captionabstract.dataset.previouscontent = captionabstract.textContent;
        }
    }
}



function toSubpath() {
    var selectedFilesSubPath = [];
    for (i = 0; i < selectedFiles.length; i++) {
        selectedFilesSubPath.push(subPath +"/" + selectedFiles[i])
    }
    return appendArrayToString("", selectedFilesSubPath, ";");
}

function queryKeywords(queryItem) {

    var toupdateFiles = toSubpath();
    
    var url = updateApiBase + "?f=" + toupdateFiles + "&tags=" + queryItem + "&append=true";
    loadJSON(url,
        function (data) {
            location.reload();
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

function queryCaptionAbstract(queryItem) {

    var toupdateFiles = toSubpath();

    var url = updateApiBase + "?f=" + toupdateFiles + "&description=" + queryItem + "&append=true";

    loadJSON(url,
        function (data) {
            location.reload();
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}


document.addEventListener("DOMContentLoaded", function(event) {
    loadResetButton();
});

function removeThisItem (fileName) {
    // to replace afterwards in the url
    var getSidebarWindowHashUrl = GetSidebarWindowHash("sidebar");

    selectedFiles = selectedFiles.filter(item => item !== fileName);
    
    // update the hashlist
    var toreplaceUrl = "";
    toreplaceUrl = appendArrayToString(toreplaceUrl,selectedFiles,",");

    var url =  window.location.hash.replace(getSidebarWindowHashUrl,toreplaceUrl);

    if (url !== prevURL) {
        var stateObj = { url: url };
        history.pushState(stateObj, "Qdraw", url);
    }
    prevURL = url;
    
    // do other stuff
    updateDisplayList();
    updateControls();
}

function loadResetButton() {
    console.log(document.querySelectorAll(".reset"));
    if (document.querySelectorAll(".reset").length === 1) {

        document.querySelector(".reset").addEventListener("click", function(e){
            buildSidebarPage();
            selectedFiles = [];
            updateDisplayList();
            updateControls();
            window.location.hash = window.location.hash.replace(GetSidebarWindowHash("sidebar"),"");
        }, false);

    }
}

