import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView, IRelativeObjects } from "../../../interfaces/IDetailView";
import { UpdateRelativeObject } from "../../../shared/update-relative-object";
import { PrevNext } from "./prev-next";

describe("statusRemoved", () => {
  it("renders", () => {
    new PrevNext(
      {} as IRelativeObjects,
      {} as IDetailView,
      true,
      {} as IUseLocation,
      jest.fn(),
      jest.fn
    ).next();
  });

  it("not called", () => {
    const history = { location: {} } as IUseLocation;
    history.navigate = jest.fn();

    new PrevNext(
      {} as IRelativeObjects,
      {} as IDetailView,
      true,
      history,
      jest.fn(),
      jest.fn
    ).next();

    expect(history.navigate).toHaveBeenCalledTimes(0);
  });

  it("not called 2", () => {
    const history = { location: {} } as IUseLocation;
    const navigate = jest.fn();
    history.navigate = () => Promise.resolve(navigate) as any;

    const setIsLoading = jest.fn();

    jest.spyOn(UpdateRelativeObject.prototype, "Update").mockImplementationOnce(() => {
      return Promise.resolve({
        nextFilePath: "test",
        nextHash: "test"
      } as IRelativeObjects);
    });

    new PrevNext(
      {
        nextFilePath: "test",
        nextHash: "test"
      } as IRelativeObjects,
      {
        fileIndexItem: {
          fileHash: "test"
        }
      } as IDetailView,
      true,
      history,
      setIsLoading,
      jest.fn
    ).next();

    expect(setIsLoading).toHaveBeenCalledTimes(0);
  });

  it("called 3", () => {
    const history = { location: {} } as IUseLocation;
    const navigate = jest.fn();
    history.navigate = () => Promise.resolve(navigate) as any;

    const setIsLoading = jest.fn();

    const updateSpy = jest
      .spyOn(UpdateRelativeObject.prototype, "Update")
      .mockImplementationOnce(() => {
        return Promise.resolve({
          nextFilePath: "test1",
          nextHash: "test"
        } as IRelativeObjects);
      });

    new PrevNext(
      {
        nextFilePath: "test",
        nextHash: "test"
      } as IRelativeObjects,
      {
        fileIndexItem: {
          fileHash: "test"
        },
        subPath: "test"
      } as IDetailView,
      true,
      history,
      setIsLoading,
      jest.fn
    ).next();

    expect(setIsLoading).toHaveBeenCalledTimes(0);
    expect(updateSpy).toHaveBeenCalledTimes(1);
  });

  it("called 4", () => {
    const history = { location: {} } as IUseLocation;
    const navigate = jest.fn();
    history.navigate = () => Promise.resolve(navigate) as any;

    const setIsLoading = jest.fn();

    jest.spyOn(UpdateRelativeObject.prototype, "Update").mockReset();

    const updateSpy = jest
      .spyOn(UpdateRelativeObject.prototype, "Update")
      .mockImplementationOnce(() => {
        return Promise.resolve({
          prevFilePath: "test1",
          prevHash: "test"
        } as IRelativeObjects);
      });

    new PrevNext(
      {
        prevFilePath: "test",
        prevHash: "test"
      } as IRelativeObjects,
      {
        fileIndexItem: {
          fileHash: "test"
        },
        subPath: "test"
      } as IDetailView,
      true,
      history,
      setIsLoading,
      jest.fn
    ).prev();

    expect(setIsLoading).toHaveBeenCalledTimes(0);
    expect(updateSpy).toHaveBeenCalledTimes(1);
  });

  it("called 5", () => {
    const history = { location: {} } as IUseLocation;
    const navigate = jest.fn();
    history.navigate = () => Promise.resolve(navigate) as any;

    const setIsLoading = jest.fn();

    jest.spyOn(UpdateRelativeObject.prototype, "Update").mockReset();

    const updateSpy = jest
      .spyOn(UpdateRelativeObject.prototype, "Update")
      .mockImplementationOnce(() => {
        return Promise.resolve({
          prevFilePath: "test1",
          prevHash: "test"
        } as IRelativeObjects);
      });

    new PrevNext(
      {} as IRelativeObjects,
      {
        fileIndexItem: {
          fileHash: "test"
        },
        subPath: "test"
      } as IDetailView,
      true,
      history,
      setIsLoading,
      jest.fn
    ).prev();

    expect(setIsLoading).toHaveBeenCalledTimes(0);
    expect(updateSpy).toHaveBeenCalledTimes(0);
  });

  it("called 6", () => {
    const history = { location: {} } as IUseLocation;
    const navigate = jest.fn();
    history.navigate = () => Promise.resolve(navigate) as any;

    const setIsLoading = jest.fn();

    const updateSpy = jest
      .spyOn(UpdateRelativeObject.prototype, "Update")
      .mockImplementationOnce(() => {
        return Promise.resolve({
          prevFilePath: "test1",
          prevHash: "test"
        } as IRelativeObjects);
      });

    new PrevNext(
      {} as IRelativeObjects,
      {
        fileIndexItem: {
          fileHash: "test"
        },
        subPath: "test"
      } as IDetailView,
      true,
      history,
      setIsLoading,
      jest.fn
    ).next();

    expect(setIsLoading).toHaveBeenCalledTimes(0);
    expect(updateSpy).toHaveBeenCalledTimes(0);
  });

  it("navigatePrev same so ignore", () => {
    console.log("navigatePrev same so ignore");

    const relative = {
      prevFilePath: "test",
      prevHash: "test"
    } as IRelativeObjects;

    const history = { location: {} } as IUseLocation;
    const navigate = jest.fn();
    history.navigate = () => Promise.resolve(navigate) as any;

    const setIsLoading = jest.fn();

    new PrevNext(
      relative,
      {
        fileIndexItem: {
          fileHash: "test"
        }
      } as IDetailView,
      true,
      history,
      jest.fn(),
      setIsLoading
    ).navigatePrev(relative);

    expect(navigate).toHaveBeenCalledTimes(0);
    expect(setIsLoading).toHaveBeenCalledTimes(1);
    expect(setIsLoading).toHaveBeenCalledWith(false);
  });

  it("nextPrev same so ignore", () => {
    const relative = {
      nextFilePath: "test",
      nextHash: "test"
    } as IRelativeObjects;

    const history = { location: {} } as IUseLocation;
    const navigate = jest.fn();
    history.navigate = () => Promise.resolve(navigate) as any;

    const setIsLoading = jest.fn();

    new PrevNext(
      relative,
      {
        fileIndexItem: {
          fileHash: "test"
        }
      } as IDetailView,
      true,
      history,
      jest.fn(),
      setIsLoading
    ).navigateNext(relative);

    expect(navigate).toHaveBeenCalledTimes(0);
    expect(setIsLoading).toHaveBeenCalledTimes(1);
    expect(setIsLoading).toHaveBeenCalledWith(false);
  });
});
