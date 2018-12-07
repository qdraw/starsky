/**
 * @return {string}
 */
function GetSidebarWindowHash(name) {
    // search and replace sidebar contents
    var indexofsidebar = window.location.hash.indexOf(name);
    var hashcontent = window.location.hash.substr(indexofsidebar, window.location.hash.length);
    var dotcomma = hashcontent.indexOf(";");
    if (dotcomma === -1) dotcomma = window.location.hash.length;
    hashcontent = window.location.hash.substr(indexofsidebar, dotcomma);
    // end

    hashcontent = replaceSideBarString(hashcontent);
    return hashcontent;
}

function replaceSideBarString(hashcontent) {
    hashcontent = hashcontent.replace("sidebar=","");
    hashcontent = hashcontent.replace("#sidebar=","");
    return hashcontent.replace("sidebar","");
}



if (document.querySelectorAll(".sidebar .close").length === 1) {
    document.querySelector(".sidebar .close")
        .addEventListener("click",
            function () {
                toggleSideMenu()
            }, false);
}
if (document.querySelectorAll("#js-keywords-update").length === 1) {
    document.querySelector("#js-keywords-update a")
        .addEventListener("click",
            function () {
                updateKeywords()
            }, false);
    if (document.querySelectorAll("#js-keywords-update .form-control.js-focusout").length === 1) {
        document.querySelector("#js-keywords-update .form-control.js-focusout")
            .addEventListener("focusout", function () {
                updateKeywords()
            });
    }
}

if (document.querySelectorAll("#js-captionabstract-update").length === 1) {
    document.querySelector("#js-captionabstract-update a")
        .addEventListener("click",
            function () {
                updateCaptionAbstract()
            }, false);
    if (document.querySelectorAll("#js-captionabstract-update .form-control.js-focusout").length === 1) {
        document.querySelector("#js-captionabstract-update .form-control")
            .addEventListener("focusout", function () {
                updateCaptionAbstract()
            });
    }
}

if (document.querySelectorAll(".add-colorclass").length === 1) {
    var colorClasses = document.querySelector(".add-colorclass").children;

    for (var i = 0; i < colorClasses.length; i++) {
        colorClasses[i].addEventListener("click",
            function (e) {
                var target = e.target;
                if (e.target.className === "checkbox") target = e.target.parentElement;
                updateColorClass(target)
            }, false);
    }
}


if (document.querySelectorAll("#js-objectname-update").length === 1) {
    document.querySelector("#js-objectname-update a")
        .addEventListener("click",
            function () {
                updateObjectName()
            }, false);
    if (document.querySelectorAll("#js-objectname-update .form-control.js-focusout").length === 1) {
        document.querySelector("#js-objectname-update .form-control")
            .addEventListener("focusout", function () {
                updateObjectName()
            });
    }
}



if (document.querySelectorAll(".rotation-sidebar").length === 1) {
    document.querySelector(".rotation-sidebar a.js-left")
        .addEventListener("click",
            function () {
                queryRotate(-1)
            }, false);
    document.querySelector(".rotation-sidebar a.js-right")
        .addEventListener("click",
            function () {
                queryRotate(1)
            }, false);
}


if (document.querySelectorAll(".js-toggle-addorreplace a").length === 1) {
    document.querySelector(".js-toggle-addorreplace a")
        .addEventListener("click",
            function () {
                toggleOverwriteText()
            }, false);
}