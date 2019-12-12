import { shallow } from "enzyme";
import React from 'react';
import ArchiveSidebarLabelEdit from './archive-sidebar-label-edit';

describe("ArchiveSidebarLabelEdit", () => {
  it("renders", () => {
    shallow(<ArchiveSidebarLabelEdit />)
  });
});
