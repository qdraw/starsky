import { createEvent, fireEvent, render, waitFor } from "@testing-library/react";
import { act } from "react";
import FormControl from "./form-control";

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
      (window as { getSelection: () => void }).getSelection = () => {
        return {
          removeAllRanges: () => {}
        };
      };
    });

    it("limitLengthKey - null/nothing", async () => {
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          test
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];

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
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          123456789
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];

      expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy();

      formControl.innerHTML += 1;

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "x",
        code: "x"
      });

      act(() => {
        fireEvent(formControl, keyDownEvent);
      });

      await waitFor(() =>
        expect(component.container.getElementsByClassName("warning-box").length).toBeTruthy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeTruthy());

      component.unmount();
    });

    it("limitLengthKey - keydown max limit but allow control/command a", async () => {
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          123456789
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];
      await waitFor(() =>
        expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy()
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
        expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthKey - keydown max limit but allow control/command e", async () => {
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          123456789
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];
      await waitFor(() =>
        expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy()
      );

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "e",
        code: "e",
        ctrlKey: true
      });

      fireEvent(formControl, keyDownEvent);

      await waitFor(() =>
        expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthKey - keydown ok", async () => {
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          123456
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];
      await waitFor(() =>
        expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy()
      );

      const keyDownEvent = createEvent.keyDown(formControl, {
        key: "x"
      });

      act(() => {
        fireEvent(formControl, keyDownEvent);
      });

      await waitFor(() =>
        expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy()
      );

      await waitFor(() => expect(keyDownEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthPaste - copy -> paste limit/preventDefault", async () => {
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          987654321
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];

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
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          987654321
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];

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
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={() => {}} name="test">
          &nbsp;
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];
      const blurEvent = createEvent.blur(formControl);

      act(() => {
        formControl.innerHTML = "";
        fireEvent(formControl, blurEvent);
      });

      await waitFor(() => expect(blurEvent.defaultPrevented).toBeFalsy());

      component.unmount();
    });

    it("limitLengthBlur - onBlur pushed/ok", async () => {
      const onBlurSpy = jest.fn();

      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={onBlurSpy} name="test">
          test
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];
      const blurEvent = createEvent.blur(formControl);

      act(() => {
        formControl.innerHTML += "1";
        fireEvent(formControl, blurEvent);
      });

      expect(component.container.getElementsByClassName("warning-box").length).toBeFalsy();

      onBlurSpy.mockReset();
      component.unmount();
    });

    it("limitLengthBlur - onBlur limit/preventDefault", () => {
      const onBlurSpy = jest.fn();
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={onBlurSpy} name="test123">
          012345678919000000
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];
      const blurEvent = createEvent.blur(formControl);

      // need to dispatch on child element
      act(() => {
        formControl.innerHTML += "1";
        fireEvent(formControl, blurEvent);
      });

      expect(component.container.getElementsByClassName("warning-box").length).toBeTruthy();

      component.unmount();
    });

    it("limitLengthBlur - onBlur limit", () => {
      const onBlurSpy = jest.fn();
      const component = render(
        <FormControl contentEditable={true} maxlength={10} onBlur={onBlurSpy} name="test">
          1234567890123
        </FormControl>
      );

      const formControl = component.container.getElementsByClassName("form-control")[0];
      const blurEvent = createEvent.blur(formControl);

      // need to dispatch on child element
      act(() => {
        formControl.innerHTML += "1";
        fireEvent(formControl, blurEvent);
      });

      expect(component.container.getElementsByClassName("warning-box").length).toBeTruthy();

      component.unmount();
    });
  });
});
