/* 

Start starsky with envs: 

app__AddSwaggerExport=true
app__AddSwagger=true

and copy this file
>      app__addSwaggerExport ....../bin/Debug/netcoreapp3.1/temp/starsky.json

*/

const fs = require('fs');
const swagger = require('../static/openapi/openapi.json');

function parseSwagger() {
  let output = "";

  const pathLen = 50;
  const operationLen = 6;
  const summaryLen = 80;
  const wordBoundCorrection = 25;

  output += `| Path${' '.repeat(pathLen -4)}| Type  | Description ${' '.repeat(summaryLen -12)}| \r\n`;
  output += "|" + '-'.repeat(pathLen +1) + "|" + '-'.repeat(operationLen +1)  + "|" + '-'.repeat(summaryLen+1) + "|\r\n";


  for (var path in swagger.paths) {

    const pathObject = swagger.paths[path]


    for (var operation in pathObject.operations) {

      if (operation === "Head") {
        continue;
      }

      const pathContent = `__${path}__`
      const rightPathSpace = ' '.repeat(pathLen - pathContent.length)
      const rightOperationSpace = ' '.repeat(operationLen - operation.length)

      let summary = "Missing summary"
      if (pathObject.operations[operation].summary) {
        summary = pathObject.operations[operation].summary.replace(/(\n|\r\n)/ig,"") ;
      }

      const trimmedSummary = trimString(summary, summaryLen);
      const rightSummarySpace = ' '.repeat(summaryLen - trimmedSummary.length);

      output += `| ${pathContent}${rightPathSpace}| ${operation.toUpperCase()}${rightOperationSpace}` +
      `| ${trimmedSummary}${rightSummarySpace}|\r\n`;



      let parametersDefaultValue = 'Parameters: '
      let parametersContent = parametersDefaultValue;
      for (const parameterIndex in pathObject.operations[operation].parameters) {
        const parameter = pathObject.operations[operation].parameters[parameterIndex];

        parametersContent += `${parameter.name}`;
        if (parameter.description) {
          parametersContent += ` (${parameter.description})`
        }
        if (parameterIndex != pathObject.operations[operation].parameters.length-1 ) {
          parametersContent += ", "
        }
      }

      if (parametersContent && parametersContent !== parametersDefaultValue) {

        const regex = new RegExp(`(?!\\s).{${pathLen + operationLen + summaryLen - wordBoundCorrection},}?(?=\\s|$)`, "g");
        const matches = parametersContent.match(regex);

        if (matches) {
          let parameterOutputDescription = "";
          for (const splitedContent of matches) {
            const value = (pathLen + operationLen + summaryLen) - splitedContent.length;

            const rightParameterSpace = ' '.repeat(value);
            parameterOutputDescription += `| _${splitedContent}${rightParameterSpace} _ |\r\n`;
          }

          if (matches.length >= 1) {
            const lastContentInMatch = matches[matches.length-1];
            let index = parametersContent.indexOf(lastContentInMatch) + lastContentInMatch.length;
            const splitedContent = parametersContent.substring(index , parametersContent.length);
            if (splitedContent.length >= 1) {
              const value = (pathLen + operationLen + summaryLen) - splitedContent.length;
              const rightParameterSpace = ' '.repeat(value);
              parameterOutputDescription += `| _${splitedContent}${rightParameterSpace} _ |\r\n`;
            }

          }

          output += parameterOutputDescription;
        }
      }
    }
  }
  return output;
}

function trimString(string, length) {
  return string.length > length ? string.substring(0, length - 3) + "..." : string;
}


function parseAndWrite(showLog = false) {
  const output = parseSwagger();

  let apiOutputReadme = `# API Endpoint Documentation\nThe API has two ways of authentication using Cookie Authentication via the \`/api/account/login\` endpoint and Basic Authentication\n`;
  apiOutputReadme += "\nThis document is auto generated";
  apiOutputReadme += `\n\n${output}`;
  if (showLog) {
    console.log(apiOutputReadme);
  }

  fs.writeFileSync('docs/api/readme.md', apiOutputReadme, 'utf8');
}

if (require.main === module) {
  parseAndWrite(true);
}

module.exports = { parseAndWrite };

