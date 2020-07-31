import { act } from '@testing-library/react';
import { mount, shallow } from 'enzyme';
import React from 'react';
import { IArchive, newIArchive } from '../../../interfaces/IArchive';
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import { IDetailView, PageType } from '../../../interfaces/IDetailView';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import * as FetchGet from '../../../shared/fetch-get';
import * as FetchPost from '../../../shared/fetch-post';
import { UrlQuery } from '../../../shared/url-query';
import * as Modal from '../../atoms/modal/modal';
import ModalArchiveMkdir from './modal-archive-mkdir';

describe("ModalArchiveMkdir", () => {

  it("renders", () => {
    shallow(<ModalArchiveMkdir
      isOpen={true}
      handleExit={() => { }}>
      test
    </ModalArchiveMkdir>)
  });

  describe("mkdir", () => {

    it("to non valid dirname", async () => {
      var state = {
        fileIndexItem: { status: IExifStatus.Ok, filePath: "/test/image.jpg", fileName: "image.jpg" }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues });

      var modal = mount(<ModalArchiveMkdir
        isOpen={true}
        handleExit={() => { }}></ModalArchiveMkdir>);

      var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
      expect(submitButtonBefore).toBeTruthy();

      act(() => {
        modal.find('[data-name="directoryname"]').getDOMNode().textContent = "f";
        modal.find('[data-name="directoryname"]').simulate('input');
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

    it("submit mkdir", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200 } as IConnectionDefault);

      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      // use ==> import * as FetchGet from '../shared/fetch-get';
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 202, data: {
          ...newIArchive(), fileIndexItems: [],
          pageType: PageType.Search
        } as IArchiveProps
      } as IConnectionDefault);

      var fetchGetSpy = jest.spyOn(FetchGet, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      var state = {
        subPath: '/test'
      } as IArchive;
      var contextValues = { state, dispatch: jest.fn() }

      jest.spyOn(React, 'useContext')
        .mockImplementationOnce(() => { return contextValues })
        .mockImplementationOnce(() => { return contextValues })

      var modal = mount(<ModalArchiveMkdir
        isOpen={true}
        handleExit={() => {
        }}></ModalArchiveMkdir>);

      act(() => {
        modal.find('[data-name="directoryname"]').getDOMNode().textContent = "new folder";
        modal.find('[data-name="directoryname"]').simulate('input');
      });

      // await is needed 
      await act(async () => {
        await modal.find('.btn--default').simulate('click');
      });

      // to create new directory
      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().UrlSyncMkdir(), "f=%2Ftest%2Fnew+folder");

      // to get the update
      expect(fetchGetSpy).toBeCalled();
      expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlIndexServerApi({ f: "/test" }));

      // to update context
      expect(contextValues.dispatch).toBeCalled();
      expect(contextValues.dispatch).toBeCalledWith({ "payload": { "fileIndexItems": [], "pageType": "Search" }, "type": "force-reset" });

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

      var component = mount(<ModalArchiveMkdir isOpen={true} handleExit={handleExitSpy} />);

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });

  });

});