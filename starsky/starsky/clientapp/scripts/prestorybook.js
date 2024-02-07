import { spawnSync } from "child_process";
import { join } from "path";
import { fileURLToPath } from "url";
import path from "path";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const mockToolsFolder = join(__dirname, "..", "..", "..", "..", "starsky-tools", "mock");

console.log(`running npm ci in ${mockToolsFolder}`);

const updateSpawn = spawnSync("npm", ["ci", "--no-fund", "--no-audit"], {
  cwd: mockToolsFolder,
  env: process.env,
  encoding: "utf-8"
});

console.log("-result of npm ci");
console.log(updateSpawn.stdout);
console.log(updateSpawn.stout ? updateSpawn.stout : "");

process.env.NODE_OPTIONS = "--openssl-legacy-provider --max_old_space_size=8192";
