#!/usr/bin/node

/**
 * Update the project versions to have the same version
 * Use parameter to match version and the path. it defaults to the solution
 */

const { join, dirname } = require("path");
const { readFile, writeFile } = require("fs").promises;
const { readFileSync, existsSync } = require("fs");
const { getFiles } = require("./lib/get-files-directory");
const { prefixPath } = require("./lib/prefix-path.const.js");
const { httpsGet } = require("./lib/https-get.js");

let newRunTimeVersion = "6.0.x";

// https://docs.microsoft.com/en-us/dotnet/standard/frameworks

const aspNetCorePackages = [
	"Microsoft.AspNetCore.",
	"Microsoft.Extensions.",
	"Microsoft.EntityFrameworkCore",
	"Microsoft.Data.Sqlite.Core"
];

// allow version as single argument
const argv = process.argv.slice(2);

let searchPath = join(__dirname, prefixPath);

console.log(`searchPath: ${searchPath}`);

// it checks if the parameter matches a semver string or a valid path on disk
if (argv) {
	// regex: ^(\d+\.)?(\d+\.)?(\*|x|\d+)$
	for (const argItem of argv) {
		if (argItem.match(new RegExp("^(\\d+\\.)?(\\d+\\.)?(\\*|x|\\d+)$", "i"))) {
			newRunTimeVersion = argItem;
		}
		else if (existsSync(argItem)) {
			searchPath = argItem;
			console.log(`use: path: ${argItem}`)
		}
	}
}

async function getLatestDotnetRelease() {
	const targetVersion = newRunTimeVersion.replace(".x", "");

	// runtime
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
	// // next upgrade csproj files
	getFiles(searchPath)
		.then(async (filePathList) => {
			const sortedFilterPathList = sortFilterOnExeCSproj(filePathList);

			if (newTargetVersion) { // newTargetVersion -> is that there is a result
				await updateRuntimeFrameworkVersion(
					sortedFilterPathList,
					newTargetVersion
				);

				const frameworkMonikerByPath = await updateNugetPackageVersions(
					sortedFilterPathList,
					newRunTimeVersion
				);

				const refFrameworkMonikerByPath = await addReferencedProjectMonikers(frameworkMonikerByPath);

				const sortedFrameworkMonikerByPath = await sortNetFrameworkMoniker(refFrameworkMonikerByPath);

				await updateNetFrameworkMoniker(sortedFrameworkMonikerByPath);

				console.log('---done');
			}
		})
		.catch((err) => {
			console.log(err);
		});

	getFiles(searchPath)
		.then(async (filePathList) => {
			const sdkVersion = await getSdkVersionByTarget();
			process.env["SDK_VERSION"] = sdkVersion;
			console.log(`SDK_VERSION: ${sdkVersion}`);
			// ::set-output is deprecated
			console.log(`::set-output name=SDK_VERSION::${sdkVersion}`);
			// process.env["GITHUB_STATE"] += ` SDK_VERSION=${sdkVersion} `;

            if (process.env.TF_BUILD) {
                console.log(`##vso[task.setvariable variable=SDK_VERSION;]${sdkVersion}`);
            }

			await updateAzureYmlFile(filePathList, sdkVersion);
			await updateGithubYmlFile(filePathList, sdkVersion);

			await updateMcrDockerFile(filePathList);
			await updateGlobalJsonFiles(filePathList, sdkVersion);
			await updateDockerEnvFile(filePathList);
		})
		.catch((err) => {
			console.log(err);
		});
});

async function updateDockerEnvFile(filePathList) {
	const targetVersion = newRunTimeVersion.replace(".x", "");

	const envDockerFilePathList = filePathList.filter(p => p.endsWith(".env"));

	// aspnet is runtime for asp.net apps
	const aspNetResults = await httpsGet("https://mcr.microsoft.com/v2/dotnet/aspnet/tags/list");
	const sdkResults = await httpsGet("https://mcr.microsoft.com/v2/dotnet/sdk/tags/list");

	const aspNetResult = aspNetResults.tags.find(p => p === targetVersion);
	const sdkResult = sdkResults.tags.find(p => p === targetVersion);

	if (newRunTimeVersion.includes(".x") && aspNetResult !== sdkResult) {
		console.log('DockerFile versions dont match');
		return;
	}

	for (const envDockerFilePath of envDockerFilePathList) {
		let buffer = await readFile(envDockerFilePath);
		console.log("✓ " + envDockerFilePath);
		let fileContent = buffer.toString("utf8");
		fileContent = replaceEnvDockerFileContent(fileContent, "sdk",sdkResult);
		fileContent = replaceEnvDockerFileContent(fileContent, "aspnet",aspNetResult);
		await writeFile(envDockerFilePath, fileContent);
	}

}

