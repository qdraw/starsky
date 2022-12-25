#!/usr/bin/node

const fs = require("fs");
const path = require("path");
const { spawnSync } = require('child_process');

const sourceHtaccess = path.join(__dirname, "..", ".htaccess");
const toHtaccess = path.join(__dirname, "..", "build", ".htaccess");

console.log(`${sourceHtaccess} -> ${toHtaccess}`);
fs.copyFileSync(sourceHtaccess, toHtaccess);

const sourceFavicon = path.join(__dirname, "..", "static", "img", "favicon.ico");
const toFavicon = path.join(__dirname, "..", "build", "favicon.ico");

console.log(`${sourceFavicon} -> ${toFavicon}`);
fs.copyFileSync(sourceFavicon, toFavicon);

if (process.env.GOOGLE_VERIFICATION) {
  const googleVerficationPath = path.join(__dirname, "..", "build", process.env.GOOGLE_VERIFICATION + ".html");
  console.log("process.env.GOOGLE_VERIFICATION " + process.env.GOOGLE_VERIFICATION);
  fs.writeFileSync(googleVerficationPath, 'google-site-verification: '+ process.env.GOOGLE_VERIFICATION + ".html");
}

function copyDir(src, dest) {
	fs.mkdirSync(dest, { recursive: true });
	let entries = fs.readdirSync(src, { withFileTypes: true });

	for (let entry of entries) {
		let srcPath = path.join(src, entry.name);
		let destPath = path.join(dest, entry.name);

		entry.isDirectory() ?
			copyDir(srcPath, destPath) :
			fs.copyFileSync(srcPath, destPath);
	}
}

const sourceLegalFolder = path.join(__dirname, "..", "..", "starsky", "starsky", "wwwroot", "legal");
const toLegalFolder = path.join(__dirname, "..", "build", "legal");

console.log(`${sourceLegalFolder} -> ${toLegalFolder}`);
copyDir(sourceLegalFolder, toLegalFolder);


const gitHash = spawnSync("git", ["log", "-1", "--format=\"%H\""], {
    cwd: __dirname,
    env: process.env,
    encoding: "utf-8"
  });

if (!gitHash.stdout) {
    return;
}

const hash = gitHash.stdout.replace(/\"|\n/ig,"")

const versionTextPath = path.join(__dirname, "..", "build", "version.txt");

console.log(versionTextPath + " -> " + hash);

fs.writeFileSync(versionTextPath, hash)
