import { createEvent, fireEvent, render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import { OnMoveMouseTouchAction } from "./on-move-mouse-touch-action";
import { OnWheelMouseAction } from "./on-wheel-mouse-action";
import PanAndZoomImage from "./pan-and-zoom-image";

describe("PanAndZoomImage", () => {
  it("renders", () => {
    render(
      <PanAndZoomImage
        src=""
        translateRotation={Orientation.Horizontal}
        onWheelCallback={jest.fn()}
        onResetCallback={jest.fn()}
      />
    );
  });

  describe("PanAndZoomImage", () => {
    it("mouseDown & mousemove event triggerd", () => {
      const onWheelCallback = jest.fn();

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const zoomIn = component.queryAllByTestId("zoom_in")[0];

      act(() => {
        zoomIn.click();
      });

      const panZoomImage = component.queryAllByTestId("pan-zoom-image")[0];

      const pasteEvent = createEvent.mouseDown(panZoomImage, {
        clientX: 300,
        clientY: 300
      });

      fireEvent(panZoomImage, pasteEvent);

      let ev = new MouseEvent("mousemove", {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 9,
        clientY: 9
      });

      act(() => {
        document.dispatchEvent(ev);
      });

      expect(panZoomImage.innerHTML).toContain("transform: translate(-291px");

      component.unmount();
    });

    it("expect trigger onTouchMove", () => {
      const onWheelCallback = jest.fn();

      const moveSpy = jest
        .spyOn(OnMoveMouseTouchAction.prototype, "move")
        .mockImplementationOnce(() => {});

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      let ev = new MouseEvent("mousemove", {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 9,
        clientY: 9
      });

      act(() => {
        document.dispatchEvent(ev);
      });

      expect(moveSpy).toBeCalled();
      expect(moveSpy).toBeCalledWith(9, 9);

      component.unmount();
    });

    it("mouse Up should ignore mousemove", () => {
      const onWheelCallback = jest.fn();

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      let ev = new MouseEvent("mouseup", {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 9,
        clientY: 9
      });

      act(() => {
        document.dispatchEvent(ev);
      });

      let ev2 = new MouseEvent("mousemove", {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 9,
        clientY: 9
      });

      act(() => {
        document.dispatchEvent(ev2);
      });
      const panZoomImage = component.queryAllByTestId("pan-zoom-image")[0];

      expect(panZoomImage.innerHTML).toContain(
        "transform: translate(0px, 0px) scale(1)"
      );

      component.unmount();
    });

    it("wheel minus should scale up", () => {
      const onWheelCallback = jest.fn();

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const panZoomImage = component.queryAllByTestId("pan-zoom-image")[0];

      const pasteEvent = createEvent.wheel(panZoomImage, {
        deltaY: -300
      });

      fireEvent(panZoomImage, pasteEvent);

      expect(panZoomImage.innerHTML).toContain("scale(1.1)");

      component.unmount();
    });

    it("wheel plus should scale up", () => {
      const onWheelCallback = jest.fn();

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const panZoomImage = component.queryAllByTestId("pan-zoom-image")[0];

      const pasteEvent = createEvent.wheel(panZoomImage, {
        deltaY: 300
      });

      fireEvent(panZoomImage, pasteEvent);

      expect(panZoomImage.innerHTML).toContain("scale(0.9)");

      component.unmount();
    });

    it("click on zoom in button", () => {
      // after simulate wheel due spy on
      const zoomSpy = jest
        .spyOn(OnWheelMouseAction.prototype, "zoom")
        .mockImplementationOnce(() => {});

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
        />
      );

      const zoom_in = component.queryAllByTestId("zoom_in")[0];
      zoom_in.click();

      expect(zoomSpy).toBeCalled();
      expect(zoomSpy).toBeCalledWith(-1);

      component.unmount();
    });

    it("click on zoom Out button", () => {
      // after simulate wheel due spy on

      const zoomSpy = jest
        .spyOn(OnWheelMouseAction.prototype, "zoom")
        .mockImplementationOnce(() => {});

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
        />
      );

      const zoom_out = component.queryAllByTestId("zoom_out")[0];
      zoom_out.click();

      expect(zoomSpy).toBeCalled();
      expect(zoomSpy).toBeCalledWith(1);

      component.unmount();
    });

    it("when pessing cmd+0 expect reset callback to be called", () => {
      const onResetCallbackSpy = jest.fn();
      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={onResetCallbackSpy}
        />
      );

      var event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "0",
        metaKey: true
      });
      window.dispatchEvent(event);

      expect(onResetCallbackSpy).toBeCalled();
      component.unmount();
    });

    it("click on zoom in and reset button", () => {
      jest.spyOn(OnWheelMouseAction.prototype, "zoom").mockRestore();

      const onResetCallbackSpy = jest.fn();
      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={onResetCallbackSpy}
        />
      );

      const zoom_in = component.queryAllByTestId("zoom_in")[0];

      act(() => {
        zoom_in.click();
      });

      const zoom_reset = component.queryAllByTestId("zoom_reset")[0];

      act(() => {
        zoom_reset.click();
      });

      expect(onResetCallbackSpy).toBeCalled();

      component.unmount();
    });
  });
});
