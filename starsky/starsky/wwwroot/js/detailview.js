
// Req: base url = > updateApiBase
updateApiBase = updateApiBase.replace("&amp;", "&");
infoApiBase = infoApiBase.replace("&amp;", "&") + "&collections=true";
thumbnailApiBase = thumbnailApiBase.replace("&amp;", "&");
    


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
            updateColorClassButtons(data[0].colorClass);
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a onClick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
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
               document.querySelector(".addDeleteTag a").classList.remove("btn-danger");
               document.querySelector(".addDeleteTag a").classList.add("btn-warning");
               document.querySelector(".js-keywords").classList.add("disabled");
               document.querySelector("#js-keywords-update a").classList.add("disabled"); //
               document.querySelector('.js-keywords').contentEditable = false;
               document.querySelector(".js-captionabstract").classList.add("disabled");
               document.querySelector('.js-captionabstract').contentEditable = false;
               document.querySelector(".js-objectname").classList.add("disabled");
               document.querySelector('.js-objectname').contentEditable = false;
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
               document.querySelector(".js-objectname").classList.remove("disabled");
               document.querySelector('.js-objectname').contentEditable = true;
       }
   }
}


if (document.querySelectorAll("#js-keywords-update").length === 1 && 
    document.querySelectorAll("#js-captionabstract-update").length === 1) 
{
   loadJSON(infoApiBase,
       function(data) {
           updateCollectionsSwitch(data);
           updateDeletedKeywordElement(data[0]);
           updateColorClassButtons(data[0].colorClass);
           updateCaptionAbstractFromInput(data[0]);
           updateObjectNameFromInput(data[0]);
           hidePreloader();
       },
       function (xhr) {
            if (xhr.status === 404 || xhr.status === 203) {
                if (document.querySelectorAll(".sidebar").length >= 0) {
                    // toggleSideMenu(true);
                    document.querySelector(".sidebar").classList.add("readonly");
                    document.querySelector("#js-keywords-update .btn").classList.add("disabled");

                    hidePreloader();
                    if (document.querySelectorAll(".head").length >= 0) {
                        document.querySelector(".head").classList.add("head-gray");
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
    
    if (queryItem.indexOf("!delete!") >= 0) {
        updateColorClass(document.querySelector(".add-colorclass a.colorclass-8"))
    }
    
    var url = updateApiBase + "&tags=" + queryItem;
    loadJSON(url,
        function (data) {
            hideUnloadWarning();
            hidePreloader();
            updateDeletedKeywordElement(data[0]);
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a onClick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST"
    );
}

function queryCaptionAbstract(queryItem) {

    addUnloadWarning();
    showPreloader();

    var url = updateApiBase + "&description=" + queryItem;
    loadJSON(url,
        function (data) {
            hideUnloadWarning();
            hidePreloader();
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a onClick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST"
    );
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

function updateObjectName() {
    if (document.querySelectorAll("#js-objectname-update").length === 1){
        var objectname = document.querySelector('.js-objectname');

        // check if content already is send to the server
        if (objectname.textContent !== objectname.dataset.previouscontent) {
            queryObjectName(objectname.textContent);
            objectname.dataset.previouscontent = objectname.textContent;
        }
    }
}

function queryObjectName(queryItem) {

    addUnloadWarning();
    showPreloader();

    var url = updateApiBase + "&title=" + queryItem;
    loadJSON(url,
        function (data) {
            hideUnloadWarning();
            hidePreloader();
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a onClick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST"
    );
}

function updateObjectNameFromInput(data) {
    if (document.querySelectorAll("#js-objectname-update").length === 1) {
        document.querySelector('.js-objectname').textContent = data["title"];
        document.querySelector('#js-objectname-update .btn').classList.remove("disabled");
    }
}


function updateCaptionAbstractFromInput(data) {
    if (document.querySelectorAll("#js-captionabstract-update").length === 1) {
        document.querySelector('.js-captionabstract').textContent = data["description"];
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


// Adding filter options to current breadcrum indexer
// > transform it to javascript fast filter style
// looking for ?f=/20161217_180000_imc.jpg&colorclass=7,0 urls

if (document.querySelectorAll(".breadcrumb").length >= 1) {
    var breadcrumbObject = document.querySelector(".breadcrumb").children;

    // add it to breadcrumb if collections is in the url
    if (window.location.search.indexOf("collections=false") >= 0) {
        for (var i = 0; i < breadcrumbObject.length; i++) {
            breadcrumbObject[i].href += "&collections=false";
        }
    }
    
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


document.addEventListener('keydown', function (event) {
    
    if (document.activeElement.className.indexOf("form-control") === -1) {
        const keyName = event.key;
    
        if (keyName === "Escape" && document.querySelectorAll(".breadcrumb").length === 1){
            var breadcrumbObjectKey = document.querySelector(".breadcrumb").children;
            if (breadcrumbObjectKey.length >= 4) {
                // :not(.removecache)
                for (var i = 0; i < breadcrumbObjectKey.length; i++) {
                    if (i === breadcrumbObjectKey.length - 3 &&
                    document.querySelector("#popup").className.indexOf("on") === -1) {
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
            event.preventDefault();
            // Set the selection to the end of the input field
            var content = document.querySelector(".js-keywords");
            content.focus();
            var selection;
            var char = document.querySelector(".js-keywords").innerHTML.length;

            // trick to fill the field before selecting
            if (char === 0) {
                document.querySelector(".js-keywords").innerHTML = "&nbsp;";
            }
            
            if (document.selection) {
                selection = document.selection.createRange();
                selection.moveStart('character', char);
                selection.select();
            }
            else {
                selection = window.getSelection();
                selection.collapse(content.firstChild, char);
            }
            

        }
    }
},false);    




function checkIfContentIsNot204or202() {
    
    if (thumbnailApiBase === undefined) return;
    
    loadJSON(thumbnailApiBase,
        function (data) {},
        function (xhr) { 
            if (xhr.status === 204 && document.querySelectorAll(".status204button").length >= 0) 
            {
                document.querySelector(".status204button").classList.remove("hide");
            }
            if (xhr.status === 202 && document.querySelectorAll(".main-image").length >= 1) {
                rotateOn202();
                if(document.querySelectorAll(".breadcrumb").length >= 1) {
                    document.querySelector(".breadcrumb").classList.add("nothumbnail");
                }
            }
        },
        "GET"
    );
}
checkIfContentIsNot204or202();

function rotateOn202() {

    if (document.querySelectorAll(".main-image img").length === 0) return;

    // in safari on iOS, but not on Mac OS, images are rotating based on jpeg exif
    var isWebkit = 'WebkitAppearance' in document.documentElement.style;
    var isSafari = navigator.userAgent.indexOf("Safari") >= 1;
    if (((navigator.userAgent.indexOf("iPhone") > -1) || (navigator.userAgent.indexOf("iPad") > -1) )
        && (isWebkit && isSafari ) && 
        (updateApiBase.indexOf(".jpg") >= 1 || updateApiBase.indexOf(".jpeg") >= 1 ) ) {
        return;
    }
    
    var classList = document.querySelector(".main-image img").classList;
    if (classList.contains("disabled-Rotate90Cw")) {
        document.querySelector(".main-image img").classList.remove("disabled-Rotate90Cw");
        document.querySelector(".main-image img").classList.add("Rotate90Cw");
        if (document.querySelectorAll(".sidebar .content img").length >= 1) {
            document.querySelector(".sidebar .content img").classList.add("Rotate90Cw");
        }
    }

    if (classList.contains("disabled-Rotate270Cw")) {
        document.querySelector(".main-image img").classList.remove("disabled-Rotate270Cw");
        document.querySelector(".main-image img").classList.add("Rotate270Cw");
        if (document.querySelectorAll(".sidebar .content img").length >= 1) {
            document.querySelector(".sidebar .content img").classList.add("Rotate270Cw");
        }
    }

    if (classList.contains("disabled-Rotate180")) {
        document.querySelector(".main-image img").classList.remove("disabled-Rotate180");
        document.querySelector(".main-image img").classList.add("Rotate180");
        if (document.querySelectorAll(".sidebar .content img").length >= 1) {
            document.querySelector(".sidebar .content img").classList.add("Rotate180");
        }
    }
}



// function imageToDataUri(img, width, height) {
//
//     // create an off-screen canvas
//     var canvas = document.createElement('canvas'),
//         ctx = canvas.getContext('2d');
//
//     // set its dimension to target size
//     canvas.width = width;
//     canvas.height = height;
//
//     // draw source image into the off-screen canvas:
//     ctx.drawImage(img, 0, 0, width, height);
//
//     // encode image to data-uri with base64 version of compressed image
//     return canvas.toDataURL();
// }
//
// function rotateImage(srcBase64, srcOrientation, callback) {
//     var img = new Image();
//
//     img.onload = function() {
//         var width = img.width,
//             height = img.height,
//             canvas = document.createElement('canvas'),
//             ctx = canvas.getContext("2d");
//
//         // set proper canvas dimensions before transform & export
//         if (4 < srcOrientation && srcOrientation < 9) {
//             canvas.width = height;
//             canvas.height = width;
//         } else {
//             canvas.width = width;
//             canvas.height = height;
//         }
//
//         // transform context before drawing image
//         switch (srcOrientation) {
//             case 2: ctx.transform(-1, 0, 0, 1, width, 0); break;
//             case 3: ctx.transform(-1, 0, 0, -1, width, height ); break;
//             case 4: ctx.transform(1, 0, 0, -1, 0, height ); break;
//             case 5: ctx.transform(0, 1, 1, 0, 0, 0); break;
//             case 6: ctx.transform(0, 1, -1, 0, height , 0); break;
//             case 7: ctx.transform(0, -1, -1, 0, height , width); break;
//             case 8: ctx.transform(0, -1, 1, 0, 0, width); break;
//             default: break;
//         }
//
//         // draw image
//         ctx.drawImage(img, 0, 0);
//
//         // export base64
//         callback(canvas.toDataURL());
//     };
//
//     img.src = srcBase64;
// }




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




// function rotateImageRight(currentOrientation,relativeRotation) {
//     // document.querySelector()
//     console.log(currentOrientation);
//     console.log(relativeRotation);
//     console.log("--");
//
//     // should not happen =>
//     currentOrientation = currentOrientation.replace("DoNotChange","Horizontal");
//    
//    
//     var typesOrRotation = [
//         "Horizontal",
//         "Rotate90Cw",
//         "Rotate180",
//         "Rotate270Cw"
//     ];
//     var intForRotation = [
//         1,
//         6,
//         3,
//         8
//     ];
//
//     var currentOrentation = typesOrRotation.indexOf(currentOrientation);
//     var transformToInt = null;
//     if (currentOrentation >= 0 && currentOrentation+relativeRotation <= typesOrRotation.length && currentOrentation+relativeRotation >= 0) {
//         transformToInt = intForRotation[currentOrentation+relativeRotation]
//     }
//     if (currentOrentation+relativeRotation === -1) {
//         transformToInt = intForRotation[typesOrRotation.length-1]
//     }
//     if (currentOrentation+relativeRotation >= typesOrRotation.length) {
//         transformToInt = intForRotation[0]
//     }
//    
//     if (transformToInt !== null) {
//         queryRotate(transformToInt);
//     }
//     console.log(transformToInt);
//
// }

function queryRotate(queryItem) {
    // reload afterwards
    
    addUnloadWarning();
    showPreloader();

    var url = updateApiBase + "&rotateClock=" + queryItem;
    loadJSON(url,
        function (data) {
            setTimeout(function(){
                hideUnloadWarning();
                hidePreloader();
                location.reload();
            }, 1000);
        },
        function (xhr) { 
            console.error(xhr);
            showPopupDialog("Sorry er is iets misgegaan, probeer het aub opnieuw" +
                "<p>\n" +
                "<a onClick=\"location.reload()\" class=\"btn-sm btn btn-default\">Herlaad pagina</a>\n" +
                "</p>");
        },
        "POST"
    );
}


function updateCollectionsSwitch(data) {
    if (data.length <= 1) return;
    if (document.querySelectorAll(".js-collectionsswitch div").length === -1) return;
    
    document.querySelector(".js-collectionsswitch div").innerHTML = "";
    
    for (var i = 0; i < data.length; i++) {
        document.querySelector(".js-collectionsswitch div").innerHTML += "<a class='btn btn-default small' href='?f="+ data[i].filePath +"'>"+ data[i].imageFormat+ "</a> ";
        console.log(data)
    }
}
document.addEventListener("DOMContentLoaded", function() {
    if (document.querySelectorAll(".main-image").length === 1){
        document.querySelector(".main-image").classList.add("contentloaded");
    }
});