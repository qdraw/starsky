const swagger = require('./assets/config/starsky.json');

function parseSwagger() {
  let output = "";

  const pathLen = 35;
  const operationLen = 6;
  const summaryLen = 75;

  output += `| Path${' '.repeat(pathLen -4)}| Type  | Description ${' '.repeat(summaryLen -12)}| \r\n`;
  output += "|" + '-'.repeat(pathLen +1) + "|" + '-'.repeat(operationLen +1)  + "|" + '-'.repeat(summaryLen+1) + "|\r\n";


  for (var path in swagger.Paths) {

    const pathObject = swagger.Paths[path]

    for (var operation in pathObject.Operations) {

      const rightPathSpace = ' '.repeat(pathLen - path.length)
      const rightOperationSpace = ' '.repeat(operationLen - operation.length)

      const summary = pathObject.Operations[operation].Summary.replace(/(\n|\r\n)/ig,"");

      const trimmedSummary = trimString(summary, summaryLen);
      const rightSummarySpace = ' '.repeat(summaryLen - trimmedSummary.length)

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
