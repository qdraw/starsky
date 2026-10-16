#!/usr/bin/node

/**
 * Compute SonarQube-style coverage restricted to the lines changed on this branch.
 *
 * Replicates the "coverage on new code" figure: (covered lines + covered branches) /
 * (total lines + total branches) over only the added/changed lines of a file.
 *
 * Input is the merged .NET + clientapp report that the build writes to
 * starsky/starskytest/coverage-merge-sonarqube.xml
 * (see starsky/build/Build.cs -> MergeCoverageFiles).
 *
 * Usage:
 *   node coverage-new-code.js                     all changed source files
 *   node coverage-new-code.js <path> [<path>..]   only these files
 *   node coverage-new-code.js --summary           skip the per-line detail
 *   node coverage-new-code.js --base=develop      diff against another base branch
 */

const {execFileSync} = require("child_process");
const {existsSync, readFileSync} = require("fs");
const {join, resolve} = require("path");

// this file lives in <repo>/starsky-tools/build-tools
const repoRoot = resolve(__dirname, "..", "..");
const coverageXmlPath = join(repoRoot, "starsky", "starskytest", "coverage-merge-sonarqube.xml");

const defaultBase = "master";

// only these kinds of file end up in the coverage report
const sourceFileRegex = /\.(cs|ts|tsx|js|jsx)$/;

// neither the dotnet test project nor jest test/story files are measured, so
// listing them would only add noise to the default run
const testFileRegex = /(^|\/)(starskytest|__tests__|__mocks__)\/|\.(test|spec|stories)\.[jt]sx?$/;

const xmlEntities = {amp: "&", lt: "<", gt: ">", quot: '"', apos: "'"};

function git(args) {
	return execFileSync("git", args, {cwd: repoRoot, encoding: "utf8"});
}

/**
 * New-side line numbers that the branch added or rewrote.
 */
function changedLines(relativePath, base) {
	const diff = git(["diff", `${base}...HEAD`, "--unified=0", "--", relativePath]);
	const lines = new Set();
	for (const raw of diff.split("\n")) {
		if (!raw.startsWith("@@")) {
			continue;
		}
		// @@ -12,4 +56,3 @@ optional context
		const newPart = raw.split("+")[1].split("@@")[0].trim();
		const [start, count] = newPart.includes(",")
			? newPart.split(",").map(Number)
			: [Number(newPart), 1];

		// a count of 0 is a pure deletion hunk, it has no new side lines
		for (let line = start; line < start + count; line++) {
			lines.add(line);
		}
	}
	return lines;
}

function decodeXml(value) {
	return value.replace(/&(#[xX]?[0-9a-fA-F]+|[a-zA-Z]+);/g, (entity, body) => {
		if (body.startsWith("#x") || body.startsWith("#X")) {
			return String.fromCodePoint(parseInt(body.slice(2), 16));
		}
		if (body.startsWith("#")) {
			return String.fromCodePoint(parseInt(body.slice(1), 10));
		}
		return xmlEntities[body] ?? entity;
	});
}

function attribute(attributes, name) {
	const match = attributes.match(new RegExp(`${name}="([^"]*)"`));
	return match ? match[1] : null;
}

/**
 * lineNumber -> {covered, coveredBranches, branchesToCover} for one repo relative file.
 *
 * The report stores absolute paths, so files are matched on their repo relative
 * suffix instead; that keeps the tool working on CI runners and other checkouts.
 */
