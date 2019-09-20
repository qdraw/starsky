
document.querySelector(".head #hamburger").addEventListener("click", function(e){ hamburger(e); }, false);

var isHamburgerActive = true;

function hamburger(e) {

	if (e !== undefined) {
		e.preventDefault();
	}
	document.querySelector(".head #hamburger").innerHTML = "";

	if (isHamburgerActive === false) {
		document.querySelector(".head #menu ul").className = "moveto";
		document.querySelector(".head #hamburger").className = "close";
	}

	if (isHamburgerActive === true) {
		document.querySelector(".head #menu ul").className = "moveaway";
		document.querySelector(".head #hamburger").className = "hamburger";
	}

	isHamburgerActive = !isHamburgerActive;
	console.log("hamburger" + isHamburgerActive);

}

if (window.innerWidth <= 670) {
	hamburger();
}

window.addEventListener('resize', function() {

	// Close or open menu if change in resolution
	if (window.innerWidth >= 670) {
		isHamburgerActive = false;
		hamburger();
	}

	if (window.innerWidth <= 670) {
		isHamburgerActive = true;
		hamburger();
	}


}, true);
