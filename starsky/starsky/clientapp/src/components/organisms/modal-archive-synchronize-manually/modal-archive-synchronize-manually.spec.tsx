import { mount, ReactWrapper, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as FetchGet from '../../../shared/fetch-get';
import { UrlQuery } from '../../../shared/url-query';
import ModalArchiveSynchronizeManually from './modal-archive-synchronize-manually';

describe("ModalArchiveSynchronizeManually", () => {

  it("renders", () => {
    shallow(<ModalArchiveSynchronizeManually
      isOpen={true}
      parentFolder="/"
      handleExit={() => { }}>
      test
    </ModalArchiveSynchronizeManually>)
  });

  describe("with Context", () => {
    describe("buttons exist", () => {

      var modal: ReactWrapper;
      beforeAll(() => {
        modal = mount(<ModalArchiveSynchronizeManually parentFolder={"/"} isOpen={true} handleExit={() => { }} />)
      });

      afterAll(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
          modal.unmount();
        });
      });

      it("force-sync", () => {
        expect(modal.exists('[data-test="force-sync"]')).toBeTruthy();
      });
      it("remove-cache", () => {
        expect(modal.exists('[data-test="remove-cache"]')).toBeTruthy();
      });
      it("geo-sync", () => {
        expect(modal.exists('[data-test="geo-sync"]')).toBeTruthy();
      });
      it("thumbnail-generation", () => {
        expect(modal.exists('[data-test="thumbnail-generation"]')).toBeTruthy();
      });
    });

    describe("click button", () => {
      var modal: ReactWrapper;
      beforeEach(() => {
        jest.useFakeTimers();
        modal = mount(<ModalArchiveSynchronizeManually parentFolder={"/"} isOpen={true} handleExit={() => { }} />)
      });

      afterEach(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
          modal.unmount();
        });
        jest.useRealTimers();
      })

      it("force-sync (only first get)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
          statusCode: 200, data: null
        } as IConnectionDefault);

        var fetchGetSpy = jest.spyOn(FetchGet, 'default')
          .mockImplementationOnce(() => mockGetIConnectionDefault)

        modal.find('[data-test="force-sync"]').simulate('click');

        expect(fetchGetSpy).toBeCalled();
        expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlSync("/"));

        fetchGetSpy.mockReset();
      });

      it("remove-cache (only first get)", () => {
        const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
          statusCode: 200, data: null
        } as IConnectionDefault);

        var fetchGetSpy = jest.spyOn(FetchGet, 'default')
          .mockImplementationOnce(() => mockGetIConnectionDefault)

        act(() => {
          modal.find('[data-test="remove-cache"]').simulate('click');
        });

        expect(fetchGetSpy).toBeCalled();
        expect(fetchGetSpy).toBeCalledWith(new UrlQuery().UrlRemoveCache("/"));

        fetchGetSpy.mockReset();
      });

      it("geo-sync (only first post)", () => {

        // FetchPOST is magicly not working
        var urlGeoSyncUrlQuerySpy = jest.spyOn(UrlQuery.prototype, 'UrlGeoSync')
          .mockImplementationOnce(() => "")

        expect(modal.exists('[data-test="geo-sync"]')).toBeTruthy();

        modal.find('[data-test="geo-sync"]').simulate('click');
        expect(urlGeoSyncUrlQuerySpy).toBeCalled();

      });
    });
  });

});
