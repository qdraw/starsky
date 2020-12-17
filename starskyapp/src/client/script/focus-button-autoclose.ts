if (document.querySelectorAll("#autoclose").length === 1) {
  const autoCloseElement = document.querySelector("#autoclose") as HTMLElement;
  autoCloseElement.focus();

  autoCloseElement.addEventListener("click", function () {
    window.close();
  });

  setTimeout(() => {
    window.close();
  }, 20000);
}

if (document.querySelectorAll(".error").length === 1) {
  const errorElement = document.querySelector(".error") as HTMLElement;
  const error = new URLSearchParams(window.location.search).get("error");
  if (error) {
    errorElement.innerText = error;
  }
}
