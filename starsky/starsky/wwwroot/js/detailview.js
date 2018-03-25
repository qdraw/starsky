
// Req: base url = > updateApiBase
updateApiBase = updateApiBase.replace("&amp;", "&");
infoApiBase = infoApiBase.replace("&amp;", "&");

function loadJSON(path, success, error, type)
{
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function()
    {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status === 200 || xhr.status === 205) {
                if (success) {
                    console.log(xhr.responseText);
                    success(JSON.parse(xhr.responseText));
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
   document.querySelector('.js-keywords').textContent = data.keywords;


   if (document.querySelectorAll(".addDeleteTag").length >= 1 ) {
       document.querySelector(".addDeleteTag").style.display = "inline-block";
       if (data.keywords === null) data.keywords = "";

       if (data.keywords.indexOf("!delete!") >= 0) {
               document.querySelector(".addDeleteTag a").innerHTML = "Zet terug uit prullenmand";
               document.querySelector("#js-keywords-update a").classList.add("disabled") //
               document.querySelector('.js-keywords').contentEditable = false;
               document.querySelector(".addDeleteTag a").classList.remove("btn-danger");
               document.querySelector(".addDeleteTag a").classList.add("btn-warning");
               document.querySelector(".js-keywords").classList.add("disabled");
    
           } else {
               document.querySelector(".addDeleteTag a").innerHTML = "Verplaats naar prullenmand";
               document.querySelector(".addDeleteTag a").classList.remove("btn-warning");
               document.querySelector(".addDeleteTag a").classList.add("btn-danger");
               document.querySelector("#js-keywords-update a").classList.remove("disabled");
               document.querySelector(".js-keywords").classList.remove("disabled");
               document.querySelector('.js-keywords').contentEditable = true;

       }
   }
}


if (document.querySelectorAll("#js-keywords-update").length === 1) {
   loadJSON(infoApiBase,
       function(data) { 
           
           updateDeletedKeywordElement(data);
           hidePreloader();

       },
       function (xhr) { console.error(xhr); },
       "GET"
   );
}

function addDeleteTag() {
    document.querySelector(".addDeleteTag").style.display = "none";

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
    
    var url = updateApiBase + "&keywords=" + queryItem;
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

function updateKeywords() {
    if (document.querySelectorAll("#js-keywords-update").length === 1){
        var keywords = document.querySelector('.js-keywords').textContent;
        console.log(keywords);
        
        queryKeywords(keywords);
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
            document.querySelector(".sidebar .close").classList.add("collapsed");
            sideBarDefaultWidth = document.querySelector(".sidebar").style.width;
            document.querySelector(".sidebar").style.width = "0px";
            sideBarDefaultPadding = document.querySelector(".sidebar").style.padding;
            document.querySelector(".sidebar").style.padding = "0px";
            document.querySelector(".sidebar .content").classList.add("collapsed");
            
            if(document.querySelectorAll(".main-image").length === 1){
                document.querySelector(".main-image").classList.remove("collapsed")
            }
            if(document.querySelectorAll(".detailview").length === 1){
                document.querySelector(".detailview").classList.remove("collapsed")
            }
            if(document.querySelectorAll(".body-content").length === 1){
                document.querySelector(".body-content").classList.remove("collapsed")
            }
            document.querySelector("body").classList.remove("collapsed")

        }
        else {
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
            if(document.querySelectorAll(".main-image").length === 1){
                document.querySelector(".main-image").classList.add("collapsed")
            }
            if(document.querySelectorAll(".detailview").length === 1){
                document.querySelector(".detailview").classList.add("collapsed")
            }
            if(document.querySelectorAll("body-content").length === 1){
                document.querySelector(".body-content").classList.add("collapsed")
            }
            document.querySelector("body").classList.add("collapsed")

        }
    }

    if(window.innerWidth <= 650) {
        toggleSideMenu(true);
    }
    else {
        toggleSideMenu(true);
        toggleSideMenu(true);
    }
    
}