function coverageForFile(xml, relativePath) {
	const suffix = "/" + relativePath.replaceAll("\\", "/");
	const lines = new Map();
	const matchedFiles = new Set();

	const fileRegex = /<file path="([^"]*)"[^>]*>([\s\S]*?)<\/file>/g;
	let file;
	while ((file = fileRegex.exec(xml)) !== null) {
		const filePath = decodeXml(file[1]);
		if (!filePath.endsWith(suffix)) {
			continue;
		}
		matchedFiles.add(filePath);
		for (const line of file[2].matchAll(/<lineToCover\b([^>]*?)\/?>/g)) {
			lines.set(Number(attribute(line[1], "lineNumber")), {
				covered: attribute(line[1], "covered") === "true",
				coveredBranches: Number(attribute(line[1], "coveredBranches") ?? 0),
				branchesToCover: Number(attribute(line[1], "branchesToCover") ?? 0)
			});
		}
	}

	// a bare file name such as "Program.cs" is a suffix of many files, so say so
	// instead of quietly reporting numbers for a file the caller did not mean
	if (matchedFiles.size > 1) {
		const shorterPaths = [...matchedFiles].map((path) => path.replace(`${repoRoot}/`, ""));
		console.error(
			`! ${relativePath} matches ${matchedFiles.size} files in the coverage report, ` +
			`use a longer path: ${shorterPaths.join(", ")}`
		);
	}
	return lines;
}

function reportFile(xml, relativePath, base, summary) {
	const coverage = coverageForFile(xml, relativePath);
	const changed = changedLines(relativePath, base);

	const newLines = [...coverage.entries()]
		.filter(([lineNumber]) => changed.has(lineNumber))
		.sort((first, second) => first[0] - second[0]);

	const lineCount = newLines.length;
	const coveredLineCount = newLines.filter(([, line]) => line.covered).length;
	const branchCount = newLines.reduce((total, [, line]) => total + line.branchesToCover, 0);
	const coveredBranchCount = newLines.reduce((total, [, line]) => total + line.coveredBranches, 0);

	const total = lineCount + branchCount;
	const percentage = total === 0 ? 100 : ((coveredLineCount + coveredBranchCount) / total) * 100;

	// 0/0 is a vacuous 100%, so flag the two cases where that number means nothing
	let note = "";
	if (coverage.size === 0) {
		note = "   <-- not in coverage report";
	} else if (changed.size === 0) {
		note = `   <-- no changed lines vs ${base}`;
	}

	console.log(
		`${percentage.toFixed(2).padStart(6)}%  lines ${coveredLineCount}/${lineCount}  ` +
			`branches ${coveredBranchCount}/${branchCount}  ${relativePath}${note}`
	);

	if (summary) {
		return;
	}

	for (const [lineNumber, line] of newLines) {
		const branches = line.branchesToCover
			? ` branches ${line.coveredBranches}/${line.branchesToCover}`
			: "";
		const uncovered =
			line.covered && line.coveredBranches === line.branchesToCover ? "" : "   <-- uncovered";
		console.log(
			`        line ${String(lineNumber).padStart(4)}: ` +
				`covered=${String(line.covered).padEnd(5)}${branches}${uncovered}`
		);
	}
}

function changedSourceFiles(base) {
	return git(["diff", "--name-only", "--diff-filter=ACMR", `${base}...HEAD`])
		.split("\n")
		.map((line) => line.trim())
		.filter((line) => sourceFileRegex.test(line) && !testFileRegex.test(line));
}

function parseArguments() {
	const args = process.argv.slice(2);
	const baseArgument = args.find((argument) => argument.startsWith("--base="));
	return {
		summary: args.includes("--summary"),
		base: baseArgument ? baseArgument.slice("--base=".length) : defaultBase,
		paths: args.filter((argument) => !argument.startsWith("--"))
	};
}

function main() {
	const {summary, base, paths} = parseArguments();

	if (!existsSync(coverageXmlPath)) {
		console.error(`✖ - Missing coverage report ${coverageXmlPath}`);
		console.error("    Generate it first: cd starsky && ./build.sh --target CoverageReport");
		process.exit(1);
	}

	try {
		git(["rev-parse", "--verify", "--quiet", base]);
	} catch {
		console.error(`✖ - Unknown base branch or ref "${base}"`);
		process.exit(1);
	}

	const files = paths.length > 0 ? paths : changedSourceFiles(base);
	if (files.length === 0) {
		console.log(`No changed source files vs ${base}`);
		return;
	}

	const xml = readFileSync(coverageXmlPath, "utf8");
	for (const relativePath of files) {
		reportFile(xml, relativePath, base, summary);
	}
}

main();
