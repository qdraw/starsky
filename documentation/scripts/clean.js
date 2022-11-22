#!/usr/bin/node

const fs = require("fs");
const { spawnSync } = require("child_process");
const path = require("path");
const { env, exit } = require("process");

const docsDirectory = path.join(__dirname, "..", "docs");

function cleanFolder(input) {
  const inputPath = path.join(docsDirectory, input);

  fs.rmSync(inputPath, { recursive: true, force: true });
}

cleanFolder("starsky");
cleanFolder("starsky-tools");
cleanFolder("starskydesktop");
cleanFolder("history.md");
