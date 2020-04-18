'use strict';

exports.default = context => {
  console.log('-beforebuild');

  console.log(context);
  console.log(`../../starsky/${context.platform.name}`);


  const _promises = [];
  return Promise.all(_promises);
};