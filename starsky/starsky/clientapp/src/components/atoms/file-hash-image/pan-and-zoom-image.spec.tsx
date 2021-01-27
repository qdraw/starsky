import { mount, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import PanAndZoomImage from "./pan-and-zoom-image";

describe("PanAndZoomImage", () => {
  it("renders", () => {
    shallow(
      <PanAndZoomImage
        src=""
        translateRotation={Orientation.Horizontal}
        onWheelCallback={jest.fn()}
      />
    );
  });

  describe("PanAndZoomImage", () => {
    // it("modal-exit-button", () => {
    //   const onWheelCallback = jest.fn();

    //   const component = mount(
    //     <PanAndZoomImage
    //       src=""
    //       translateRotation={Orientation.Horizontal}
    //       onWheelCallback={onWheelCallback}
    //     />
    //   );
    //   component.simulate("load");

    //   component.unmount();
    // });

    it("mouseDown & mousemove event triggerd", () => {
      const onWheelCallback = jest.fn();

      const component = mount(
        <PanAndZoomImage
          src=""
          setIsLoading={null as any}
          translateRotation={Orientation.Horizontal}
          onWheelCallback={onWheelCallback}
        />
      );

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

      expect(component.find(".pan-zoom-image-container").html()).toContain(
        "transform: translate(-291px, -291px)"
      );

      component.unmount();
    });
  });
});
