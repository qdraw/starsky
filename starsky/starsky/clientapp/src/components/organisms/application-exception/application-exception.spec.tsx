import { render, screen } from "@testing-library/react";
import * as MenuDefault from "../menu-default/menu-default";
import ApplicationException from "./application-exception";

describe("ApplicationException", () => {
  it("renders", () => {
    render(<ApplicationException>t</ApplicationException>);
  });

  it("should have menu", () => {
    const menuDefaultSpy = jest
      .spyOn(MenuDefault, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    const component = render(<ApplicationException>t</ApplicationException>);

    expect(menuDefaultSpy).toBeCalled();

    component.unmount();
  });

  it("should have warning", () => {
    jest.spyOn(MenuDefault, "default").mockImplementationOnce(() => {
      return <></>;
    });

    const component = render(<ApplicationException>t</ApplicationException>);

    expect(screen.getByTestId("application-exception-header")).toBeTruthy();

    component.unmount();
  });

  it("click on reload", () => {
    const { location } = window;
    // @ts-ignore
    delete window.location;
    // @ts-ignore
    window.location = { reload: jest.fn() };

    const reloadSpy = jest.spyOn(window.location, "reload").mockReturnValue();

    jest.spyOn(MenuDefault, "default").mockImplementationOnce(() => {
      return <></>;
    });

    const component = render(<ApplicationException>t</ApplicationException>);

    expect(window.location.reload).not.toHaveBeenCalled();

    const reload = screen.queryByTestId("reload") as HTMLButtonElement;

    reload.click();

    expect(reloadSpy).toBeCalledTimes(1);

    // restore window object
    window.location = location;

    component.unmount();
  });
});
