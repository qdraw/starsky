// nodejs
var path = require("path");
var fs = require("fs");
var marked = require("marked");

var prefixPath = "../../";

function defaultHeader() {
	var contents = fs.readFileSync(
		path.join(__dirname, "default_header.md"),
		"utf8"
	);
	var contentsHtml = marked.parse(contents);
	contentsHtml = contentsHtml.replace(/"[^"]+"/g, function (m) {
		return m.replace(/\.md/g, ".html");
	});
	return contentsHtml;
}

var defaultHeaderContent = defaultHeader();

var filePathList = [
	"index.md",
	"readme.md",
	"history.md",
	"starsky-tools/readme.md",
	"starsky/readme.md",
	"starsky/readme-docker-development.md",
	"starsky/readme-docker-hub.md",
	"starsky/starsky/readme.md",
	"starsky/starsky/clientapp/readme.md",
	"starsky/starskybusinesslogic/readme.md",
	"starsky/starsky.feature.geolookup/readme.md",
	"starsky/starsky.foundation.injection/readme.md",
	"starsky/starskysynchronizecli/readme.md",
	"starsky/starskythumbnailcli/readme.md",
	"starsky/starskyadmincli/readme.md",
	"starsky/starskygeocli/readme.md",
	"starsky/starskyimportercli/readme.md",
	"starsky/starskytest/readme.md",
	"starsky/starskywebftpcli/readme.md",
	"starsky/starskywebhtmlcli/readme.md",
	"starskyapp/readme.md",
	"starsky-tools/docs/readme.md",
	"starsky-tools/mail/readme.md",
	"starsky-tools/thumbnail/readme.md",
	"starsky-tools/localtunnel/readme.md",
	"starsky-tools/sync/readme.md",
	"starsky-tools/dropbox-import/readme.md",
	"starsky-tools/end2end/readme.md",
	"starsky-tools/build-tools/readme.md",
	"starsky-tools/release-tools/readme.md",
	"starsky-tools/slack-notification/readme.md",
];

var blobPathList = [
	"starsky/starsky/pm2-new-instance.sh",
	"starsky/docs/starsky-mac-v043-home-nl.jpg",
	"starskyapp/docs-assets/starskyapp-versions.jpg",
	"starskyapp/docs-assets/starskyapp-remote-options-v040.jpg",
	"starskyapp/docs-assets/starskyapp-mac-gatekeeper.jpg",
];


marked.setOptions({
  mangle:false // avoid emails to have different outputs
});


/**
 * Look ma, it's cp -R.
 * @param {string} src  The path to the thing to copy.
 * @param {string} dest The path to the new copy.
 */
var copyRecursiveSync = function (src, dest) {
	var exists = fs.existsSync(src);
	var stats = exists && fs.statSync(src);
	var isDirectory = exists && stats.isDirectory();
	if (isDirectory) {
		fs.mkdirSync(dest);
		fs.readdirSync(src).forEach(function (childItemName) {
			copyRecursiveSync(
				path.join(src, childItemName),
				path.join(dest, childItemName)
			);
		});
	} else {
		fs.copyFileSync(src, dest);
	}
};

var outputFolderName = "output_folder";

// remove and create dir
var outputFolderFullPath = path.join(__dirname, outputFolderName);
if (fs.existsSync(outputFolderFullPath)) {
	fs.rmSync(outputFolderFullPath, { recursive: true });
}
fs.mkdirSync(outputFolderFullPath);

// Assets folder
copyRecursiveSync(
	path.join(__dirname, "assets"),
	path.join(__dirname, outputFolderName, "assets")
);

// create dirs
createDirectories(filePathList);
createDirectories(blobPathList);

var sourceFullPathList = [];
var htmlFullPathList = [];

for (var i = 0; i < filePathList.length; i++) {
	var filePath = filePathList[i];
	var relativeSource = path.join(__dirname, prefixPath, filePath);
	sourceFullPathList.push(relativeSource);
	var htmlPath = path.join(
		__dirname,
		outputFolderName,
		filePath.replace(".md", ".html")
	);
	htmlFullPathList.push(htmlPath);
}

function createDirectories(inputList) {
	for (var i = 0; i < inputList.length; i++) {
		var filePath = inputList[i];

		if (filePath.indexOf("/") >= 0) {
			var dir = path.join(
				__dirname,
				outputFolderName,
				filePath.substr(0, filePath.lastIndexOf("/"))
			);
			if (!fs.existsSync(dir)) {
				fs.mkdirSync(dir);
			}
		}
	}
}

