#!/usr/bin/node

const fs = require("fs");
const path = require("path");

const docsDirectory = path.join(__dirname, "..", "docs");

function cleanFolder(input) {
  const inputPath = path.join(docsDirectory, input);

  fs.rmSync(inputPath, { recursive: true, force: true });
}

cleanFolder("advanced-options/starsky");
cleanFolder("advanced-options/starsky-tools");
cleanFolder("advanced-options/starskydesktop");
cleanFolder("advanced-options/history.md");
