import { act } from '@testing-library/react';
import { mount, shallow } from 'enzyme';
import React from 'react';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import { IDetailView } from '../../../interfaces/IDetailView';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import * as FetchPost from '../../../shared/fetch-post';
import { UrlQuery } from '../../../shared/url-query';
import * as Modal from '../../atoms/modal/modal';
import ModalDetailviewRenameFile from './modal-detailview-rename-file';

describe("ModalDetailviewRenameFile", () => {

  it("renders", () => {
    shallow(<ModalDetailviewRenameFile
      isOpen={true}
      handleExit={() => { }}>
      test
    </ModalDetailviewRenameFile>)
  });

  describe("rename", () => {
    it("to wrong extension", async () => {

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var state = {
        fileIndexItem: { status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var modal = mount(<ModalDetailviewRenameFile
        isOpen={true}
        handleExit={() => { }}></ModalDetailviewRenameFile>);

      var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
      expect(submitButtonBefore).toBeTruthy();

      act(() => {
        modal.find('[data-name="filename"]').getDOMNode().textContent = "file-with-different-extension.tiff";
        modal.find('[data-name="filename"]').simulate('input');
      });

      // await is needed 
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSyncRename(), "f=%2Ftest%2Fimage.jpg&to=%2Ftest%2Ffile-with-different-extension.tiff");

      // find does not work in this case
      expect(modal.html()).toContain('warning-box');
      expect(modal.html()).toContain('disabled');

      // cleanup
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });

    it("to non valid extension", async () => {
      var state = {
        fileIndexItem: { status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues });

      var modal = mount(<ModalDetailviewRenameFile
        isOpen={true}
        handleExit={() => { }}></ModalDetailviewRenameFile>);

      var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
      expect(submitButtonBefore).toBeTruthy();

      act(() => {
        modal.find('[data-name="filename"]').getDOMNode().textContent = "file-without-extension";
        modal.find('[data-name="filename"]').simulate('input');
      });

      // await is needed => there is no button
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      expect(modal.exists('.warning-box')).toBeTruthy();

      var submitButtonAfter = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled;
      expect(submitButtonAfter).toBeTruthy();

      // cleanup
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });

    it("submit filename change", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var state = {
        fileIndexItem: { status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var modal = mount(<ModalDetailviewRenameFile
        isOpen={true}
        handleExit={() => {
        }}></ModalDetailviewRenameFile>);

      act(() => {
        modal.find('[data-name="filename"]').getDOMNode().textContent = "name.jpg";
        modal.find('[data-name="filename"]').simulate('input');
      });

      // await is needed 
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSyncRename(), "f=%2Ftest%2Fimage.jpg&to=%2Ftest%2Fname.jpg");

      // cleanup
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, 'default').mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>
      });

      var handleExitSpy = jest.fn();

      var component = mount(<ModalDetailviewRenameFile isOpen={true} handleExit={handleExitSpy} />);

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });

  });

});