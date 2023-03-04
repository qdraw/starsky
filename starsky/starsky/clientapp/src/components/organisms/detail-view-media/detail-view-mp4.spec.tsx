import { act, fireEvent, render, screen } from "@testing-library/react";
import React from "react";
import ReactDOM from "react-dom";
import { IDetailView } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as Notification from "../../atoms/notification/notification";
import DetailViewMp4 from "./detail-view-mp4";

describe("DetailViewMp4", () => {
  it("renders (without state component)", () => {
    render(<DetailViewMp4></DetailViewMp4>);
  });

  describe("with Context", () => {
    beforeEach(() => {
      jest
        .spyOn(HTMLMediaElement.prototype, "load")
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });
    });

    it("click to play video resolve", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockImplementationOnce(() => Promise.resolve());

      const figure = screen.queryByTestId("video") as HTMLElement;
      figure.click();

      expect(playSpy).toBeCalled();

      component.unmount();
    });

    it("click to play video rejected", async () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      jest.spyOn(HTMLMediaElement.prototype, "play").mockReset();

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockImplementationOnce(() => {
          return Promise.reject();
        });

      const figure = screen.queryByTestId("video") as HTMLElement;
      await figure.click();

      expect(playSpy).toBeCalled();
      expect(playSpy).toBeCalledTimes(1);
      await act(async () => {
        await component.unmount();
      });
    });
    it("click to play video and timeupdate", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      expect(screen.queryByTestId("video-time")?.textContent).toBe("");

      const figure = screen.queryByTestId("video") as HTMLElement;
      figure.click();

      expect(screen.queryByTestId("video-time")?.textContent).toBe(
        "0:00 / 0:00"
      );

      expect(playSpy).toBeCalled();

      component.unmount();
    });

    it("progress DOM", () => {
      const component = document.createElement("div");
      ReactDOM.render(<DetailViewMp4 />, component);
      const progress = component.querySelector("progress");
      if (progress == null) throw new Error("missing progress tag");
      progress.click();
    });

    it("progress", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      Object.defineProperty(HTMLElement.prototype, "offsetParent", {
        get() {
          return this.parentNode;
        }
      });
      jest
        .spyOn(HTMLMediaElement.prototype, "load")
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      const progress = component.container.querySelector(
        "progress"
      ) as HTMLElement;

      // ClickEvent
      fireEvent(
        progress,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          target: progress
        } as any)
      );

      expect(screen.queryByTestId("video-time")?.textContent).toBe(
        "0:00 / 0:00"
      );

      expect(playSpy).toBeCalled();

      component.unmount();
    });

    it("state not found and show error", () => {
      const state = {
        fileIndexItem: {
          status: IExifStatus.NotFoundSourceMissing
        } as IFileIndexItem
      } as IDetailView;

      const contextValues = { state, dispatch: jest.fn() };

      const useContextSpy = jest
        .spyOn(React, "useContext")
        .mockImplementation(() => contextValues);

      const notificationSpy = jest
        .spyOn(Notification, "default")
        .mockImplementationOnce(() => {
          return <></>;
        });
      const component = render(<DetailViewMp4 />);

      expect(useContextSpy).toBeCalled();
      expect(notificationSpy).toBeCalled();

      component.unmount();
    });
  });
});
