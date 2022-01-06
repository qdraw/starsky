import { SortType } from "../interfaces/IArchive";
import { IFileIndexItem, ImageFormat } from "../interfaces/IFileIndexItem";
import { sorter } from "./sorter";

describe("sorter", () => {
  it("sort on non valid null", () => {
    const list = [
      {
        fileName: "b"
      } as IFileIndexItem,
      {
        fileName: "a"
      } as IFileIndexItem
    ] as IFileIndexItem[];

    const resultList = sorter(list, null as any);

    expect(resultList.length).toBe(0);
  });

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

  it("sort on imageFormat when is undefined", () => {
    const list = [
      {
        fileName: "b"
      } as IFileIndexItem,
      {
        fileName: "a"
      } as IFileIndexItem
    ] as IFileIndexItem[];

    const resultList = sorter(list, SortType.imageFormat);

    expect(resultList[0].fileName).toBe("b");
    expect(resultList[1].fileName).toBe("a");
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

    expect(resultList[0].fileName).toBe("b");
    expect(resultList[1].fileName).toBe("a");
  });

  it("sort on imageFormat 2 same type", () => {
    const list = [
      {
        fileName: "a",
        imageFormat: ImageFormat.bmp
      } as IFileIndexItem,
      {
        fileName: "b",
        imageFormat: ImageFormat.bmp
      } as IFileIndexItem
    ] as IFileIndexItem[];

    const resultList = sorter(list, SortType.imageFormat);

    expect(resultList[0].fileName).toBe("a");
    expect(resultList[1].fileName).toBe("b");
  });

  it("sort on imageFormat 2 same type", () => {
    const list = [
      {
        fileName: "b",
        imageFormat: ImageFormat.unknown
      } as IFileIndexItem,
      {
        fileName: "a",
        imageFormat: ImageFormat.unknown
      } as IFileIndexItem,

    ] as IFileIndexItem[];

    const resultList = sorter(list, SortType.imageFormat);

    expect(resultList[0].fileName).toBe("a");
    expect(resultList[1].fileName).toBe("b");
  });

  it("sort on imageFormat, example 2", () => {
    const list = [
      {
        fileName: "a",
        imageFormat: ImageFormat.mp4
      },
      {
        fileName: "a",
        imageFormat: ImageFormat.gpx
      },
      {
        fileName: "b",
        imageFormat: ImageFormat.xmp
      } as IFileIndexItem,
      {
        fileName: "a",
        imageFormat: ImageFormat.gif
      },
      {
        fileName: "a",
        imageFormat: ImageFormat.bmp
      },

      {
        fileName: "a",
        imageFormat: ImageFormat.tiff
      },
      {
        fileName: "a",
        imageFormat: ImageFormat.notfound
      },
      {
        fileName: "a",
        imageFormat: ImageFormat.png
      },
      {
        fileName: "a",
        imageFormat: ImageFormat.unknown
      },
      {
        fileName: "a",
        imageFormat: ImageFormat.jpg
      } as IFileIndexItem,
      {
        fileName: "a",
        imageFormat: undefined
      } as IFileIndexItem
    ] as IFileIndexItem[];
    //

    const resultList = sorter(list, SortType.imageFormat);

    expect(resultList[0].imageFormat).toBe(ImageFormat.notfound);
    expect(resultList[1].imageFormat).toBe(ImageFormat.unknown);
    expect(resultList[2].imageFormat).toBe(ImageFormat.unknown); // undefined
    expect(resultList[3].imageFormat).toBe(ImageFormat.jpg);
    expect(resultList[4].imageFormat).toBe(ImageFormat.tiff);
    expect(resultList[5].imageFormat).toBe(ImageFormat.bmp);
    expect(resultList[6].imageFormat).toBe(ImageFormat.gif);
    expect(resultList[7].imageFormat).toBe(ImageFormat.png);
    expect(resultList[8].imageFormat).toBe(ImageFormat.xmp);
    expect(resultList[9].imageFormat).toBe(ImageFormat.gpx);
    expect(resultList[10].imageFormat).toBe(ImageFormat.mp4);
  });
});
