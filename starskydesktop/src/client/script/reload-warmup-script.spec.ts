/* eslint-disable jest/no-done-callback */
import { IPreloadApi } from "../../preload/IPreloadApi";
import { warmupScript } from "./reload-warmup-script";

declare global {
  // eslint-disable-next-line vars-on-top, no-var
  var api: IPreloadApi;
}

describe("reload redirect", () => {
  function mockFetch(status: number) {
    const mockFetchPromise = Promise.resolve({
      status,
      text: () => Promise.resolve("Health")
    });
    window.fetch = jest.fn().mockImplementation(() => mockFetchPromise);
  }

  function mockFetchReject() {
    const mockFetchPromise = Promise.reject();
    window.fetch = jest.fn().mockImplementation(() => mockFetchPromise);
  }

  afterEach(() => {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
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
      console.log(result);
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
