var prevURL = "";
var selectedFiles = [];

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
                        window.location.hash = window.location.hash.replace(GetSidebarWindowHash("anchor"),"");
                        
                    }


                    var url = "";
                    if (!this.classList.contains("on")) {
                        // This features add it;
                        console.log(this.className.indexOf("on"));

                        this.classList.add("on");

                        selectedFiles.push(this.dataset.filename);
                        console.log(selectedFiles);
                    }
                    else {
                        console.log(this.className);

                        this.classList.remove("on");
                        // ES5
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

                    console.log(url);
                    url = appendArrayToString(url,selectedFiles);
                    console.log(selectedFiles);
                    console.log(url);

                    // var   
                    if (url !== prevURL) {
                        var stateObj = { url: url };
                        history.pushState(stateObj, "Qdraw", url);
                    }
                    prevURL = url;


                    console.log("0")
                }
            }

        }, false);
    }
        
}

// /**
//  * @return {string}
//  */
// function GetSidebarWindowHash() {
//     // search and replace sidebar contents
//     var indexofsidebar = window.location.hash.indexOf("sidebar");
//     var hashcontent = window.location.hash.substr(indexofsidebar, window.location.hash.length);
//     var dotcomma = hashcontent.indexOf(";");
//     if (dotcomma === -1) dotcomma = window.location.hash.length;
//     hashcontent = window.location.hash.substr(indexofsidebar, dotcomma);
//     // end
//
//     hashcontent = replaceSideBarString(hashcontent);
//     return hashcontent;
// }
//
// function replaceSideBarString(hashcontent) {
//     hashcontent = hashcontent.replace("sidebar=","");
//     hashcontent = hashcontent.replace("#sidebar=","");
//     return hashcontent.replace("sidebar","");
// }

function appendArrayToString(url,selectedFiles) {
    for (var i = 0; i < selectedFiles.length; i++) {

        if (i === selectedFiles.length-1) {
            url += selectedFiles[i];
        }
        else {
            url += selectedFiles[i] + ",";
        }
    }
    return url;
}