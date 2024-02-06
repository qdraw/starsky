import { fireEvent, render, RenderResult } from "@testing-library/react";
import { useState } from "react";
import { act } from "react-dom/test-utils";
import { BrowserRouter } from "react-router-dom";
import * as FileHashImage from "../../components/atoms/file-hash-image/file-hash-image";
import { IFileHashImageProps } from "../../components/atoms/file-hash-image/file-hash-image";
import * as Link from "../../components/atoms/link/link";
import * as MenuDetailView from "../../components/organisms/menu-detail-view/menu-detail-view";
import * as ContextDetailview from "../../contexts/detailview-context";
import * as useFetch from "../../hooks/use-fetch";
import * as useGestures from "../../hooks/use-gestures/use-gestures";
import * as useLocation from "../../hooks/use-location/use-location";
import { newIConnectionDefault } from "../../interfaces/IConnectionDefault";
import {
  IDetailView,
  IRelativeObjects,
  newDetailView,
  PageType
} from "../../interfaces/IDetailView";
import { IExifStatus } from "../../interfaces/IExifStatus";
import { IFileIndexItem, Orientation } from "../../interfaces/IFileIndexItem";
import { Router } from "../../router-app/router-app";
import { UpdateRelativeObject } from "../../shared/update-relative-object";
import { UrlQuery } from "../../shared/url-query";
import DetailView from "./detailview";

