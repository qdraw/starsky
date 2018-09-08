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
    xhr.setRequestHeader("Cache-Control", "max-age=0");
    xhr.send();
}