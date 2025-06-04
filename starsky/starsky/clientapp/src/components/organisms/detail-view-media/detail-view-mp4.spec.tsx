import { createEvent, fireEvent, render, screen } from "@testing-library/react";
import React, { act } from "react";
import { Root, createRoot } from "react-dom/client";
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
      jest.spyOn(HTMLMediaElement.prototype, "load").mockImplementationOnce(() => {
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

      expect(playSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("keyDown Tab ignore", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockReset()
        .mockImplementationOnce(() => Promise.resolve());

      const figure = screen.queryByTestId("video") as HTMLElement;
      const inputEvent = createEvent.keyDown(figure, { key: "Tab" });
      fireEvent(figure, inputEvent);

      expect(playSpy).toHaveBeenCalledTimes(0);

      component.unmount();
    });

    it("keyDown Enter video resolve", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockReset()
        .mockImplementationOnce(() => Promise.resolve());

      const figure = screen.queryByTestId("video") as HTMLElement;
      const inputEvent = createEvent.keyDown(figure, { key: "Enter" });
      fireEvent(figure, inputEvent);

      expect(playSpy).toHaveBeenCalledTimes(1);

      component.unmount();
    });

    it("click to play video rejected", async () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      jest.spyOn(HTMLMediaElement.prototype, "play").mockReset();

      const playSpy = jest.spyOn(HTMLMediaElement.prototype, "play").mockImplementationOnce(() => {
        return Promise.reject();
      });

      const figure = screen.queryByTestId("video") as HTMLElement;
      await figure.click();

      expect(playSpy).toHaveBeenCalled();
      expect(playSpy).toHaveBeenCalledTimes(1);
      await act(async () => {
        await component.unmount();
      });
    });

    it("click to play video and timeupdate", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest.spyOn(HTMLMediaElement.prototype, "play").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      expect(screen.queryByTestId("video-time")?.textContent).toBe("");

      const figure = screen.queryByTestId("video") as HTMLElement;
      figure.click();

      expect(screen.queryByTestId("video-time")?.textContent).toBe("0:00 / 0:00");

      expect(playSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("keyDown Enter to play video and timeupdate", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      expect(screen.queryByTestId("video-time")?.textContent).toBe("");

      const figure = screen.queryByTestId("video") as HTMLElement;
      const inputEvent = createEvent.keyDown(figure, { key: "Enter" });
      fireEvent(figure, inputEvent);

      expect(screen.queryByTestId("video-time")?.textContent).toBe("0:00 / 0:00");

      expect(playSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("keyDown Tab ignored", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest
        .spyOn(HTMLMediaElement.prototype, "play")
        .mockReset()
        .mockImplementationOnce(() => {
          return Promise.resolve();
        });

      expect(screen.queryByTestId("video-time")?.textContent).toBe("");

      const figure = screen.queryByTestId("video") as HTMLElement;
      const inputEvent = createEvent.keyDown(figure, { key: "Tab" });
      fireEvent(figure, inputEvent);

      expect(screen.queryByTestId("video-time")?.textContent).toBe("");

      expect(playSpy).toHaveBeenCalledTimes(0);

      component.unmount();
    });

    it("progress DOM", (done) => {
      const component = document.createElement("div");
      document.body.appendChild(component); // Append the component to the body

      let root: Root;
      act(() => {
        root = createRoot(component);
        root.render(<DetailViewMp4 />);
      });

      // Wait for the component to finish rendering
      setTimeout(() => {
        console.log(component.innerHTML);

        const progress = component.querySelector("progress");
        if (progress == null) throw new Error("missing progress tag");
        progress.click();
        root.unmount();
        done();
      }, 0);
    });

    it("progress", () => {
      const component = render(<DetailViewMp4></DetailViewMp4>);

      const playSpy = jest.spyOn(HTMLMediaElement.prototype, "play").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      Object.defineProperty(HTMLElement.prototype, "offsetParent", {
        get() {
          return this.parentNode;
        }
      });
      jest.spyOn(HTMLMediaElement.prototype, "load").mockImplementationOnce(() => {
        return Promise.resolve();
      });

      const progress = component.container.querySelector("progress") as HTMLElement;

      // ClickEvent
      fireEvent(
        progress,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          target: progress
        } as unknown as Event)
      );

      expect(screen.queryByTestId("video-time")?.textContent).toBe("0:00 / 0:00");

      expect(playSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("state not found and show error", () => {
      const state = {
        fileIndexItem: {
          status: IExifStatus.NotFoundSourceMissing
        } as IFileIndexItem
      } as IDetailView;

      const contextValues = { state, dispatch: jest.fn() };

      const useContextSpy = jest.spyOn(React, "useContext").mockImplementation(() => contextValues);

      const notificationSpy = jest.spyOn(Notification, "default").mockImplementationOnce(() => {
        return <></>;
      });
      const component = render(<DetailViewMp4 />);

      expect(useContextSpy).toHaveBeenCalled();
      expect(notificationSpy).toHaveBeenCalled();

      component.unmount();
    });
  });
});
