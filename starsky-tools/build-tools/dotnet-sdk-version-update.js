/**
 * Update the project versions to have the same version
 */

const { join } = require("path");
const { readFile, writeFile } = require("fs").promises;

const { getFiles } = require("./lib/get-files-directory");
const { prefixPath } = require("./lib/prefix-path.const.js");
const { httpsGet } = require("./lib/https-get.js");

var newRunTimeVersion = "3.1.x";

const aspNetCorePackages = [
	"Microsoft.AspNetCore.",
	"Microsoft.Extensions.",
	"Microsoft.EntityFrameworkCore",
];

// allow version as single argument
const argv = process.argv.slice(2);
if (argv && argv.length === 1) {
	newRunTimeVersion = argv[0];
}

async function getLatestDotnetRelease() {
	const results = await httpsGet(
		"https://api.github.com/repos/dotnet/core/releases"
	);
	if (results.message && results.message.startsWith("API rate limit")) {
		console.log(results.message);
		return [];
	}
	const targetVersion = newRunTimeVersion.replace(".x", "");
	let versions = [];
	for (const item of results) {
		if (
			!item.prerelease &&
			!item.tag_name.includes("preview") &&
			!item.tag_name.includes("rc") &&
			item.tag_name.startsWith("v" + targetVersion)
		) {
			versions.push(item.tag_name);
		}
	}
	if (versions.length == 0) {
		console.log(`\nThere are no versions matching ${newRunTimeVersion}`);
		return;
	}

	versions = versions.sort().reverse();

	return versions[0].replace(/^v/, "");
}

console.log(`\nUpgrade version in csproj-files to ${newRunTimeVersion}\n`);

getLatestDotnetRelease().then((newTargetVersion) => {
	getFiles(join(__dirname, prefixPath)) // add "starsky" back when netframework is removed
		.then(async (filePathList) => {
			if (newTargetVersion) {
				await updateRuntimeFrameworkVersion(
					filePathList,
					newTargetVersion
				);
				await updateNugetPackageVersions(
					filePathList,
					newRunTimeVersion
				);
				// await is not working right here
			}
		})
		.catch((err) => {
			console.log(err);
		});

	getFiles(join(__dirname, prefixPath))
		.then(async (filePathList) => {
			const sdkVersion = await getSdkVersionByTarget();
			process.env["SDK_VERSION"] = sdkVersion;
			console.log(`::set-output name=SDK_VERSION::${sdkVersion}`);

			await updateAzureYmlFile(filePathList, sdkVersion);
			await updateGithubYmlFile(filePathList, sdkVersion);
		})
		.catch((err) => {
			console.log(err);
		});
});

async function getSdkVersionByTarget() {
	const results = await httpsGet(
		"https://api.github.com/repos/dotnet/sdk/releases"
	);
	if (results.message && results.message.startsWith("API rate limit")) {
		console.log(results.message);
		return [];
	}
	let versions = [];
	const targetVersion = newRunTimeVersion.replace(".x", "");

	for (const item of results) {
		if (
			!item.prerelease &&
			!item.tag_name.includes("preview") &&
			!item.tag_name.includes("rc") &&
			item.tag_name.startsWith("v" + targetVersion)
		) {
			versions.push(item.tag_name);
		}
	}

	if (versions.length == 0) {
		console.log(`\nThere are no versions matching ${targetVersion}`);
		return;
	}

	versions = versions.sort().reverse();
	return versions[0].replace(/^v/, "");
}

async function updateAzureYmlFile(filePathList, sdkVersion) {
	await filePathList.forEach(async (filePath) => {
		if (filePath.match(new RegExp("^.+.yml$", "i"))) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			var taskUseDotNetRegex = new RegExp("task: UseDotNet@2", "g");

			const taskUseDotNetMatch = taskUseDotNetRegex.exec(fileContent);

			if (taskUseDotNetMatch != null) {
				let startNewLineIndex = 0;
				for (const iterator of fileContent.matchAll("\n")) {
					if (taskUseDotNetMatch.index >= iterator.index) {
						startNewLineIndex = iterator.index;
					}
				}
				const numberOfSpacesBefore =
					taskUseDotNetMatch.index - startNewLineIndex + 1;

				var versionTag = " ".repeat(numberOfSpacesBefore) + "version: ";

				var versionRegex = new RegExp(versionTag + "[0-9.]+", "g");
				if (fileContent.match(versionRegex)) {
					fileContent = fileContent.replace(
						versionRegex,
						versionTag + sdkVersion
					);
				}

				var displayNameTag =
					" ".repeat(numberOfSpacesBefore - 2) + "displayName: ";
				var displayNameTagRegex = new RegExp(
					displayNameTag + ".+",
					"g"
				);

				if (fileContent.match(displayNameTagRegex)) {
					fileContent = fileContent.replace(
						displayNameTagRegex,
						displayNameTag +
							"'Use .NET Core sdk " +
							sdkVersion +
							"'"
					);
				}
				await writeFile(filePath, fileContent);
				console.log(
					`✓ ${filePath} - Azure Yml is updated to ${sdkVersion}`
				);
			}
		}
	});
}

