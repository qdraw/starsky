import { mount, shallow } from 'enzyme';
import React from 'react';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import { IDetailView } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
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
    it("to wrong extension", () => {

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

      modal.find('[data-name="filename"]').getDOMNode().textContent = "file-without-extension";
      modal.find('[data-name="filename"]').simulate('input');

      expect(modal.exists('.warning-box')).toBeTruthy();

      var submitButtonAfter = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
      expect(submitButtonAfter).toBeFalsy();
    });

    it("submit filename change", () => {
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
        .mockImplementationOnce(() => { return contextValues });

      var modal = mount(<ModalDetailviewRenameFile
        isOpen={true}
        handleExit={() => { }}></ModalDetailviewRenameFile>);

      modal.find('[data-name="filename"]').getDOMNode().textContent = "name.jpg";
      modal.find('[data-name="filename"]').simulate('input');

      modal.find('.btn--default').simulate('click');

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSyncRename(), "f=%2Ftest%2Fimage.jpg&to=%2Ftest%2Fname.jpg");
    });

  });

});