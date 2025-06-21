import { render, screen } from "@testing-library/react";
import * as useLocation from "../../../hooks/use-location/use-location-legacy";

import React, { act } from "react";
import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import { Router } from "../../../router-app/router-app";
import * as ModalGeo from "../modal-geo/modal-geo";
import DetailViewInfoLocation from "./detailview-info-location";

describe("DetailViewInfoLocation", () => {
  it("renders", () => {
    render(
      <DetailViewInfoLocation
        fileIndexItem={{} as IFileIndexItem}
        isFormEnabled={false}
        dispatch={jest.fn()}
        setFileIndexItem={jest.fn()}
      ></DetailViewInfoLocation>
    );
  });

  it("[DetailViewInfoLocation] should open modal", async () => {
    Router.navigate("/");

    const modalSpy = jest
      .spyOn(ModalGeo, "default")
      .mockImplementationOnce(() => <div data-test="modal-geo-tmp">data_11</div>);
    const modal = render(
      <DetailViewInfoLocation
        fileIndexItem={{} as IFileIndexItem}
        isFormEnabled={false}
        dispatch={jest.fn()}
        setFileIndexItem={jest.fn()}
      ></DetailViewInfoLocation>
    );

    const modalObject1 = screen.queryByTestId("modal-geo-tmp");
    expect(modalObject1).toBe(null);

    await act(async () => {
      (await screen.findByTestId("detailview-info-location-open-modal")).click();
    });

    const modalObject = await screen.findByTestId("modal-geo-tmp");
    expect(modalObject.innerHTML).toStrictEqual("data_11");

    expect(modalSpy).toHaveBeenCalledTimes(1);
    modalSpy.mockReset();

    modal.unmount();
  });

  it("should display city", () => {
    render(
      <DetailViewInfoLocation
        fileIndexItem={{ locationCity: "hi", locationCountry: "s" } as IFileIndexItem}
        isFormEnabled={false}
        locationCity="hi"
        locationCountry="hi"
        dispatch={jest.fn()}
        setFileIndexItem={jest.fn()}
      ></DetailViewInfoLocation>
    );

    const modalObject1 = screen.queryByTestId("detailview-info-location-city");
    expect(modalObject1?.innerHTML).toBe("hi");
  });

  it("should close without no data", () => {
    jest.spyOn(React, "useState").mockImplementationOnce(() => {
      return [true, jest.fn()];
    });

    const locationMock = {
      location: {
        href: ""
      },
      navigate: jest.fn()
    } as unknown as IUseLocation;
    jest.spyOn(useLocation, "default").mockImplementationOnce(() => locationMock);

    const modalSpy = jest.spyOn(ModalGeo, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit(null);
      });
      return <div data-test="modal-geo-tmp-2">data_11</div>;
    });

    const setFileIndexItemSpy = jest.fn();

    act(() => {
      const modal = render(
        <DetailViewInfoLocation
          fileIndexItem={{} as IFileIndexItem}
          isFormEnabled={false}
          dispatch={jest.fn()}
          setFileIndexItem={setFileIndexItemSpy}
        ></DetailViewInfoLocation>
      );

      expect(setFileIndexItemSpy).toHaveBeenCalledTimes(0);

      modalSpy.mockReset();

      modal.unmount();
    });
  });

  it("should close with callback data", () => {
    jest.spyOn(React, "useState").mockReset().mockReturnValueOnce([true, jest.fn()]);

    const locationMock = {
      location: {
        href: "",
        search: ""
      },
      navigate: jest.fn()
    } as unknown as IUseLocation;
    jest
      .spyOn(useLocation, "default")
      .mockReset()
      .mockImplementationOnce(() => locationMock);

    const modalSpy = jest.spyOn(ModalGeo, "default").mockImplementationOnce((props) => {
      act(() => {
        props.handleExit({ locationCity: "1a" } as IGeoLocationModel);
      });
      return <div data-test="modal-geo-tmp-2">data_11</div>;
    });

    const setFileIndexItemSpy = jest.fn();
    const modal = render(
      <DetailViewInfoLocation
        fileIndexItem={{} as IFileIndexItem}
        isFormEnabled={true}
        dispatch={jest.fn()}
        setFileIndexItem={setFileIndexItemSpy}
      ></DetailViewInfoLocation>
    );

    expect(setFileIndexItemSpy).toHaveBeenCalledTimes(1);
    expect(setFileIndexItemSpy).toHaveBeenCalledWith({ locationCity: "1a" });

    modalSpy.mockReset();

    modal.unmount();
  });
});
