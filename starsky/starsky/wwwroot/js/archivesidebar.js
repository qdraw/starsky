
// use updateApiBase
// use deleteApiBase
// use subpath
// use syncApiBase

if(document.querySelectorAll("#js-settings").length === 1) {
    var updateApiBase = document.getElementById("js-settings").getAttribute("data-updateApiBase");
    var deleteApiBase = document.getElementById("js-settings").getAttribute("data-deleteApiBase");
    var subPath = document.getElementById("js-settings").getAttribute("data-subPath");
    var syncApiBase = document.getElementById("js-settings").getAttribute("data-syncApiBase");
    var exportZipApiBase = document.getElementById("js-settings").getAttribute("data-exportZipApiBase");
}

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

                            url = replaceSideBarString(window.location.hash);
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

        // for escaping the + sign
        url = url.replace(/\+/ig, "%2B");
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
        //console.log(document.querySelectorAll(query1).length)

        if (document.querySelectorAll(query1).length === 1 && 
            !document.querySelector(query1).classList.contains("on") ) {
            document.querySelector(query1).classList.add("on");
        }
    }
}

window.addEventListener("hashchange", function (e) {
    buildSidebarPage();
    startIsSingleitem();
    updateDisplayList();
    updateControls();
}, false);

buildSidebarPage();
updateDisplayList();
updateControls();
// start in DOM

