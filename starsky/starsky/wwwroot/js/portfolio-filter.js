


var tagList = [];
var prefFilter = "";
var currentFilter = "";

makeTagList();

tagList = removeArrayDuplicate(tagList);
// alphabetic order
tagList = tagList.sort();

writeFilterList (tagList);


// Write down a list of portfolio catogories
function makeTagList() {
    if (document.querySelectorAll("#portfolio-data").length === 1 && document.querySelectorAll("#portfolio-filter").length === 1){
        // var data = {};


        for (var i = 0; i < document.querySelector("#portfolio-data").children.length; i++) {
            var object = document.querySelector("#portfolio-data").children[i];
            var tags = object.getAttribute('data-tag');
            var name = object.getAttribute('href');

            if (name !== null) {
                name = name.replace(".html","");
            }

            if (tags !== null) {
                tags = 	tags.split(",");

                for (var j = 0; j < tags.length; j++) {
                    tagList.push(tags[j])
                }
            }

        }

    }
}


function removeArrayDuplicate (array) {

    var temp = {};
    for (var i = 0; i < array.length; i++)
        temp[array[i]] = true;
    var r = [];
    for (var k in temp)
        r.push(k);
    return r;

}//e//arrayDuplicate

// Writing filter for portfolio page
function writeFilterList (tags) {

    if (document.querySelectorAll("#portfolio-data").length === 1 && 
        document.querySelectorAll("#portfolio-filter").length === 1)
    {
        // add reset
        document.querySelector("#portfolio-filter").innerHTML = "";
        
        var ul = document.createElement("ul");
        ul.className = "tags";
        document.querySelector("#portfolio-filter").appendChild(ul);

        var filterarticle = document.querySelector("#portfolio-filter ul")

        if(tags.length >= 2){
            // ALLES!! nu RESET
            var li_alles = document.createElement("li");
            var a_alles = document.createElement("a");
            var currentitem_alles = filterarticle.appendChild(li_alles).appendChild(a_alles);
            currentitem_alles.innerHTML = "Reset";
            currentitem_alles.className = "reset";
            currentitem_alles.addEventListener("click", function(e){ var those = this; resetCheckBoxes();setVariable([]); constructURL(); }, false);
            // EINDE ALLES

            var newTags = [];
            if(tags.indexOf("Winner") >= 0) {
                newTags.push(tags[tags.indexOf("Winner")])
            }

            if(tags.indexOf("WinnerAlt") >= 0) {
                newTags.push(tags[tags.indexOf("WinnerAlt")])
            }

            if(tags.indexOf("Superior") >= 0) {
                newTags.push(tags[tags.indexOf("Superior")])
            }

            if(tags.indexOf("SuperiorAlt") >= 0) {
                newTags.push(tags[tags.indexOf("SuperiorAlt")])
            }

            if(tags.indexOf("Typical") >= 0) {
                newTags.push(tags[tags.indexOf("Typical")])
            }

            if(tags.indexOf("TypicalAlt") >= 0) {
                newTags.push(tags[tags.indexOf("TypicalAlt")])
            }

            if(tags.indexOf("Extras") >= 0) {
                newTags.push(tags[tags.indexOf("Extras")])
            }
            if(tags.indexOf("Trash") >= 0) {
                newTags.push(tags[tags.indexOf("Trash")])
            }

            if(tags.indexOf("None") >= 0) {
                newTags.push(tags[tags.indexOf("None")])
            }

            window.subject = newTags;

            for (var i = 0; i < newTags.length; i++) {
                var li = document.createElement("li");
                var a = document.createElement("a");

                var currentitem = filterarticle.appendChild(li).appendChild(a);

                currentitem.addEventListener("click", function(e){ var those = this; readVariable(those); }, false);
                //   currentitem.href = "#" + newTags[i].toLowerCase().replace(/ /ig, "-");
                
                currentitem.id = newTags[i].toLowerCase().replace(/ /ig, "-");
                currentitem.innerHTML = "<span class='checkbox'></span>" + newTags[i];
                //"<span class='checkbox'></span>"
            }
            
        }
    }

}///e/writeFilterList


