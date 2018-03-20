var base32 = require('hi-base32');

const crypto = require('crypto');
const fs = require('fs');

var hash = crypto.createHash('md5'),
    stream = fs.createReadStream('20180103_215324_im.jpg');

stream.on('data', function (data) {
    hash.update(data, 'utf8');
});

stream.on('end', function () {
    var hex = hash.digest('Uint8Array'); // 34f7a3113803f8ed3b8fd7ce5656ebec
	var t = base32.encode(hex);
	t = t.replace(/=/ig,"");
	console.log(t);
});
