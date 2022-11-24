#!/usr/bin/node

const fs = require("fs");
const path = require("path");

const sourceHtaccess = path.join(__dirname, "..", ".htaccess");
const toHtaccess = path.join(__dirname, "..", "build", ".htaccess");

console.log(sourceHtaccess);
console.log(toHtaccess);

fs.copyFileSync(sourceHtaccess, toHtaccess);