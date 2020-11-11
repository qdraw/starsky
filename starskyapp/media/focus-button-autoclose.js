
if (document.querySelectorAll("#autoclose").length === 1) {

    document.querySelector("#autoclose").focus();

    document.querySelector('#autoclose').addEventListener('click', function() {
        window.close();
    });

    setTimeout(()=>{
        window.close();
    }, 20000);
}

if (document.querySelectorAll(".error").length === 1) {
    var error = new URLSearchParams(window.location.search).get("error");
    if (error) {
        document.querySelector(".error").innerText = error;
    }
}