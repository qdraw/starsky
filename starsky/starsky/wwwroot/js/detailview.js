
// Req: base url = > updateApiBase
updateApiBase = updateApiBase.replace("&amp;", "&");
infoApiBase = infoApiBase.replace("&amp;", "&");
thumbnailApiBase = thumbnailApiBase.replace("&amp;", "&");
    
function loadJSON(path, success, error, type)
{
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function()
    {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status === 200) {
                if (success) {
                    try {
                        success(JSON.parse(xhr.responseText));
                    }
                    catch(e) {
                        error(xhr)
                    }
                };
            } else {
                if (error)
                    error(xhr);
            }
        }
    };
    xhr.open(type, path, true);
    xhr.send();
}

// Used in <div class="add-colorclass">
function updateColorClass(those) {
    var url = updateApiBase + "&colorClass=" + those.dataset.colorclass;
    
    addUnloadWarning();
    showPreloader();
    console.log(those.dataset.colorclass)
    updateColorClassButtons(those.dataset.colorclass);
    
    loadJSON(url,
        function(data) {
            hideUnloadWarning();
            hidePreloader();
            updateColorClassButtons(data.colorClass);
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

function showPreloader() {
    if (document.querySelectorAll(".preloader").length === 1){
        document.querySelector(".preloader").style.display = "block";
    }
}
function hidePreloader() {
    if (document.querySelectorAll(".preloader").length === 1){
        document.querySelector(".preloader").style.display = "none";
    }
    // document.querySelector('.js-keywords').classList.remove("disabled");
    // document.querySelector("#js-keywords-update .btn").classList.remove("disabled");
}

function updateColorClassButtons(selectedIntColorClass) {
    if (document.querySelectorAll(".add-colorclass").length === 1) {

        for (var i = 0; i < document.querySelector(".add-colorclass").children.length; i++) {

            selectedIntColorClass = parseInt(selectedIntColorClass);
            var datasetItem = parseInt(document.querySelector(".add-colorclass").children[i].dataset.colorclass);
            if (datasetItem === selectedIntColorClass) {
                document.querySelector(".add-colorclass").children[i].classList.add("on");
            }
            else {
                document.querySelector(".add-colorclass").children[i].classList.remove("on");
            }
        }
    }
}

function updateDeletedKeywordElement(data) {
   document.querySelector("#js-keywords-update a").style.display = "inline-block";
   // console.log(document.querySelector("#js-keywords-update a.btn-default").style.display);
   // document.querySelector('.js-keywords').value = data.keywords;
   document.querySelector('.js-keywords').textContent = data.tags;


   if (document.querySelectorAll(".addDeleteTag").length >= 1 ) {
       
       document.querySelector(".addDeleteTag").classList.remove("disabled");
       
       if (data.tags === null) data.tags = "";

       if (data.tags.indexOf("!delete!") >= 0) {
               document.querySelector(".addDeleteTag").classList.add("fileIsDeleted");
               document.querySelector(".addDeleteTag a").innerHTML = "Zet terug uit prullenmand";
               document.querySelector("#js-keywords-update a").classList.add("disabled"); //
               document.querySelector('.js-keywords').contentEditable = false;
               document.querySelector(".addDeleteTag a").classList.remove("btn-danger");
               document.querySelector(".addDeleteTag a").classList.add("btn-warning");
               document.querySelector(".js-keywords").classList.add("disabled");
               document.querySelector(".js-captionabstract").classList.add("disabled");
               document.querySelector('.js-captionabstract').contentEditable = false;

       } else {
               document.querySelector(".addDeleteTag").classList.remove("fileIsDeleted");
               document.querySelector(".addDeleteTag a").innerHTML = "Verplaats naar prullenmand";
               document.querySelector(".addDeleteTag a").classList.remove("btn-warning");
               document.querySelector(".addDeleteTag a").classList.add("btn-danger");
               document.querySelector("#js-keywords-update a").classList.remove("disabled");
               document.querySelector(".js-keywords").classList.remove("disabled");
               document.querySelector('.js-keywords').contentEditable = true;
               document.querySelector(".js-captionabstract").classList.remove("disabled");
               document.querySelector('.js-captionabstract').contentEditable = true;
       }
   }
}


if (document.querySelectorAll("#js-keywords-update").length === 1 && 
    document.querySelectorAll("#js-captionabstract-update").length === 1) 
{
   loadJSON(infoApiBase,
       function(data) {
           
           updateDeletedKeywordElement(data);
           updateCaptionAbstractFromInput(data);
           hidePreloader();
       },
       function (xhr) {
            if (xhr.status === 404) {
                if (document.querySelectorAll(".sidebar").length >= 0) {
                    // toggleSideMenu(true);
                    document.querySelector(".sidebar").classList.add("readonly");
                    document.querySelector("#js-keywords-update .btn").classList.add("disabled");

                    hidePreloader();
                    if (document.querySelectorAll(".navbar").length >= 0) {
                        document.querySelector(".navbar").classList.add("navbar-gray");
                    }
                    if (document.querySelectorAll(".js-filterinfo").length >= 0) {
                        document.querySelector(".js-filterinfo").innerHTML += "<span class='red'>Alleen lezen</span>"
                    }
                }                
            }
            console.error(xhr); 
       },
       "GET"
   );
}

function addDeleteTag() {
    document.querySelector(".addDeleteTag").classList.add("disabled");

    var queryItem = document.querySelector('.js-keywords').textContent;
    
    if (queryItem.length === 0) {
        queryItem = "!delete!";
    } 
    else {
        if (queryItem.indexOf("!delete!") >= 0) {
            queryItem = queryItem.replace("!delete!","");
        } else {
            queryItem += ", !delete!";
        }
    }
    
    if (queryItem === "") {
        queryItem = "?";
    }
    queryKeywords(queryItem);
}

function queryKeywords(queryItem) {

    addUnloadWarning();
    showPreloader();
    
    var url = updateApiBase + "&tags=" + queryItem;
    loadJSON(url,
        function (data) {
            hideUnloadWarning();
            hidePreloader();
            updateDeletedKeywordElement(data);
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

function queryCaptionAbstract(queryItem) {

    addUnloadWarning();
    showPreloader();

    var url = updateApiBase + "&captionabstract=" + queryItem;
    loadJSON(url,
        function (data) {
            hideUnloadWarning();
            hidePreloader();
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

function updateKeywords() {
    if (document.querySelectorAll("#js-keywords-update").length === 1){
        var keywords = document.querySelector('.js-keywords').textContent;
        console.log(keywords);
        
        queryKeywords(keywords);
    } 
}

function updateCaptionAbstract() {
    if (document.querySelectorAll("#js-captionabstract-update").length === 1){
        var captionabstract = document.querySelector('.js-captionabstract').textContent;
        console.log(captionabstract);

        queryCaptionAbstract(captionabstract);
    }
}

function updateCaptionAbstractFromInput(data) {
    if (document.querySelectorAll("#js-captionabstract-update").length === 1) {
        document.querySelector('.js-captionabstract').textContent = data["Caption-Abstract"];
        document.querySelector('#js-captionabstract-update .btn').classList.remove("disabled");
    
    }
}
    


var formSubmitting = true;
var addUnloadWarning = function() { formSubmitting = false; };
var hideUnloadWarning = function() { formSubmitting = true; };

window.onload = function() {
    window.addEventListener("beforeunload", function (e) {
        if (formSubmitting) {
            return undefined;
        }

        var confirmationMessage = 'It looks like you have been editing something. '
            + 'If you leave before saving, your changes will be lost.';

        (e || window.event).returnValue = confirmationMessage; //Gecko + IE
        return confirmationMessage; //Gecko + Webkit, Safari, Chrome etc.
    });
};

var sideBarDefaultWidth;
if (document.querySelectorAll(".sidebar").length === 1) {
    function toggleSideMenu(isStartup) {

        if (document.querySelector(".sidebar .close").className.indexOf("collapsed") === -1) {
            if (!isStartup){
                window.location.hash = window.location.hash.replace("#sidebar", "");
            }
            updatePrevNextHash();

            document.querySelector(".sidebar .close").classList.add("collapsed");
            sideBarDefaultWidth = document.querySelector(".sidebar").style.width;
            document.querySelector(".sidebar").style.width = "0px";
            sideBarDefaultPadding = document.querySelector(".sidebar").style.padding;
            document.querySelector(".sidebar").style.padding = "0px";
            document.querySelector(".sidebar .content").classList.add("collapsed");

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
                window.location.hash = '#sidebar';
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

    // Default on mobile disabled
    // Default on desktop enabled
    toggleSideMenu(true);
    toggleSideMenu(true);

    if (window.location.hash.indexOf("sidebar") === -1 && window.innerWidth <= 650) {
        toggleSideMenu(true);
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

            object[j].href = object[j].href.replace("#sidebar","");
            object[j].href += window.location.hash;
        }
    }

}

// Adding filter options to current breadcrum indexer
// > transform it to javascript fast filter style
// looking for ?f=/20161217_180000_imc.jpg&colorclass=7,0 urls

if (document.querySelectorAll(".breadcrumb").length >= 1) {
    var breadcrumbObject = document.querySelector(".breadcrumb").children;
    if(window.location.search.indexOf("colorclass") === -1) {
        if (breadcrumbObject.length >= 4) {
            for (var i = 0; i < breadcrumbObject.length; i++) {
                if (i === breadcrumbObject.length - 3) {
                    breadcrumbObject[i].href += "#anchor=" + breadcrumbObject[i + 2].textContent.trim()
                }
            }
        }
    }

    if(window.location.search.indexOf("colorclass") >= 0) {
        var searchPositionCl = window.location.search.indexOf("colorclass") - 1;
        addcolorclassHash = window.location.search.substr(searchPositionCl+("colorclass".length+2), window.location.search.length);
        var addcolorclassArray = [];
        if (addcolorclassHash.indexOf(",")>= 0){
            addcolorclassArray = addcolorclassHash.split(",");
        }
        else {
            addcolorclassArray.push(addcolorclassHash);
        }

        if (breadcrumbObject.length >= 4) {
            for (var i = 0; i < breadcrumbObject.length; i++) {

                console.log(breadcrumbObject)

                
                if (i === breadcrumbObject.length-3) {
                    breadcrumbObject[i].href += "#colorclass=";
                    for (var j = 0; j < addcolorclassArray.length; j++) {
                        breadcrumbObject[i].href += "colorclass-" + addcolorclassArray[j] + ",";
                    }
                    breadcrumbObject[i].href = breadcrumbObject[i].href.replace(/,$/ig, "");

                    if (addcolorclassArray.length >= 1) {
                        breadcrumbObject[i].href += ";anchor=" + breadcrumbObject[i + 2].textContent.trim()
                    }
                    
                }
            }
        }
    }
}


document.addEventListener('keydown', (event) => {
    if (document.activeElement.className.indexOf("form-control") === -1) {
        const keyName = event.key;

        if (keyName === "Escape" && document.querySelectorAll(".breadcrumb").length === 1){
            var breadcrumbObjectKey = document.querySelector(".breadcrumb").children;
            if (breadcrumbObjectKey.length >= 4) {
                for (var i = 0; i < breadcrumbObjectKey.length; i++) {
                    if (i === breadcrumbObjectKey.length - 3) {
                        console.log("esc");
                        breadcrumbObjectKey[i].click();
                    }
                }
            }
        }
        
        if (keyName === "8" && document.querySelectorAll(".add-colorclass .colorclass-8").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-8"));
        }
    
        if (keyName === "7" && document.querySelectorAll(".add-colorclass .colorclass-7").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-7"));
        }

        if (keyName === "6" && document.querySelectorAll(".add-colorclass .colorclass-6").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-6"));
        }
        if (keyName === "5" && document.querySelectorAll(".add-colorclass .colorclass-5").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-5"));
        }

        if (keyName === "4" && document.querySelectorAll(".add-colorclass .colorclass-4").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-4"));
        }
        if (keyName === "3" && document.querySelectorAll(".add-colorclass .colorclass-3").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-3"));
        }
        if (keyName === "2" && document.querySelectorAll(".add-colorclass .colorclass-2").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-2"));
        }
        if (keyName === "1" && document.querySelectorAll(".add-colorclass .colorclass-1").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-1"));
        }
        if ((keyName === "0" || keyName === "9" ) && document.querySelectorAll(".add-colorclass .colorclass-0").length === 1){
            updateColorClass(document.querySelector(".add-colorclass .colorclass-0"));
        }
        if (keyName === "Delete" && document.querySelectorAll(".addDeleteTag").length === 1){
            addDeleteTag()
        }
        if (keyName === "i" && document.querySelectorAll(".js-keywords").length === 1){
            document.querySelector(".js-keywords").focus();
        }
    }
});


function checkIfContentIsNot204() {
    
    if (thumbnailApiBase === undefined) return;
    
    loadJSON(thumbnailApiBase,
        function (data) {},
        function (xhr) { 
            if (xhr.status === 204 && 
                document.querySelectorAll(".status204button").length >= 0) 
            {
                document.querySelector(".status204button").classList.remove("hide");
            }
        },
        "GET"
    );
}
checkIfContentIsNot204();

function retry204() {
    showPreloader();
    var url = thumbnailApiBase += "&retryThumbnail=True";
    loadJSON(url,
        function () {
            location.reload();
        },
        function (xhr) {
            location.reload();
        },
        "GET"
    );  
}
