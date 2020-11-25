const {getFiles} = require('./lib/get-files-directory');
const { prefixPath } = require('./lib/prefix-path.const.js');
const { join } = require('path');
const { readdir, readFile, writeFile } = require('fs').promises;

const starskySolutionFolder = join(__dirname, prefixPath, "starsky");

getFiles(starskySolutionFolder).then(async (filePathList) => {
  await checkCsProjFile(filePathList);
}).catch((err) => {
  console.log(err);
});

async function checkCsProjFile(filePathList) {
  var packageReferenceSet = new Set([]);

  for (var filePath of filePathList) {
    if (filePath.match(new RegExp("[a-z]((\.feature|\.foundation)|core)?(\.[a-z]+)?\.csproj$", "i"))) {
      let buffer = await readFile(filePath);
      let fileContent = buffer.toString('utf8');

      // unescaped: PackageReference Include="([^"]*)" Version="([^"]*)
      const promackageReferenceRegex = new RegExp("PackageReference Include=\"([^\"]*)\" Version=\"([^\"]*)", "g");
      var fileMatches = fileContent.match(promackageReferenceRegex);
      if (fileMatches != null) {
        for (var match of fileMatches) {
          packageReferenceSet.add(match.replace(/PackageReference Include=\"/ig,"").replace("\" Version=\""," "))
        }
      }
    }
  }

  console.log("Updated Nuget Packages List -> " + join(starskySolutionFolder,"nuget-packages-list.json"));
  await writeFile(join(starskySolutionFolder,"nuget-packages-list.json"),
    JSON.stringify(Array.from(packageReferenceSet),null, 2));
}
