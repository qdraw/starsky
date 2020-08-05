import { globalHistory } from '@reach/router';
import { mount, ReactWrapper, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as Modal from '../../atoms/modal/modal';
import ModalDisplayOptions from './modal-display-options';

describe("ModalDisplayOptions", () => {

  it("renders", () => {
    shallow(<ModalDisplayOptions
      isOpen={true}
      parentFolder="/"
      handleExit={() => { }}>
      test
    </ModalDisplayOptions>)
  });

  describe("with Context", () => {
    describe("buttons exist", () => {

      var modal: ReactWrapper;
      beforeAll(() => {
        modal = mount(<ModalDisplayOptions parentFolder={"/"} isOpen={true} handleExit={() => { }} />)
      });

      afterAll(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
          modal.unmount();
        });
      });

      it("toggle-collections", () => {
        expect(modal.exists('[data-test="toggle-collections"]')).toBeTruthy();
      });
      it("toggle-slow-files", () => {
        expect(modal.exists('[data-test="toggle-slow-files"]')).toBeTruthy();
      });
    });

    describe("click button", () => {
      var modal: ReactWrapper;
      beforeEach(() => {
        jest.useFakeTimers();
        modal = mount(<ModalDisplayOptions parentFolder={"/"} isOpen={true} handleExit={() => { }} />)
      });

      afterEach(() => {
        // and clean afterwards
        act(() => {
          jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
          modal.unmount();
        });
        jest.useRealTimers();
      })

      it("toggle-collections", () => {
        modal.find('[data-test="toggle-collections"] input').first().simulate('change');

        expect(globalHistory.location.search).toBe('?collections=false');

        modal.find('[data-test="toggle-collections"] input').last().simulate('change');

        expect(globalHistory.location.search).toBe('?collections=true');
      });

      it("toggle-slow-files", () => {
        modal.find('[data-test="toggle-slow-files"] input').first().simulate('change');

        expect(localStorage.getItem("issingleitem")).toBe('false');

        modal.find('[data-test="toggle-slow-files"] input').last().simulate('change');

        expect(localStorage.getItem("issingleitem")).toBe('true');
      });

    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, 'default').mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>
      });

      var handleExitSpy = jest.fn();

      var component = mount(<ModalDisplayOptions parentFolder="/" isOpen={true} handleExit={handleExitSpy} />);

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });

  });

});