function updateDisplayList() {

    if (document.querySelectorAll(".js-selectedimages").length === 1) {
        var html = "<h2><span class='js-selectedcount'>Geen bestanden geselecteerd</span></h2>";

        html += "<h2><a class='colorbutton js-selectallnone selectall' data-on-click='toggleSelectAll()'><span class='checkbox'></span> Selecteer alles</a>"
            + "<a class='colorbutton js-resetselect' data-on-onclick='resetSelection()'>Deselecteer</a></h2>";
        html += "<h2 class='selectfiletypes'></h2>";
        html += "<ul>";
        for (var i = 0; i < selectedFiles.length; i++) {
            html += "<li title='" + selectedFiles[i] + "'><a class='close' data-filename='" + selectedFiles[i] + "'></a> " + selectedFiles[i] + "</li>";
        }
        html += "</ul>";
        document.querySelector(".js-selectedimages").innerHTML = html;

        // add onclick
        document.querySelector(".js-selectedimages h2 a")
            .addEventListener("click",
                function () {
                    toggleSelectAll()
                }, false);

        document.querySelector(".js-resetselect")
            .addEventListener("click",
                function () {
                    resetSelection()
                }, false);

        for (var i = 0; i < document.querySelectorAll(".js-selectedimages ul li a").length; i++) {
            document.querySelectorAll(".js-selectedimages ul li a")[i]
                .addEventListener("click",
                    function (e) {
                        removeThisItem(e.target.getAttribute("data-filename"))
                    }, false);

        }
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
            document.querySelector(".js-selectallnone").classList.add("on");

            document.querySelector(".js-resetselect").classList.add("hide");

            

            document.querySelector(".js-selectallnone").innerHTML = "Selectie ongedaan maken";
        }
        else {
            document.querySelector(".js-selectallnone").classList.remove("selectnone");
            document.querySelector(".js-selectallnone").classList.add("selectall");
            document.querySelector(".js-selectallnone").classList.remove("on");
            document.querySelector(".js-selectallnone").innerHTML = " Selecteer alles";
            document.querySelector(".js-resetselect").classList.remove("hide");

        }
        // no Deselecteer options when there is nothing selected
        if (selectedFiles.length === 0) {
            document.querySelector(".js-resetselect").classList.add("hide");
        }
        
        // remove content is there a no files
        if (collectionscount === 0) {
            document.querySelector(".js-selectallnone").innerHTML = "";
        }

        addUpdateDisplayTypes();

    }
    // console.log(document.querySelector(".js-exportzip"))

    if (selectedFiles.length === 0 && document.querySelectorAll(".js-exportzip").length === 1) {
        document.querySelector(".js-exportzip").classList.add("disabled");
    }
    if(selectedFiles.length >= 1 && document.querySelectorAll(".js-exportzip").length === 1){
        document.querySelector(".js-exportzip").classList.remove("disabled");
    }

    if (selectedFiles.length === 0 && document.querySelectorAll(".js-exportzip-thumbnail").length === 1) {
        document.querySelector(".js-exportzip-thumbnail").classList.add("disabled");
    }
    if(selectedFiles.length >= 1 && document.querySelectorAll(".js-exportzip-thumbnail").length === 1){
        document.querySelector(".js-exportzip-thumbnail").classList.remove("disabled");
    }
   
}
function updateControls() {
    // console.log(selectedFiles);
    // console.log(document.querySelectorAll(".js-controls"));

    if (selectedFiles.length === 0 && document.querySelectorAll(".js-controls").length === 1) {
        document.querySelector(".js-controls").classList.add("disabled");
        if (document.querySelectorAll(".js-toggle-addorreplace").length === 1) {
            document.querySelector(".js-toggle-addorreplace .colorbutton").classList.add("disabled");
            document.querySelector(".js-keywords").classList.add("disabled");
        }
        if (document.querySelectorAll("#js-keywords-update").length === 1) {
            document.querySelector("#js-keywords-update .btn").classList.add("disabled");
            document.querySelector(".js-captionabstract").classList.add("disabled");
        }
        if (document.querySelectorAll("#js-captionabstract-update").length === 1) {
            document.querySelector("#js-captionabstract-update .btn").classList.add("disabled");
            document.querySelector(".js-objectname").classList.add("disabled");
        }
        if (document.querySelectorAll("#js-objectname-update").length === 1) {
            document.querySelector("#js-objectname-update .btn").classList.add("disabled");
            document.querySelector(".add-colorclass").classList.add("disabled");
        }
        if (document.querySelectorAll(".addDeleteTag").length === 1) {
            document.querySelector(".addDeleteTag").classList.add("disabled");
        }
    }

    if (selectedFiles.length >= 1 && document.querySelectorAll(".js-controls").length === 1) {
        document.querySelector(".js-controls").classList.remove("disabled");
        if (document.querySelectorAll(".js-toggle-addorreplace").length === 1) {
            document.querySelector(".js-toggle-addorreplace .colorbutton").classList.remove("disabled");
            document.querySelector(".js-keywords").classList.remove("disabled");
        }
        if (document.querySelectorAll("#js-keywords-update").length === 1) {
            document.querySelector("#js-keywords-update .btn").classList.remove("disabled");
            document.querySelector(".js-captionabstract").classList.remove("disabled");
        }
        if (document.querySelectorAll("#js-captionabstract-update").length === 1) {
            document.querySelector("#js-captionabstract-update .btn").classList.remove("disabled");
            document.querySelector(".js-objectname").classList.remove("disabled");
        }
        if (document.querySelectorAll("#js-objectname-update").length === 1) {
            document.querySelector("#js-objectname-update .btn").classList.remove("disabled");
            document.querySelector(".add-colorclass").classList.remove("disabled");
        }

        if (document.querySelectorAll(".addDeleteTag").length === 1) {
            document.querySelector(".addDeleteTag").classList.remove("disabled");
        }
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

function updateObjectName() {
    if (document.querySelectorAll("#js-objectname-update").length === 1){
        var keywords = document.querySelector('.js-objectname');

        // check if content already is send to the server
        if (keywords.textContent !== keywords.dataset.previouscontent) {
            queryObjectName(keywords.textContent);
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

function appendOrOverwriteByToggle() {
    var addOrReplaceObject = document.querySelectorAll(".js-toggle-addorreplace .colorbutton");
    if (addOrReplaceObject.length === 1){
        if (addOrReplaceObject[0].className.indexOf(" on") >= 1){
            return "false"; //overwrite or not append
        }
        else {
            return "true"; //overwrite or not append
        }
    }
}

function queryKeywords(queryItem) {
    // keywords!!
    
    var toupdateFiles = toSubpath();

    addNoClickToSidebar();
    showPreloader();
    
    loadJSON(updateApiBase,
        function (data) {
            location.reload();
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST",
        "f=" + toupdateFiles + "&tags=" + queryItem + "&append=" + appendOrOverwriteByToggle()
    );
}

function queryCaptionAbstract(queryItem) {

    var toupdateFiles = toSubpath();

    addNoClickToSidebar();
    showPreloader();
    
    loadJSON(updateApiBase,
        function (data) {
            location.reload();
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST",
        "f=" + toupdateFiles + "&description=" + queryItem + "&append=" + appendOrOverwriteByToggle()
    );
}

function queryObjectName(queryItem) {

    var toupdateFiles = toSubpath();

    addNoClickToSidebar();
    showPreloader();
    
    loadJSON(updateApiBase,
        function (data) {
            location.reload();
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST",
        "f=" + toupdateFiles + "&title=" + queryItem + "&append=" + appendOrOverwriteByToggle()
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
        if (halfitems[i].className.indexOf("hide") === -1 
            && halfitems[i].className.indexOf("directory-false") >= 1) {
            selectedFiles.push(halfitems[i].dataset.filename);
            halfitems[i].classList.add("on");
        }
    }
    var toreplaceUrl = "";
    toreplaceUrl = appendArrayToString(toreplaceUrl,selectedFiles,",");
    // console.log(toreplaceUrl);
    // console.log(getSidebarWindowHashUrl);
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
    startIsSingleitem();
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

// feature for dropdown 
function addUpdateDisplayTypes() {

    if(document.querySelectorAll(".selectfiletypes").length !== 1) return;
    if(document.querySelectorAll(".js-collections-box").length !== 1) return;
    if(document.querySelectorAll(".js-collections").length !== 1) return;

    // only if collections is enabled
    if(document.querySelector(".js-collections").className.indexOf(" on") >= 0 ) return;

    var portfolioData = document.querySelectorAll("#portfolio-data");
    var archive = document.querySelectorAll(".archive");

    var fileUrlsByImageFormat = {};
    
    if (portfolioData.length === 1 && archive.length === 1) {
        for (var i = 0; i < portfolioData[0].children.length; i++) {
            var item = portfolioData[0].children[i];
            var format = item.getAttribute('data-imageformat');
            
            // no dirs
            if(item.className.indexOf("directory-false") === -1) continue;
            
            // add when new
            if(fileUrlsByImageFormat[format] === undefined) fileUrlsByImageFormat[format] = [];
            
            fileUrlsByImageFormat[format].push(item.getAttribute('data-filename'));
        }
    }
  
    // when there are no images at all
    // or then there is only one type
    if(Object.keys(fileUrlsByImageFormat).length <= 1) return;

    // show the dropdown
    document.querySelector(".selectfiletypes").innerHTML += "<div class=\"dropdown\">\n" +
        "  <input type=\"checkbox\" id='selectfiletypes-dropdown' value=\"\" name=\"addUpdateDisplayTypes\">\n" +
        "  <label for=\"selectfiletypes-dropdown\"\n" +
        "     data-toggle=\"dropdown\">\n" +
        "  Bestandstype naar selectie\n" +
        "  </label>\n" +
        "  <ul>\n" +
        "  </ul>\n" +
        "</div>";


    Object.keys(fileUrlsByImageFormat).forEach(function(key) {
        
        var tocreateUrl = "";
        for (var i = 0; i < fileUrlsByImageFormat[key].length; i++) {

            if(i !== fileUrlsByImageFormat[key].length-1) {
                tocreateUrl += fileUrlsByImageFormat[key][i] + ",";
            }
            else {
                tocreateUrl += fileUrlsByImageFormat[key][i];
            }
        }
        // console.log(tocreateUrl);
        
        document.querySelector(".selectfiletypes ul").innerHTML += "<li><li><a href=\"#sidebar="+ tocreateUrl +"\">"+key+"</a></li></li>";
        // console.log(key, obj[key]);

    });

    
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

    // remove the selected items in the halfitem
    var halfitems = document.querySelectorAll(".halfitem");
    for (var i = 0; i < halfitems.length; i++) {
        halfitems[i].classList.remove("on");
    }

}

function addNoClickToSidebar() {
    if (document.querySelectorAll(".sidebar").length === 1) {
        console.log("noclick")
        document.querySelector(".sidebar").classList.add("noclick");

        if (document.querySelectorAll(".js-keywords").length === 1) {
            // hide buttons
            document.querySelector(".js-keywords").classList.add("disabled");
            document.querySelector("#js-keywords-update a").classList.add("disabled");
            document.querySelector('.js-keywords').contentEditable = false;
        }
        if (document.querySelectorAll(".js-captionabstract").length === 1) {
            document.querySelector("#js-captionabstract-update a").classList.add("disabled"); //
            document.querySelector(".js-captionabstract").classList.add("disabled");
            document.querySelector('.js-captionabstract').contentEditable = false;
        }
        if (document.querySelectorAll(".js-objectname").length === 1){
            document.querySelector("#js-objectname-update a").classList.add("disabled");
            document.querySelector(".js-objectname").classList.add("disabled");
            document.querySelector('.js-objectname').contentEditable = false;     
        }
    }
}

// Used in <div class="add-colorclass">
function updateColorClass(those) {
    var toupdateFiles = toSubpath();
    
    addNoClickToSidebar();
    showPreloader();
    
    loadJSON(updateApiBase,
        function(data) {
            location.reload();
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST",
        "f=" + toupdateFiles + "&colorClass=" + those.dataset.colorclass
    );
}

// For readonly folder >= you can't edit them anyway
if (document.querySelectorAll(".sidebar").length === 1) {
    if (document.querySelector(".sidebar").className.indexOf("readonly-true") >= 1) {
        addNoClickToSidebar(); 
    }
}


function toggleOverwriteText() {
    if (document.querySelectorAll(".js-toggle-addorreplace .colorbutton").length === 1) {

        var addOrReplaceObject = document.querySelector(".js-toggle-addorreplace .colorbutton");

        if (addOrReplaceObject.className.indexOf(" on") >= 0) { // with space, butt{on}

            addOrReplaceObject.classList.remove("on");
            if (document.querySelectorAll(".js-title-addorreplace").length === 1){
                document.querySelector(".js-title-addorreplace").innerHTML = "toevoegen"
            }
            document.querySelector("#js-keywords-update > a").innerHTML = "Toevoegen";
            document.querySelector("#js-keywords-update > a").classList.remove("btn-warning");
            document.querySelector("#js-captionabstract-update > a").innerHTML = "Toevoegen";
            document.querySelector("#js-captionabstract-update > a").classList.remove("btn-warning");
            document.querySelector("#js-objectname-update > a").innerHTML = "Toevoegen";
            document.querySelector("#js-objectname-update > a").classList.remove("btn-warning");
        }
        else {
            addOrReplaceObject.classList.add("on");
            if (document.querySelectorAll(".js-title-addorreplace").length === 1){
                document.querySelector(".js-title-addorreplace").innerHTML = "overschrijven"
            }
            document.querySelector("#js-keywords-update > a").innerHTML = "Overschrijf";
            document.querySelector("#js-keywords-update > a").classList.add("btn-warning");
            document.querySelector("#js-captionabstract-update > a").innerHTML = "Overschrijf";
            document.querySelector("#js-captionabstract-update > a").classList.add("btn-warning");
            document.querySelector("#js-objectname-update > a").innerHTML = "Overschrijf";
            document.querySelector("#js-objectname-update > a").classList.add("btn-warning");

            
            

        }
        console.log(addOrReplaceObject)

    }
}


function queryDeleteApi() {

    // uses data-filepath instead of data-filename

    var selectedFilesFullFilePaths = [];
    for (var i = 0; i < selectedFiles.length; i++) {
        var query = ".halfitem[data-filename=\"" + selectedFiles[i] + "\"]";
        var fullFileName = document.querySelector(query).getAttribute('data-filepath');
        selectedFilesFullFilePaths.push(fullFileName);
    }

    var toupdateFiles =  appendArrayToString("", selectedFilesFullFilePaths, ";");
    
    var url = deleteApiBase + "?f=" + toupdateFiles + "&collections=false";

    addNoClickToSidebar();
    showPreloader();
    
    loadJSON(url,
        function (data) {
            location.reload();
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "DELETE"
    );
    
}

function showPreloader() {
    if (document.querySelectorAll(".preloader").length === 1){
        document.querySelector(".preloader").style.display = "block";
    }
}
function hidePreloader() {
    if (document.querySelectorAll(".preloader").length === 1) {
        document.querySelector(".preloader").style.display = "none";
    }
}

if (document.querySelectorAll(".trash").length === 1 ) {
    hidePreloader();
}


if (document.querySelectorAll(".js-collections-box span").length >= 1) {
    if (window.location.search.indexOf("collections=false") >= 0) {
        document.querySelector(".js-collections-box span").innerHTML =  "<a href='" + window.location.search.replace("&collections=false","") + "#sidebar' class='js-collections colorbutton'><span class=\"checkbox\"></span>Collections</a>";
    }
    else if (window.location.search === "") {
        document.querySelector(".js-collections-box span").innerHTML = "<a href='" + window.location.search + "?f=/&collections=false#sidebar' class='js-collections colorbutton on'><span class=\"checkbox\"></span>Collections</a>";
    }
    else {
        document.querySelector(".js-collections-box span").innerHTML =    "<a href='" + window.location.search + "&collections=false#sidebar' class='js-collections colorbutton on'><span class=\"checkbox\"></span>Collections</a>";
    } 
}
// add to all items in index
if (window.location.search.indexOf("collections=false") >= 0) {
    var halfitems = document.querySelectorAll(".halfitem");
    for (var i = 0; i < halfitems.length; i++) {
        halfitems[i].href += "&collections=false"
    }
}
// temp
addUpdateDisplayTypes();

function forceSync(subPath) {
    // force

    console.log(subPath);
    addNoClickToSidebar();
    showPreloader();

    loadJSON(syncApiBase,
        function (data) {
            showPopupDialog("De server gaat nu op de achtergrond bekijken of de inhoud van deze pagina up-to-date is, een klein momentje geduld a.u.b" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        function (xhr) {
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST",
        "f=" + subPath
    );
}



if (document.querySelectorAll(".js-forcesync").length === 1) {
    document.querySelector(".js-forcesync")
        .addEventListener("click",
            function () {
                forceSync(subPath)
            }, false);
}



if (document.querySelectorAll(".js-exportzip").length === 1) {
    document.querySelector(".js-exportzip")
        .addEventListener("click",
            function () {
                exportZip()
            }, false);
}

if (document.querySelectorAll(".js-exportzip-thumbnail").length === 1) {
    document.querySelector(".js-exportzip-thumbnail")
        .addEventListener("click",
            function () {
                exportZip(true)
            }, false);
}


function exportZip(isThumbnail) {
    if(isThumbnail === undefined) isThumbnail = false;
    
    // force

    console.log(subPath);
    showPreloader();
    console.log(exportZipApiBase);

    var toupdateFiles = toSubpath();
   
    loadJSON(exportZipApiBase,
        function (data) {
            var exportZipUrl = "/export/zip/" + data +".zip?json=true";
            var filename = data + ".zip";


            showPopupDialog("Een moment geduld, op de achtergrond wordt een export gemaakt. De duur is afhankelijk van de selectie.");
            
        
            var exportZipUrlsetInterval = setInterval(function () {
                loadJSON(exportZipUrl,
                    function (data) {
                        
                        // temp
                        console.log(data);
                        
                        if(data === "OK") clearInterval(exportZipUrlsetInterval);
                        showPopupDialog("Je kunt het bestand nu downloaden" +
                            "<p>\n" +
                            "<a download='"+filename+"' href='"+exportZipUrl.replace("?json=true","?")+"' class=\"btn-sm btn btn-default\">Download Export als zip</a>\n" +
                            "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Sluit venster</a>\n" +
                            "</p>");
                    },
                    function (xhr) {
                        console.log(xhr)
                    },
                    "GET")
            }, 1000);

        },
        function (xhr) {
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a data-onclick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST",
        "f=" + toupdateFiles + "&json=true&thumbnail="+ isThumbnail
    );
}