describe("DetailView", () => {
  const fileHashImageMock = (props: IFileHashImageProps) => {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    const [isError, setIsError] = useState(false);
    return (
      <div data-test="pan-zoom-image" className={isError ? "main--error" : undefined}>
        <img
          src={
            "http://localhost" +
            new UrlQuery().UrlThumbnailImageLargeOrExtraLarge(props.fileHash, props.id, true)
          }
          onError={() => setIsError(true)}
        />
      </div>
    );
  };

  beforeEach(() => {
    jest
      .spyOn(FileHashImage, "default")
      .mockImplementationOnce((props) => fileHashImageMock(props));
  });

  it("renders", () => {
    jest
      .spyOn(FileHashImage, "default")
      .mockImplementationOnce((props) => fileHashImageMock(props));
    render(<DetailView {...newDetailView()} />);
  });

  const defaultState = {
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
    let Component: RenderResult;

    // Setup mock
    beforeEach(() => {
      contextProvider = {
        dispatch: () => jest.fn(),
        state: defaultState
      };

      TestComponent = () => (
        <BrowserRouter>
          <ContextDetailview.DetailViewContext.Provider value={contextProvider}>
            <DetailView {...newDetailView()} />
          </ContextDetailview.DetailViewContext.Provider>
        </BrowserRouter>
      );

      // Show extra information
      Router.navigate("/?details=true");

      Component = render(<TestComponent />);
    });

    afterEach(() => {
      Component.unmount();
    });

    afterAll(() => {
      Component = render(<></>);
      TestComponent = () => <></>;
    });

    it("test if image is loaded", () => {
      const imgContainer = Component.queryByTestId("pan-zoom-image") as HTMLDivElement;
      expect(imgContainer).toBeTruthy();

      const image = imgContainer?.querySelector("img") as HTMLImageElement;
      expect(image).toBeTruthy();

      expect(image.src).toBe(
        "http://localhost" +
          new UrlQuery().UrlThumbnailImageLargeOrExtraLarge(
            contextProvider.state.fileIndexItem.fileHash,
            contextProvider.state.fileIndexItem.filePath,
            true
          )
      );

      const mainError = Component.container.querySelector(".main--error");
      expect(mainError).toBeFalsy();
    });

    it("test if image is failed", () => {
      const imgContainer = Component.queryByTestId("pan-zoom-image") as HTMLDivElement;
      expect(imgContainer).toBeTruthy();

      const image = imgContainer?.querySelector("img") as HTMLImageElement;
      expect(image).toBeTruthy();

      fireEvent.error(image);

      const mainError = Component.container.querySelector(".main--error");
      expect(mainError).toBeTruthy();
    });

    it("check if Details exist", () => {
      expect(Component.queryByTestId("detailview-sidebar") as HTMLDivElement).toBeTruthy();
    });
  });

  describe("Next's/Prev clicks ++ Rotation check", () => {
    let TestComponent: () => JSX.Element;

    beforeAll(() => {
      act(() => {
        Router.navigate("/?details=true");
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
        <BrowserRouter>
          <ContextDetailview.DetailViewContext.Provider value={contextProvider}>
            <DetailView {...newDetailView()} />
          </ContextDetailview.DetailViewContext.Provider>
        </BrowserRouter>
      );

      // usage ==> import * as useFetch from '../hooks/use-fetch';
      jest.spyOn(useFetch, "default").mockImplementationOnce(() => newIConnectionDefault());
    });

    it("Next Click (click)", () => {
      jest.spyOn(MenuDetailView, "default").mockImplementationOnce(() => <></>);
      jest.spyOn(FileHashImage, "default").mockImplementationOnce(() => <></>);

      const navigateSpy = jest.fn().mockResolvedValueOnce("");

      const locationSpyData = {
        location: {
          ...Router.state.location,
          href: "",
          search: ""
        },
        navigate: navigateSpy
      };

      jest
        .spyOn(useFetch, "default")
        .mockReset()
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault())
        .mockImplementationOnce(() => newIConnectionDefault());

      // use as ==> import * as useLocation from '../hooks/use-location/use-location';
      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockReset()
        .mockImplementationOnce(() => locationSpyData)
        .mockImplementationOnce(() => locationSpyData)
        .mockImplementationOnce(() => locationSpyData)
        .mockImplementationOnce(() => locationSpyData)
        .mockImplementationOnce(() => locationSpyData);

      const detailview = render(<TestComponent />);

      const next = detailview.queryByTestId("detailview-next") as HTMLDivElement;

      expect(next).toBeTruthy();

      act(() => {
        next.click();
      });

      expect(locationSpy).toHaveBeenCalled();

      expect(navigateSpy).toHaveBeenCalled();
      expect(navigateSpy).toHaveBeenCalledWith("/?f=next", {
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
        location: {
          ...Router.state.location,
          href: "",
          search: ""
        },
        navigate: navigateSpy
      };
      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockReset()
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      const detailview = render(<TestComponent />);

      const prev = detailview.queryByTestId("detailview-prev") as HTMLDivElement;

      expect(prev).toBeTruthy();

      act(() => {
        prev.click();
      });

      expect(locationSpy).toHaveBeenCalled();

      expect(navigateSpy).toHaveBeenCalled();
      expect(navigateSpy).toHaveBeenCalledWith("/?f=prev", {
        replace: true
      });

      act(() => {
        detailview.unmount();
      });
    });

    it("Prev Keyboard", () => {
      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      jest.spyOn(FileHashImage, "default").mockImplementationOnce(() => <></>);

      const locationObject = {
        location: {
          ...Router.state.location,
          href: "",
          search: ""
        },
        navigate: navigateSpy
      };
      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockReset()
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      jest.spyOn(UpdateRelativeObject.prototype, "Update").mockImplementationOnce(() => {
        return Promise.resolve() as any;
      });

      const detailview = render(<TestComponent />);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "ArrowLeft",
        shiftKey: true
      });

      act(() => {
        window.dispatchEvent(event);
      });

      expect(locationSpy).toHaveBeenCalled();

      expect(navigateSpy).toHaveBeenCalled();
      expect(navigateSpy).toHaveBeenCalledWith("/?f=prev", {
        replace: true
      });

      act(() => {
        detailview.unmount();
      });
    });

    it("Next Keyboard", () => {
      jest.spyOn(MenuDetailView, "default").mockImplementationOnce(() => <></>);
      jest.spyOn(FileHashImage, "default").mockImplementationOnce(() => <></>);

      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: { ...window.location, search: "" },
        navigate: navigateSpy
      };
      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockReset()
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve() as any;
        });

      const component = render(<TestComponent />);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "ArrowRight",
        shiftKey: true
      });

      act(() => {
        window.dispatchEvent(event);
      });

      expect(locationSpy).toHaveBeenCalled();

      expect(navigateSpy).toHaveBeenCalled();
      expect(navigateSpy).toHaveBeenCalledWith("/?f=next", {
        replace: true
      });
      component.unmount();
    });

    it("[SearchResult] Next", async () => {
      console.log("[SearchResult] Next");
      jest.spyOn(FileHashImage, "default").mockImplementationOnce(() => <></>);

      // add search query to url
      act(() => {
        Router.navigate("/?t=test&p=0");
      });

      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationFaker = () => {
        return {
          location: window.location,
          navigate: navigateSpy
        };
      };

      const updateRelativeObjectSpy = jest
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

      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockReset()
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker)
        .mockImplementationOnce(locationFaker);

      jest.spyOn(Link, "default").mockImplementationOnce(() => <></>);

      const detailview = render(<TestComponent />);

      const prev = detailview.queryByTestId("detailview-prev") as HTMLDivElement;
      expect(prev).toBeTruthy();
      act(() => {
        prev.click();
      });

      expect(locationSpy).toHaveBeenCalled();
      expect(navigateSpy).toHaveBeenCalled();

      // could not check values :(
      expect(updateRelativeObjectSpy).toHaveBeenCalled();

      // reset afterwards
      act(() => {
        detailview.unmount();
      });
    });

    it("Escape key Keyboard", () => {
      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: { ...window.location, search: "" },
        navigate: navigateSpy
      };

      const locationSpy = jest
        .spyOn(useLocation, "default")
        .mockReset()
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);
      jest.spyOn(UpdateRelativeObject.prototype, "Update").mockImplementationOnce(() => {
        return Promise.resolve() as any;
      });

      const component = render(<TestComponent />);

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "Escape",
        shiftKey: true
      });
      window.dispatchEvent(event);

      expect(locationSpy).toHaveBeenCalled();

      expect(navigateSpy).toHaveBeenCalled();
      expect(navigateSpy).toHaveBeenNthCalledWith(1, "/?f=/parentDirectory", {
        state: { filePath: "/parentDirectory/test.jpg" }
      });

      component.unmount();
    });

    it("should update when swipe left", () => {
      console.log("- - - -");

      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: { ...window.location, search: "" },
        navigate: navigateSpy
      };

      jest
        .spyOn(useLocation, "default")
        .mockReset()
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      const updateRelativeObjectSpy = jest
        .spyOn(UpdateRelativeObject.prototype, "Update")
        .mockImplementationOnce(() => Promise.resolve() as any);

      let handlerOnSwipeLeft: Function | undefined;
      jest
        .spyOn(useGestures, "useGestures")
        .mockImplementationOnce((_, handler) => {
          handlerOnSwipeLeft = handler.onSwipeLeft;
        })
        .mockImplementationOnce(() => {});

      const fakeElement = (
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        _props: React.PropsWithChildren<FileHashImage.IFileHashImageProps>
      ) => (
        <>
          <button
            onClick={() => {
              if (handlerOnSwipeLeft) {
                handlerOnSwipeLeft();
              }
            }}
            data-test="fake-button"
            id="fake-button"
          ></button>
        </>
      );

      jest.spyOn(FileHashImage, "default").mockReset().mockImplementationOnce(fakeElement);

      const component = render(<TestComponent />);

      (component.queryByTestId("fake-button") as HTMLButtonElement).click();

      expect(updateRelativeObjectSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("should update when swipe right", () => {
      console.log("- - - -");

      const navigateSpy = jest.fn().mockResolvedValueOnce("");
      const locationObject = {
        location: { ...window.location, search: "" },
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
        .spyOn(useGestures, "useGestures")
        .mockImplementationOnce((_, handler) => {
          handlerOnSwipeLeft = handler.onSwipeRight;
        })
        .mockImplementationOnce(() => {});

      const fakeElement = (
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        _props: React.PropsWithChildren<FileHashImage.IFileHashImageProps>
      ) => (
        <button
          onClick={() => {
            if (handlerOnSwipeLeft) {
              handlerOnSwipeLeft();
            }
          }}
          id="fake-button"
          data-test="fake-button"
        ></button>
      );

      jest.spyOn(FileHashImage, "default").mockReset().mockImplementationOnce(fakeElement);

      const component = render(<TestComponent />);

      (component.queryByTestId("fake-button") as HTMLButtonElement).click();

      expect(updateRelativeObjectSpy).toHaveBeenCalled();

      component.unmount();
    });
  });
});
