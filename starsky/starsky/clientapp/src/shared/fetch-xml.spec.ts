import FetchGet from "./fetch-xml";

describe("fetch-xml", () => {
  it("default string response", async () => {
    const responseString = "<div>response</div>";
    const mockFetchAsXml: Promise<Response> = Promise.resolve(
      new Response(responseString)
    );
    const spy = jest
      .spyOn(window, "fetch")
      .mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");

    const xmlParser = new DOMParser();

    expect(spy).toBeCalledWith("/test", {
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
    const mockFetchAsXml: Promise<Response> = Promise.resolve(
      new Response(responseString)
    );
    const spy = jest
      .spyOn(window, "fetch")
      .mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");

    expect(spy).toBeCalledWith("/test", {
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
    const spy = jest
      .spyOn(window, "fetch")
      .mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");
    const xmlParser = new DOMParser();

    expect(spy).toBeCalledWith("/test", {
      credentials: "include",
      headers: { Accept: "text/xml" },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: xmlParser.parseFromString(responseString, "text/xml"),
      statusCode: 500
    });
  });
});
