import { IExifTimezoneCorrectionResultContainer } from "../../../../interfaces/ITimezone";
import * as FetchPostModule from "../../../../shared/fetch/fetch-post";
import { generateTimezonePreview } from "./generate-timezone-preview";

const mockSetIsLoadingPreview = jest.fn();
const mockSetPreview = jest.fn();
const mockSetError = jest.fn();

const preview = { timezoneData: [], offsetData: [] } as IExifTimezoneCorrectionResultContainer;
const filePathList = ["/test.jpg"];
const recordedTimezoneId = "Europe/Amsterdam";
const correctTimezoneId = "Europe/London";

describe("generateTimezonePreview error cases", () => {
  let fetchPostSpy: jest.SpyInstance;
  beforeEach(() => {
    jest.clearAllMocks();
    if (fetchPostSpy) fetchPostSpy.mockRestore();
  });

  it("does nothing if filePathList is empty", async () => {
    await generateTimezonePreview(
      [],
      recordedTimezoneId,
      correctTimezoneId,
      mockSetIsLoadingPreview,
      preview,
      mockSetPreview,
      mockSetError
    );
    expect(mockSetIsLoadingPreview).not.toHaveBeenCalled();
    expect(mockSetError).not.toHaveBeenCalled();
    expect(mockSetPreview).not.toHaveBeenCalled();
  });

  it("sets error if response statusCode is not 200", async () => {
    fetchPostSpy = jest
      .spyOn(FetchPostModule, "default")
      .mockResolvedValue({ statusCode: 500, data: [] });
    await generateTimezonePreview(
      filePathList,
      recordedTimezoneId,
      correctTimezoneId,
      mockSetIsLoadingPreview,
      preview,
      mockSetPreview,
      mockSetError
    );
    expect(mockSetError).toHaveBeenCalledWith("Failed to generate preview");
    expect(mockSetPreview).not.toHaveBeenCalled();
  });

  it("sets error if response data is not an array", async () => {
    fetchPostSpy = jest
      .spyOn(FetchPostModule, "default")
      .mockResolvedValue({ statusCode: 200, data: null });
    await generateTimezonePreview(
      filePathList,
      recordedTimezoneId,
      correctTimezoneId,
      mockSetIsLoadingPreview,
      preview,
      mockSetPreview,
      mockSetError
    );
    expect(mockSetError).toHaveBeenCalledWith("Failed to generate preview");
    expect(mockSetPreview).not.toHaveBeenCalled();
  });

  it("sets error if FetchPost throws", async () => {
    fetchPostSpy = jest.spyOn(FetchPostModule, "default").mockImplementation(() => {
      throw new Error("fail");
    });
    await generateTimezonePreview(
      filePathList,
      recordedTimezoneId,
      correctTimezoneId,
      mockSetIsLoadingPreview,
      preview,
      mockSetPreview,
      mockSetError
    );
    expect(mockSetError).toHaveBeenCalledWith("Failed to generate preview");
    expect(mockSetPreview).not.toHaveBeenCalled();
  });
});
