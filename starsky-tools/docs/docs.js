// nodejs
var path = require('path');
var fs = require('fs');
var showdown = require('showdown');
showdown.setFlavor('github');

var converter = new showdown.Converter();
converter.setOption('completeHTMLDocument', true);

var prefixPath = "../../";

var filePathList = [
	"readme.md",
	"history.md",
	"starsky-tools/readme.md",
	"inotify-settings/readme.md",
	"starsky/readme.md",
	"starsky/starsky/readme.md",
	"starsky/starsky/clientapp/readme.md",
	"starsky/starskycore/readme.md",
	"starsky/starskygeocore/readme.md",
	"starsky/starsky/readme_api.md",
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
	"starsky-tools/localtunnel/readme.md",
	"starsky-tools/sync/readme.md",
	"starsky-tools/dropbox-import/readme.md"
];

var blobPathList = [
	"starsky/starsky/pm2-new-instance.sh",
	"azure-pipelines-starsky.yml",
];

// create dirs
var sourceFullPathList = [];
var htmlFullPathList = [];

for (var i = 0; i < filePathList.length; i++) {
	var filePath = filePathList[i];

	if (filePath.indexOf("/") >= 0) {
		var dir = path.join(__dirname, filePath.substr(0, filePath.lastIndexOf("/")));
		if (!fs.existsSync(dir)) {
			fs.mkdirSync(dir);
		}
	}
	var relativeSource = path.join(__dirname, prefixPath, filePath);
	sourceFullPathList.push(relativeSource);
	var htmlPath = path.join(__dirname, filePath.replace(".md", ".html"));
	htmlFullPathList.push(htmlPath);

}

for (var i = 0; i < blobPathList.length; i++) {
	var filePath = blobPathList[i];
	var relativeSource = path.join(__dirname, prefixPath, filePath);
	var outputPath = path.join(__dirname, filePath);
	console.log(outputPath);
	fs.copyFileSync(relativeSource, outputPath, { flag: 'w' })
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

	var menuHtml = `<div class="head">
		<div id="menu"> 
			<ul> 
				<li>
					<a href="` + relativeCssPath + `readme.html">Home</a>
				</li> 
			</ul> 
		</div> 
		<a href="#hamburger" id="hamburger" class="hamburger">Menu</a> 
		<a href="` + relativeCssPath + `readme.html" class="logo">
		Docs
		</a>
	</div>`;

	var outputHtml = contentsHtml.replace(/<\/head>\n<body>/ig, "<title>" + getTitle(contentsHtml) + "</title><link rel=\"stylesheet\" href=\"" + relativeCssPath + "style.css\"><\/head>\n<body>\n" + menuHtml + "\n<div class=\"container\"><div class=\"entry-content\">");

	contentsHtml = outputHtml.replace(/<\/body>\n/ig, "</div>\n</div>\n <script defer src=\"" + relativeCssPath + "menu.js\"></script></body>\n");
	console.log(htmlPath);
	fs.writeFileSync(htmlPath, contentsHtml);
}

function getTitle(contentsHtml) {
	var regexStartTag = /<h1( [a-z-="]+)?>/ig
	var regexEndTag = /<\/h1>/ig
	var title = "-- Starsky Docs (unknown page)"

	var match = regexStartTag.exec(contentsHtml);
	if (match) {
		var restOfString = contentsHtml.substring(match.index);
		var restOfStringMatch = regexEndTag.exec(restOfString);
		if(restOfStringMatch) {
			restOfString = restOfString.substr(0,restOfStringMatch.index) 
			title = restOfString.replace(regexStartTag,"")
		}
	}
	else {
		console.log(">> " + htmlPath + " has no title");
	}
	return title;
}
