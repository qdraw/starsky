var imaps = require('imap-simple');
var request = require('request');
var path = require('path');
require('dotenv').config({path:path.join(__dirname,".env")});

// process.env.IMAPUSER
// process.env.IMAPPASSWORD
// process.env.STARKSYACCESSTOKEN < base64
// process.env.STARKSYURL

console.log(process.env.IMAPUSER);

var config = {
    imap: {
        user: process.env.IMAPUSER,
        password: process.env.IMAPPASSWORD,
        host: 'imap.gmail.com',
        port: 993,
        tls: true,
        authTimeout: 3000
    }
};

imaps.connect(config).then(function (connection) {

    connection.openBox('INBOX').then(function () {

        // Fetch emails from the last 48h
        var delay = 48 * 3600 * 1000;
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
                        return {
                            filename: part.disposition.params.filename,
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
            if(attachments[i].filename.indexOf(".gpx") === -1) continue;
            if(attachments[i].label.indexOf("gpx") === -1) continue;

            console.log(attachments[i]);





            var formData = {
                image_file: {
                    value: attachments[i].data, // Upload the first file in the multi-part post
                    options: {
                       filename: attachments[i].filename
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
                    console.log(res.statusCode);
                    console.log(body);
            });
        }


        connection.end();

    });
});
