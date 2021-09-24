import { render } from "@testing-library/react";
import React from "react";
import ArchiveSidebarLabelEdit from "./archive-sidebar-label-edit";

describe("ArchiveSidebarLabelEdit", () => {
  it("renders", () => {
    render(<ArchiveSidebarLabelEdit />);
  });

  it("click on SwitchButton go to Replace", () => {
    var component = render(
      <ArchiveSidebarLabelEdit>t</ArchiveSidebarLabelEdit>
    );

    var item = component.find('input[type="radio"]').last();
    item.simulate("change");

    expect(component.exists('[data-name="replace-tags"]')).toBeTruthy();
  });
});
