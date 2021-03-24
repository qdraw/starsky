import { mount, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import { OnMoveMouseTouchAction } from "./on-move-mouse-touch-action";
import { OnWheelMouseAction } from "./on-wheel-mouse-action";
import PanAndZoomImage from "./pan-and-zoom-image";

describe("PanAndZoomImage", () => {
  it("renders", () => {
    shallow(
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

      const component = mount(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      component.find('[data-test="zoom_in"]').simulate("click");

      component
        .find(".pan-zoom-image-container")
        .simulate("mousedown", { clientX: 300, clientY: 300 });

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
      component.render();

      expect(component.find(".pan-zoom-image-container").html()).toContain(
        "transform: translate(-291px"
      );

      component.unmount();
    });

    it("expect trigger onTouchMove", () => {
      const onWheelCallback = jest.fn();

      const moveSpy = jest
        .spyOn(OnMoveMouseTouchAction.prototype, "move")
        .mockImplementationOnce(() => {});

      const component = mount(
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
      component.render();

      expect(moveSpy).toBeCalled();
      expect(moveSpy).toBeCalledWith(9, 9);

      component.unmount();
    });

    it("mouse Up should ignore mousemove", () => {
      const onWheelCallback = jest.fn();

      const component = mount(
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
      expect(component.find(".pan-zoom-image-container").html()).toContain(
        "transform: translate(0px, 0px) scale(1)"
      );

      component.unmount();
    });

    it("wheel minus should scale up", () => {
      const onWheelCallback = jest.fn();

      const component = mount(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      component
        .find(".pan-zoom-image-container")
        .simulate("wheel", { deltaY: -300 });

      component.render();
      expect(component.find(".pan-zoom-image-container").html()).toContain(
        "scale(1.1)"
      );

      component.unmount();
    });

    it("wheel plus should scale up", () => {
      const onWheelCallback = jest.fn();

      const component = mount(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
          onResetCallback={jest.fn()}
        />
      );

      component
        .find(".pan-zoom-image-container")
        .simulate("wheel", { deltaY: 300 });

      expect(component.find(".pan-zoom-image-container").html()).toContain(
        "scale(0.9)"
      );

      component.unmount();
    });

    it("click on zoom in button", () => {
      // after simulate wheel due spy on
      const zoomSpy = jest
        .spyOn(OnWheelMouseAction.prototype, "zoom")
        .mockImplementationOnce(() => {});

      const component = mount(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
        />
      );

      component.find("[data-test='zoom_in']").simulate("click");

      expect(zoomSpy).toBeCalled();
      expect(zoomSpy).toBeCalledWith(-1);

      component.unmount();
    });

    it("click on zoom Out button", () => {
      // after simulate wheel due spy on

      const zoomSpy = jest
        .spyOn(OnWheelMouseAction.prototype, "zoom")
        .mockImplementationOnce(() => {});

      const component = mount(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={jest.fn()}
          onResetCallback={jest.fn()}
        />
      );

      component.find("[data-test='zoom_out']").simulate("click");

      expect(zoomSpy).toBeCalled();
      expect(zoomSpy).toBeCalledWith(1);

      component.unmount();
    });

    it("when pessing cmd+0 expect reset callback to be called", () => {
      const onResetCallbackSpy = jest.fn();
      const component = mount(
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
  });
});
