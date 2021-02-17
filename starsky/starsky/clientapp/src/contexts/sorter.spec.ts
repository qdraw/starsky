import { SortType } from "../interfaces/IArchive";
import { IFileIndexItem, ImageFormat } from "../interfaces/IFileIndexItem";
import { sorter } from "./sorter";

describe("sorter", () => {
  it("sort on filename", () => {
    const list = [
      {
        fileName: "b"
      } as IFileIndexItem,
      {
        fileName: "a"
      } as IFileIndexItem
    ] as IFileIndexItem[];

    const resultList = sorter(list, SortType.fileName);

    expect(resultList[0].fileName).toBe("a");
    expect(resultList[1].fileName).toBe("b");
  });

  it("sort on imageFormat", () => {
    const list = [
      {
        fileName: "b",
        imageFormat: ImageFormat.bmp
      } as IFileIndexItem,
      {
        fileName: "a",
        imageFormat: ImageFormat.png
      } as IFileIndexItem
    ] as IFileIndexItem[];
    //

    const resultList = sorter(list, SortType.imageFormat);

    expect(resultList[0].fileName).toBe("a");
    expect(resultList[1].fileName).toBe("b");
  });

  it("sort on imageFormat, example 2", () => {
    const list = [
      {
        fileName: "a",
        imageFormat: ImageFormat.mp4
      } as IFileIndexItem,
      {
        fileName: "b",
        imageFormat: ImageFormat.xmp
      } as IFileIndexItem
    ] as IFileIndexItem[];
    //

    const resultList = sorter(list, SortType.imageFormat);

    expect(resultList[0].fileName).toBe("a");
    expect(resultList[1].fileName).toBe("b");
  });
});