function createSlug(tagList) {
    var linkTagList = [];
    for (var i = 0; i < tagList.length; i++) {
        linkTagList.push(tagList[i].toLowerCase().replace(/ /ig, "-"));
    }
    return linkTagList;
}


//
// window.onhashchange = function() {
//     filterPage();
// };
//

function resetCheckBoxes() {
    var object = document.querySelector("#portfolio-filter ul").children;
    for (var i = 0; i < object.length; i++) {

        if (object[i].children.length >= 1) {
            linkobject = object[i].children[0];

            linkobject.classList.remove("active");
            linkobject.classList.add("none");

        }
    }
}

//
// function showLegenda() {
//    
//     if (window.location.hash != "") {
//
//         var hashname = window.location.hash.substring(1,window.location.hash.length).split(",");
//         console.log(hashname);
//        
//         reset();
//        
//         var legendaobject = document.querySelector("#portfolio-filter" + " ." +  hashname[0]);
//         legendaobject.className  = legendaobject.className.replace(/ off| on/ig,"") + " on"
//     }
//     if (window.location.hash === "") {
//         reset();
//         if (document.querySelectorAll("#portfolio-filter .alles").length === 1) {
//             document.querySelector("#portfolio-filter .alles").className = "alles on"
//         }
//     }
// }
//
// function filterPage() {
//     console.log("sd")
//
//     var hashword = "";
//     linkTagList = createSlug(tagList);
//
//     if (window.location.hash !== "") {
//         hashword = window.location.hash.substring(1,window.location.hash.length);
//     }
//
//     if (window.location.hash !== "" && linkTagList.indexOf(hashword) >= 0) {
//         showLegenda();
//
//         currentFilter = hashword;
//         if (linkTagList.indexOf(hashword) >= 0) {
//             hideElementsByTag(hashword);
//         }
//     }
//
//     if (window.location.hash === "") {
//         showLegenda();
//         var object = document.querySelector("#portfolio-data").children;
//         for (var i = 0; i < object.length; i++) {
//             object[i].classList.remove("hide");
//             object[i].classList.add("show");
//         }
//     }
// }
//
//
// function hideElementsByTag(hashword) {
//     if (document.querySelectorAll("#portfolio-data").length === 1 
//         && document.querySelectorAll("#portfolio-filter").length === 1)
//     {
//    
//         var object = document.querySelector("#portfolio-data").children;
//         for (var i = 0; i < object.length; i++) {
//
//             var tags = object[i].getAttribute('data-tag');
//             var thisTags = [];
//
//             if (tags !== null) {
//                 tags = 	tags.split(",");
//
//                 for (var j = 0; j < tags.length; j++) {
//                     thisTags.push(tags[j])
//                 }
//                 thisTags = createSlug(thisTags);
//
//             }
//             if (thisTags.indexOf(hashword) >= 0) {
//                 // object[i].className = "halfitem show";
//                 object[i].classList.remove("hide");
//                 object[i].classList.add("show");
//
//             }
//             if (thisTags.indexOf(hashword) === -1) {
//                 // object[i].className = "halfitem hide";
//                 object[i].classList.remove("show");
//                 object[i].classList.add("hide");
//             }
//
//         }
//
//     }
// }
//






var selectedVar = [];
function readVariable (those) {
    // those is this of the eventListerer


    // When you click on the checkbox


    // There used to be a reset;	
    // isFilterCountryActive = false;

    var i;

    if (those !== undefined) {

        if ( selectedVar.indexOf(those.id) === -1 ) {
            selectedVar.push(those.id);
        }
        else {
            var index = selectedVar.indexOf(those.id);
            // delete storeActiveVar[index]; // replace this item of the list with: ""
            selectedVar.splice( index, 1 ); // delete this item of the array;
        }

        // reset all checkboxes to none;
        for (i = 0; i < window.subject.length; i++) {
            resetCheckBoxes();
        }

        // set the selected checkboxes to active;
        for (i = 0; i < selectedVar.length; i++) {
            document.querySelector("#portfolio-filter ul #" + selectedVar[i]).className = "active";
        }
    }
    else {
        // direct input; for example using a url (no reset needed);
        for (i = 0; i < selectedVar.length; i++) {
            document.querySelector("#portfolio-filter ul #" + selectedVar[i]).className = "active";
        }
    }

    // exept for direct input;	> create an unique url;
    if (those !== undefined) {
        constructURL();
    }

    setVariable(selectedVar);
}


