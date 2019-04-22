#!/usr/bin/env node

var path = require('path');
require('dotenv').config({path:path.join(__dirname,".env")});

var core = require('./thumbnail.core');

var searchQuery = parseArgs();

function parseArgs() {
	var args = process.argv.slice(2);
	if (args.length >= 1) {
		var parsed = parseInt(args[0])
		if (isNaN(parsed)) {
			return args[0];
		}
		else if(parsed === 0 ){
			// Search for today
			return "-Datetime>0 -ImageFormat:jpg -!delete";
		}
		else {
			// 1 = yesterday
			return "-Datetime>" + parsed  + " -Datetime<"+ (parsed - 1) + " -ImageFormat:jpg -!delete";
		}
	}

	var parsedDefault = 1;
	return "-Datetime>" + (parsedDefault)  + " -ImageFormat:jpg -!delete";
}

core.getSearchStart(searchQuery,0);
