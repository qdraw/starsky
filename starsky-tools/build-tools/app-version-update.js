#!/usr/bin/node

/**
 * Update the project versions to have the same version
 */

// other script: use release-version-check.js to check if the version is correct based on the branch name in the CI

const {join} = require("path");
const {readFile, writeFile} = require("fs").promises;
const {getFiles} = require("./lib/get-files-directory");
const {prefixPath} = require("./lib/prefix-path.const.js");

let newVersion = "0.9.0-beta.4";

// allow version as single argument
const argv = process.argv.slice(2);
if (argv && argv.length === 1) {
	newVersion = argv[0];
}

function checkNewVersion() {
	const versionRegexChecker = new RegExp(
		"^([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?$",
		"g"
	);
	const versionRegexMatch = newVersion.match(versionRegexChecker);
	if (versionRegexMatch == null) {
		console.log(
			`✖ - Version  ${newVersion} is not supported - please updated it and run it again.`
		);
		process.exit(1);
	}
}

checkNewVersion();

// Derives a monotonically-increasing integer from a semver string for use as
// CFBundleVersion (which must be numeric-only per Apple's requirements).
// Formula: major * 1_000_000 + minor * 10_000 + patch * 100 + preType * 30 + preNumber
// preType: alpha=0, beta=1, rc=2, stable=3 (preNumber must be < 30)
// Stable releases use preType=3, preNumber=9 → slot value 99, always above any pre-release.
// Example: 0.9.0-alpha.1 → 90001, 0.9.0-beta.1 → 90031, 0.9.0-rc.1 → 90061, 0.9.0 → 90099
const ALLOWED_PRE_RELEASE_TYPES = ["alpha", "beta", "rc"];

function computeBuildNumber(version) {
	const match = version.match(
		/^(\d+)\.(\d+)\.(\d+)(?:-([a-z]+)\.(\d+))?/
	);
	if (!match) return 1;
	const major = parseInt(match[1], 10);
	const minor = parseInt(match[2], 10);
	const patch = parseInt(match[3], 10);

	if (match[4] !== undefined) {
		if (!ALLOWED_PRE_RELEASE_TYPES.includes(match[4])) {
			console.error(
				`✖ Unsupported pre-release type "${match[4]}" in version "${version}". ` +
				`Allowed types: ${ALLOWED_PRE_RELEASE_TYPES.join(", ")}.`
			);
			process.exit(1);
		}
		const preNum = parseInt(match[5], 10);
		if (preNum >= 30) {
			console.error(
				`✖ Pre-release number ${preNum} in version "${version}" must be < 30.`
			);
			process.exit(1);
		}
	}

	const preTypeMap = {alpha: 0, beta: 1, rc: 2};
	const preType = match[4] !== undefined ? preTypeMap[match[4]] : 3;
	const preNum = match[5] !== undefined ? parseInt(match[5], 10) : 9;
	return major * 1_000_000 + minor * 10_000 + patch * 100 + preType * 30 + preNum;
}

console.log(
	`\nUpgrade version in csproj-files and package.json to ${newVersion}\n`
);

getFiles(join(__dirname, prefixPath, "starsky"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

getFiles(join(__dirname, prefixPath, "windows"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

getFiles(join(__dirname, prefixPath, "starsky-tools"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

getFiles(join(__dirname, prefixPath, "documentation"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

getFiles(join(__dirname, prefixPath, "mac"))
	.then(async (filePathList) => {
		await updateVersions(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

async function updateVersions(filePathList) {
	checkNewVersion();
	await filePathList.forEach(async (filePath) => {
		if (
			filePath.match(
				new RegExp(
					"[a-z]((.feature|.foundation)|core)?(.[a-z]+)?.csproj$",
					"i"
				)
			)
		) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			// unescaped: (<Version>)([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?(<\/Version>)
			const versionXMLRegex = new RegExp(
				"(<Version>)([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(</Version>)",
				"g"
			);
			const fileXmlMatch = fileContent.match(versionXMLRegex);
			if (fileXmlMatch == null) {
				console.log("✖ " + filePath + " - Version tag is not included");
			} else if (fileXmlMatch != null) {
				fileContent = fileContent.replace(
					versionXMLRegex,
					`<Version>${newVersion}</Version>`
				);
				await writeFile(filePath, fileContent);
				console.log(
					`✓ ${filePath} - Version is updated to ${newVersion}`
				);
			}
		} else if (filePath.match(new RegExp("Info\\.plist$", "i"))) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");
			// matches: <key>CFBundleShortVersionString</key>\n\t<string>X.Y.Z</string>
			const plistVersionRegex = new RegExp(
				"(<key>CFBundleShortVersionString</key>\\s*<string>)([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(</string>)",
				"g"
			);
			const filePlistMatch = fileContent.match(plistVersionRegex);
			if (filePlistMatch == null) {
				console.log(
					"✖ " + filePath + " - CFBundleShortVersionString tag is not included"
				);
			} else {
				fileContent = fileContent.replace(
					plistVersionRegex,
					`$1${newVersion}$6`
				);
				const buildNumber = computeBuildNumber(newVersion);
				const bundleVersionRegex = new RegExp(
					"(<key>CFBundleVersion</key>\\s*<string>)\\d+(</string>)",
					"g"
				);
				if (fileContent.match(bundleVersionRegex)) {
					fileContent = fileContent.replace(
						bundleVersionRegex,
						`$1${buildNumber}$2`
					);
					console.log(
						`✓ ${filePath} - CFBundleVersion is updated to ${buildNumber}`
					);
				}
				await writeFile(filePath, fileContent);
				console.log(
					`✓ ${filePath} - Version is updated to ${newVersion}`
				);
			}
		} else if (filePath.match(new RegExp("package.json?$", "i"))) {
			let buffer = await readFile(filePath);
			let fileJsonContent = buffer.toString("utf8");
			const versionJsonRegex = new RegExp(
				'"version": ?"([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(s?)"(s?)',
				"g"
			);
			const fileJsonMatch = fileJsonContent.match(versionJsonRegex);
			if (fileJsonMatch == null) {
				console.log(
					"✖ " + filePath + "  - Version tag is not included "
				);
			} else if (fileJsonMatch != null) {
				fileJsonContent = fileJsonContent.replace(
					versionJsonRegex,
					`"version": "${newVersion}"`
				);
				await writeFile(filePath, fileJsonContent);
				console.log(
					`✓ ${filePath} - Version is updated to ${newVersion}`
				);
			}
		}
	});
}
