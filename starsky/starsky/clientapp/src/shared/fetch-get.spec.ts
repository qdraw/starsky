import FetchGet from "./fetch-get";

describe("fetch-get", () => {
  it("default string response", async () => {
    const response = new Response(JSON.stringify("response"));
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest
      .spyOn(window, "fetch")
      .mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");

    expect(spy).toBeCalledWith("/test", {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "X-Requested-With": "XMLHttpRequest"
      },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: "response",
      statusCode: 200
    });
  });

  it("default object response", async () => {
    const response = new Response(JSON.stringify({ test: true }));
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest
      .spyOn(window, "fetch")
      .mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");

    expect(spy).toBeCalledWith("/test", {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "X-Requested-With": "XMLHttpRequest"
      },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: { test: true },
      statusCode: 200
    });
  });

  it("corrupt object response", async () => {
    const response = new Response("{test: }");
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest
      .spyOn(window, "fetch")
      .mockImplementationOnce(() => mockFetchAsXml);

    console.error("the response has a wrong input ==>");
    const result = await FetchGet("/test");

    expect(spy).toBeCalledWith("/test", {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "X-Requested-With": "XMLHttpRequest"
      },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: null,
      statusCode: 200
    });
  });

  it("error string response", async () => {
    const response = new Response(JSON.stringify("response"), {
      statusText: "error",
      status: 500
    });
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest
      .spyOn(window, "fetch")
      .mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchGet("/test");

    expect(spy).toBeCalledWith("/test", {
      credentials: "include",
      headers: {
        Accept: "application/json",
        "X-Requested-With": "XMLHttpRequest"
      },
      method: "GET"
    });
    expect(result).toStrictEqual({
      data: "response",
      statusCode: 500
    });
  });
});