/**
 * copy files
 */
for (var i = 0; i < blobPathList.length; i++) {
	var filePath = blobPathList[i];
	var relativeSource = path.join(__dirname, prefixPath, filePath);
	var outputPath = path.join(__dirname, outputFolderName, filePath);
	console.log(relativeSource, outputPath);
	fs.copyFileSync(relativeSource, outputPath);
}

for (var i = 0; i < htmlFullPathList.length; i++) {
	var htmlPath = htmlFullPathList[i];

	var contents = fs.readFileSync(sourceFullPathList[i], "utf8");

	let defaultHead = fs.readFileSync(
		path.join(__dirname, "default_head.html"),
		"utf8"
	);
	var contentsHtml = defaultHead.replace("$content$", marked.parse(contents));

	contentsHtml = contentsHtml.replace(/"[^"]+"/g, function (m) {
		return m.replace(/\.md/g, ".html");
	});

	// used for relative css and js files
	var split = filePathList[i].split("/");
	var relativeCssPath = "";
	for (var j = 0; j < split.length - 1; j++) {
		relativeCssPath += "../";
	}

	var menuHtml =
		`
	<header class="docs-header">
		<div class="wrapper">
			<div class="detective"></div> <a href="` +
		relativeCssPath +
		`index.html" class="name">Starsky Docs</a>
		</div>
	</header>`;

	var outputHtml = contentsHtml.replace(
		/<\/head>\n(\s+)<body>/gi,
		"<title>" +
			getTitle(contentsHtml) +
			'</title><link rel="stylesheet" href="' +
			relativeCssPath +
			'assets/style/style.css"><meta name="viewport" content="width=device-width,initial-scale=1"/></head>\n<body>\n' +
			menuHtml +
			'\n<div id="app"><div class="container"><div class="entry-content">' +
			breadcrumb(htmlPath)
	);

	contentsHtml = outputHtml.replace(
		/<\/body>\n/gi,
		"</div>\n</div>\n</div>\n </body>\n"
	);
	contentsHtml = contentsHtml.replace(
		/<head>/gi,
		`<head>\n <!-- \n\nDo NOT edit this file -  \nThis Generated by Starsky Docs \n\n-->`
	);

	var defaultHeaderExist =
		contentsHtml.includes('<h2 id="list-of-starsky-projects">') ||
		contentsHtml.includes('<h2 id="list-of-starskyreadmemd-projects">') ||
		contentsHtml.includes('<h2 id="list-of-__starskyreadmemd__-projects">');

	contentsHtml = classMapper(contentsHtml);

	if (!defaultHeaderExist) {
		const editedHeaderContent = linkRelativeReplacer(filePathList[i]);
		contentsHtml = contentsHtml.replace(
			"<!-- end breadcrumb -->",
			editedHeaderContent
		);
	}

	contentsHtml = contentsHtml.replace(
		/Projects<\/h2>\n<ul>/gi,
		"Projects</h2><ul id='list-of-starsky-projects-element'>"
	);

	console.log(htmlPath);
	fs.writeFileSync(htmlPath, contentsHtml);
}

/**
 * used to display buttons
 * @param {*} contentsHtml html
 * @returns
 */
