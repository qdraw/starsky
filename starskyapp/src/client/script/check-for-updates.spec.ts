import { checkForUpdates } from "./check-for-updates";

describe("reload redirect", () => {
  function mockFetch(status: number) {
    const mockFetchPromise = Promise.resolve({
      status: status
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

  it("status success", async () => {
    mockFetch(200);
    const result = await checkForUpdates("t", "1");
    expect(result).toBe(200);
  });

  it("status upgrade", async () => {
    mockFetch(400);
    document.body.innerHTML =
      "<div class='upgrade'></div><div class='preloader'></div>";
    const result = await checkForUpdates("t", "1");

    expect(
      (document.querySelector(".upgrade") as HTMLElement).style.display
    ).toBe("block");

    document.body.innerHTML = "";
    expect(result).toBe(400);
  });

  it("non valid status", async () => {
    mockFetch(500);
    jest.spyOn(window, "alert").mockImplementationOnce(() => {});

    let errorMessage = null;
    try {
      await checkForUpdates("t", "1");
    } catch (error) {
      errorMessage = error;
    }
    expect(errorMessage).toBeUndefined();
  });

  it("reject from api", async () => {
    mockFetchReject();
    jest.spyOn(window, "alert").mockImplementationOnce(() => {});

    let errorMessage = null;
    try {
      await checkForUpdates("t", "1");
    } catch (error) {
      errorMessage = error;
    }
    expect(errorMessage).toBeUndefined();
  });
});
