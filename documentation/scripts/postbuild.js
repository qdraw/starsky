#!/usr/bin/node

const fs = require("fs");
const path = require("path");

const sourceHtaccess = path.join(__dirname, "..", ".htaccess");
const toHtaccess = path.join(__dirname, "..", "build", ".htaccess");

console.log(`${sourceHtaccess} -> ${toHtaccess}`);
fs.copyFileSync(sourceHtaccess, toHtaccess);

const sourceFavicon = path.join(__dirname, "..", "static", "img", "favicon.ico");
const toFavicon = path.join(__dirname, "..", "build", "favicon.ico");

console.log(`${sourceFavicon} -> ${toFavicon}`);
fs.copyFileSync(sourceFavicon, toFavicon);