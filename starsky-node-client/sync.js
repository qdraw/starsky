var base32 = require('hi-base32');
const crypto = require('crypto');
const fs = require('fs');

function base32File(fullFilePath) {
	return new Promise(function(resolve, reject) {

        var hash = crypto.createHash('md5'),
            stream = fs.createReadStream(fullFilePath);

        stream.on('data', function (data) {
            hash.update(data, 'utf8');
        });

        stream.on('end', function () {
            var hex = hash.digest('Uint8Array');
        	var t = base32.encode(hex);
        	t = t.replace(/=/ig,"");
        	resolve(t);
        });

    });
}

module.exports = base32File;
