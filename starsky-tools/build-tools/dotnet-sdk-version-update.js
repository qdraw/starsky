/**
 * Update the project versions to have the same version
 */

const { join, basename, dirname } = require("path");
const { readFile, writeFile } = require("fs").promises;
const { readFileSync } = require("fs");
const { getFiles } = require("./lib/get-files-directory");
const { prefixPath } = require("./lib/prefix-path.const.js");
const { httpsGet } = require("./lib/https-get.js");

var newRunTimeVersion = "6.0.x";
const fallbackLibVersion = "netstandard2.0";

// https://docs.microsoft.com/en-us/dotnet/standard/frameworks

const aspNetCorePackages = [
	"Microsoft.AspNetCore.",
	"Microsoft.Extensions.",
	"Microsoft.EntityFrameworkCore",
	"Microsoft.Data.Sqlite.Core"
];

// allow version as single argument
const argv = process.argv.slice(2);
if (argv && argv.length === 1) {
	newRunTimeVersion = argv[0];
}

async function getLatestDotnetRelease() {
	const targetVersion = newRunTimeVersion.replace(".x", "");

	const data = await getByBlobMicrosoft(targetVersion, true);
	if (data) {
		return data[0];
	}

	const gData = await getByGithubReleases(targetVersion, true);
	if (gData) {
		return gData;
	}
}

console.log(`\nUpgrade version in csproj-files to ${newRunTimeVersion}\n`);

