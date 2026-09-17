import * as FetchPostModule from "./fetch/fetch-post";
import { moveDragAndDropFiles } from "./move-file-helper";

jest.mock("./fetch/fetch-post");

const mockFetchPost = FetchPostModule.default as jest.MockedFunction<typeof FetchPostModule.default>;

describe("moveDragAndDropFiles", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockFetchPost.mockResolvedValue({ statusCode: 200, data: [{ status: "Ok" }] } as any);
  });

  it("returns false when no paths given", async () => {
    const result = await moveDragAndDropFiles([], [], "/target");
    expect(result).toBe(false);
    expect(mockFetchPost).not.toHaveBeenCalled();
  });

  it("sends files to target folder path directly", async () => {
    const result = await moveDragAndDropFiles(["/photos/a.jpg", "/photos/b.jpg"], [], "/target");
    expect(result).toBe(true);
    const body = new URLSearchParams(mockFetchPost.mock.calls[0][1] as string);
    expect(body.get("f")).toBe("/photos/a.jpg;/photos/b.jpg");
    expect(body.get("to")).toBe("/target;/target");
  });

  it("appends folder name to target for folder moves", async () => {
    const result = await moveDragAndDropFiles([], ["/photos/vacation"], "/target");
    expect(result).toBe(true);
    const body = new URLSearchParams(mockFetchPost.mock.calls[0][1] as string);
    expect(body.get("f")).toBe("/photos/vacation");
    expect(body.get("to")).toBe("/target/vacation");
  });

  it("handles root target for folder moves", async () => {
    await moveDragAndDropFiles([], ["/photos/vacation"], "/");
    const body = new URLSearchParams(mockFetchPost.mock.calls[0][1] as string);
    expect(body.get("to")).toBe("/vacation");
  });

  it("mixes files and folders correctly", async () => {
    await moveDragAndDropFiles(["/photos/a.jpg"], ["/photos/sub"], "/dest");
    const body = new URLSearchParams(mockFetchPost.mock.calls[0][1] as string);
    expect(body.get("f")).toBe("/photos/a.jpg;/photos/sub");
    expect(body.get("to")).toBe("/dest;/dest/sub");
  });

  it("returns false on non-200 response", async () => {
    mockFetchPost.mockResolvedValue({ statusCode: 500, data: [] } as any);
    const result = await moveDragAndDropFiles(["/photos/a.jpg"], [], "/target");
    expect(result).toBe(false);
  });

  it("trims trailing slashes from target", async () => {
    await moveDragAndDropFiles([], ["/photos/vacation"], "/target/");
    const body = new URLSearchParams(mockFetchPost.mock.calls[0][1] as string);
    expect(body.get("to")).toBe("/target/vacation");
  });
});
