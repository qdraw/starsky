
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
                            // workaround for IE11
                            var those = this;
                            selectedFiles = selectedFiles.filter(function(item) {
                                return item !== those.getAttribute('data-filename');
                            });
                            // ES5 > remove item from array
                            // selectedFiles = selectedFiles.filter(item => item !== this.dataset.filename);
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
        var html = "<h2><span class='js-selectedcount'>Geen bestanden geselecteerd</span></h2>";
        html +=    "<h2><a class='js-selectallnone selectall' onclick='toggleSelectAll()'>Selecteer alles</a></h2>";
        html +=    "<ul>";
        for (var i = 0; i < selectedFiles.length; i++) {
            html += "<li><a class='close' onclick='removeThisItem(\"" + selectedFiles[i] + "\")'></a> " + selectedFiles[i] + "</li>";
        }
        html += "</ul>";
        document.querySelector(".js-selectedimages").innerHTML = html;
    }
    if (document.querySelectorAll(".js-selectedcount").length === 1) {
        var selectedcountElement = document.querySelector(".js-selectedcount");
        if (selectedFiles.length === 0) {
            selectedcountElement.innerHTML = "Geen bestanden geselecteerd";
        }
        else if (selectedFiles.length === 1) {
            selectedcountElement.innerHTML = "1 geselecteerd bestand";
        }
        else {
            selectedcountElement.innerHTML = selectedFiles.length + " geselecteerde bestanden";
        }

    }
    if (document.querySelectorAll(".js-selectedcount").length === 1 &&
        document.querySelectorAll(".js-selectallnone").length === 1 &&
        document.querySelectorAll(".js-collectionscount").length === 1
    ) {

        // to switch select all, select none
        var collectionscount = parseInt(document.querySelector(".js-collectionscount").innerHTML);
        
        if (collectionscount === selectedFiles.length) {
            document.querySelector(".js-selectallnone").classList.remove("selectall");
            document.querySelector(".js-selectallnone").classList.add("selectnone");
            document.querySelector(".js-selectallnone").innerHTML = "Selectie ongedaan maken";
        }
        else {
            document.querySelector(".js-selectallnone").classList.remove("selectnone");
            document.querySelector(".js-selectallnone").classList.add("selectall");
            document.querySelector(".js-selectallnone").innerHTML = "Selecteer alles";
        }
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
    addNoClickToSidebar();

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
    addNoClickToSidebar();

    loadJSON(url,
        function (data) {
            location.reload();
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

function toggleSelectAll() {
    if (document.querySelectorAll(".js-selectallnone").length === 1) {
        var className = document.querySelector(".js-selectallnone");
        if (className.className.indexOf("selectnone") === -1) {
            console.log("1");
            selectAllCurrentVisableItems();
        }
        else {
            console.log("0");
            resetSelection();
        }
    }
}

function selectAllCurrentVisableItems() {
    selectedFiles = [];
    var getSidebarWindowHashUrl = GetSidebarWindowHash("sidebar");

    var halfitems = document.querySelectorAll(".halfitem");

    for (var i = 0; i < halfitems.length; i++) {
        if (halfitems[i].className.indexOf("hide") === -1) {
            selectedFiles.push(halfitems[i].dataset.filename);
            halfitems[i].classList.add("on");
        }
    }
    var toreplaceUrl = "";
    toreplaceUrl = appendArrayToString(toreplaceUrl,selectedFiles,",");
    console.log(toreplaceUrl);
    console.log(getSidebarWindowHashUrl);
    var url =  window.location.hash.replace(getSidebarWindowHashUrl, toreplaceUrl);

    // overwrite when no items are selected
    if (getSidebarWindowHashUrl === ""){
        url = window.location.hash + toreplaceUrl;
    }

    if (url !== prevURL) {
        var stateObj = { url: url };
        history.pushState(stateObj, "Qdraw", url);
    }
    prevURL = url;
    
    updateDisplayList();
    updateControls();

}


document.addEventListener("DOMContentLoaded", function(event) {
    loadResetButton();
});

function removeThisItem (fileName) {
    // to replace afterwards in the url
    var getSidebarWindowHashUrl = GetSidebarWindowHash("sidebar");

    // selectedFiles = selectedFiles.filter(item => item !== fileName);
    selectedFiles = selectedFiles.filter(function(item) {
        return item !== fileName;
    });
    
    // remove this checkbox in the item itself
    var halfitemFilename = document.querySelectorAll(".halfitem[data-filename=\""+ fileName +"\"]");
    if (document.querySelectorAll(".halfitem[data-filename=\""+ fileName +"\"]").length === 1){
        halfitemFilename[0].classList.remove("on");
    }
    
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
    if (document.querySelectorAll(".reset").length === 1) {
        document.querySelector(".reset").addEventListener("click", function(e){
            resetSelection();
        }, false);
    }
}
function resetSelection() {
    buildSidebarPage();
    selectedFiles = [];
    updateControls();

    // Push to history
    var url = window.location.hash.replace(GetSidebarWindowHash("sidebar"),"");
    if (url !== prevURL) {
        var stateObj = { url: url };
        history.pushState(stateObj, "Qdraw", url);
    }
    prevURL = url;
    
    updateDisplayList();
}

function addNoClickToSidebar() {
    if (document.querySelectorAll(".sidebar").length === 1) {
        console.log("noclick")
        document.querySelector(".sidebar").classList.add("noclick");

        // hide buttons
        document.querySelector(".js-keywords").classList.add("disabled");
        document.querySelector("#js-keywords-update a").classList.add("disabled");
        document.querySelector('.js-keywords').contentEditable = false;
        document.querySelector("#js-captionabstract-update a").classList.add("disabled"); //
        document.querySelector(".js-captionabstract").classList.add("disabled");
        document.querySelector('.js-captionabstract').contentEditable = false;
        if (document.querySelectorAll(".js-objectname").length === 1){
            document.querySelector(".js-objectname").classList.add("disabled");
            document.querySelector('.js-objectname').contentEditable = false;     
        }
    }
}

// Used in <div class="add-colorclass">
function updateColorClass(those) {
    var toupdateFiles = toSubpath();
    var url = updateApiBase + "?f=" + toupdateFiles + "&colorClass=" + those.dataset.colorclass;
    addNoClickToSidebar();
    loadJSON(url,
        function(data) {
            location.reload();
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

if (document.querySelectorAll(".sidebar").length === 1) {
    if (document.querySelector(".sidebar").className.indexOf("readonly-true")) {
        addNoClickToSidebar(); 
    }
}

