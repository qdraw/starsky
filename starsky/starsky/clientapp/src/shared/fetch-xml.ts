import { IConnectionDefault } from '../interfaces/IConnectionDefault';

function isParseError(parsedDocument: any) {
  // parser and parsererrorNS could be cached on startup for efficiency
  const parser = new DOMParser(),
    errorneousParse = parser.parseFromString('<', 'text/xml'),
    parsererrorNS = errorneousParse.getElementsByTagName("parsererror")[0].namespaceURI;

  if (parsererrorNS === 'http://www.w3.org/1999/xhtml') {
    // In PhantomJS the parseerror element doesn't seem to have a special namespace, so we are just guessing here :(
    return parsedDocument.getElementsByTagName("parsererror").length > 0;
  }

  return parsedDocument.getElementsByTagNameNS(parsererrorNS, 'parsererror').length > 0;
}

const FetchXml = async (url: string): Promise<IConnectionDefault> => {
  const settings = {
    method: 'GET',
    credentials: "include" as RequestCredentials,
    headers: {
      'Accept': 'text/xml',
    }
  };
  const res = await fetch(url, settings);
  try {
    const response = await res.text();
    const xmlParser = new DOMParser();
    const data = xmlParser.parseFromString(response, 'text/xml');

    if (isParseError(data)) {
      return {
        statusCode: 999,
        data: null
      } as IConnectionDefault;
    }

    return {
      statusCode: res.status,
      data
    } as IConnectionDefault;

  } catch (err) {
    console.error(err);
    return {
      statusCode: res.status,
      data: null
    } as IConnectionDefault;
  }
};

export default FetchXml;
