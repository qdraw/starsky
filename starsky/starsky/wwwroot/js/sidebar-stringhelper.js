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