function classMapper(contentsHtml) {
	// (?<=<a href=".+classes=).+">
	var regex = /(?<=<a href=\".+classes=).+\">/gi;

	for (var i = 0; i < Array.from(contentsHtml.matchAll(regex)).length; i++) {
		var match = Array.from(contentsHtml.matchAll(regex))[i];
		var endIndex = match.index + match[0].length - 1;

		var className = match[0].replace(/,/i, " ");
		className = className.substring(0, className.length - 2);

		contentsHtml =
			contentsHtml.substring(0, endIndex) +
			' class="' +
			className +
			'"' +
			contentsHtml.substr(endIndex);
	}

	return contentsHtml;
}

function breadcrumb(htmlPath) {
	var dirName = path.join(__dirname, outputFolderName);
	var subpath = htmlPath
		.replace(dirName, "")
		.replace(/\/|\\/gi, "/")
		.replace(/^\//gi, "")
		.replace(".html", "");
	var result = "";
	var splitedPaths = subpath.split("/");

	var relative = [];
	for (var i = 0; i < splitedPaths.length; i++) {
		if (i === 0) {
			relative.push("");
			continue;
		}
		relative.push(relative[i - 1] + "../");
	}

	var relative = relative.reverse();

	for (var i = 0; i < splitedPaths.length; i++) {
		if (i >= relative.length) {
			result += splitedPaths[i];
			continue;
		}

		var name =
			splitedPaths[i - 1] === undefined ? "home" : splitedPaths[i - 1];
		var subpath =
			splitedPaths[i - 1] === undefined
				? relative[i] + "index.html"
				: relative[i] + "readme.html";
		result += '<a href="' + subpath + '">' + name + "</a> » ";
	}

	result += splitedPaths[splitedPaths.length - 1];
	return (
		'<div class="breadcrumb">' + result + "</div>\n<!-- end breadcrumb -->"
	);
	// keep the end breadcrumb to search and replace for
}

function getTitle(contentsHtml) {
	var regexStartTag = /<h1( [a-z-="]+)?>/gi;
	var regexEndTag = /<\/h1>/gi;
	var title = "Starsky Docs (unknown page)";

	var match = regexStartTag.exec(contentsHtml);
	if (match) {
		var restOfString = contentsHtml.substring(match.index);
		var restOfStringMatch = regexEndTag.exec(restOfString);
		if (restOfStringMatch) {
			restOfString = restOfString.substr(0, restOfStringMatch.index);
			title = restOfString.replace(regexStartTag, "");
		}
	} else {
		console.log(">> " + htmlPath + " has no title");
	}
	return title;
}

String.prototype.replaceBetween = function (start, end, what) {
	return this.substring(0, start) + what + this.substring(end);
};

function linkRelativeReplacer(subPath) {
	// example url : 'assets/download/download.html'

	const ahrefLinkRegex = new RegExp(
		"(<|&lt;)a.*? hrefs*=s*('|\"|&apos;|&quot;)(.+?)(<|&lt;)/a(>|&gt;)",
		"ig"
	);
	const targetRegex = new RegExp(
		"targets*=s*(\"|'|&apos;|&quot;)((?!(\"|'|&apos;|&quot;)).)*(\"|'|&apos;|&quot;)",
		"ig"
	);
	const hrefRegex = new RegExp(
		"hrefs*=s*(\"|'|&apos;|&quot;)(.+?)" +
			"((?!(\"|'|&apos;|&quot;)).)*(\"|'|&apos;|&quot;)",
		"ig"
	);
	const tagContentRegex = new RegExp("(>|&gt;).*(<|&lt;)", "ig");

	const ahrefsMatches = defaultHeaderContent.match(ahrefLinkRegex);
	let editedHeaderContent = defaultHeaderContent;

	for (const inputMatch of Array.from(ahrefsMatches)) {
		const targetMatches = inputMatch.match(targetRegex);
		const targetMatch = targetMatches ? targetMatches[0] : "";

		const hrefMatches = inputMatch.match(hrefRegex);
		const hrefMatch = hrefMatches ? hrefMatches[0] : "";
		const url = hrefMatch.replace('href="', "");
		if (url.startsWith("http")) {
			continue;
		}

		let newUrl = "";
		for (let index = 0; index < subPath.split("/").length - 1; index++) {
			newUrl += "../";
		}
		newUrl += url;

		const tagContentMatches = inputMatch.match(tagContentRegex);
		const tagContentMatch = tagContentMatches ? tagContentMatches[0] : "";

		const newOutput = `<a href="${newUrl} ${targetMatch}${tagContentMatch}/a>`;
		editedHeaderContent = editedHeaderContent.replace(
			inputMatch,
			newOutput
		);
	}
	return editedHeaderContent;
}

function searchReplaceStaticContent(subPath) {
	var filePath = path.join(__dirname, subPath);
	var contents = fs.readFileSync(filePath, "utf8");

	const startTag = "<!-- default_header begin -->";
	const endTag = "<!-- default_header end -->";

	const startIndex = contents.indexOf(startTag);
	const endIndex = contents.indexOf(endTag);

	const editedHeaderContent = linkRelativeReplacer(subPath);
	const contentsHtml = contents.replaceBetween(
		startIndex,
		endIndex,
		startTag + editedHeaderContent
	);
	fs.writeFileSync(filePath, contentsHtml);
}

searchReplaceStaticContent("assets/download/download.html");