getLatestDotnetRelease().then((newTargetVersion) => {
	getFiles(join(__dirname, prefixPath)) // add "starsky" back when netframework is removed
		.then(async (filePathList) => {
			const sortedFilterPathList = sortFilterOnExeCSproj(filePathList);

			if (newTargetVersion) {
				await updateRuntimeFrameworkVersion(
					sortedFilterPathList,
					newTargetVersion
				);
				const frameworkMonikerByPath = await updateNugetPackageVersions(
					sortedFilterPathList,
					newRunTimeVersion
				);

				// const frameworkMonikerByPath = {
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.export/starsky.feature.export.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.geolookup/starsky.feature.geolookup.csproj': [ 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.health/starsky.feature.health.csproj': [ 'net6.0', 'netstandard2.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.import/starsky.feature.import.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.metaupdate/starsky.feature.metaupdate.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.rename/starsky.feature.rename.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.webftppublish/starsky.feature.webftppublish.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.feature.webhtmlpublish/starsky.feature.webhtmlpublish.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.accountmanagement/starsky.foundation.accountmanagement.csproj': [ 'net6.0', 'netstandard2.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.consoletelemetry/starsky.foundation.consoletelemetry.csproj': [ 'net6.0', 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.database/starsky.foundation.database.csproj': [ 'netstandard2.0', 'net6.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.databasetelemetry/starsky.foundation.databasetelemetry.csproj': [ 'net6.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.http/starsky.foundation.http.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.injection/starsky.foundation.injection.csproj': [ 'net6.0', 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.platform/starsky.foundation.platform.csproj': [ 'netstandard2.0', 'netstandard2.1', 'net6.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.readmeta/starsky.foundation.readmeta.csproj': [ 'netstandard2.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.realtime/starsky.foundation.realtime.csproj': [ 'net6.0', 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.storage/starsky.foundation.storage.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.sync/starsky.foundation.sync.csproj': [ 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.thumbnailgeneration/starsky.foundation.thumbnailgeneration.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.thumbnailmeta/starsky.foundation.thumbnailmeta.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.webtelemetry/starsky.foundation.webtelemetry.csproj': [ 'netcoreapp3.1', 'net6.0', 'netstandard2.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.worker/starsky.foundation.worker.csproj': [ 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starsky.foundation.writemeta/starsky.foundation.writemeta.csproj': [ 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starskycore/starskycore.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starsky/starsky.csproj': [ 'net6.0' ],
				// 	'/Users/dion/data/git/starsky/starsky/starskyadmincli/starskyadmincli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starskygeocli/starskygeocli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starskyimportercli/starskyimportercli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starskysynchronizecli/starskysynchronizecli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starskytest/starskytest.csproj': [ 'net6.0', 'netstandard2.0', 'netstandard2.1' ],
				// 	'/Users/dion/data/git/starsky/starsky/starskythumbnailcli/starskythumbnailcli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starskythumbnailmetacli/starskythumbnailmetacli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starskywebftpcli/starskywebftpcli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky/starskywebhtmlcli/starskywebhtmlcli.csproj': [],
				// 	'/Users/dion/data/git/starsky/starsky-tools/socket/ChannelWSClient.csproj': []
				//   }

				const sortedFrameworkMonikerByPath = await sortNetFrameworkMoniker(frameworkMonikerByPath);
				await updateNetFrameworkMoniker(sortedFrameworkMonikerByPath);

				console.log('---done');
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

async function getByBlobMicrosoft(targetVersion, isRuntime) {
	var what = "latest-sdk"
	if (isRuntime) what = "latest-runtime"

	const resultsDotnetCli = await httpsGet(
		"https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json"
	);

	if (resultsDotnetCli["releases-index"] !== undefined) {
		var versionObject = resultsDotnetCli["releases-index"].find(p => p["channel-version"] == targetVersion);
		if (versionObject && versionObject[what].startsWith(targetVersion)) {
			return [versionObject[what], versionObject];
		}
	}
	console.log(`\n[Blob] There are no versions matching ${targetVersion}`);
	return null;
}

async function getByGithubReleases(targetVersion, isRuntime) {
	let url = "https://api.github.com/repos/dotnet/sdk/releases"
	if (isRuntime) url = "https://api.github.com/repos/dotnet/core/releases";

	const results = await httpsGet(
		url
	);
	if (results.message && results.message.startsWith("API rate limit")) {
		console.log(results.message);
		return [];
	}
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
		console.log(`\n[Github] There are no versions matching ${targetVersion}`);
		return;
	}

	versions = versions.sort().reverse();
	return versions[0].replace(/^v/, "");
}

async function getSdkVersionByTarget() {
	const targetVersion = newRunTimeVersion.replace(".x", "");

	const blobObject = await getByBlobMicrosoft(targetVersion, false);
	if (blobObject) {
		await getBlobSdkReleaseNotesPage(blobObject);
		return blobObject[0];
	}

	const gData = await getByGithubReleases(targetVersion, false);
	if (gData) {
		return gData;
	}
}

async function getBlobSdkReleaseNotesPage(blobObject) {
	const releaseJsonFile = blobObject[1]["releases.json"];
	if (!releaseJsonFile) return;

	const resultsReleaseJsonFile = await httpsGet(
		releaseJsonFile
	);

	const findVersion = resultsReleaseJsonFile.releases.find(p => p.sdk.version == blobObject[0]);
	if (findVersion && findVersion["release-notes"]) {
		process.env["SDK_RELEASE_NOTES"] = findVersion["release-notes"];
		console.log(`::set-output name=SDK_RELEASE_NOTES::${findVersion["release-notes"]}`);
		return findVersion["release-notes"];
	}
	return null;
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

async function sortNetFrameworkMoniker(frameworkMonikerByPath) {

	for (let [filePath,netMonikers] of Object.entries(frameworkMonikerByPath)) {
		const referencedProjectPaths = await getReferencedProjectPaths(filePath);
		for (const refPath of referencedProjectPaths) {
			for (const netMoniker of netMonikers) {
				if (!frameworkMonikerByPath[refPath].includes(netMoniker)) {
					frameworkMonikerByPath[refPath].push(netMoniker)
				}
			}
		}
	}
	return frameworkMonikerByPath;
}


async function getReferencedProjectPaths(filePath) {
	let buffer = await readFile(filePath);
	let fileContent = buffer.toString("utf8");
	const currentDirName = dirname(filePath)

	const localProjectReferenceRegex = new RegExp(
		'<ProjectReference Include=".+" />',
		"ig"
	);
	const localProjectReferenceMatches = fileContent.matchAll(
		localProjectReferenceRegex
	);

	let localProjectPackagesPaths = [];

	for (const result of localProjectReferenceMatches) {

		let name = result[0]
			.replace('<ProjectReference Include="', "")
			.replace(/\" \/>$/, "")
			.replace(/\\/ig,"/");

		// console.log(join(currentDirName,name));

		const combinedPath = join(currentDirName,name);

		localProjectPackagesPaths.push(combinedPath);

	}
	return localProjectPackagesPaths;
}



async function updateNetFrameworkMoniker(sortedFrameworkMonikerByPath) {

	for (let [filePath,usedTargetFrameworkMonikers] of Object.entries(sortedFrameworkMonikerByPath)) {
		// reverse sort
		usedTargetFrameworkMonikers = usedTargetFrameworkMonikers.sort();

		if (usedTargetFrameworkMonikers.find(p => p.startsWith("net"))) {
			const lastNet = usedTargetFrameworkMonikers[0];

			var targetFrameworkRegex = new RegExp(
				"<TargetFramework>.+<\/TargetFramework>",
				"g"
			);

			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			fileContent = fileContent.replace(
				targetFrameworkRegex,
				`<TargetFramework>${lastNet}<\/TargetFramework>`
			);

			await writeFile(filePath, fileContent);
			console.log(
				`✓ ${filePath} - .NET is updated to ${lastNet}`
			);

			if (!usedTargetFrameworkMonikers[filePath]) {
				usedTargetFrameworkMonikers[filePath] = []
			}
			usedTargetFrameworkMonikers[filePath].push(lastNet)
		}	
	}
}

async function updateRuntimeFrameworkVersion(filePathList, newTargetVersion) {
	for (const filePath of filePathList) {
		if (
			filePath.match(
				new RegExp(
					"[a-z]+?.csproj$",
					"i"
				)
			)
		) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			// <TargetFramework>net
			var targetFrameworkNetStandard = new RegExp(
				"(<TargetFramework>net)",
				"g"
			);
			var targetFrameworkNetStandardMatch = fileContent.match(
				targetFrameworkNetStandard
			);

			// Should check if file contains TargetFramework
			if (targetFrameworkNetStandardMatch != null) {
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
	}
}

async function updateNugetPackageVersions(filePathList) {
	const frameworkMonikerByPath = {}
	for (const filePath of filePathList) {
		frameworkMonikerByPath[filePath] = await updateSingleNugetPackageVersion(filePath);
	}
	return frameworkMonikerByPath;
}

async function updateSingleNugetPackageVersion(filePath) {
	let usedTargetFrameworkMonikers = [];

	if (
		filePath.match(
			new RegExp(
				"[a-z].csproj$",
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

							// NetMoniker
							if (sortfindedVersions[0]["@id"]) {
								const versionSpecificData = await httpsGet(sortfindedVersions[0]["@id"]);
								const catalogEntryData = await httpsGet(versionSpecificData.catalogEntry);

								var netStandardlist = catalogEntryData.dependencyGroups.filter(p => p.targetFramework.toLowerCase().includes(".netstandard"))
								var netList = catalogEntryData.dependencyGroups.filter(p => p.targetFramework.toLowerCase().startsWith("net"));

								for (const item of [...netStandardlist,...netList]) {
									const parsedName = item.targetFramework.replace(/^\./ig,"").toLowerCase()

									if (!usedTargetFrameworkMonikers.includes(parsedName) ) {
										usedTargetFrameworkMonikers.push(parsedName)										
									}
								}
							} //e.

							var versionXMLRegex = new RegExp(
								'(Version=")([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(" )',
								"g"
							);

							updatedPackageReference = toUpdatePackageReference.replace(
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
		} // e. toupdate

	}
	return [...new Set(usedTargetFrameworkMonikers)];
}


function sortFilterOnExeCSproj(filePathList) {
	const exeFilePathList = [];
	const libsFilePathList = [];

	for (const filePath of filePathList) {

		if (!filePath.match(
			new RegExp(
				"[a-z]?.csproj$",
				"i"
			)
		)) {
			continue;	
		}
		

		let buffer = readFileSync(filePath);
		let fileContent = buffer.toString("utf8");

		// <OutputType>Exe<\/OutputType>|(Microsoft\.NET\.Sdk\.Web)|(Microsoft\.NET\.Test\.Sdk)
		var isExeRegex = new RegExp(
			'<OutputType>Exe<\/OutputType>|(Microsoft\.NET\.Sdk\.Web)|(Microsoft\.NET\.Test\.Sdk)',
			"ig"
		);

		var isExeMatches = fileContent.match(
			isExeRegex
		);

		if (!isExeMatches) {
			libsFilePathList.push(filePath);
			continue;
		}
		exeFilePathList.push(filePath);
		
	}
	return [...libsFilePathList,...exeFilePathList];
}