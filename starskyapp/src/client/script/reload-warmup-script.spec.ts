describe("reload redirect", () => {
  it("...", () => {
    const mockJsonPromise = Promise.resolve("Health");
    const mockFetchPromise = Promise.resolve({
      statusCode: 200,
      text: () => {
        console.log("--t");

        return mockJsonPromise;
      }
    });
    window.fetch = jest.fn().mockImplementation(() => mockFetchPromise);

    (global.fetch as any).mockClear();
    delete global.fetch;
  });
});
