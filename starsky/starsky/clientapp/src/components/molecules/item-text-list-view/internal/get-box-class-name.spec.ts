import { IFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { IExifStatus } from "../../../../interfaces/IExifStatus";
import { GetBoxClassName } from "./get-box-class-name";

describe("GetBoxClassName function", () => {
  it("should return correct class name for directory items", () => {
    const item: IFileIndexItem = { isDirectory: true, status: null } as unknown as IFileIndexItem;
    expect(GetBoxClassName(item)).toBe("box isDirectory-true");
  });

  it("should return correct class name for items with OK status", () => {
    const item: IFileIndexItem = {
      isDirectory: false,
      status: IExifStatus.Ok
    } as unknown as IFileIndexItem;
    expect(GetBoxClassName(item)).toBe("box isDirectory-false");
  });

  it("should return correct class name for items with Default status", () => {
    const item: IFileIndexItem = {
      isDirectory: false,
      status: IExifStatus.Default
    } as unknown as IFileIndexItem;
    expect(GetBoxClassName(item)).toBe("box isDirectory-false");
  });

  it("should return correct class name for items with OKAndSame status", () => {
    const item: IFileIndexItem = {
      isDirectory: false,
      status: IExifStatus.OkAndSame
    } as unknown as IFileIndexItem;
    expect(GetBoxClassName(item)).toBe("box isDirectory-false");
  });

  it("should return correct class name for items with error status", () => {
    const item: IFileIndexItem = {
      isDirectory: false,
      status: "SomeErrorStatus"
    } as unknown as IFileIndexItem;
    expect(GetBoxClassName(item)).toBe("box isDirectory-false error");
  });
});