async function updateGithubYmlFile(filePathList, sdkVersion) {
	await filePathList.forEach(async (filePath) => {
		if (filePath.match(new RegExp("^.+.github.+.yml$", "i"))) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			var actionsSetupDotNet = new RegExp(
				"uses: actions\\/setup-dotnet@v1\n\\s+with:\n\\s+dotnet-version: [0-9.]+",
				"g"
			);
			const actionsSetupDotNetMatch =
				fileContent.match(actionsSetupDotNet);
			if (actionsSetupDotNetMatch) {
				const actionsSetupDotNetReplaced =
					actionsSetupDotNetMatch[0].replace(/[0-9.]+$/, sdkVersion);
				fileContent = fileContent.replace(
					actionsSetupDotNetMatch[0],
					actionsSetupDotNetReplaced
				);

				await writeFile(filePath, fileContent);
				console.log(
					`✓ ${filePath} - Github Yml is updated to ${sdkVersion}`
				);
			}
		}
	});
}

async function updateRuntimeFrameworkVersion(filePathList, newTargetVersion) {
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

			// <TargetFramework>netstandard
			var targetFrameworkNetStandard = new RegExp(
				"(<TargetFramework>netstandard)",
				"g"
			);
			var targetFrameworkNetStandardMatch = fileContent.match(
				targetFrameworkNetStandard
			);

			// Should skip netstardard libs due the fact that those dont have RuntimeFrameworkVersion included

			if (targetFrameworkNetStandardMatch == null) {
				// unescaped: (<RuntimeFrameworkVersion>)([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?(<\/RuntimeFrameworkVersion>)
				var runtimeFrameworkVersionXMLRegex = new RegExp(
					"(<RuntimeFrameworkVersion>)([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(</RuntimeFrameworkVersion>)",
					"g"
				);
				var fileXmlMatch = fileContent.match(
					runtimeFrameworkVersionXMLRegex
				);
				if (fileXmlMatch == null) {
					console.log(
						"✖ " +
							filePath +
							" - RuntimeFrameworkVersion tag is not included"
					);
				} else if (fileXmlMatch != null) {
					fileContent = fileContent.replace(
						runtimeFrameworkVersionXMLRegex,
						`<RuntimeFrameworkVersion>${newTargetVersion}</RuntimeFrameworkVersion>`
					);
					await writeFile(filePath, fileContent);
					console.log(
						`✓ ${filePath} - RuntimeFrameworkVersion is updated to ${newTargetVersion}`
					);
				}
			}
		}
	});
}

async function updateNugetPackageVersions(filePathList) {
	filePathList.forEach(async (filePath) => {
		await updateSingleNugetPackageVersion(filePath);
	});
}

async function updateSingleNugetPackageVersion(filePath) {
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

		// unescaped: <PackageReference Include="[a-z.]+" Version="[0-9.]+" />
		var packageReferenceRegex = new RegExp(
			'<PackageReference Include="[a-z.]+" Version="[0-9.]+" />',
			"ig"
		);

		var packageReferenceMatches = fileContent.matchAll(
			packageReferenceRegex
		);

		let toUpdatePackages = [];

		for (const result of packageReferenceMatches) {
			var name = result[0]
				.replace('<PackageReference Include="', "")
				.replace(/" Version="[0-9.]+" \/>/, "");

			var version = result[0]
				.replace(/<PackageReference Include="[a-z.]+" Version="/gi, "")
				.replace('" />', "");

			for (const aspNetName of aspNetCorePackages) {
				if (name.startsWith(aspNetName)) {
					toUpdatePackages.push([name, version, result[0]]);
				}
			}
		}
		// e.

		if (toUpdatePackages.length >= 1) {
			var searchVersion = newRunTimeVersion.replace(".x", "");

			for (const item of toUpdatePackages) {
				const toUpdatePackageName = item[0];
				const toUpdatePackageVersion = item[1];
				const toUpdatePackageReference = item[2];

				const url = `https://azuresearch-usnc.nuget.org/query?take=1&q=${toUpdatePackageName}&prerelease=false`;
				const nugetResult = await httpsGet(url);
				if (
					nugetResult.totalHits >= 1 &&
					!!nugetResult.data &&
					!!nugetResult.data[0]
				) {
					const firstResult = { ...nugetResult.data[0] };
					if (firstResult.id === toUpdatePackageName) {
						var findedVersions = firstResult.versions.filter((x) =>
							x.version.startsWith(searchVersion)
						);

						if (findedVersions.length >= 1) {
							var sortfindedVersions = findedVersions
								.sort((x) => x.version)
								.reverse();

							const newVersion = sortfindedVersions[0].version;

							var versionXMLRegex = new RegExp(
								'(Version=")([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(" )',
								"g"
							);

							updatedPackageReference =
								toUpdatePackageReference.replace(
									versionXMLRegex,
									`Version="${newVersion}" `
								);

							fileContent = fileContent.replace(
								toUpdatePackageReference,
								updatedPackageReference
							);
							await writeFile(filePath, fileContent);

							console.log(
								`✓ ${filePath} - ${toUpdatePackageName} is updated to ${newVersion}`
							);
						} else {
							console.log(
								`✖ ${filePath} - ${toUpdatePackageName} is skipped to ${toUpdatePackageVersion}`
							);
						}
					}
				} else {
					console.log(
						"✖ " + filePath + " - Version tag is not included"
					);
				}
			}
		}
	}
}
