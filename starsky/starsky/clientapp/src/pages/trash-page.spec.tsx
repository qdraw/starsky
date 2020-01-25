import { mount } from 'enzyme';
import React from 'react';
import * as ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import * as useTrashList from '../hooks/use-trashlist';
import { IUseTrashList } from '../hooks/use-trashlist';
import { newIArchive } from '../interfaces/IArchive';
import { PageType } from '../interfaces/IDetailView';
import TrashPage from './trash-page';

describe("TrashPage", () => {

  it("default error case", () => {
    jest.spyOn(useTrashList, 'default').mockImplementationOnce(() => {
      return {} as IUseTrashList
    });

    var error = mount(<TrashPage></TrashPage>);
    expect(error.text()).toBe("Something went wrong")
  });

  it("check if context is called", () => {
    var contextSpy = jest.spyOn(ArchiveContextWrapper, 'default').mockImplementationOnce(() => { return <></> });

    jest.spyOn(useTrashList, 'default').mockImplementationOnce(() => {
      return {
        archive: newIArchive(),
        pageType: PageType.Trash
      } as IUseTrashList
    });

    var trashPage = mount(<TrashPage></TrashPage>);

    expect(contextSpy).toBeCalled();

    trashPage.unmount();
  });
});