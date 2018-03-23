
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

    loadJSON(url,
        function(data) {
            updateColorClassButtons(data.colorClass);
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

function updateColorClassButtons(selectedIntColorClass) {
    if (document.querySelectorAll(".add-colorclass").length === 1) {

        for (var i = 0; i < document.querySelector(".add-colorclass").children.length; i++) {

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
   document.querySelector('.js-keywords').value = data.keywords;
   document.querySelector('.js-keywords').disabled = false;

   if (document.querySelectorAll(".addDeleteTag").length >= 1 ) {
       document.querySelector(".addDeleteTag").style.display = "inline-block";
       if (data.keywords === null) data.keywords = "";

       if (data.keywords.indexOf("!delete!") >= 0) {
               document.querySelector(".addDeleteTag a").innerHTML = "Zet terug uit prullenmand";
               document.querySelector("#js-keywords-update a").style.display = "none"; //
               document.querySelector('.js-keywords').disabled = true;
               document.querySelector(".addDeleteTag a").classList.remove("btn-danger");
               document.querySelector(".addDeleteTag a").classList.add("btn-warning");
    
           } else {
               document.querySelector(".addDeleteTag a").innerHTML = "Verplaats naar prullenmand";
               document.querySelector(".addDeleteTag a").classList.remove("btn-warning");
               document.querySelector(".addDeleteTag a").classList.add("btn-danger");
           }
   }

}


if (document.querySelectorAll("#js-keywords-update").length === 1) {
   loadJSON(infoApiBase,
       function(data) { updateDeletedKeywordElement(data); },
       function (xhr) { console.error(xhr); },
       "GET"
   );
}

function addDeleteTag() {
    document.querySelector(".addDeleteTag").style.display = "none";

    var queryItem = document.querySelector('.js-keywords').value;
    
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
    var url = updateApiBase + "&keywords=" + queryItem;
    loadJSON(url,
        function (data) {
            updateDeletedKeywordElement(data)
        },
        function (xhr) { console.error(xhr); },
        "POST"
    );
}

function updateKeywords() {
    if (document.querySelectorAll("#js-keywords-update").length === 1){
        var keywords = document.querySelector('.js-keywords').value;
        queryKeywords(keywords);
    } 
}


