import { act, render, screen } from "@testing-library/react";
import * as useLocation from "../../../hooks/use-location";

import React from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import * as ModalGeo from "../modal-geo/modal-geo";
import DetailViewInfoLocation from "./detailview-info-location";

describe("ModalGeo", () => {
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

  it("should open modal", async () => {
    const modalSpy = jest
      .spyOn(ModalGeo, "default")
      .mockImplementationOnce(() => (
        <div data-test="modal-geo-tmp">data_11</div>
      ));
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
      (
        await screen.findByTestId("detailview-info-location-open-modal")
      ).click();
    });

    const modalObject = await screen.findByTestId("modal-geo-tmp");
    expect(modalObject.innerHTML).toStrictEqual("data_11");

    expect(modalSpy).toBeCalledTimes(1);
    modalSpy.mockReset();

    modal.unmount();
  });

  it("should display city", () => {
    render(
      <DetailViewInfoLocation
        fileIndexItem={
          { locationCity: "hi", locationCountry: "s" } as IFileIndexItem
        }
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
    } as any;
    jest
      .spyOn(useLocation, "default")
      .mockImplementationOnce(() => locationMock);

    const modalSpy = jest
      .spyOn(ModalGeo, "default")
      .mockImplementationOnce((props) => {
        act(() => {
          props.handleExit(null);
        });
        return <div data-test="modal-geo-tmp-2">data_11</div>;
      });

    const setFileIndexItemSpy = jest.fn();
    const modal = render(
      <DetailViewInfoLocation
        fileIndexItem={{} as IFileIndexItem}
        isFormEnabled={false}
        dispatch={jest.fn()}
        setFileIndexItem={setFileIndexItemSpy}
      ></DetailViewInfoLocation>
    );

    expect(setFileIndexItemSpy).toBeCalledTimes(0);

    modalSpy.mockReset();

    modal.unmount();
  });

  it("should close with callback data", () => {
    jest.spyOn(React, "useState").mockImplementationOnce(() => {
      return [true, jest.fn()];
    });

    const locationMock = {
      location: {
        href: ""
      },
      navigate: jest.fn()
    } as any;
    jest
      .spyOn(useLocation, "default")
      .mockImplementationOnce(() => locationMock);

    const modalSpy = jest
      .spyOn(ModalGeo, "default")
      .mockImplementationOnce((props) => {
        act(() => {
          props.handleExit({ locationCity: "1a" } as IGeoLocationModel);
        });
        return <div data-test="modal-geo-tmp-2">data_11</div>;
      });

    const setFileIndexItemSpy = jest.fn();
    const modal = render(
      <DetailViewInfoLocation
        fileIndexItem={{} as IFileIndexItem}
        isFormEnabled={false}
        dispatch={jest.fn()}
        setFileIndexItem={setFileIndexItemSpy}
      ></DetailViewInfoLocation>
    );

    expect(setFileIndexItemSpy).toBeCalledTimes(1);
    expect(setFileIndexItemSpy).toBeCalledWith({ locationCity: "1a" });

    modalSpy.mockReset();

    modal.unmount();
  });
});
