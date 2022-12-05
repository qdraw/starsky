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