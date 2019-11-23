// nodejs
var path = require('path');
var fs = require('fs');
var showdown = require('showdown');
showdown.setFlavor('github');

var converter = new showdown.Converter();
converter.setOption('completeHTMLDocument', true);

// var exec = require('child_process').exec;
// function execute(command, callback){
//     exec(command, function(error, stdout, stderr){ callback(stdout); });
// }
//
// execute("which dotnet", function(name){
// 	if (name.indexOf("dotnet") >= 0) {
// 		execute("dotnet test ../../starsky/starskyTests --filter ClassName=starskytests.Helpers.SwaggerHelperTest", function(name){
// 			console.log(name);
//
// 		});
// 	}
// });


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
	"starsky/starsky/pm2-starksy-new.sh",
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

	var menuHtml = '<div class="head"><div id="menu"> <ul> <li><a href="' + relativeCssPath + 'readme.html">Home</a></li> <li><a href="https://qdraw.nl/contact.html">Contact</a></li> </ul> </div> <a href="#hamburger" id="hamburger" class="hamburger">Menu</a> <a href="' + relativeCssPath + 'readme.html" class="logo">Qdraw.nl</a></div>';
	var outputHtml = contentsHtml.replace(/<\/head>\n<body>/ig, "<link rel=\"stylesheet\" href=\"" + relativeCssPath + "style.css\"><\/head>\n<body>\n" + menuHtml + "\n<div class=\"container\"><div class=\"entry-content\">");

	contentsHtml = outputHtml.replace(/<\/body>\n/ig, "</div>\n</div>\n <script defer src=\"" + relativeCssPath + "menu.js\"></script></body>\n");
	console.log(htmlPath);
	fs.writeFileSync(htmlPath, contentsHtml);
}
