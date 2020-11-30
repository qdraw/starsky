import { mount, shallow } from 'enzyme';
import React from 'react';
import { IRelativeObjects, newIRelativeObjects } from '../../../interfaces/IDetailView';
import ArchivePagination from './archive-pagination';

describe("ArchivePagination", () => {

  it("renders new object", () => {
    shallow(<ArchivePagination relativeObjects={newIRelativeObjects()} />)
  });

  var relativeObjects = { nextFilePath: "next", prevFilePath: 'prev' } as IRelativeObjects;
  var Component = mount(<ArchivePagination relativeObjects={relativeObjects} />)

  it("next page exist", () => {
    expect(Component.find('a.next').props().href).toBe('/?f=next')
  });

  it("prev page exist", () => {
    expect(Component.find('a.prev').props().href).toBe('/?f=prev')
  });
});
