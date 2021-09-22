import {
  createEvent,
  fireEvent,
  render,
  waitFor
} from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import FormControl from "./form-control";
import { LimitLength } from "./limit-length";

describe("FormControl", () => {
  it("renders", () => {
    render(
      <FormControl contentEditable={true} onBlur={() => {}} name="test">
        &nbsp;
      </FormControl>
    );
  });

  describe("with events", () => {
    beforeAll(() => {
      (window as any).getSelection = () => {
        return {
          removeAllRanges: () => {}
        };
      };
    });

    it("limitLengthKey - null/nothing", async () => {
      const component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          test
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "x",
        code: "x"
      });

      act(() => {
        fireEvent(formControl, keyDownEvent);
      });

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthKey - keydown max limit/preventDefault", async () => {
      const component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          123456789
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];

      expect(
        component.container.getElementsByClassName("warning-box").length
      ).toBeFalsy();

      formControl.innerHTML += 1;

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "x",
        code: "x"
      });

      act(() => {
        fireEvent(formControl, keyDownEvent);
      });

      await waitFor(() =>
        expect(
          component.container.getElementsByClassName("warning-box").length
        ).toBeTruthy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeTruthy());

      component.unmount();
    });

    it("limitLengthKey - keydown max limit but allow control/command a", async () => {
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          123456789
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];
      await waitFor(() =>
        expect(
          component.container.getElementsByClassName("warning-box").length
        ).toBeFalsy()
      );

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "a",
        code: "a",
        metaKey: true
      });

      act(() => {
        fireEvent(formControl, keyDownEvent);
      });

      await waitFor(() =>
        expect(
          component.container.getElementsByClassName("warning-box").length
        ).toBeFalsy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthKey - keydown max limit but allow control/command e", async () => {
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          123456789
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];
      await waitFor(() =>
        expect(
          component.container.getElementsByClassName("warning-box").length
        ).toBeFalsy()
      );

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "e",
        code: "e",
        ctrlKey: true
      });

      act(() => {
        fireEvent(formControl, keyDownEvent);
      });

      await waitFor(() =>
        expect(
          component.container.getElementsByClassName("warning-box").length
        ).toBeFalsy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthKey - keydown ok", async () => {
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          123456
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];
      await waitFor(() =>
        expect(
          component.container.getElementsByClassName("warning-box").length
        ).toBeFalsy()
      );

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "x"
      });

      act(() => {
        fireEvent(formControl, keyDownEvent);
      });

      await waitFor(() =>
        expect(
          component.container.getElementsByClassName("warning-box").length
        ).toBeFalsy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthPaste - copy -> paste limit/preventDefault", async () => {
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          987654321
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];

      const pasteEvent = createEvent.paste(formControl, {
        clipboardData: {
          getData: () => "10"
        }
      });

      act(() => {
        fireEvent(formControl, pasteEvent);
      });

      // limit!
      await waitFor(() => expect(pasteEvent.defaultPrevented).toBeTruthy());

      component.unmount();
    });

    it("limitLengthPaste - copy -> paste ok", async () => {
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          987654321
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];

      const pasteEvent = createEvent.paste(formControl, {
        clipboardData: {
          getData: () => "1"
        }
      });

      act(() => {
        fireEvent(formControl, pasteEvent);
      });

      // limit!
      await waitFor(() => expect(pasteEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthBlur - null/nothing", async () => {
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={() => {}}
          name="test"
        >
          &nbsp;
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];
      const blurEvent = createEvent.blur(formControl);

      act(() => {
        formControl.innerHTML = "";
        fireEvent(formControl, blurEvent);
      });

      await waitFor(() => expect(blurEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthBlur - onBlur pushed/ok", async () => {
      var onBlurSpy = jest.fn();

      const limitSpy = jest
        .spyOn(LimitLength.prototype, "LimitLengthBlur")
        .mockImplementationOnce(() => () => {});

      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={onBlurSpy}
          name="test"
        >
          abcdefghi
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName(
        "form-control"
      )[0];
      const blurEvent = createEvent.blur(formControl);

      act(() => {
        fireEvent(formControl, blurEvent);
      });

      expect(
        component.container.getElementsByClassName("warning-box").length
      ).toBeFalsy();

      await waitFor(() => expect(limitSpy).toBeCalled());

      expect(limitSpy).toBeCalled();

      onBlurSpy.mockReset();
      component.unmount();
    });

    xit("limitLengthBlur - onBlur limit/preventDefault", () => {
      var onBlurSpy = jest.fn();
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={onBlurSpy}
          name="test123"
        >
          012345678919000000
        </FormControl>
      );

      // need to dispatch on child element
      component.find(".form-control").simulate("blur");

      expect(onBlurSpy).toBeCalledTimes(0);
      expect(component.exists(".warning-box")).toBeTruthy();

      component.unmount();
    });

    xit("limitLengthBlur - onBlur limit", () => {
      var onBlurSpy = jest.fn();
      var component = render(
        <FormControl
          contentEditable={true}
          maxlength={10}
          onBlur={onBlurSpy}
          name="test"
        >
          1234567890123
        </FormControl>
      );

      act(() => {
        component.simulate("blur");
      });

      expect(component.exists(".warning-box")).toBeTruthy();
      expect(onBlurSpy).toBeCalledTimes(0);

      component.unmount();
    });
  });
});
