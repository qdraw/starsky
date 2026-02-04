const {getFiles} = require("./lib/get-files-directory");
const {prefixPath} = require("./lib/prefix-path.const.js");
const {join} = require("path");
const {readFile, writeFile} = require("fs").promises;

const starskySolutionFolder = join(__dirname, prefixPath, "starsky");

getFiles(starskySolutionFolder)
	.then(async (filePathList) => {
		await checkCsProjFile(filePathList);
	})
	.catch((err) => {
		console.log(err);
	});

async function checkCsProjFile(filePathList) {
	const packageReferenceSet = new Set([]);

	for (const filePath of filePathList) {
		if (
			filePath.match(
				/[a-z]((.feature|.foundation)|core)?(.[a-z]+)?.csproj$/i
			)
		) {
			let buffer = await readFile(filePath);
			let fileContent = buffer.toString("utf8");

			// unescaped: PackageReference Include="([^"]*)" Version="([^"]*)
			const promackageReferenceRegex = /PackageReference Include="([^"]*)" Version="([^"]*)/g;
			const fileMatches = fileContent.match(promackageReferenceRegex);
			if (fileMatches != null) {
				for (const match of fileMatches) {
					packageReferenceSet.add(
						match
							.replace(/PackageReference Include="/gi, "")
							.replace('" Version="', " ")
					);
				}
			}
		}
	}

	console.log(
		"Updated Nuget Packages List -> " +
		join(starskySolutionFolder, "nuget-packages-list.json")
	);

	// https://stackoverflow.com/a/70567308
	const result = JSON.stringify(Array.from(packageReferenceSet), null, 2).replace(/^(\s*).*\\n.*$/gm, (line, indent) =>
		line.replace(/\\n/g, "\n" + indent)
	);

	await writeFile(
		join(starskySolutionFolder, "nuget-packages-list.json"),
		result
	);
}

module.exports = {
	checkCsProjFile,
	starskySolutionFolder,
};