function replaceEnvDockerFileContent(fileContent, what, sdkResult) {
	// replace variables with sdk version
	const buildBaseImageRegex = new RegExp("((BUILD)|(BASE))_?(IMAGE)=mcr\\.microsoft\\.com\/dotnet\/"+ what +":(\d|\.)+", "ig");
	const buildBaseImageMatches = fileContent.match(buildBaseImageRegex);
	if (buildBaseImageMatches) {
		for (const buildBaseImageMatch of buildBaseImageMatches) {
			const replacedResult = buildBaseImageMatch.replace(/(\d|\.)+$/g,sdkResult); // sdkResult = to version
			console.log("  ✓  replaceEnvDockerFileContent - build/base " + replacedResult);
			fileContent = fileContent.replace(buildBaseImageMatch, replacedResult);
		}
	}
	return fileContent;
}


async function updateMcrDockerFile(filePathList) {
	const targetVersion = newRunTimeVersion.replace(".x", "");

	// aspnet is runtime for asp.net apps
	const aspNetResults = await httpsGet("https://mcr.microsoft.com/v2/dotnet/aspnet/tags/list");
	const sdkResults = await httpsGet("https://mcr.microsoft.com/v2/dotnet/sdk/tags/list");

	const aspNetResult = aspNetResults.tags.find(p => p === targetVersion);
	const sdkResult = sdkResults.tags.find(p => p === targetVersion);

	if (newRunTimeVersion.includes(".x") && aspNetResult !== sdkResult) {
		console.log('DockerFile versions dont match');
		return;
	}

	const dockerFilePathList = filePathList.filter(p => p.endsWith("Dockerfile"));
	for (const dockerFilePath of dockerFilePathList) {
		let buffer = await readFile(dockerFilePath);
		console.log("✓ " + dockerFilePath);
		let fileContent = buffer.toString("utf8");
		fileContent = replaceMcrFileContent(fileContent, "sdk",sdkResult);
		fileContent = replaceMcrFileContent(fileContent, "aspnet",aspNetResult);
		await writeFile(dockerFilePath, fileContent);
	}

}

function replaceMcrFileContent(fileContent, what, sdkResult) {
	// FROM (--platform=\$BUILDPLATFORM)?( )?mcr\.microsoft\.com\/dotnet\/sdk:(\d|\.)+ AS
	const sdkFromBuildPlatformRegex = new RegExp("FROM (--platform=\\$BUILDPLATFORM)?( )?mcr\\.microsoft\\.com\/dotnet\/"+ what +":(\d|\.)+ AS", "g");
	const sdkfromBuildPlatformMatches = fileContent.match(sdkFromBuildPlatformRegex);
	if (sdkfromBuildPlatformMatches) {
		for (const sdkfromBuildPlatformMatch of sdkfromBuildPlatformMatches) {
			const replacedResult = sdkfromBuildPlatformMatch.replace(/(\d|\.)+ AS$/g,sdkResult + " AS"); // sdkResult = to version
			console.log("  ✓  replaceMcrFileContent " + replacedResult);
			fileContent = fileContent.replace(sdkfromBuildPlatformMatch, replacedResult);
		}
	}

	// replace variables with sdk version
	const buildBaseImageRegex = new RegExp("ARG ((BUILD)|(BASE))_(IMAGE)=mcr\\.microsoft\\.com\/dotnet\/"+ what +":(\d|\.)+", "g");
	const buildBaseImageMatches = fileContent.match(buildBaseImageRegex);
	if (buildBaseImageMatches) {
		for (const buildBaseImageMatch of buildBaseImageMatches) {
			const replacedResult = buildBaseImageMatch.replace(/(\d|\.)+$/g,sdkResult); // sdkResult = to version
			console.log("  ✓  replaceMcrFileContent - build/base " + replacedResult);
			fileContent = fileContent.replace(buildBaseImageMatch, replacedResult);
		}
	}

	return fileContent;
}

