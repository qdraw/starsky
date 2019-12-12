import { mount, shallow } from 'enzyme';
import React from 'react';
import * as Archive from '../containers/archive';
import * as Search from '../containers/search';
import { newIArchive } from '../interfaces/IArchive';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import ArchiveContextWrapper from './archive-wrapper';


describe("ArchiveContextWrapper", () => {

  it("renders", () => {
    shallow(<ArchiveContextWrapper {...newIArchive()}></ArchiveContextWrapper>)
  });


  describe("with mount", () => {

    it("check if archive is mounted", () => {
      var args = { ...newIArchive(), fileIndexItems: [], pageType: PageType.Archive } as IArchiveProps;
      var archive = jest.spyOn(Archive, 'default').mockImplementationOnce(() => { return <></> })

      args.fileIndexItems.push({} as IFileIndexItem);
      mount(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);
      expect(archive).toBeCalled();
    });

    it("check if search is mounted", () => {
      var args = { ...newIArchive(), fileIndexItems: [], pageType: PageType.Search } as IArchiveProps;
      var search = jest.spyOn(Search, 'default').mockImplementationOnce(() => { return <></> });

      // for loading
      jest.spyOn(Archive, 'default').mockImplementationOnce(() => { return <></> });

      args.fileIndexItems.push({} as IFileIndexItem);

      mount(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);
      expect(search).toBeCalled();
    });

  });

  describe("no context", () => {
    it("No context if used", () => {
      jest.spyOn(React, 'useContext').mockImplementationOnce(() => { return { state: null, dispatch: jest.fn() } })
      var args = { ...newIArchive(), fileIndexItems: [], pageType: PageType.Search } as IArchiveProps;
      var compontent = mount(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);

      expect(compontent.text()).toBe('(ArchiveWrapper) => no state')
    });

    it("No fileIndexItems", () => {
      jest.spyOn(React, 'useContext').mockImplementationOnce(() => { return { state: { fileIndexItems: undefined }, dispatch: jest.fn() } })
      var args = { ...newIArchive(), fileIndexItems: [], pageType: PageType.Search } as IArchiveProps;
      var compontent = mount(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);

      expect(compontent.text()).toBe('')
    });

    it("No pageType", () => {
      jest.spyOn(React, 'useContext').mockImplementationOnce(() => { return { state: { fileIndexItems: [], pageType: undefined }, dispatch: jest.fn() } })
      var args = { ...newIArchive(), fileIndexItems: [], pageType: PageType.Search } as IArchiveProps;
      var compontent = mount(<ArchiveContextWrapper {...args}></ArchiveContextWrapper>);

      expect(compontent.text()).toBe('')
    });

  });

});