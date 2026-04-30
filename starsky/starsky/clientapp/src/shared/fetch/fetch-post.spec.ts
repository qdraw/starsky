import FetchPost from "./fetch-post";

// FetchPost tests
describe("fetch-post", () => {
  afterEach(() => {
    window.history.pushState({}, "", "/");
  });

  it("default string response", async () => {
    const response = new Response(JSON.stringify("response"));
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchPost("/test", "");

    expect(spy).toHaveBeenCalledWith("/test", {
      body: "",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/x-www-form-urlencoded",
        "X-XSRF-TOKEN": ""
      },
      method: "post"
    });
    expect(result.data).toStrictEqual("response");
  });

  it("default object response", async () => {
    const response = new Response(JSON.stringify({ test: true }));
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchPost("/test", "");

    expect(spy).toHaveBeenCalledWith("/test", {
      body: "",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/x-www-form-urlencoded",
        "X-XSRF-TOKEN": ""
      },
      method: "post"
    });
    expect(result.data).toStrictEqual({
      test: true
    });
  });

  it("should give application/json header for string", async () => {
    const response = new Response(JSON.stringify({ test: true }));
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchPost("/test", "{}", "post", { "Content-Type": "application/json" });

    expect(spy).toHaveBeenCalledWith("/test", {
      body: "{}",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        "X-XSRF-TOKEN": ""
      },
      method: "post"
    });
    expect(result.data).toStrictEqual({
      test: true
    });
  });

  it("error string response", async () => {
    const response = new Response(JSON.stringify("response"), {
      statusText: "error",
      status: 500
    });
    const mockFetchAsXml: Promise<Response> = Promise.resolve(response);
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => mockFetchAsXml);
    const result = await FetchPost("/test", "");

    expect(spy).toHaveBeenCalledWith("/test", {
      body: "",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/x-www-form-urlencoded",
        "X-XSRF-TOKEN": ""
      },
      method: "post"
    });
    expect(result.data).toBe("response");
  });

  it("bad network connection", async () => {
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => {
      throw new Error("bad connection");
    });
    const result = await FetchPost("/test", "");

    expect(spy).toHaveBeenCalledWith("/test", {
      body: "",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/x-www-form-urlencoded",
        "X-XSRF-TOKEN": ""
      },
      method: "post"
    });
    expect(result.data).toStrictEqual(null);
    expect(result.statusCode).toStrictEqual(999);
  });

  it("normalizes tenant-prefixed f and filePath in string body", async () => {
    window.history.pushState({}, "", "/main/");
    const response = new Response(JSON.stringify({ ok: true }));
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => Promise.resolve(response));

    await FetchPost(
      "/test",
      "f=%2Fmain%2F101NZ_50__nikon_raw%2FDSC_0054.JPG&filePath=%2Fmain%2F101NZ_50__nikon_raw%2FDSC_0054.JPG"
    );

    expect(spy).toHaveBeenCalledWith("/test", {
      body: "f=%2F101NZ_50__nikon_raw%2FDSC_0054.JPG&filePath=%2F101NZ_50__nikon_raw%2FDSC_0054.JPG",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/x-www-form-urlencoded",
        "X-XSRF-TOKEN": ""
      },
      method: "post"
    });
  });

  it("normalizes tenant-prefixed multi-path f in string body", async () => {
    window.history.pushState({}, "", "/main/");
    const response = new Response(JSON.stringify({ ok: true }));
    const spy = jest.spyOn(window, "fetch").mockImplementationOnce(() => Promise.resolve(response));

    await FetchPost(
      "/test",
      "f=%2Fmain%2Fa.jpg%3B%2Fmain%2Fb.jpg"
    );

    expect(spy).toHaveBeenCalledWith("/test", {
      body: "f=%2Fa.jpg%3B%2Fb.jpg",
      credentials: "include",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/x-www-form-urlencoded",
        "X-XSRF-TOKEN": ""
      },
      method: "post"
    });
  });
});
