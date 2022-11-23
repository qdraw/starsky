
/* 

Start starsky with envs: 

app__AddSwaggerExport=true
app__AddSwagger=true

and copy this file
>      app__addSwaggerExport ....../bin/Debug/netcoreapp3.1/temp/starsky.json

*/

const swagger = require('./assets/config/starsky.json');

function parseSwagger() {
  let output = "";

  const pathLen = 35;
  const operationLen = 6;
  const summaryLen = 75;

  output += `| Path${' '.repeat(pathLen -4)}| Type  | Description ${' '.repeat(summaryLen -12)}| \r\n`;
  output += "|" + '-'.repeat(pathLen +1) + "|" + '-'.repeat(operationLen +1)  + "|" + '-'.repeat(summaryLen+1) + "|\r\n";


  for (var path in swagger.paths) {

    const pathObject = swagger.paths[path]

    for (var operation in pathObject.operations) {

      const rightPathSpace = ' '.repeat(pathLen - path.length)
      const rightOperationSpace = ' '.repeat(operationLen - operation.length)

      let summary = "Missing summary"
      if (pathObject.operations[operation].summary) {
        summary = pathObject.operations[operation].summary.replace(/(\n|\r\n)/ig,"");
      }

      const trimmedSummary = trimString(summary, summaryLen);
      const rightSummarySpace = ' '.repeat(summaryLen - trimmedSummary.length);

      output += `| ${path}${rightPathSpace}| ${operation.toUpperCase()}${rightOperationSpace}` +
      `| ${trimmedSummary}${rightSummarySpace}|\r\n`;
    }
  }
  return output;
}

console.log(parseSwagger());

function trimString(string, length) {
  return string.length > length ? string.substring(0, length - 3) + "..." : string;
}
