


var tagList = [];
var prefFilter = "";
var currentFilter = "";

makeTagList();

tagList = removeArrayDuplicate(tagList);
// alphabetic order

// // not  => reverse
// tagList = tagList.reverse();

writeFilterList (tagList);


// Write down a list of colorclass catogories
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
                    if (tags[j] !== "colorclass--1"){
                        tagList.push(tags[j])
                    }
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
    tags.sort();

    if (document.querySelectorAll("#portfolio-data").length === 1 && 
        document.querySelectorAll("#portfolio-filter").length === 1)
    {
        // add reset
        document.querySelector("#portfolio-filter").innerHTML = "";
        
        var ul = document.createElement("ul");
        ul.className = "tags";
        document.querySelector("#portfolio-filter").appendChild(ul);

        var filterarticle = document.querySelector("#portfolio-filter ul");

        if(tags.length >= 2){
            // ALLES!! nu RESET
            var li_alles = document.createElement("li");
            var a_alles = document.createElement("a");
            var currentitem_alles = filterarticle.appendChild(li_alles).appendChild(a_alles);
            currentitem_alles.innerHTML = "Reset";
            currentitem_alles.className = "reset";
            currentitem_alles.addEventListener("click", function(e){
                
                // dependency on sidebar
                if (window.location.hash.indexOf("sidebar") >= 0) {
                    toggleSideMenu(true);
                }
                var those = this; 
                resetCheckBoxes(); 
                selectedVar = []; 
                setVariable([]); 
                constructURL();
                updateCollectionscount();

            }, false);
            // EINDE ALLES


            window.subject = tags;

            for (var i = 0; i < tags.length; i++) {
                var li = document.createElement("li");
                var a = document.createElement("a");

                var currentitem = filterarticle.appendChild(li).appendChild(a);
                currentitem.addEventListener("click", function(e){ var those = this; readVariable(those); }, false);
                currentitem.id = tags[i].toLowerCase().replace(/ /ig, "-");

               
                if (document.querySelectorAll("#portfolio-data ."+tags[i] ).length >= 0){
                    var tagnl = document.querySelector("#portfolio-data ."+tags[i] ).dataset.tagnl;

                    currentitem.innerHTML = "<span class='checkbox'></span>" + tagnl
                }
                
                //"<span class='checkbox'></span>"
            }//e/for
            
            //updateCollectionscount();
        }
    }

}///e/writeFilterList


// function createSlug(tagList) {
//     var linkTagList = [];
//     for (var i = 0; i < tagList.length; i++) {
//         linkTagList.push(tagList[i].toLowerCase().replace(/ /ig, "-"));
//     }
//     return linkTagList;
// }


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

    updateCollectionscount();

}

