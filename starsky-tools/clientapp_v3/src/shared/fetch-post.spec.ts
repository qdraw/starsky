import FetchPost from './fetch-post';

describe("fetch-post", () => {

  it("default string response", async () => {
    var response = new Response(JSON.stringify("response"));
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    var spy = jest.spyOn(window, 'fetch').mockImplementationOnce(() => mockFetchAsXml);
    var result = await FetchPost("/test", "")

    expect(spy).toBeCalledWith("/test", { "body": "", "credentials": "include", "headers": { "Accept": "application/json", "Content-type": "application/x-www-form-urlencoded", "X-XSRF-TOKEN": "X-XSRF-TOKEN", }, "method": "post" })
    expect(result.data).toStrictEqual("response");
  });

  it("default object response", async () => {
    var response = new Response(JSON.stringify({ test: true }));
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    var spy = jest.spyOn(window, 'fetch').mockImplementationOnce(() => mockFetchAsXml);
    var result = await FetchPost("/test", "")

    expect(spy).toBeCalledWith("/test", { "body": "", "credentials": "include", "headers": { "Accept": "application/json", "Content-type": "application/x-www-form-urlencoded", "X-XSRF-TOKEN": "X-XSRF-TOKEN", }, "method": "post" })
    expect(result.data).toStrictEqual({
      "test": true,
    });
  });

  it("error string response", async () => {
    var response = new Response(JSON.stringify("response"), {
      statusText: "error",
      status: 500
    });
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    var spy = jest.spyOn(window, 'fetch').mockImplementationOnce(() => mockFetchAsXml);
    var result = await FetchPost("/test", "")

    expect(spy).toBeCalledWith("/test", { "body": "", "credentials": "include", "headers": { "Accept": "application/json", "Content-type": "application/x-www-form-urlencoded", "X-XSRF-TOKEN": "X-XSRF-TOKEN", }, "method": "post" })
    expect(result.data).toBe("response")
  });

});