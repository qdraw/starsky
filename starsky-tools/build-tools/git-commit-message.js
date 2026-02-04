#!/usr/bin/node

const { spawnSync } = require("child_process");
const { join } = require("path");
const { exit } = require("process");
const { prefixPath } = require("./lib/prefix-path.const.js");

let searchPath = join(__dirname, prefixPath);

const argv = process.argv.slice(2);

if (argv) {
	for (const argItem of argv) {
		if (existsSync(argItem)) {
			searchPath = argItem;
			console.log(`use: path: ${argItem}`);
		}
	}
}

console.log(`searchPath: ${searchPath}`);

let gitCommitMessage = process.env.GIT_COMMIT_MESSAGE;
if (!gitCommitMessage) {
	console.log("GIT_COMMIT_MESSAGE is not defined");
	exit(1);
}

let gitUserEmail = process.env.GIT_USER_EMAIL;
if (!gitUserEmail) {
	console.log("GIT_USER_EMAIL is not defined");
	exit(1);
}

let gitUserName = process.env.GIT_USER_NAME;
if (!gitUserName) {
	console.log("GIT_USER_NAME is not defined");
	exit(1);
}

// -----------------------  check if git is there ----------------------------

const gitVersion = spawnSync("git", ["--version"], {
	cwd: searchPath,
	env: process.env,
	encoding: "utf-8",
});

if (gitVersion.stdout.indexOf("git version") === -1) {
	console.error("git not found");
	exit(1);
}

// -----------------------  file changes ----------------------------

const gitStatusPorcelain = spawnSync("git", ["status", "--porcelain"], {
	cwd: searchPath,
	env: process.env,
	encoding: "utf-8",
});

if (!gitStatusPorcelain.stdout) {
	console.log("no changes");
	exit(0);
}

// -----------------------  set git username /email ----------------------------

console.log("next set git user/email");

const getPreviousGitUserEmail = spawnSync(
	"git",
	["config", "--global", "user.email"],
	{
		cwd: searchPath,
		env: process.env,
		encoding: "utf-8",
	}
);

const getPreviousGitUserName = spawnSync(
	"git",
	["config", "--global", "user.name"],
	{
		cwd: searchPath,
		env: process.env,
		encoding: "utf-8",
	}
);

const setGitUserEmail = spawnSync(
	"git",
	["config", "--global", "user.email", gitUserEmail],
	{
		cwd: searchPath,
		env: process.env,
		encoding: "utf-8",
	}
);

const setGitUserName = spawnSync(
	"git",
	["config", "--global", "user.name", gitUserName],
	{
		cwd: searchPath,
		env: process.env,
		encoding: "utf-8",
	}
);

console.log(
	`setGitUserEmail: ${gitUserEmail} -  ${setGitUserEmail.stdout} \n ${setGitUserEmail.stderr}`
);
console.log(
	`setGitUserName: ${gitUserName} -  ${setGitUserName.stdout}\n ${setGitUserName.stderr}`
);

// -----------------------  add ----------------------------

console.log("next add");

const addMessage = spawnSync("git", ["add", "."], {
	cwd: searchPath,
	env: process.env,
	encoding: "utf-8",
});

console.log(`addMessage: ${addMessage.stdout}`);
console.log(`${addMessage.stderr}`);

// -----------------------  commit ---------------------------------
console.log("next commit");

const commitMessage = spawnSync("git", ["commit", "-m", gitCommitMessage], {
	cwd: searchPath,
	env: process.env,
	encoding: "utf-8",
});

console.log(`commitMessage ${commitMessage.stdout}`);
console.log(`${commitMessage.stderr}`);

console.log("next push");

const pushMessage = spawnSync("git", ["push"], {
	cwd: searchPath,
	env: process.env,
	encoding: "utf-8",
});

console.log(`pushMessage ${pushMessage.stdout}`);
console.log(`${pushMessage.stderr}`);

// ------------------------- set back -------------------------------

// and put it back
const setGitUserEmailBack = spawnSync(
	"git",
	["config", "--global", "user.email", getPreviousGitUserEmail.stdout],
	{
		cwd: searchPath,
		env: process.env,
		encoding: "utf-8",
	}
);

const setGitUserNameBack = spawnSync(
	"git",
	["config", "--global", "user.name", getPreviousGitUserName.stdout],
	{
		cwd: searchPath,
		env: process.env,
		encoding: "utf-8",
	}
);

console.log(`setGitUserEmailBack: ${setGitUserEmailBack.stdout}`);
console.log(`setGitUserNameBack ${setGitUserNameBack.stdout}`);

if (process.env.TF_BUILD) {
	console.log(`##vso[task.setvariable variable=GIT_FILES_CHANGED;]true`);
}
