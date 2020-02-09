import { mount, shallow } from 'enzyme';
import React from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import * as ItemTextListView from './item-text-list-view';
import ModalDropAreaFilesAdded from './modal-drop-area-files-added';

describe("ModalDropAreaFilesAdded", () => {

  it("renders", () => {
    var component = shallow(<ModalDropAreaFilesAdded isOpen={true} uploadFilesList={[]} handleExit={() => { }} />);
    component.unmount();
  });

  describe("with Context", () => {

    beforeEach(() => {
      jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })
    });

    it("list is rendered", () => {

      jest.spyOn(ItemTextListView, 'default').mockImplementationOnce((props) => {
        return <span id="data-test-0">{props.fileIndexItems[0].fileName}</span>
      });

      var exampleList = [{
        fileName: 'test.jpg',
        filePath: "/test.jpg"
      } as IFileIndexItem];

      var handleExitSpy = jest.fn();

      var component = mount(<ModalDropAreaFilesAdded isOpen={true} uploadFilesList={exampleList} handleExit={handleExitSpy} />);

      expect(component.exists("#data-test-0")).toBeTruthy()
      expect(component.find("#data-test-0").text()).toBe('test.jpg')

      component.unmount();

    });

  });
});