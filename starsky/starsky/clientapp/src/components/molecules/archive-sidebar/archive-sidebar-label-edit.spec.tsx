import { mount, shallow } from "enzyme";
import React from 'react';
import ArchiveSidebarLabelEdit from './archive-sidebar-label-edit';

describe("ArchiveSidebarLabelEdit", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarLabelEdit />)
  });

  it("click on SwitchButton go to Replace", () => {
    var component = mount(<ArchiveSidebarLabelEdit />);

    var item = component.find('input[type="radio"]').last();
    item.simulate('change');

    expect(component.exists('[data-name="replace-tags"]')).toBeTruthy();
  });
});
