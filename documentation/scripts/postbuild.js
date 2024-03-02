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
  fs.writeFileSync(googleVerficationPath, 'google-site-verification: ' + process.env.GOOGLE_VERIFICATION + ".html");
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

const hash = gitHash.stdout.replace(/\"|\n/ig, "")

const versionTextPath = path.join(__dirname, "..", "build", "version.txt");

console.log(versionTextPath + " -> " + hash);

fs.writeFileSync(versionTextPath, hash)

// robots txt generator
const documentationDirectory = path.join(__dirname, ".."); // /git/starsky/documentation - without docs

function readFile(rootPath, from) {
  const filename = path.join(rootPath, from);
  if (fs.existsSync(filename) === false) {
    return null;
  }
  return fs.readFileSync(filename, { encoding: "utf8" });
}

function replaceRobotsTxt() {
  let robots = readFile(documentationDirectory, path.join("static", "robots.template"));
  console.log(robots.length >= 1 ? "robots.template contains content" : "robots.template is empty");

  robots = robots.replace(/\{date\}/g, new Date().toLocaleDateString('en-UK', { year: 'numeric', month: 'long', day: 'numeric' }));

  if (process.env.DOCS_URL) {
    robots = robots.replace(/\{domain\}/g, process.env.DOCS_URL);
  }
  else {
    console.error("env: DOCS_URL is not set, so skipping robots.txt");
    return;
  }

  fs.mkdirSync(path.join(documentationDirectory, "build"), { recursive: true });
  const filename = path.join(documentationDirectory, "build", "robots.txt");
  console.log(filename);
  fs.writeFileSync(filename, robots);
  console.log(`${filename} generated`);
}

replaceRobotsTxt();


if (process.env.GTAG) {

  fs.mkdirSync(path.join(documentationDirectory, "build"), { recursive: true });
  const filePath = path.join(documentationDirectory, "build", "analytics.js")

  let analyticsTemplate = readFile(documentationDirectory, path.join("static", "analytics.template"));
  analyticsTemplate = analyticsTemplate.replace(/\G-999X9XX9XX/g, process.env.GTAG);
  console.log(`GTAG generated ${filePath}`);
  fs.writeFileSync(filePath, analyticsTemplate);
}
