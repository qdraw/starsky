import { render } from "@testing-library/react";
import Portal from "./portal";

describe("Portal", () => {
  it("renders", () => {
    render(<Portal />);
  });

  it("default render", () => {
    const component = render(<Portal>test</Portal>);
    expect(document.querySelectorAll("#portal-root").length).toBe(1);
    component.unmount();
  });

  it("default cleanup after render", () => {
    const component = render(<Portal>test</Portal>);
    expect(document.querySelectorAll("#portal-root").length).toBe(1);
    console.log(document.body.innerHTML);

    component.unmount();
    expect(document.querySelectorAll("#portal-root").length).toBe(0);

    // it should not exist in the body anymore
  });

  it("null cleanup after render", () => {
    const component = render(<Portal>test</Portal>);
    expect(document.querySelectorAll("#portal-root").length).toBe(1);

    const tempItem = document.querySelector("#portal-root");
    if (!tempItem) throw Error("missing item");
    tempItem.remove();

    component.unmount();
    expect(document.querySelectorAll("#portal-root").length).toBe(0);
  });
});
