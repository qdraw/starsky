
/**
 * Each csproj file needs to have a unique ProjectGuid
 */

const { resolve, join } = require('path');
const { readdir, readFile } = require('fs').promises;

var prefixPath = "../../";

async function getFiles(dir) {
  const dirents = await readdir(dir, { withFileTypes: true });
  const files = await Promise.all(dirents.map((dirent) => {
    const res = resolve(dir, dirent.name);
    return dirent.isDirectory() && dirent.name != "generic-netcore" && dirent.name != "build" &&
      dirent.name != "node_modules" && dirent.name != "obj" && dirent.name != "bin" &&
      dirent.name != "osx.10.12-x64" && dirent.name != "linux-arm64" && dirent.name != "win7-x86" &&
      dirent.name != "coverage" && dirent.name != "coverage-report" && dirent.name != "Cake" &&
      dirent.name != "linux-arm" && !dirent.name.startsWith(".") ? getFiles(res) : res;
  }));
  return Array.prototype.concat(...files);
}

var uniqueGuids = [];

getFiles(join(__dirname, prefixPath, "starsky")).then(async (filePathList) => {

  await filePathList.forEach(async filePath => {
    try {
      if (filePath.match(new RegExp("[a-z]((\.feature|\.foundation)|core)?(\.[a-z]+)?\.csproj$", "i"))) {
        let buffer = await readFile(filePath);
        let fileContent = buffer.toString('utf8');

        // For example <ProjectGuid>{23e26a58-29c5-4d0c-813b-9f7bd991b107}</ProjectGuid>
        var projectGUIDRegex = new RegExp("(<ProjectGuid>){(([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}))}(<\/ProjectGuid>)", "g");
        var fileXmlMatch = fileContent.match(projectGUIDRegex);
        if (fileXmlMatch == null) {
          throw new Error("✖ " + filePath + " - No ProjectGuid in file")
        }

        if (uniqueGuids.indexOf(fileXmlMatch[0]) >= 0) {
          throw new Error("✖ " + filePath + " - ProjectGuid is not Unique")
        }
        uniqueGuids.push(fileXmlMatch[0]);
        console.log(`✓ ${filePath} - Is Ok`);
      }
    } catch (e) {
      console.error(e);
    }
  });
}).catch((err) => {
  console.log(err);
});
