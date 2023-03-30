import { IUseLocation } from "../../../hooks/use-location";
import { IDetailView, IRelativeObjects } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as moveFolderUp from "./move-folder-up";
import { PrevNext } from "./prev-next";
import { statusRemoved } from "./status-removed";

describe("statusRemoved", () => {
  it("renders", () => {
    statusRemoved(
      {} as IDetailView,
      {} as IRelativeObjects,
      true,
      {} as IUseLocation,
      jest.fn(),
      jest.fn
    );
  });

  it("should call next", () => {
    const prevNextSpy = jest
      .spyOn(PrevNext.prototype, "next")
      .mockImplementationOnce(() => {});
    const moveFolderUpSpy = jest
      .spyOn(moveFolderUp, "moveFolderUp")
      .mockImplementationOnce(() => {});

    statusRemoved(
      {
        fileIndexItem: {
          status: IExifStatus.NotFoundSourceMissing
        }
      } as IDetailView,
      { nextFilePath: "/" } as IRelativeObjects,
      true,
      {
        location: {
          search: ""
        } as Location
      } as IUseLocation,
      jest.fn(),
      jest.fn
    );
    expect(prevNextSpy).toHaveBeenCalled();
    expect(moveFolderUpSpy).toHaveBeenCalledTimes(0);
  });

  it("should call up", () => {
    const prevNextSpy = jest
      .spyOn(PrevNext.prototype, "next")
      .mockImplementationOnce(() => {});
    const moveFolderUpSpy = jest
      .spyOn(moveFolderUp, "moveFolderUp")
      .mockImplementationOnce(() => {});

    statusRemoved(
      {
        fileIndexItem: {
          status: IExifStatus.NotFoundSourceMissing
        }
      } as IDetailView,
      {} as IRelativeObjects,
      true,
      {
        location: {
          search: ""
        } as Location
      } as IUseLocation,
      jest.fn(),
      jest.fn
    );
    expect(prevNextSpy).toHaveBeenCalledTimes(0);
    expect(moveFolderUpSpy).toHaveBeenCalledTimes(1);
  });

  it("should not call up due delete", () => {
    const prevNextSpy = jest
      .spyOn(PrevNext.prototype, "next")
      .mockImplementationOnce(() => {});
    const moveFolderUpSpy = jest
      .spyOn(moveFolderUp, "moveFolderUp")
      .mockImplementationOnce(() => {});

    statusRemoved(
      {
        fileIndexItem: {
          status: IExifStatus.NotFoundSourceMissing
        }
      } as IDetailView,
      {} as IRelativeObjects,
      true,
      {
        location: {
          search: "!delete!"
        } as Location
      } as IUseLocation,
      jest.fn(),
      jest.fn
    );
    expect(prevNextSpy).toHaveBeenCalledTimes(0);
    expect(moveFolderUpSpy).toHaveBeenCalledTimes(0);
  });

  it("should tigger none", () => {
    const prevNextSpy = jest
      .spyOn(PrevNext.prototype, "next")
      .mockImplementationOnce(() => {});
    const moveFolderUpSpy = jest
      .spyOn(moveFolderUp, "moveFolderUp")
      .mockImplementationOnce(() => {});

    statusRemoved(
      {
        fileIndexItem: {
          status: IExifStatus.Ok
        }
      } as IDetailView,
      { nextFilePath: "/" } as IRelativeObjects,
      true,
      {
        location: {
          search: ""
        } as Location
      } as IUseLocation,
      jest.fn(),
      jest.fn
    );
    expect(prevNextSpy).toHaveBeenCalledTimes(0);
    expect(moveFolderUpSpy).toHaveBeenCalledTimes(0);
  });

  it("skip when undefined", () => {
    const prevNextSpy = jest
      .spyOn(PrevNext.prototype, "next")
      .mockImplementationOnce(() => {});
    const moveFolderUpSpy = jest
      .spyOn(moveFolderUp, "moveFolderUp")
      .mockImplementationOnce(() => {});

    statusRemoved(
      {
        fileIndexItem: {
          status: undefined
        } as unknown as IFileIndexItem
      } as IDetailView,
      {} as IRelativeObjects,
      true,
      {} as IUseLocation,
      jest.fn(),
      jest.fn
    );
    expect(prevNextSpy).toHaveBeenCalledTimes(0);
    expect(moveFolderUpSpy).toHaveBeenCalledTimes(0);
  });
});
