import { render } from "@testing-library/react";
import ModalGeo, { addDefaultMarker, getZoom, ILatLong } from "./modal-geo";

describe("ModalGeo", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalGeo
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
        latitude={51}
        longitude={3}
        isFormEnabled={false}
      ></ModalGeo>
    );
  });

  describe("getZoom", () => {
    it("getZoom none", () => {
      const result = getZoom({} as ILatLong);
      expect(result).toBe(12);
    });

    it("getZoom with location", () => {
      const result = getZoom({ latitude: 10, longitude: 15 } as ILatLong);
      expect(result).toBe(15);
    });
  });

  describe("addDefaultMarker", () => {
    it("no location so not called", () => {
      const map = {
        addLayer: jest.fn()
      } as unknown as L.Map;
      addDefaultMarker({} as ILatLong, map, true, jest.fn(), jest.fn());

      expect(map.addLayer).toBeCalledTimes(0);
    });

    it("location so called", () => {
      const map = {
        addLayer: jest.fn()
      } as unknown as L.Map;
      addDefaultMarker(
        { latitude: 10, longitude: 15 } as ILatLong,
        map,
        true,
        jest.fn(),
        jest.fn()
      );

      expect(map.addLayer).toBeCalledTimes(1);
    });
  });

  // it("should fetchPost and dispatch", async () => {
  //   const fetchSpy = jest
  //     .spyOn(FetchPost, "default")
  //     .mockImplementationOnce(async () => {
  //       return { statusCode: 200 } as IConnectionDefault;
  //     });

  //   const dispatch = jest.fn();
  //   const modal = render(
  //     <ModalForceDelete
  //       isOpen={true}
  //       handleExit={() => {}}
  //       dispatch={dispatch}
  //       select={["test.jpg"]}
  //       setIsLoading={jest.fn()}
  //       setSelect={jest.fn()}
  //       state={
  //         {
  //           fileIndexItems: [
  //             {
  //               parentDirectory: "/",
  //               filePath: "/test.jpg",
  //               fileName: "test.jpg"
  //             }
  //           ]
  //         } as IArchiveProps
  //       }
  //     ></ModalForceDelete>
  //   );

  //   const forceDelete = modal.queryByTestId("force-delete");
  //   expect(forceDelete).toBeTruthy();
  //   // need to await here
  //   await forceDelete?.click();

  //   expect(fetchSpy).toBeCalled();
  //   expect(fetchSpy).toBeCalledWith(
  //     new UrlQuery().UrlDeleteApi(),
  //     "f=%2Ftest.jpg&collections=false",
  //     "delete"
  //   );
  //   expect(dispatch).toBeCalled();
  //   modal.unmount();
  // });

  // it("should fetchPost and not dispatch due status error", async () => {
  //   const fetchSpy = jest
  //     .spyOn(FetchPost, "default")
  //     .mockImplementationOnce(async () => {
  //       return { statusCode: 500 } as IConnectionDefault;
  //     });

  //   const dispatch = jest.fn();
  //   const modal = render(
  //     <ModalForceDelete
  //       isOpen={true}
  //       handleExit={() => {}}
  //       dispatch={dispatch}
  //       select={["test.jpg"]}
  //       setIsLoading={jest.fn()}
  //       setSelect={jest.fn()}
  //       state={
  //         {
  //           fileIndexItems: [
  //             {
  //               parentDirectory: "/",
  //               filePath: "/test.jpg",
  //               fileName: "test.jpg"
  //             }
  //           ]
  //         } as IArchiveProps
  //       }
  //     ></ModalForceDelete>
  //   );

  //   const forceDelete = modal.queryByTestId("force-delete");
  //   expect(forceDelete).toBeTruthy();
  //   // need to await here
  //   await forceDelete?.click();

  //   expect(fetchSpy).toBeCalled();
  //   expect(fetchSpy).toBeCalledWith(
  //     new UrlQuery().UrlDeleteApi(),
  //     "f=%2Ftest.jpg&collections=false",
  //     "delete"
  //   );
  //   expect(dispatch).toBeCalledTimes(0);
  //   modal.unmount();
  // });

  // it("test if handleExit is called", () => {
  //   // simulate if a user press on close
  //   // use as ==> import * as Modal from './modal';
  //   jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
  //     props.handleExit();
  //     return <>{props.children}</>;
  //   });

  //   const handleExitSpy = jest.fn();

  //   const component = render(
  //     <ModalForceDelete
  //       isOpen={true}
  //       dispatch={jest.fn()}
  //       select={[]}
  //       setIsLoading={jest.fn()}
  //       setSelect={jest.fn()}
  //       state={newIArchive()}
  //       handleExit={handleExitSpy}
  //     />
  //   );

  //   expect(handleExitSpy).toBeCalled();

  //   component.unmount();
  // });
});
