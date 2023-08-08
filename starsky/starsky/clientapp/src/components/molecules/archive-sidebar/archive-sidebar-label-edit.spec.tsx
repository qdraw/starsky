import { act, render, screen } from "@testing-library/react";
import React from "react";
import ArchiveSidebarLabelEdit from "./archive-sidebar-label-edit";

describe("ArchiveSidebarLabelEdit", () => {
  it("renders", () => {
    render(<ArchiveSidebarLabelEdit />);
  });

  it("click on SwitchButton go to Replace", () => {
    const state = {
      state: { isReadOnly: false },
    };
    jest
      .spyOn(React, "useContext")
      .mockImplementationOnce(() => state)
      .mockImplementationOnce(() => state)
      .mockImplementationOnce(() => state)
      .mockImplementationOnce(() => state);

    const component = render(<ArchiveSidebarLabelEdit />);

    const item = screen.queryByTestId("switch-button-right") as HTMLElement;

    let formControls = screen.queryAllByRole("form-control");
    formControls.forEach((element) => {
      // Not Contain
      expect(element).not.toContain("replace-");
    });

    act(() => {
      item.click();
    });

    formControls = screen.queryAllByRole("form-control");
    formControls.forEach((element) => {
      expect(element).toContain("replace-");
    });

    component.unmount();
  });
});
