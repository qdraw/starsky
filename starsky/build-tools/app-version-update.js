
const { resolve, join } = require('path');
const { readdir, readFile, writeFile } = require('fs').promises;

var newVersion = "0.2.4";

console.log(`\nUpgrade version in csproj-files and package.json to ${newVersion}\n`);

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

getFiles(join(__dirname, prefixPath, "starsky")).then(async (filePathList) => {
  await filePathList.forEach(async filePath => {
    if (filePath.match(new RegExp("starsky((\.feature|\.foundation)|core)?(\.[a-z]+)?\.csproj$", "i"))) {
      let buffer = await readFile(filePath);
      let fileContent = buffer.toString('utf8');

      // unescaped: (<Version>)([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?(<\/Version>)
      var versionXMLRegex = new RegExp("(<Version>)([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(<\/Version>)", "g");
      var fileXmlMatch = fileContent.match(versionXMLRegex);
      if (fileXmlMatch == null) {
        console.log("✖ " + filePath + " - Version tag is not included");
      }
      else if (fileXmlMatch != null) {
        fileContent = fileContent.replace(versionXMLRegex, `<Version>${newVersion}</Version>`)
        await writeFile(filePath, fileContent);
        console.log(`✓ ${filePath} - Version is updated to ${newVersion}`);
      }
    }
    else if (filePath.match(new RegExp("package.json(-lock)?$", "i"))) {
      let buffer = await readFile(filePath);
      let fileJsonContent = buffer.toString('utf8');
      var versionJsonRegex = new RegExp("\"version\": ?\"([0-9]+)\\.([0-9]+)\\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\\.[0-9A-Za-z-]+)*))?(?:\\+[0-9A-Za-z-]+)?(\s?)\"(\s?)", "g");
      var fileJsonMatch = fileJsonContent.match(versionJsonRegex);
      if (fileJsonMatch == null) {
        console.log("✖ " + filePath + "  - Version tag is not included ");
      }
      else if (fileJsonMatch != null) {
        fileJsonContent = fileJsonContent.replace(versionJsonRegex, `"version": "${newVersion}"`)
        await writeFile(filePath, fileJsonContent);
        console.log(`✓ ${filePath} - Version is updated to ${newVersion}`);
      }
    }
  });

}).catch((err) => {
  console.log(err);

});



// (?!<Version>)([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+)?(?=<\/Version>)
