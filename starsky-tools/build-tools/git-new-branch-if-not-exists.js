#!/usr/bin/node

const { spawnSync } = require('child_process');
const { join } = require("path");
const { exit } = require('process');
const { prefixPath } = require("./lib/prefix-path.const.js");

let searchPath = join(__dirname, prefixPath);

const argv = process.argv.slice(2);

if (argv) {
	for (const argItem of argv) {
		if (existsSync(argItem)) {
			searchPath = argItem;
			console.log(`use: path: ${argItem}`)
		}
	}
}

console.log(`searchPath: ${searchPath}`);


const gitVersion = spawnSync('git', ['--version'], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

if (gitVersion.stdout.indexOf("git version") === -1) {
    console.error("git not found");
    exit(1);
}

const gitStatusPorcelain = spawnSync('git', ['status', '--porcelain'], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

if (!gitStatusPorcelain.stdout) {
    console.log("no changes");
    exit(0);
}


let branchName = process.env.BRANCH_NAME; 
if (!branchName) {
    console.log('BRANCH_NAME is not defined');
    exit(1);
}

spawnSync('git', ['pull'], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

// git ls-remote --heads origin feature/dependencies-update-6.0.404
const branchExists = spawnSync('git', ['ls-remote', '--heads', 'origin', `${branchName}`], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

if (!branchExists.stdout.trim()) {
    console.log(`branch: ${branchName} does not exist, so create`);
    const createBranch = spawnSync('git', ['checkout', '-b', branchName], {
        cwd: searchPath,
        env: process.env,
        encoding: 'utf-8'
    });
    console.log(`createBranch ${createBranch.stdout}`);

    console.log('next push branch');

    const pushBranch = spawnSync('git', ['push', '--set-upstream', 'origin', branchName], {
        cwd: searchPath,
        env: process.env,
        encoding: 'utf-8'
    });

    console.log(`pushBranch ${pushBranch.stdout}`);
    console.log(`${pushBranch.stderr}`);
}
else
{
    console.log(`branch: ${branchName} exist`);
    const switchBranch = spawnSync('git', ['checkout', branchName], {
        cwd: searchPath,
        env: process.env,
        encoding: 'utf-8'
    });
    console.log(`switchBranch ${switchBranch.stdout}`);
}




