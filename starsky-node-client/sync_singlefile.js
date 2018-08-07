#!/usr/bin/env node
var sync = require('./sync.js');


sync("/Volumes/gaia/storage/git/starsky/starsky-node-client/20180806_211106_APC_003201.dng")
	.then(function(base32hash){
		console.log(base32hash);
	}, function (err) {
});