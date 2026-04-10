import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import * as FetchPost from "../../../../shared/fetch/fetch-post";
import { loadRenamePreview, LoadRenamePreviewParams } from "./load-rename-preview";

describe("loadRenamePreview error cases", () => {
  const mockState = {
    fileIndexItems: [{ fileName: "test.jpg", filePath: "/test.jpg" }]
  } as unknown as IArchiveProps;
  const select = ["/test.jpg"];
  const collections = true;
  let setIsLoadingRename: jest.Mock;
  let setRenamePreview: jest.Mock;
  let setRenameError: jest.Mock;

  beforeEach(() => {
    setIsLoadingRename = jest.fn();
    setRenamePreview = jest.fn();
    setRenameError = jest.fn();
  });

  it("should set error if mode is invalid", async () => {
    await loadRenamePreview({
      mode: "offset",
      select,
      state: mockState,
      collections,
      setIsLoadingRename,
      setRenamePreview,
      setRenameError
    } as LoadRenamePreviewParams);
    expect(setRenameError).toHaveBeenCalledWith("Invalid mode or missing data");
    expect(setIsLoadingRename).toHaveBeenLastCalledWith(false);
  });

  it("should set error if FetchPost fails (non-200)", async () => {
    jest.spyOn(FetchPost, "default").mockResolvedValue({ statusCode: 500, data: null });
    await loadRenamePreview({
      mode: "offset",
      select,
      state: mockState,
      collections,
      offsetData: { year: 0, month: 0, day: 0, hour: 0, minute: 0, second: 0 },
      setIsLoadingRename,
      setRenamePreview,
      setRenameError
    } as LoadRenamePreviewParams);
    expect(setRenameError).toHaveBeenCalledWith("Failed to load preview");
    expect(setIsLoadingRename).toHaveBeenLastCalledWith(false);
  });

  it("should set error if FetchPost throws", async () => {
    jest.spyOn(FetchPost, "default").mockImplementation(() => {
      throw new Error("fail");
    });
    await loadRenamePreview({
      mode: "offset",
      select,
      state: mockState,
      collections,
      offsetData: { year: 0, month: 0, day: 0, hour: 0, minute: 0, second: 0 },
      setIsLoadingRename,
      setRenamePreview,
      setRenameError
    } as LoadRenamePreviewParams);
    expect(setRenameError).toHaveBeenCalledWith("Failed to load preview");
    expect(setIsLoadingRename).toHaveBeenLastCalledWith(false);
  });

  it("should set error if FetchPost returns no data", async () => {
    jest.spyOn(FetchPost, "default").mockResolvedValue({ statusCode: 200, data: null });
    await loadRenamePreview({
      mode: "offset",
      select,
      state: mockState,
      collections,
      offsetData: { year: 0, month: 0, day: 0, hour: 0, minute: 0, second: 0 },
      setIsLoadingRename,
      setRenamePreview,
      setRenameError
    });
    expect(setRenameError).toHaveBeenCalledWith("Failed to load preview");
    expect(setIsLoadingRename).toHaveBeenLastCalledWith(false);
  });

  it("should set error if timezone mode but missing timezoneData", async () => {
    await loadRenamePreview({
      mode: "timezone",
      select,
      state: mockState,
      collections,
      setIsLoadingRename,
      setRenamePreview,
      setRenameError
    } as LoadRenamePreviewParams);
    expect(setRenameError).toHaveBeenCalledWith("Invalid mode or missing data");
    expect(setIsLoadingRename).toHaveBeenLastCalledWith(false);
  });
});
