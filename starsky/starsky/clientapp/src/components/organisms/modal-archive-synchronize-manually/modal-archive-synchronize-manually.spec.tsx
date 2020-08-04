import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
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
    describe("forceSync", () => {
      it("no date input", () => {
        // // use ==> import * as useFetch from '../hooks/use-fetch';
        // const mockGetIConnectionDefault = {
        //   statusCode: 200, data: null
        // } as IConnectionDefault;
        // var useFetchSpy = jest.spyOn(useFetch, 'default')
        //   .mockImplementationOnce(() => mockGetIConnectionDefault)

        var modal = mount(<ModalArchiveSynchronizeManually parentFolder={"/"} isOpen={true} handleExit={() => { }} />)

        expect(useFetchSpy).toBeCalled();
        expect(modal.exists('[data-test="thumbnail"]')).toBeTruthy();
        expect(modal.exists('[data-test="orginal"]')).toBeTruthy();

        // and clean afterwards
        act(() => {
          jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
          modal.unmount();
        });

      });

    });
  });

});