function updateCollectionscount() {
    var portfoliodata = document.querySelectorAll("#portfolio-data");
    if (portfoliodata.length === 1 &&
        document.querySelectorAll(".js-collectionscount").length === 1) {

        var counter = 0;
        for (var i = 0; i < portfoliodata[0].children.length; i++) {
            if(portfoliodata[0].children[i].className.indexOf("show") >= 0){
                counter++;
            }
        }
        document.querySelector(".js-collectionscount").innerHTML = counter;
    }
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
        
        // default add sidebar data
        if (window.location.hash.indexOf("sidebar") >= 0) {
            url += ";sidebar=" + GetSidebarWindowHash("sidebar");
        }
        
        // sidebar disabled
        if (selectedVar.length === 0) {
            url = "#";
        }
        
        // when the sidebar has items
        if (selectedVar.length === 0 && window.location.hash.indexOf("sidebar") >= 0) {
            url += "sidebar=" + GetSidebarWindowHash("sidebar");
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
    // updateBreadCrumb();

    if (document.querySelectorAll("#portfolio-data").length === 1 
        && document.querySelectorAll("#portfolio-filter").length === 1)
    {

        if(hashList[0] === "") {
            hashList = [];
        }
        
        var object = document.querySelector("#portfolio-data").children;
        // console.log(object);
        
        for (var i = 0; i < object.length; i++) {

            var tag = object[i].getAttribute('data-tag').toLowerCase();
            
            // tag = tag.replace("colorclass-","");
            // console.log(tag)
           
            if(hashList.length >= 0) {
                object[i].classList.remove("hide");
                object[i].classList.add("show");     
            }


            // Add to Url
            if (object[i].href.indexOf("&colorclass") === -1 && hashList.length >= 1) {
                object[i].href += "&colorclass=";
            }
            else if (object[i].href.indexOf("&colorclass") >= 0 && hashList.length <= 0) {
                //  if press reset
                var position = object[i].href.indexOf("&colorclass");
                object[i].href = object[i].href.substr(0, position);        
            }
            else if(hashList.length >= 1) {
                var position = object[i].href.indexOf("&colorclass");
                object[i].href = object[i].href.substr(0, position);
                object[i].href += "&colorclass=";
            }

            if (hashList.length >= 1) {
                for (var j = 0; j < hashList.length; j++) {
                    if (j !== hashList.length - 1) {
                        object[i].href += hashList[j].replace("colorclass-","") + ",";
                    }
                    else {
                        object[i].href += hashList[j].replace("colorclass-","");
                    }
                }
            }
            // end add to url

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

    if (document.querySelectorAll(".preloader").length === 1) {
        document.querySelector(".preloader").style.display = "none";
    }

    // read from url
    // console.log(window.location.hash);

    var urlsubject = [];

    var colorclassurl = window.location.hash;
    var anchorurl = null;
    if (colorclassurl.indexOf(";") >= 0) {
        var hashSplit = window.location.hash.split(";"); // ;
        for (var i = 0; i < hashSplit.length; i++) {
            if (hashSplit[i].indexOf("colorclass") >= 0) {
                colorclassurl = hashSplit[i];
                break;
            }
        }
        for (var i = 0; i < hashSplit.length; i++) {
            if (hashSplit[i].indexOf("anchor") >= 0) {
                anchorurl = hashSplit[i];
                break;
            }
        }
        // console.log(anchorurl)
        // colorclassurl = window.location.hash.split(";")[colorClassIndex];
    }
    // single file
    if (colorclassurl.indexOf(";") === -1) {
        if (colorclassurl.indexOf("anchor") >= 0) {
            anchorurl = colorclassurl;
        }
    }


    // console.log(colorclassurl)
    // console.log(anchorurl)

    if (anchorurl != null) {
        anchorurl = anchorurl.replace(/(#a|a)nchor=/ig, "");
        if (document.querySelectorAll('a[data-filename="' + anchorurl + '"]').length >= 1) {
            var positionx = document.querySelector('a[data-filename="' + anchorurl + '"]').offsetTop;
            console.log(positionx)
            window.scrollTo(0, positionx);
        }

    }

    if (colorclassurl.indexOf("#colorclass") === -1) return;

    colorclassurl = colorclassurl.replace("#colorclass=", "");

    if (colorclassurl.indexOf(",") >= 0) {
        urlsubject = colorclassurl.split(",");
    }
    else {
        if (colorclassurl.indexOf("colorclass") >= 0) {
            urlsubject.push(colorclassurl)
        }
    }

    selectedVar = urlsubject;


    var object = document.querySelector("#portfolio-filter .tags").children;

    console.log("urlsubject");

    console.log(urlsubject);

    for (var i = 0; i < object.length; i++) {
        object[i].children[0].classList.remove("active");
    }

    setVariable(urlsubject);

    if (urlsubject.length >= 1 && urlsubject[0] !== "") {

        for (var i = 0; i < urlsubject.length; i++) {
            if (document.querySelectorAll("#portfolio-filter #" + urlsubject[i]).length === 1) {
                (document.querySelector("#portfolio-filter #" + urlsubject[i]).classList.remove("active"));
            }
        }

        for (var i = 0; i < urlsubject.length; i++) {
            if (document.querySelectorAll("#portfolio-filter #" + urlsubject[i]).length === 1) {
                (document.querySelector("#portfolio-filter #" + urlsubject[i]).classList.add("active"));
            }
        }

        var object = document.querySelector("#portfolio-filter .tags").children;
        for (var i = 0; i < object.length; i++) {
            // console.log(object[i].children[0].id)
        }
    }
    // dependecy on sidebar
    updatePrevNextHash();
    updateCollectionscount();


}
window.onhashchange = function() {
    if (window.innerDocClick) {
        // Thanks: http://stackoverflow.com/questions/25806608/how-to-detect-browser-back-button-event-cross-browser
        window.innerDocClick = false;
        
        buildPage()

    } else {
        if (window.location.hash != '#undefined') {
            
            buildPage()
            

        } else {
            // Go back to (for example) Google;
            history.pushState("", document.title, window.location.pathname);
            location.reload();
        }
    }
};




buildPage();

// function updateBreadCrumb() {
//     // looking for anchor based urls
//     if (document.querySelectorAll(".breadcrumb").length >= 1) {
//         var breadcrumbObject = document.querySelector(".breadcrumb").children;
//         if (window.location.hash.indexOf("colorclass") >= 0) {
//             if (breadcrumbObject.length >= 2) {
//                 for (var i = 0; i < breadcrumbObject.length; i++) {
//                     if (i === breadcrumbObject.length - 1) {
//                         if (breadcrumbObject[i].href.indexOf("#colorclass") >= 0) {
//                             var index = breadcrumbObject[i].href.indexOf("#colorclass");
//                             breadcrumbObject[i].href = breadcrumbObject[i].href.substr(0,index);
//                         }
//                         if (breadcrumbObject[i].href.indexOf("colorclass") === -1) {
//                             breadcrumbObject[i].href += window.location.hash;
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }