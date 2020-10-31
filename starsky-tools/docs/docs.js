// nodejs
var path = require('path');
var fs = require('fs');
var showdown = require('showdown');
showdown.setFlavor('github');

var converter = new showdown.Converter();
converter.setOption('completeHTMLDocument', true);

var prefixPath = "../../";

var filePathList = [
	"index.md",
	"readme.md",
	"history.md",
	"starsky-tools/readme.md",
	"inotify-settings/readme.md",
	"starsky/readme.md",
	"starsky/readme-docker-instructions.md",
	"starsky/starsky/readme.md",
	"starsky/starsky/clientapp/readme.md",
	"starsky/starskybusinesslogic/readme.md",
	"starsky/starsky.feature.geolookup/readme.md",
	"starsky/starsky.foundation.injection/readme.md",
	"starsky/starskyadmincli/readme.md",
	"starsky/starskygeocli/readme.md",
	"starsky/starskyimportercli/readme.md",
	"starsky/starskysynccli/readme.md",
	"starsky/starskytest/readme.md",
	"starsky/starskywebftpcli/readme.md",
	"starsky/starskywebhtmlcli/readme.md",
	"starsky.netframework/readme.md",
	"starskyapp/readme.md",
	"starsky-tools/docs/readme.md",
	"starsky-tools/mail/readme.md",
	"starsky-tools/thumbnail/readme.md",
	"starsky-tools/localtunnel/readme.md",
	"starsky-tools/sync/readme.md",
	"starsky-tools/dropbox-import/readme.md",
	"starsky-tools/build-tools/readme.md"
];

var blobPathList = [
	"starsky/starsky/pm2-new-instance.sh",
	"azure-pipelines-starsky.yml",
	"starsky/docs/starsky-mac-v025-home-nl.jpg",
	"starskyapp/docs-assets/starskyapp-versions.jpg",
	"starskyapp/docs-assets/starskyapp-remote-options-v040beta1.jpg"
];

// create dirs
createDirectories(filePathList);
createDirectories(blobPathList);

var sourceFullPathList = [];
var htmlFullPathList = [];

for (var i = 0; i < filePathList.length; i++) {
	var filePath = filePathList[i];
	var relativeSource = path.join(__dirname, prefixPath, filePath);
	sourceFullPathList.push(relativeSource);
	var htmlPath = path.join(__dirname, filePath.replace(".md", ".html"));
	htmlFullPathList.push(htmlPath);
}

function createDirectories(inputList) {
	for (var i = 0; i < inputList.length; i++) {
		var filePath = inputList[i];

		if (filePath.indexOf("/") >= 0) {
			var dir = path.join(__dirname, filePath.substr(0, filePath.lastIndexOf("/")));
			if (!fs.existsSync(dir)) {
				fs.mkdirSync(dir);
			}
		}
	}
}

for (var i = 0; i < blobPathList.length; i++) {
	var filePath = blobPathList[i];
	var relativeSource = path.join(__dirname, prefixPath, filePath);
	var outputPath = path.join(__dirname, filePath);
	console.log(relativeSource, outputPath);
	fs.copyFileSync(relativeSource, outputPath)
}

for (var i = 0; i < htmlFullPathList.length; i++) {
	var htmlPath = htmlFullPathList[i];

	var contents = fs.readFileSync(sourceFullPathList[i], 'utf8')
	var contentsHtml = converter.makeHtml(contents);

	contentsHtml = contentsHtml.replace(/"[^"]+"/g, function (m) {
		return m.replace(/\.md/g, '\.html');
	});

	// used for relative css and js files
	var split = filePathList[i].split("/");
	var relativeCssPath = "";
	for (var j = 0; j < split.length - 1; j++) {
		relativeCssPath += "../";
	}
	var menuHtml = `
	<header class="docs-header">
		<div class="wrapper">
			<div class="detective"></div> <a href="` + relativeCssPath + `index.html" class="name">Starsky Docs</a>
		</div>
	</header>`;

	var outputHtml = contentsHtml.replace(/<\/head>\n<body>/ig, "<title>" + getTitle(contentsHtml) + "</title><link rel=\"stylesheet\" href=\"" + relativeCssPath
		+ "assets/style/style.css\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"/><\/head>\n<body>\n" + menuHtml +
		 "\n<div id=\"app\"><div class=\"container\"><div class=\"entry-content\">"+ breadcrumb(htmlPath));

	contentsHtml = outputHtml.replace(/<\/body>\n/ig, "</div>\n</div>\n</div>\n </body>\n");
	contentsHtml = contentsHtml.replace(/<head>/ig, `<head>\n <!-- \n\nDo NOT edit this file -  \nThis Generated by Starsky Docs \n\n-->`);

	console.log(htmlPath);
	fs.writeFileSync(htmlPath, contentsHtml);
}

function breadcrumb(htmlPath) {
	var path = htmlPath.replace(__dirname, "").replace(/\/|\\/ig,"/").replace(/^\//ig,"").replace(".html","");
	var result = "";
	var splitedPaths = path.split("/");

	var relative = [];
	for (var i = 0; i < splitedPaths.length; i++) {
		if (i === 0) {
			relative.push("")
			continue;
		}
		relative.push(relative[i-1] +"../")
	}

	var relative = relative.reverse();

	for (var i = 0; i < splitedPaths.length; i++) {
		if (i >= relative.length) {
			result += splitedPaths[i]
			continue;
		}

		var name = splitedPaths[i-1] === undefined ? "home" : splitedPaths[i-1];
		var path = splitedPaths[i-1] === undefined ? relative[i] + "index.html" : relative[i] + "readme.html";
		result += "<a href=\""+ path +"\">"+ name +"</a> » "
	}

	result += splitedPaths[splitedPaths.length-1];
	return "<div class=\"breadcrumb\">" + result + "</div>";
}

function getTitle(contentsHtml) {
	var regexStartTag = /<h1( [a-z-="]+)?>/ig
	var regexEndTag = /<\/h1>/ig
	var title = "Starsky Docs (unknown page)"

	var match = regexStartTag.exec(contentsHtml);
	if (match) {
		var restOfString = contentsHtml.substring(match.index);
		var restOfStringMatch = regexEndTag.exec(restOfString);
		if (restOfStringMatch) {
			restOfString = restOfString.substr(0, restOfStringMatch.index)
			title = restOfString.replace(regexStartTag, "")
		}
	}
	else {
		console.log(">> " + htmlPath + " has no title");
	}
	return title;
}
