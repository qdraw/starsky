import { globalHistory } from "@reach/router";
import { mount, ReactWrapper, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import * as FileHashImage from "../components/atoms/file-hash-image/file-hash-image";
import * as ContextDetailview from "../contexts/detailview-context";
import * as useFetch from "../hooks/use-fetch";
import * as useGestures from "../hooks/use-gestures/use-gestures";
import * as useLocation from "../hooks/use-location";
import { newIConnectionDefault } from "../interfaces/IConnectionDefault";
import {
  IDetailView,
  IRelativeObjects,
  newDetailView,
  PageType
} from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { IFileIndexItem, Orientation } from "../interfaces/IFileIndexItem";
import { UpdateRelativeObject } from "../shared/update-relative-object";
import { UrlQuery } from "../shared/url-query";
import DetailView from "./detailview";

describe("DetailView", () => {
  it("renders", () => {
    shallow(<DetailView {...newDetailView()} />);
  });

  var defaultState = {
    breadcrumb: [],
    isReadOnly: false,
    fileIndexItem: {
      fileHash: "hash",
      tags: "tags!",
      description: "description!",
      title: "title!",
      colorClass: 3,
      dateTime: "2019-09-15T17:29:59",
      lastEdited: new Date().toISOString(),
      make: "apple",
      model: "iPhone",
      aperture: 2,
      focalLength: 10,
      longitude: 1,
      latitude: 1,
      orientation: Orientation.Horizontal,
      fileName: "test.jpg",
      filePath: "/parentDirectory/test.jpg",
      parentDirectory: "/parentDirectory",
      status: IExifStatus.Ok
    } as IFileIndexItem,
    relativeObjects: {
      nextFilePath: "next",
      prevFilePath: "prev"
    } as IRelativeObjects,
    status: IExifStatus.Default,
    pageType: PageType.DetailView,
    colorClassActiveList: [],
    subPath: "/parentDirectory/test.jpg",
    dateCache: Date.now()
  } as IDetailView;

  describe("With context and test if image is loaded", () => {
    let contextProvider: any;
    let TestComponent: () => JSX.Element;
    let Component: ReactWrapper<any, Readonly<{}>>;

    // Setup mock
    beforeEach(() => {
      contextProvider = {
        dispatch: () => jest.fn(),
        state: defaultState
      };

      TestComponent = () => (
        <ContextDetailview.DetailViewContext.Provider value={contextProvider}>
          <DetailView {...newDetailView()} />
        </ContextDetailview.DetailViewContext.Provider>
      );

      // Show extra information
      globalHistory.navigate("/?details=true");

      Component = mount(<TestComponent />);
    });

    afterEach(() => {
      Component.unmount();
    });

    afterAll(() => {
      Component = mount(<></>);
      TestComponent = () => <></>;
    });

    it("test if image is loaded", () => {
      var image = Component.find(".image--default");
      act(() => {
        image.simulate("load");
      });
      expect(image.props().src).toBe(
        new UrlQuery().UrlThumbnailImageLargeOrExtraLarge(
          contextProvider.state.fileIndexItem.fileHash,
          true
        )
      );
      expect(Component.exists(".main--error")).toBeFalsy();
    });

    it("test if image is failed", () => {
      var image = Component.find(".image--default");
      image.simulate("error");
      expect(Component.exists(".main--error")).toBeTruthy();
    });

    it("check if Details exist", () => {
      expect(Component.exists(".detailview-sidebar")).toBeTruthy();
    });
  });

  describe("Nexts/Prev clicks ++ Rotation check", () => {
    let TestComponent: () => JSX.Element;

    beforeAll(() => {
      act(() => {
        globalHistory.navigate("/?details=true");
      });
    });

    // // Setup mock
    beforeEach(() => {
      defaultState.fileIndexItem.orientation = Orientation.Rotate270Cw;
      const contextProvider = {
        dispatch: () => jest.fn(),
        state: defaultState
      };

      TestComponent = () => (
        <ContextDetailview.DetailViewContext.Provider value={contextProvider}>
          <DetailView {...newDetailView()} />
        </ContextDetailview.DetailViewContext.Provider>
      );

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => {
        return newIConnectionDefault();
      });
    });

    it("Next Click (click)", () => {
      const navigateSpy = jest.fn().mockResolvedValueOnce("");

      // use as ==> import * as useLocation from '../hooks/use-location';
      var locationSpy = jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => {
          return {
            location: globalHistory.location,
            navigate: navigateSpy
          };
        });

      var detailview = mount(<TestComponent />);

      act(() => {
        detailview.find(".nextprev--next").simulate("click");
      });
      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=next", {
        replace: true
      });

      act(() => {
        detailview.unmount();
      });
      jest.spyOn(useLocation, "default").mockReset();
    });

    it("Prev Click", () => {
      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: globalHistory.location,
        navigate: navigateSpy
      };
      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      const detailview = mount(<TestComponent />);

      act(() => {
        detailview.find(".nextprev--prev").simulate("click");
      });

      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=prev", {
        replace: true
      });

      act(() => {
        detailview.unmount();
      });
    });

    it("Prev Keyboard", () => {
      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: globalHistory.location,
        navigate: navigateSpy
      };
      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockImplementationOnce(() => {
          return Promise.resolve() as any;
        });

      const detailview = mount(<TestComponent />);

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "ArrowLeft",
        shiftKey: true
      });

      act(() => {
        window.dispatchEvent(event);
      });

      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=prev", {
        replace: true
      });

      act(() => {
        detailview.unmount();
      });
    });

    it("Next Keyboard", () => {
      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: globalHistory.location,
        navigate: navigateSpy
      };
      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockImplementationOnce(() => {
          return Promise.resolve() as any;
        });

      var compontent = mount(<TestComponent />);

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "ArrowRight",
        shiftKey: true
      });

      act(() => {
        window.dispatchEvent(event);
      });

      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toBeCalledWith("/?details=true&f=next", {
        replace: true
      });
      compontent.unmount();
    });

    it("[SearchResult] Next", async () => {
      console.log("[SearchResult] Next");

      // add search query to url
      act(() => {
        globalHistory.navigate("/?t=test&p=0");
      });

      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      var locationFaker = () => {
        return {
          location: globalHistory.location,
          navigate: navigateSpy
        };
      };

      var updateRelativeObjectSpy = jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockImplementationOnce(() => {
          return Promise.resolve({
            nextFilePath: "t",
            nextHash: "t"
          } as IRelativeObjects);
        })
        .mockImplementationOnce(() => {
          return Promise.resolve({
            nextFilePath: "t",
            nextHash: "t"
          } as IRelativeObjects);
        });

      var locationSpy = jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker);

      var detailview = mount(<TestComponent />);
      var item = detailview.find(".nextprev--prev");

      await act(async () => {
        await item.simulate("click");
      });

      expect(locationSpy).toBeCalled();
      expect(navigateSpy).toBeCalled();

      // could not check values :(
      expect(updateRelativeObjectSpy).toBeCalled();

      // reset afterwards
      act(() => {
        detailview.unmount();
      });
    });

    it("Escape key Keyboard", () => {
      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: { ...globalHistory.location, search: "" },
        navigate: navigateSpy
      };

      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);
      jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockImplementationOnce(() => {
          return Promise.resolve() as any;
        });

      var component = mount(<TestComponent />);

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "Escape",
        shiftKey: true
      });
      window.dispatchEvent(event);

      expect(locationSpy).toBeCalled();

      expect(navigateSpy).toBeCalled();
      expect(navigateSpy).toHaveBeenNthCalledWith(1, "/?f=/parentDirectory", {
        state: { filePath: "/parentDirectory/test.jpg" }
      });

      component.unmount();
    });
    it("should update when swipe left", () => {
      console.log("- - - -");

      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: { ...globalHistory.location, search: "" },
        navigate: navigateSpy
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      const updateRelativeObjectSpy = jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockImplementationOnce(() => Promise.resolve() as any);

      let handlerOnSwipeLeft: Function | undefined;
      jest
        .spyOn(useGestures, "default")
        .mockImplementationOnce((_, handler) => {
          handlerOnSwipeLeft = handler.onSwipeLeft;
        })
        .mockImplementationOnce(() => {});

      const fakeElement = (
        props: React.PropsWithChildren<FileHashImage.IFileHashImageProps>
      ) => (
        <>
          <button
            onClick={() => {
              if (handlerOnSwipeLeft) {
                handlerOnSwipeLeft();
              }
            }}
            id="fake-button"
          ></button>
        </>
      );

      jest.spyOn(FileHashImage, "default").mockImplementationOnce(fakeElement);
      var component = mount(<TestComponent />);

      component.find("#fake-button").simulate("click");

      expect(updateRelativeObjectSpy).toBeCalled();

      component.unmount();
    });

    it("should update when swipe right", () => {
      console.log("- - - -");

      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: { ...globalHistory.location, search: "" },
        navigate: navigateSpy
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      const updateRelativeObjectSpy = jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockImplementationOnce(() => Promise.resolve() as any);

      let handlerOnSwipeLeft: Function | undefined;
      jest
        .spyOn(useGestures, "default")
        .mockImplementationOnce((_, handler) => {
          handlerOnSwipeLeft = handler.onSwipeRight;
        })
        .mockImplementationOnce(() => {});

      const fakeElement = (
        props: React.PropsWithChildren<FileHashImage.IFileHashImageProps>
      ) => (
        <>
          <button
            onClick={() => {
              if (handlerOnSwipeLeft) {
                handlerOnSwipeLeft();
              }
            }}
            id="fake-button"
          ></button>
        </>
      );

      jest.spyOn(FileHashImage, "default").mockImplementationOnce(fakeElement);
      var component = mount(<TestComponent />);

      component.find("#fake-button").simulate("click");

      expect(updateRelativeObjectSpy).toBeCalled();

      component.unmount();
    });
  });
});
