const splash = new BrowserWindow({
width: 300,
height: 200,
transparent: true,
frame: false,
alwaysOnTop: true
});

splash.loadFile(path.join(
\_\_dirname,
"..",
"..",
"client/pages/splash/splash.html"
));
splash.center();

console.log('-djfnlksdlksfd');

await RetryGetNetRequest(`http://localhost:${appPort}/api/health`)

// newWindow.once("ready-to-show",async () => {
// newWindow.show();
// });
