const { readdir, readFile, writeFile } = require("fs").promises;
const { resolve, join } = require("path");

async function getFiles(dir) {
	const dirents = await readdir(dir, { withFileTypes: true });
	const files = await Promise.all(
		dirents.map((dirent) => {
			// path resolve NOT! promise resolve
			const res = resolve(dir, dirent.name);
			return dirent.isDirectory() &&
				dirent.name != "generic-netcore" &&
				dirent.name != "build" &&
				dirent.name != "node_modules" &&
				dirent.name != "obj" &&
				dirent.name != "bin" &&
				dirent.name != "linux-arm64" &&
				dirent.name != "win-x86" &&
				dirent.name != "win-x64" &&
				dirent.name != "osx-x64" &&
				dirent.name != "osx-arm64" &&
				dirent.name != "linux-arm64" &&
				dirent.name != "coverage" &&
				dirent.name != "coverage-report" &&
				dirent.name != "linux-arm" &&
				dirent.name != "dist"
				? getFiles(res)
				: res;
		})
	);
	return Array.prototype.concat(...files);
}

module.exports = {
	getFiles,
};
