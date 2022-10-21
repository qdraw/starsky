if (document.querySelectorAll("#autoclose").length === 1) {
  const autoCloseElement = document.querySelector("#autoclose");
  autoCloseElement.focus();

  autoCloseElement.addEventListener("click", () => {
    window.close();
  });

  setTimeout(() => {
    window.close();
  }, 20000);
}

if (document.querySelectorAll(".error").length === 1) {
  const errorElement = document.querySelector(".error");
  const error = new URLSearchParams(window.location.search).get("error");
  if (error) {
    errorElement.innerText = error;
  }
}
