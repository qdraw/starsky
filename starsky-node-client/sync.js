var base32 = require('hi-base32');
const crypto = require('crypto');
const fs = require('fs');

function base32File(fullFilePath) {
	return new Promise(function(resolve, reject) {

        var hash = crypto.createHash('md5');
        const chunks = [];
        var hexs = [];

        var stream = fs.createReadStream(fullFilePath, { start: 0, end: 10000 })

        stream.on('data', function (data) {
            chunks.push(data);
            hash.update(data)
        });

        stream.on('end', function () {
            // var concatBuffer = Buffer.concat(chunks);
            // console.log(concatBuffer);

            var hex = hash.digest('ArrayBuffer');

            // hash.update(concatBuffer)
            // var hex = hash.digest('ArrayBuffer');
            // var hex = hash.digest('Uint8Array');
            // console.log(hex);
        	var t = base32.encode(hex);
        	resolve(t);
        });

    });
}

module.exports = base32File;
