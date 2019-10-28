
var path = require('path');
require('dotenv').config({ path: path.join(__dirname,".env") });

var dropboxCore = require('./dropbox-core');

var dropbox = new dropboxCore(process.env.DROPBOX_ACCESSTOKEN);

dropbox.listFiles("/Camera Uploads/").then((entries) => {
    console.log('d111ddd');

    entries.forEach(element => {
        console.log(element.path_display);
    });
   
});


// var requestOptions = {
//     url: '',
//     method: "GET",
//     headers: {
//         'User-Agent': 'MS FrontPage Express',
//         'Authorization': 'Basic ' + process.env.DROPBOX_ACCESSTOKEN,
//     },
// };


// Axios(downloadFileRequestOptions).then((response) => {
//     const writer = fs.createWriteStream(filePath)

//     response.data.pipe(writer);

//     writer.on('finish', resolve) // not able to return bool
//     writer.on('error', resolve)

// }).catch(function (thrown) {
//     resolve(false);
// });

// https://api.dropboxapi.com/2/files/list_folder



