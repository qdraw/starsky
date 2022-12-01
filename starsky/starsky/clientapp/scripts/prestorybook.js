const { spawnSync } = require("child_process");
const path = require("path");

const mockToolsFolder = path.join(
  __dirname,
  "..",
  "..",
  "..",
  "..",
  "starsky-tools",
  "mock"
);

console.log(`running npm ci in ${mockToolsFolder}`);

const updateSpawn = spawnSync("npm", ["ci", "--no-fund", "--no-audit"], {
  cwd: mockToolsFolder,
  env: process.env,
  encoding: "utf-8"
});

console.log("-result of npm ci");
console.log(updateSpawn.stdout);
console.log(updateSpawn.stout ? updateSpawn.stout : "");

process.env.NODE_OPTIONS =
  "--openssl-legacy-provider --max_old_space_size=8192";