var prevURL;
function constructURL() {
    // Please check: buildURL() for usage;

    if (history.pushState) { // for old browsers like: IE9/IE10

        var url = "#colorclass=";
        for (var i = 0; i < selectedVar.length; i++) {

            if (i === selectedVar.length-1) {
                url += selectedVar[i];
            }
            else {
                url += selectedVar[i] + ",";
            }
        }
        
        if (selectedVar.length === 0) {
            url = "#";
        }
        
        // Check if this function isn't repeated excuded;
        if (url !== prevURL) {
            var stateObj = { url: url };
            history.pushState(stateObj, "Qdraw", url);
        }
        prevURL = url;
    }

}

function setVariable(hashList) {
    
    if (document.querySelectorAll("#portfolio-data").length === 1 
        && document.querySelectorAll("#portfolio-filter").length === 1)
    {

        if(hashList[0] === "") {
            hashList = [];
        }
        
        var object = document.querySelector("#portfolio-data").children;

        for (var i = 0; i < object.length; i++) {

            var tag = object[i].getAttribute('data-tag').toLowerCase();
           
            if(hashList.length >= 0) {
                object[i].classList.remove("hide");
                object[i].classList.add("show");     
            }

            if(hashList.length >= 1) {
    
                if (hashList.indexOf(tag) >= 0) {
                    object[i].classList.remove("hide");
                    object[i].classList.add("show");
                }
                if (hashList.indexOf(tag) === -1) {
                    object[i].classList.remove("show");
                    object[i].classList.add("hide");
                }
            }
        }
        
    }
}

function buildPage() {
    
    if (document.querySelectorAll(".preloader").length === 1){
        document.querySelector(".preloader").style.display = "none";
    }
    
    var urlsubject = location.hash.replace("#colorclass=", "");
    urlsubject = urlsubject.split(",");

    var object = document.querySelector("#portfolio-filter .tags").children;
    // console.log(urlsubject);

    for (var i = 0; i < object.length; i++) {
        object[i].children[0].classList.remove("active");
    }
    
    setVariable(urlsubject);

    if (urlsubject.length >= 1 && urlsubject[0] !== ""){
    
        for (var i = 0; i < urlsubject.length; i++) {
            if (document.querySelectorAll("#portfolio-filter #" + urlsubject[i]).length === 1){
                (document.querySelector("#portfolio-filter #" + urlsubject[i]).classList.remove("active"));
            }
        }
        
        for (var i = 0; i < urlsubject.length; i++) {
            if (document.querySelectorAll("#portfolio-filter #" + urlsubject[i]).length === 1){
                (document.querySelector("#portfolio-filter #" + urlsubject[i]).classList.add("active"));
            }
        }
        
        var object = document.querySelector("#portfolio-filter .tags").children;
        for (var i = 0; i < object.length; i++) {
            // console.log(object[i].children[0].id)
        }
    }
}
window.onhashchange = function() {
    if (window.innerDocClick) {
        // Thanks: http://stackoverflow.com/questions/25806608/how-to-detect-browser-back-button-event-cross-browser
        window.innerDocClick = false;

        console.log("innerDocClick");

        buildPage()

    } else {
        if (window.location.hash != '#undefined') {

            console.log("eiwn");

            buildPage()
            

        } else {
            // Go back to (for expample) Google;
            history.pushState("", document.title, window.location.pathname);
            location.reload();
        }
    }
};

buildPage();