import { mount, shallow } from 'enzyme';
import React from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import * as ItemTextListView from './item-text-list-view';
import ModalDropAreaFilesAdded from './modal-drop-area-files-added';

describe("ModalDropAreaFilesAdded", () => {

  it("renders", () => {
    shallow(<ModalDropAreaFilesAdded isOpen={true} uploadFilesList={[]} handleExit={() => { }} />)
  });

  describe("with Context", () => {
    xit("renders", () => {

      var itemTextListViewSpy = jest.spyOn(ItemTextListView, 'default');

      var exampleList = [{
        fileName: 'test'
      } as IFileIndexItem];
      var handleExitSpy = jest.fn();
      var component = mount(<ModalDropAreaFilesAdded isOpen={true} uploadFilesList={exampleList} handleExit={handleExitSpy} />);

      //   expect.any(Function)
      expect(itemTextListViewSpy).toBeCalledWith({ fileIndexItems: exampleList });

      component.unmount();

    });

  });
});