async function getByBlobMicrosoft(targetVersion, isRuntime) {
	var what = "latest-sdk"
	if (isRuntime) what = "latest-runtime"

	const resultsDotnetCli = await httpsGet(
		"https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json"
	);

	if (resultsDotnetCli["releases-index"] !== undefined) {
		const versionObject = resultsDotnetCli["releases-index"].find(p => p["channel-version"] === targetVersion);
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

	if (versions.length === 0) {
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

	const findVersion = resultsReleaseJsonFile.releases.find(p => p.sdk.version === blobObject[0]);
	if (findVersion && findVersion["release-notes"]) {
		process.env["SDK_RELEASE_NOTES"] = findVersion["release-notes"];
		console.log(`::set-output name=SDK_RELEASE_NOTES::${findVersion["release-notes"]}`);
        if (process.env.TF_BUILD) {
            console.log(`##vso[task.setvariable variable=SDK_RELEASE_NOTES;]${findVersion["release-notes"]}`);
        }
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

			// Update Docker Variables in Azure Yml
			// replace the following variables:
			// 	variables:
			//   buildImage: "mcr.microsoft.com/dotnet/sdk:6.0"
			//   baseImage: "mcr.microsoft.com/dotnet/aspnet:6.0"

			var buildBaseImageVariableRegex = new RegExp("(build|base)Image: ?\"mcr.microsoft.com", "ig");
			const buildBaseImageVariableMatch = buildBaseImageVariableRegex.exec(fileContent);
			if (buildBaseImageVariableMatch != null) {
				console.log(
					`✓ ${filePath} - Azure Yml (build/base) is updated to ${newRunTimeVersion.replace(".x", "")}`
				);
				fileContent = await replaceAzureDockerYmlFile(fileContent, "sdk");
				fileContent = await replaceAzureDockerYmlFile(fileContent, "aspnet");
				await writeFile(filePath, fileContent);
			}

		}
	});
}

