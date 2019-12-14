var imaps = require('imap-simple');
var request = require('request');
var path = require('path');
require('dotenv').config({ path: path.join(__dirname, ".env") });

// process.env.IMAPUSER
// process.env.IMAPPASSWORD
// process.env.STARKSYACCESSTOKEN < base64
// process.env.STARKSYURL

console.log("Checking mail for: " + process.env.IMAPUSER);

var config = {
    imap: {
        user: process.env.IMAPUSER,
        password: process.env.IMAPPASSWORD,
        host: 'imap.gmail.com',
        port: 993,
        tls: true,
        authTimeout: 3000,
        tlsOptions: { servername: 'imap.gmail.com' }
    }
};

function slugify(text) {
    return text.toString().toLowerCase()
        .replace(/\s+/g, '-')           // Replace spaces with -
        .replace(/[^\w\-\.]+/g, '')       // Remove all non-word chars (keep dots)
        .replace(/\-\-+/g, '-')         // Replace multiple - with single -
        .replace(/^-+/, '')             // Trim - from start of text
        .replace(/-+$/, '');            // Trim - from end of text
}

imaps.connect(config).then(function (connection) {

    connection.openBox('INBOX').then(function () {

        // Fetch emails from the last 60h
        var delay = 60 * 3600 * 1000;
        var yesterday = new Date();
        yesterday.setTime(Date.now() - delay);
        yesterday = yesterday.toISOString();
        var searchCriteria = [['SINCE', yesterday]];
        var fetchOptions = { bodies: ['HEADER.FIELDS (FROM TO SUBJECT DATE)'], struct: true };

        // retrieve only the headers of the messages
        return connection.search(searchCriteria, fetchOptions);
    }).then(function (messages) {


        var attachments = [];

        messages.forEach(function (message) {

            var parts = imaps.getParts(message.attributes.struct);
            attachments = attachments.concat(parts.filter(function (part) {
                return part.disposition && part.disposition.type.toUpperCase() === 'ATTACHMENT';
            }).map(function (part) {

                // retrieve the attachments only of the messages with attachments
                return connection.getPartData(message, part)
                    .then(function (partData) {

                        var filename = "default";
                        if (part !== undefined) {
                            if (part.disposition !== undefined &&
                                part.disposition !== null) {
                                if (part.disposition.params !== undefined &&
                                    part.disposition.params !== null) {
                                    if (part.disposition.params.filename !== undefined) {
                                        filename = part.disposition.params.filename
                                    }
                                }
                            }
                        }
                        return {
                            filename: filename,
                            data: partData,
                            label: message.attributes["x-gm-labels"]
                        };
                    });
            }));
        });

        return Promise.all(attachments);
    }).then(function (attachments) {

        // =>
        //    [ { filename: 'cats.jpg', data: Buffer() },
        //      { filename: 'pay-stub.pdf', data: Buffer() } ]

        for (var i = 0; i < attachments.length; i++) {

            // return non gpx
            // Need to have a gmail filter to white list the users
            if (attachments[i].filename.indexOf(".gpx") === -1) continue;
            if (attachments[i].label.indexOf("gpx") === -1) continue;

            // Escape strange filenames
            //    { filename: '=?UTF-8?Q?Dag_e=CC=81e=CC=81n_avondrit_9-8-2019.gpx?=' } }
            var fileName = attachments[i].filename.replace(/(^=\?)|(UTF-8)|(\?Q\?)|(\?=$)/, "");
            filename = slugify(fileName);

            console.log("file: " + filename + " (" + attachments[i].filename + ")");

            var formData = {
                image_file: {
                    value: attachments[i].data, // Upload the first file in the multi-part post
                    options: {
                        filename: filename
                    }
                }
            };

            request({
                headers: {
                    'Authorization': 'Basic ' + process.env.STARKSYACCESSTOKEN,
                    'Structure': '/yyyy/MM/yyyy_MM_dd*/__\\g\\p\\x__yyyyMMdd_HHmmss_{filenamebase}.ext'
                },
                formData: formData,
                uri: process.env.STARKSYURL,
                method: 'POST'
            }, function (err, res, body) {
                console.log('>> sending to starsky');
                if (err) {
                    throw new Error(err);
                }
                console.log(res.statusCode);
            });
        }
        connection.end();
    });
});
