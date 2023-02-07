#!/usr/bin/node

/**
 * Check if version prefix is supported
 */

const stdin = process.openStdin();

stdin.addListener("data", function(d) {
	// note:  d is an object, and when converted to a string it will
	// end with a linefeed.  so we (rather crudely) account for that  
	// with toString() and then trim() 
	const value = d.toString().trim();
	checkNewVersion(value);
	console.log("you entered: [" +
		d.toString().trim() + "]");
});


function checkNewVersion(newVersion) {
	const versionRegexChecker = new RegExp(
		"^([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?$",
		"g"
	);
	const versionRegexMatch = newVersion.match(versionRegexChecker);
	if (versionRegexMatch == null) {
		console.log(
			`✖ - Version  ${newVersion} is not supported`
		);
		return;
	}
	console.log(
		`v - Version  ${newVersion} is supported`
	);
}

