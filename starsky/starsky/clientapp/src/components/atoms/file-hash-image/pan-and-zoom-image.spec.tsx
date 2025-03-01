import { createEvent, fireEvent, render, screen } from "@testing-library/react";
import React, { act } from "react";
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
    it("mouseDown & mousemove event triggered", () => {
      const onWheelCallback = jest.fn();

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const zoomIn = screen.queryAllByTestId("zoom_in")[0];

      act(() => {
        zoomIn.click();
      });

      const panZoomImage = screen.queryAllByTestId("pan-zoom-image")[0];

      const pasteEvent = createEvent.mouseDown(panZoomImage, {
        clientX: 300,
        clientY: 300
      });

      fireEvent(panZoomImage, pasteEvent);

      const ev = new MouseEvent("mousemove", {
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
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const ev = new MouseEvent("mousemove", {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 9,
        clientY: 9
      });

      act(() => {
        document.dispatchEvent(ev);
      });

      expect(moveSpy).toHaveBeenCalled();
      expect(moveSpy).toHaveBeenCalledWith(9, 9);

      component.unmount();
    });

    it("mouse Up should ignore mousemove", () => {
      const onWheelCallback = jest.fn();

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const ev = new MouseEvent("mouseup", {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 9,
        clientY: 9
      });

      act(() => {
        document.dispatchEvent(ev);
      });

      const ev2 = new MouseEvent("mousemove", {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 9,
        clientY: 9
      });

      act(() => {
        document.dispatchEvent(ev2);
      });
      const panZoomImage = screen.queryAllByTestId("pan-zoom-image")[0];

      expect(panZoomImage.innerHTML).toContain("transform: translate(0px, 0px) scale(1)");

      component.unmount();
    });

    it("wheel minus should scale up", () => {
      const onWheelCallback = jest.fn();

      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const panZoomImage = screen.queryAllByTestId("pan-zoom-image")[0];

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
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      const panZoomImage = screen.queryAllByTestId("pan-zoom-image")[0];

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
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
        />
      );

      const zoom_in = screen.queryAllByTestId("zoom_in")[0];
      zoom_in.click();

      expect(zoomSpy).toHaveBeenCalled();
      expect(zoomSpy).toHaveBeenCalledWith(-1);

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
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
        />
      );

      const zoom_out = screen.queryAllByTestId("zoom_out")[0];
      zoom_out.click();

      expect(zoomSpy).toHaveBeenCalled();
      expect(zoomSpy).toHaveBeenCalledWith(1);

      component.unmount();
    });

    it("when pressing cmd+0 expect reset callback to be called", () => {
      const onResetCallbackSpy = jest.fn();
      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={onResetCallbackSpy}
        />
      );

      const event = new KeyboardEvent("keydown", {
        bubbles: true,
        cancelable: true,
        key: "0",
        metaKey: true
      });
      window.dispatchEvent(event);

      expect(onResetCallbackSpy).toHaveBeenCalled();
      component.unmount();
    });

    it("click on zoom in and reset button", () => {
      jest.spyOn(OnWheelMouseAction.prototype, "zoom").mockRestore();

      const onResetCallbackSpy = jest.fn();
      const component = render(
        <PanAndZoomImage
          src=""
          setIsLoading={null as unknown as React.Dispatch<React.SetStateAction<boolean>>}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={onResetCallbackSpy}
        />
      );

      const zoom_in = screen.queryAllByTestId("zoom_in")[0];

      act(() => {
        zoom_in.click();
      });

      const zoom_reset = screen.queryAllByTestId("zoom_reset")[0];

      act(() => {
        zoom_reset.click();
      });

      expect(onResetCallbackSpy).toHaveBeenCalled();

      component.unmount();
    });

    it("should call setIsLoading with false when image is not loaded", () => {
      const setIsLoadingMock = jest.fn();

      // Mock the querySelector to return an image element that is not loaded
      const containerRefMock = {
        current: {
          querySelector: jest.fn().mockReturnValue({
            complete: true,
            naturalHeight: 0
          })
        }
      };

      jest.spyOn(React, "useRef").mockReturnValueOnce(containerRefMock);

      render(
        <PanAndZoomImage
          src="test.jpg"
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
          setIsLoading={setIsLoadingMock}
          id="test-id"
        />
      );

      // Verify that setIsLoading was called with true
      expect(setIsLoadingMock).toHaveBeenCalledWith(true);
    });

    it("should call setIsLoading with true when image is not loaded", () => {
      const setIsLoadingMock = jest.fn();

      // Mock the querySelector to return an image element that is not loaded
      const containerRefMock = {
        current: {
          querySelector: jest.fn().mockReturnValue({
            complete: false,
            naturalHeight: 0
          })
        }
      };

      jest.spyOn(React, "useRef").mockReturnValueOnce(containerRefMock);

      render(
        <PanAndZoomImage
          src="test.jpg"
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
          setIsLoading={setIsLoadingMock}
          id="test-id"
        />
      );

      // Verify that setIsLoading was called with true
      expect(setIsLoadingMock).toHaveBeenCalledWith(true);
    });

    it("should call touchMove on touch move event", async () => {
      const spyOn = jest
        .spyOn(OnMoveMouseTouchAction.prototype, "touchMove")
        .mockImplementationOnce(() => {});

      const component = render(
        <PanAndZoomImage
          src="test.jpg"
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
          setIsLoading={jest.fn()}
          id="test-id"
        />
      );

      const touchEvent = new TouchEvent("touchmove", {
        touches: [
          {
            identifier: 1,
            target: component.container,
            clientX: 500,
            clientY: 500
          } as unknown as Touch
        ]
      });

      window?.dispatchEvent(touchEvent);

      expect(spyOn).toHaveBeenCalled();

      component.unmount();
    });
  });
});
