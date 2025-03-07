#!/usr/bin/node

/**
 * Check if version prefix is supported
 */

const stdin = process.openStdin();

stdin.addListener("data", function (d) {
	// note:  d is an object, and when converted to a string it will
	// end with a linefeed.  so we (rather crudely) account for that  
	// with toString() and then trim() 
	const value = d.toString().trim();
	checkNewVersion(value);
	console.log("you entered: [" +
		value + "]");
});


function checkNewVersion(newVersion) {
	const versionRegexChecker = /^(\d+)\.(\d+)\.(\d+)(?:-([\dA-Za-z-]+(?:\.[\dA-Za-z-]+)*))?(?:\+[\dA-Za-z-]+)?$/g;
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

