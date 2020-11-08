
if (document.querySelectorAll("#autoclose").length === 1) {

    document.querySelector("#autoclose").focus();

    document.querySelector('#autoclose').addEventListener('click', function() {
        window.close();
    });

    setTimeout(()=>{
        window.close();
    }, 20000);
}

