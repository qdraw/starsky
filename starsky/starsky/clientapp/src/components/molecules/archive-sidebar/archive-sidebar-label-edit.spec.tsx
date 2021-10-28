import { act, render } from "@testing-library/react";
import React from "react";
import ArchiveSidebarLabelEdit from "./archive-sidebar-label-edit";

describe("ArchiveSidebarLabelEdit", () => {
  it("renders", () => {
    render(<ArchiveSidebarLabelEdit />);
  });

  it("click on SwitchButton go to Replace", () => {
    const state = {
      state: { isReadOnly: false }
    };
    jest
      .spyOn(React, "useContext")
      .mockImplementationOnce(() => state)
      .mockImplementationOnce(() => state)
      .mockImplementationOnce(() => state)
      .mockImplementationOnce(() => state);

    var component = render(<ArchiveSidebarLabelEdit />);

    var item = component.queryByTestId("switch-button-right") as HTMLElement;

    let formControls = component.queryAllByRole("form-control");
    formControls.forEach((element) => {
      // Not Contain
      expect(element).not.toContain("replace-");
    });

    act(() => {
      item.click();
    });

    formControls = component.queryAllByRole("form-control");
    formControls.forEach((element) => {
      expect(element).toContain("replace-");
    });
  });
});
