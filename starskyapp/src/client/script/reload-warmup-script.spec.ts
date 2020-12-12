import { warmupScript } from "./reload-warmup-script";

describe("reload redirect", () => {
  function mockFetch(status: number) {
    const mockFetchPromise = Promise.resolve({
      status: status,
      text: () => Promise.resolve("Health")
    });
    window.fetch = jest.fn().mockImplementation(() => mockFetchPromise);
  }

  function mockFetchReject() {
    const mockFetchPromise = Promise.reject();
    window.fetch = jest.fn().mockImplementation(() => mockFetchPromise);
  }

  afterEach(() => {
    (global.fetch as any).mockClear();
    delete global.fetch;
  });

  it("should success", (done) => {
    mockFetch(200);

    warmupScript("", 0, 2, (result: boolean) => {
      expect(result).toBeTruthy();
      done();
    });
  });

  it("should fail due status", (done) => {
    mockFetch(500);

    warmupScript("", 0, 0, (result: boolean) => {
      expect(result).toBeFalsy();
      done();
    });
  });

  it("should fail due rejection", (done) => {
    mockFetchReject();

    warmupScript("", 0, 0, (result: boolean) => {
      expect(result).toBeFalsy();
      done();
    });
  });
});
