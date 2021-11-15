/**
 * Update the project versions to have the same version
 */

const { join } = require("path");
const { stat, readFile, writeFile } = require("fs").promises;

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
	var results = await httpsGet(
		"https://api.github.com/repos/dotnet/core/releases"
	);
	if (results.message && results.message.startsWith("API rate limit")) {
		console.log(results.message);
		return [];
	}
	var targetVersion = newRunTimeVersion.replace(".x", "");
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

getFiles(join(__dirname, prefixPath, "starsky"))
	.then(async (filePathList) => {
		const newTargetVersion = await getLatestDotnetRelease();
		if (newTargetVersion) {
			await updateRuntimeFrameworkVersion(filePathList, newTargetVersion);
			await updateNugetPackageVersions(filePathList, newRunTimeVersion);
			// await is not working right here
		}
	})
	.catch((err) => {
		console.log(err);
	});

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
