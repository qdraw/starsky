#!/usr/bin/env node

const path = require('path');
const fs = require('fs');
var argv = require('minimist')(process.argv.slice(2));

const jimp = require('jimp');
var sync = require('./sync.js');

// if (argv.env === undefined) {
// 	var dotenv = require('dotenv').config({path: __dirname + "/.env"});
// }
// if (argv.env !== undefined) {
// 	var dotenv = require('dotenv').config({path: path.join(__dirname , argv.env) });
// }

if (argv._.length === 0) {
	var folderpath = process.cwd();
}

if (argv._.length === 1) {
	var folderpath = argv._[0];
}


console.log("> folderpath "+ folderpath);

try {
	fs.mkdirSync(path.join(__dirname,"temp"));
} catch (e) {}

checkIfFolderIsWritable(folderpath);

function checkIfFolderIsWritable(folderpath,callback) {
	fs.access(folderpath, fs.W_OK, function(err) {
		if(err){
			console.error("can't write to: " + folderpath);
			process.exit(1);
		}
		if(!err){
			filewalker(folderpath, function(err, files){
			    if(err){
			        throw err;
			    }
			    // ["c://some-existent-path/file.txt","c:/some-existent-path/subfolder"]
			    console.log(files);
				sendImageToServer(files,-1,false)
			});
		}
	});
}


function filewalker(dir, done) {
    let results = [];

    fs.readdir(dir, function(err, list) {
        if (err) return done(err);

        var pending = list.length;

        if (!pending) return done(null, results);

        list.forEach(function(file){
            file = path.resolve(dir, file);

            fs.stat(file, function(err, stat){
                // If directory, execute a recursive call
                if (stat && stat.isDirectory()) {
                    // Add directory to array [comment if you need to remove the directories from the array]
                    // results.push(file);

                    filewalker(file, function(err, res){
                        results = results.concat(res);
                        if (!--pending) done(null, results);
                    });
                } else {
					if (file.indexOf(".jpg") >= 1) {
						results.push(file);
					}
                    if (!--pending) done(null, results);
                }
            });
        });
    });
}

function sendImageToServer(files,item,err) {

	// end trick

	if (item >= -1 && item <= files.length ) {
		item++;
	}
	if (item <= files.length-1) {
		console.log(files[item]);
		if (err) {
			sendRequest(files,item,sendImageToServerRetry)
		}
		if (!err) {
			sendRequest(files,item,sendImageToServer)
		}
	}
	if (item === files.length) {
		console.log("> DONE");
	}
}

function sendImageToServerRetry(files,item,err) {
	if (err) {
		console.log(">> to many failures");
		process.exit(1)
	}
	if (!err) {
		console.log("> retry once");
		sendImageToServer(files,item,false)
	}
}



function sendRequest(files,item,callback) {

	sync(files[item])
		.then(function(base32hash){
			console.log(base32hash);

			jimp.read(files[item]).then(function (lenna) {
			    return lenna.resize(1000, jimp.AUTO)     // resize
			         .quality(80)                 // set JPEG quality
					 .exifRotate()
			         .write(path.join(__dirname,"temp",base32hash + ".jpg")); // save
			}).catch(function (err) {
			    console.error(err);
			});
			callback(files,item,false)

		}, function (err) {
	});

}
