setTimeout(()=>{
    window.close();
}, 8000);

document.querySelector("button").focus();

document.querySelector('button').addEventListener('click', function() {
    window.close();
});
