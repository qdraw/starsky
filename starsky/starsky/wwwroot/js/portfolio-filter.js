


var tagList = [];
var prefFilter = "";
var currentFilter = "";

makeTagList();

tagList = removeArrayDuplicate(tagList);
// alphabetic order
tagList = tagList.sort();

writeFilterList (tagList);

filterPage();

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

    if (document.querySelectorAll("#portfolio-data").length === 1 && document.querySelectorAll("#portfolio-filter").length === 1){
        var ul = document.createElement("ul");
        ul.className = "tags";
        document.querySelector("#portfolio-filter").appendChild(ul);

        var filterarticle = document.querySelector("#portfolio-filter ul")

        // ALLES!!
        var li_alles = document.createElement("li");
        var a_alles = document.createElement("a");
        var currentitem_alles = filterarticle.appendChild(li_alles).appendChild(a_alles);
        currentitem_alles.innerHTML = "Alles";
        currentitem_alles.className = "alles";
        currentitem_alles.href = "#";
        // EINDE ALLES
        
        var newTags = [];
        if(tags.indexOf("Winner") >= 1) {
            newTags.push(tags[tags.indexOf("Winner")])
        }
        
        if(tags.indexOf("WinnerAlt") >= 1) {
            newTags.push(tags[tags.indexOf("WinnerAlt")])
        }

        if(tags.indexOf("Superior") >= 1) {
            newTags.push(tags[tags.indexOf("Superior")])
        }

        if(tags.indexOf("SuperiorAlt") >= 1) {
            newTags.push(tags[tags.indexOf("SuperiorAlt")])
        }

        if(tags.indexOf("Typical") >= 1) {
            newTags.push(tags[tags.indexOf("Typical")])
        }

        if(tags.indexOf("TypicalAlt") >= 1) {
            newTags.push(tags[tags.indexOf("TypicalAlt")])
        }

        if(tags.indexOf("Extras") >= 1) {
            newTags.push(tags[tags.indexOf("Extras")])
        }
        if(tags.indexOf("Trash") >= 1) {
            newTags.push(tags[tags.indexOf("Trash")])
        }
        
        if(tags.indexOf("None") >= 1) {
            newTags.push(tags[tags.indexOf("None")])
        }
        

        for (var i = 0; i < newTags.length; i++) {
            var li = document.createElement("li");
            var a = document.createElement("a");

            var currentitem = filterarticle.appendChild(li).appendChild(a);
            currentitem.href = "#" + newTags[i].toLowerCase().replace(/ /ig, "-");
            currentitem.className = newTags[i].toLowerCase().replace(/ /ig, "-");
            currentitem.innerHTML = newTags[i];
        }

        // for (var i = 0; i < tags.length; i++) {
        //     var li = document.createElement("li");
        //     var a = document.createElement("a");
        //
        //     var currentitem = filterarticle.appendChild(li).appendChild(a);
        //     currentitem.href = "#" + tags[i].toLowerCase().replace(/ /ig, "-");
        //     currentitem.className = tags[i].toLowerCase().replace(/ /ig, "-");
        //     currentitem.innerHTML = tags[i];
        // }
    }

}///e/writeFilterList


function createSlug(tagList) {
    var linkTagList = [];
    for (var i = 0; i < tagList.length; i++) {
        linkTagList.push(tagList[i].toLowerCase().replace(/ /ig, "-"));
    }
    return linkTagList;
}



window.onhashchange = function() {
    filterPage();
};



function filterPage() {
    console.log("filterPage");
    function showLegenda() {

        function reset() {
            var object = document.querySelector("#portfolio-filter ul").children;
            for (var i = 0; i < object.length; i++) {

                if (object[i].children.length >= 1) {
                    linkobject = object[i].children[0];
                    linkobject.className = linkobject.className.replace(/ off| on/ig,"") + " off"
                }
            }
        }
        if (window.location.hash != "") {
            reset();
            var legendaobject = document.querySelector("#portfolio-filter" + " ." +  window.location.hash.substring(1,window.location.hash.length));
            legendaobject.className  = legendaobject.className.replace(/ off| on/ig,"") + " on"
        }
        if (window.location.hash === "") {
            reset();
            document.querySelector("#portfolio-filter .alles").className = "alles on"
        }


    }
    var hashword = "";
    linkTagList = createSlug(tagList);

    if (window.location.hash !== "") {
        hashword = window.location.hash.substring(1,window.location.hash.length);
    }

    if (window.location.hash !== "" && linkTagList.indexOf(hashword) >= 0) {
        showLegenda();

        currentFilter = hashword;
        if (linkTagList.indexOf(hashword) >= 0) {
            hideElementsByTag(hashword);
        }
    }

    if (window.location.hash === "") {
        showLegenda();
        var object = document.querySelector("#portfolio-data").children;
        for (var i = 0; i < object.length; i++) {
            object[i].classList.remove("hide");
            object[i].classList.add("show");
            // object[i].className = "halfitem show";
        }

    }
}


function hideElementsByTag(hashword) {
    if (document.querySelectorAll("#portfolio-data").length === 1 && document.querySelectorAll("#portfolio-filter").length === 1){
        var object = document.querySelector("#portfolio-data").children;
        for (var i = 0; i < object.length; i++) {

            var tags = object[i].getAttribute('data-tag');
            var thisTags = [];

            if (tags !== null) {
                tags = 	tags.split(",");

                for (var j = 0; j < tags.length; j++) {
                    thisTags.push(tags[j])
                }
                thisTags = createSlug(thisTags);

            }
            if (thisTags.indexOf(hashword) >= 0) {
                // object[i].className = "halfitem show";
                object[i].classList.remove("hide");
                object[i].classList.add("show");

            }
            if (thisTags.indexOf(hashword) === -1) {
                // object[i].className = "halfitem hide";
                object[i].classList.remove("show");
                object[i].classList.add("hide");
            }

        }

    }
}
