import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as FetchPost from '../../../shared/fetch-post';
import { UrlQuery } from '../../../shared/url-query';
import * as Modal from '../../atoms/modal/modal';
import ModalArchiveRename from './modal-archive-rename';

describe("ModalArchiveRename", () => {

  it("renders", () => {
    shallow(<ModalArchiveRename
      isOpen={true}
      subPath="/"
      handleExit={() => { }}>
      test
    </ModalArchiveRename>)
  });
  describe("rename", () => {
    it("rename to non valid directory name", async () => {

      var modal = mount(<ModalArchiveRename
        isOpen={true}
        subPath="/test"
        handleExit={() => { }}></ModalArchiveRename>);

      var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
      expect(submitButtonBefore).toBeTruthy();

      act(() => {
        modal.find('[data-name="foldername"]').getDOMNode().textContent = "directory.test";
        modal.find('[data-name="foldername"]').simulate('input');
      });

      // await is needed => there is no button
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      // See warning message
      expect(modal.exists('.warning-box')).toBeTruthy();

      // Cleanup
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });

    it("change directory name", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../../../shared/fetch-post';;
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var modal = mount(<ModalArchiveRename
        isOpen={true}
        subPath="/test"
        handleExit={() => { }}></ModalArchiveRename>);

      act(() => {
        modal.find('[data-name="foldername"]').getDOMNode().textContent = "directory";
        modal.find('[data-name="foldername"]').simulate('input');
      });

      // await is needed 
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSyncRename(), "f=%2Ftest&to=%2Fdirectory");

      // Cleanup
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });

    it("change directory name and FAIL", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../../../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 500 } as IConnectionDefault);
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var modal = mount(<ModalArchiveRename
        isOpen={true}
        subPath="/test"
        handleExit={() => { }}></ModalArchiveRename>);

      act(() => {
        modal.find('[data-name="foldername"]').getDOMNode().textContent = "directory";
        modal.find('[data-name="foldername"]').simulate('input');
      });

      // await is needed 
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSyncRename(), "f=%2Ftest&to=%2Fdirectory");

      // await is needed (button is disabled)
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      // Where should be a warning
      expect(modal.exists('.warning-box')).toBeTruthy();

      // Cleanup
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from '../../atoms/modal/modal';
      jest.spyOn(Modal, 'default').mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>
      });

      var handleExitSpy = jest.fn();

      var component = mount(<ModalArchiveRename subPath="/" isOpen={true} handleExit={handleExitSpy} />);

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });

  });
});