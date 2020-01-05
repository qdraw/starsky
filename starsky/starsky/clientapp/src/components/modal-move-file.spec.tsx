import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as useFileList from '../hooks/use-filelist';
import { IFileList } from '../hooks/use-filelist';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import { IExifStatus } from '../interfaces/IExifStatus';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import ModalMoveFile from './modal-move-file';

describe("ModalMoveFile", () => {

  it("renders", () => {
    shallow(<ModalMoveFile parentDirectory="/" selectedSubPath="/test.jpg" isOpen={true} handleExit={() => { }}></ModalMoveFile>)
  });

  it("Input Not found", () => {
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest.spyOn(useFileList, 'default').mockImplementationOnce(() => {
      return null;
    });

    var result = mount(<ModalMoveFile parentDirectory="/" selectedSubPath="/test.jpg" isOpen={true} handleExit={() => { }}></ModalMoveFile>)

    expect(result.text()).toBe('Input Not found')
  });

  const startArchive = {
    archive: {
      fileIndexItems: [
        {
          filePath: "/test/",
          fileName: "test",
          isDirectory: true,
        },
        {
          filePath: "/image.jpg",
          fileName: "image.jpg",
          status: IExifStatus.ServerError,
          isDirectory: false,
        }]
    }
  } as IFileList;

  const inTestFolderArchive = {
    archive: {
      fileIndexItems: [
        {
          filePath: "/test/photo.jpg",
          fileName: "photo.jpg",
          status: IExifStatus.Ok,
          isDirectory: false,
        }]
    }
  } as IFileList;

  it("default disabled", () => {

    // detailview get archive parent item
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest.spyOn(useFileList, 'default').mockImplementationOnce(() => {
      return startArchive;
    });

    var modal = mount(<ModalMoveFile parentDirectory="/" selectedSubPath="/test.jpg" isOpen={true} handleExit={() => { }}></ModalMoveFile>)

    expect(modal.exists('[data-test="btn-test"]')).toBeTruthy();
    expect(modal.exists('button.btn--default')).toBeTruthy();

    // can't move to the same folder
    var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
    expect(submitButtonBefore).toBeTruthy();

    jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
    modal.unmount();
  });

  it("click to folder -> move", () => {

    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest.spyOn(useFileList, 'default').mockImplementationOnce(() => {
      return startArchive;
    }).mockImplementationOnce(() => {
      return inTestFolderArchive;
    });

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: [{
        filePath: 'test'
      }]
    } as IConnectionDefault);
    var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

    var modal = mount(<ModalMoveFile parentDirectory="/" selectedSubPath="/test.jpg" isOpen={true} handleExit={() => { }}></ModalMoveFile>)

    act(() => {
      modal.find('[data-test="btn-test"]').simulate('click');
    });

    // button isn't disabled anymore
    var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
    expect(submitButtonBefore).toBeFalsy();

    act(() => {
      // now move
      modal.find('.btn--default').simulate('click');
    });

    expect(fetchPostSpy).toBeCalledTimes(1);

    // generate url
    var bodyParams = new URLSearchParams();
    bodyParams.append("f", "/test.jpg");
    bodyParams.append("to", "/test/");
    bodyParams.append("collections", true.toString());

    expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSyncRename(), bodyParams.toString());

    jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
    modal.unmount();
  });

});