async function replaceAzureDockerYmlFile(fileContent, what) {

	const targetVersion = newRunTimeVersion.replace(".x", "");

	// aspnet is runtime for asp.net apps
	const aspNetResults = await httpsGet("https://mcr.microsoft.com/v2/dotnet/aspnet/tags/list");
	const sdkResults = await httpsGet("https://mcr.microsoft.com/v2/dotnet/sdk/tags/list");

	const aspNetResult = aspNetResults.tags.find(p => p === targetVersion);
	const sdkResult = sdkResults.tags.find(p => p === targetVersion);

	if (newRunTimeVersion.includes(".x") && aspNetResult !== sdkResult) {
		console.log('DockerFile versions dont match');
		return;
	}

	const buildBaseImageRegex = new RegExp("((BUILD)|(BASE))_?(IMAGE): \"mcr\\.microsoft\\.com\/dotnet\/"+ what +":(\d|\.)+", "ig");
	const buildBaseImageMatches = fileContent.match(buildBaseImageRegex);
	if (buildBaseImageMatches) {
		for (const buildBaseImageMatch of buildBaseImageMatches) {
			const replacedResult = buildBaseImageMatch.replace(/(\d|\.)+\"$/g,targetVersion +"\""); // sdkResult = to version
			console.log("  ✓  Azure Yml - build/base " + replacedResult);
			fileContent = fileContent.replace(buildBaseImageMatch, replacedResult);
		}
	}
	return fileContent;
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

async function addReferencedProjectMonikers(frameworkMonikerByPath){

	// make sure referenced projects netmonikers are included
	for (let [filePath,_] of Object.entries(frameworkMonikerByPath)) {
		const referencedFiles = await getProjectReferences(filePath);
		for (const refPath of referencedFiles) {
			if (!frameworkMonikerByPath[refPath]) {
				continue;
			}
			frameworkMonikerByPath[filePath] = [...new Set([...frameworkMonikerByPath[filePath], ...frameworkMonikerByPath[refPath]])]
		}
	}

	return frameworkMonikerByPath;
}

async function sortNetFrameworkMoniker(frameworkMonikerByPath) {

    // references
	for (let [filePath,netMonikers] of Object.entries(frameworkMonikerByPath)) {
		const referencedProjectPaths = await getReferencedProjectPaths(filePath);
        // console.log(filePath + ' referencedProjectPaths')
        // console.log(referencedProjectPaths)

		for (const refPath of referencedProjectPaths) {
			for (const netMoniker of netMonikers) {
				if (!frameworkMonikerByPath[refPath]) {
					console.log('[x] Missing ' + refPath);
					frameworkMonikerByPath[refPath] = [];
				} 
				if (!frameworkMonikerByPath[refPath].includes(netMoniker)) {
					frameworkMonikerByPath[refPath].push(netMoniker)
				}
			}
		}
	}


	// console.log('frameworkMonikerByPath[filePath]');
	// console.log(frameworkMonikerByPath);

    // Re-order versions
    const updatedFrameworkMonikerByPath = [];

    for (let [filePath,netMonikers] of Object.entries(frameworkMonikerByPath)) {
        const newStyleDotNetRegex = new RegExp('^net\\d', 'i');

        const newStylenetMonikers  = netMonikers.filter((x)=> newStyleDotNetRegex.test(x)).sort((a,b)=>b.localeCompare(a));
        const oldStylenetMonikers  = netMonikers.filter((x)=> !newStyleDotNetRegex.test(x)).sort((a,b)=>b.localeCompare(a));

        updatedFrameworkMonikerByPath[filePath] = [...newStylenetMonikers,...oldStylenetMonikers]
	}

	return updatedFrameworkMonikerByPath;
}


async function getReferencedProjectPaths(filePath) {
	const localProjectPackagesPaths = await getProjectReferences(filePath);

    let updatedLocalProjectPackagesPaths = [];

	// add the references of that packages also to the list
    for (const singlePath of localProjectPackagesPaths) {
        const localProjectPackagesPathsRecurisive = await getProjectReferences(singlePath);

        for (const item of localProjectPackagesPathsRecurisive) {
            updatedLocalProjectPackagesPaths.push(item);
        }
    }
	return updatedLocalProjectPackagesPaths;
}

async function getProjectReferences(filePath) {
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
			.replace(/" \/>$/, "")
			.replace(/\\/ig,"/");

		// console.log(join(currentDirName,name));

		const combinedPath = join(currentDirName,name);

		localProjectPackagesPaths.push(combinedPath);
	}

    return localProjectPackagesPaths;
}

async function updateNetFrameworkMoniker(sortedFrameworkMonikerByPath) {

	for (let [filePath,usedTargetFrameworkMonikers] of Object.entries(sortedFrameworkMonikerByPath)) {

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

async function getNetMonikerByCsProjFilePath(filePath) {
	var targetFrameworkRegex = new RegExp(
		"<TargetFramework>.+<\/TargetFramework>",
		"g"
	);

	let buffer = await readFile(filePath);
	let fileContent = buffer.toString("utf8");

	const matchResults = fileContent.match(targetFrameworkRegex);
	if (matchResults.length !== 1) {
		return ""
	}
	return matchResults[0].replace("<TargetFramework>","").replace("</TargetFramework>","");
}

async function updateRuntimeFrameworkVersion(filePathList, newTargetVersion) {
	for (const filePath of filePathList) {
		if (
			filePath.endsWith(".csproj")
		) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			// <TargetFramework>net
			const targetFrameworkNetStandard = new RegExp(
				"(<TargetFramework>net)",
				"g"
			);
			const targetFrameworkNetStandardMatch = fileContent.match(
				targetFrameworkNetStandard
			);

			// Should check if file contains TargetFramework
			if (targetFrameworkNetStandardMatch != null) {
				// unescaped: (<RuntimeFrameworkVersion>)([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?(<\/RuntimeFrameworkVersion>)
				const runtimeFrameworkVersionXMLRegex = new RegExp(
					"(<RuntimeFrameworkVersion>)([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(</RuntimeFrameworkVersion>)",
					"g"
				);
				const fileXmlMatch = fileContent.match(
					runtimeFrameworkVersionXMLRegex
				);
				if (fileXmlMatch == null) {
					console.log(
						"✖ " +
							filePath +
							" - RuntimeFrameworkVersion tag is not included"
					);
				} else  { // fileXmlMatch != null
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
	// '///starsky/starsky.foundation.consoletelemetry/starsky.foundation.consoletelemetry.csproj': [ 'net6.0', 'netstandard2.0', 'netstandard2.1' ],

	if (
		filePath.endsWith(".csproj")
	) {
		let buffer = await readFile(filePath);
		let fileContent = buffer.toString("utf8");

		// unescaped: <PackageReference Include="[a-z.]+" Version="[0-9.]+" ?/?>
		const packageReferenceRegex = new RegExp(
			'<PackageReference Include="[a-z.]+" Version="[0-9.]+" ?/?>',
			"ig"
		);

		const packageReferenceMatches = fileContent.matchAll(
			packageReferenceRegex
		);

		let toUpdatePackages = [];
		for (const result of packageReferenceMatches) {
			const name = result[0]
				.replace('<PackageReference Include="', "")
				.replace(/" Version="[0-9.]+" ?\/?>/, "");

			const version = result[0]
				.replace(/<PackageReference Include="[a-z.]+" Version="/gi, "")
				.replace(/" ?\/?>/, "");

			for (const aspNetName of aspNetCorePackages) {
				if (name.startsWith(aspNetName)) {
					toUpdatePackages.push([name, version, result[0]]);
				}
			}
		}
		// e.

		if (toUpdatePackages.length >= 1) {
			const searchVersion = newRunTimeVersion.replace(".x", "");

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

								const netStandardlist = catalogEntryData.dependencyGroups.filter(p => p.targetFramework.toLowerCase().includes(".netstandard"))
								const netList = catalogEntryData.dependencyGroups.filter(p => p.targetFramework.toLowerCase().startsWith("net"));

								for (const item of [...netStandardlist,...netList]) {
									const parsedName = item.targetFramework.replace(/^\./ig,"").toLowerCase()

									if (!usedTargetFrameworkMonikers.includes(parsedName) ) {
										usedTargetFrameworkMonikers.push(parsedName)										
									}
								}
							} //e.

							const versionXMLRegex = new RegExp(
								'(Version=")([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(" )',
								"g"
							);

							const updatedPackageReference = toUpdatePackageReference.replace(
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

		if (!filePath.endsWith(".csproj")) {
			continue;
		}
		

		let buffer = readFileSync(filePath);
		let fileContent = buffer.toString("utf8");

		// <OutputType>Exe<\/OutputType>|(Microsoft\.NET\.Sdk\.Web)|(Microsoft\.NET\.Test\.Sdk)
		const isExeRegex = new RegExp(
			'<OutputType>Exe<\/OutputType>|(Microsoft\.NET\.Sdk\.Web)|(Microsoft\.NET\.Test\.Sdk)',
			"ig"
		);

		const isExeMatches = fileContent.match(
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

async function updateGlobalJsonFiles(filePathList, sdkVersionInput) {
	const sdkVersion = sdkVersionInput.replace(".x", "");

	const globalJsonPathList = filePathList.filter(p => p.endsWith("global.json"));
	for (const filePath of globalJsonPathList) {
		let globalJsonFile = {};
		try {
			globalJsonFile = JSON.parse(readFileSync(filePath).toString("utf8"));
		} catch (error) {
			console.log("✖ " + filePath + " - " + error);
		}

    // the global json needs to have a strictVersion property to be auto-upgraded
    if (globalJsonFile?.strictVersion !== true) {
      console.log("✖ " + filePath + " - strictVersion is not enabled so skip upgrade globalJson file");
    }
		
		if (globalJsonFile?.strictVersion === true && globalJsonFile?.sdk?.version !== sdkVersion) {
			globalJsonFile.sdk.version = sdkVersion;
			await writeFile(filePath, JSON.stringify(globalJsonFile, null, 4));
		}
	}
}
