import { shallow } from 'enzyme';
import React from 'react';
import { IFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import ColorClassSelect from './color-class-select';

describe("ArchiveSidebarColorClass", () => {
  it("renders", () => {
    shallow(<ColorClassSelect isEnabled={true} filePath={"/test"} onToggle={() => { }}></ColorClassSelect>)
  });

  it("onClick value return", (done) => {

    var wrapper = shallow(<ColorClassSelect clearAfter={true} isEnabled={true} filePath={"/test"} onToggle={(value) => {
      expect(value).toBe(1);
      done();
    }}></ColorClassSelect>)

    wrapper.find('a.colorclass--1').simulate('click');

  });

  it("onClick value return keypress", () => {
    const mockFetchAsXml: Promise<IFileIndexItem[]> = Promise.resolve(newIFileIndexItemArray());
    // jest.spyOn(Query.prototype, 'queryUpdateApi').mockImplementationOnce(() => mockFetchAsXml);



    // var wrapper = shallow(<ColorClassSelect clearAfter={true} isEnabled={true} filePath={"/test"} onToggle={(value) => {
    //   expect(value).toBe(1);
    //   done();
    // }}></ColorClassSelect>)
    // wrapper.simulate('keydown', { keyCode: 49 }); // keyCode === 1

  });
});