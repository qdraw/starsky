import FetchGet from "./fetch-xml";

describe("fetch-xml", () => {
  it("default string response", async () => {
    const responseString = "<div>response</div>";
    const mockFetchAsXml: Promise<Response> = Promise.resolve(new Response(responseString));
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");

    const xmlParser = new DOMParser();

    expect(spy).toHaveBeenCalledWith("/test", {
      credentials: "include",
      headers: { Accept: "text/xml" },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: xmlParser.parseFromString(responseString, "text/xml"),
      statusCode: 200
    });
  });

  it("corrupt xml object response", async () => {
    const responseString = "<div>response"; // this should not have a close div
    const mockFetchAsXml: Promise<Response> = Promise.resolve(new Response(responseString));
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");

    expect(spy).toHaveBeenCalledWith("/test", {
      credentials: "include",
      headers: { Accept: "text/xml" },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: null,
      statusCode: 999
    });
  });

  it("error string response", async () => {
    const responseString = "<div>response</div>";
    const response = new Response(responseString, {
      statusText: "error",
      status: 500
    });
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");
    const xmlParser = new DOMParser();

    expect(spy).toHaveBeenCalledWith("/test", {
      credentials: "include",
      headers: { Accept: "text/xml" },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: xmlParser.parseFromString(responseString, "text/xml"),
      statusCode: 500
    });
  });

  it("returns error result when reading the response body throws", async () => {
    const response = new Response("<div>response</div>", { status: 502 });
    jest.spyOn(response, "text").mockImplementationOnce(() => {
      throw new Error("boom");
    });
    jest.spyOn(window, "fetch").mockImplementationOnce(() => Promise.resolve(response));
    const consoleErrorSpy = jest.spyOn(console, "error").mockImplementationOnce(() => undefined);

    const result = await FetchGet("/test");

    expect(consoleErrorSpy).toHaveBeenCalled();
    expect(result).toStrictEqual({
      data: null,
      statusCode: 502
    });
  });

  it("uses the xhtml namespace fallback branch when detecting parser errors", async () => {
    const responseString = "<div>response</div>";
    const mockFetchAsXml: Promise<Response> = Promise.resolve(new Response(responseString));
    jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);

    const parseFromStringSpy = jest
      .spyOn(DOMParser.prototype, "parseFromString")
      .mockImplementation((markup: string) => {
        if (markup === "<") {
          return {
            getElementsByTagName: () => [{ namespaceURI: "http://www.w3.org/1999/xhtml" }]
          } as unknown as Document;
        }
        return {
          getElementsByTagName: (tagName: string) => (tagName === "parsererror" ? [{}] : []),
          getElementsByTagNameNS: () => []
        } as unknown as Document;
      });

    const result = await FetchGet("/test");

    expect(result).toStrictEqual({
      data: null,
      statusCode: 999
    });

    parseFromStringSpy.mockRestore();
  });
});
