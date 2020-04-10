import { mount, shallow } from 'enzyme';
import React from 'react';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import * as Modal from './modal';
import ModalDatetime from './modal-datetime';

describe("ModalArchiveMkdir", () => {

  it("renders", () => {
    shallow(<ModalDatetime
      isOpen={true}
      subPath="/"
      handleExit={() => { }}>
      test
    </ModalDatetime>)
  });
  describe("with Context", () => {

    beforeEach(() => {
      jest.spyOn(window, 'scrollTo')
        .mockImplementationOnce(() => { })
    });

    it("no date input", () => {
      var modal = mount(<ModalDatetime subPath={"/test"} isOpen={true} handleExit={() => { }} />)

      expect(modal.exists('.warning-box')).toBeTruthy();

      // and button is disabled
      var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
      expect(submitButtonBefore).toBeTruthy();
    });

    it("example date no error dialog", () => {
      var modal = mount(<ModalDatetime dateTime="2020-01-01T01:29:40" subPath={"/test"} isOpen={true} handleExit={() => { }} />)

      // no warning
      expect(modal.exists('.warning-box')).toBeFalsy();

      // and button is enabled
      var submitButtonBefore = (modal.find('.btn--default').getDOMNode() as HTMLButtonElement).disabled
      expect(submitButtonBefore).toBeFalsy();
    });

    it("change all options", (done) => {

      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ data: null, statusCode: 200 });
      var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

      var modal = mount(<ModalDatetime dateTime="2020-01-01T01:29:40" subPath={"/test"} isOpen={true} handleExit={() => {
        done();
      }} />)

      // update component + and blur
      modal.find('[data-name="year"]').getDOMNode().textContent = "1998";
      modal.find('[data-name="year"]').simulate('blur');

      modal.find('[data-name="month"]').getDOMNode().textContent = "12";
      modal.find('[data-name="month"]').simulate('blur');

      modal.find('[data-name="date"]').getDOMNode().textContent = "1";
      modal.find('[data-name="date"]').simulate('blur');

      modal.find('[data-name="hour"]').getDOMNode().textContent = "13";
      modal.find('[data-name="hour"]').simulate('blur');

      modal.find('[data-name="minute"]').getDOMNode().textContent = "5";
      modal.find('[data-name="minute"]').simulate('blur');

      modal.find('[data-name="sec"]').getDOMNode().textContent = "5";
      modal.find('[data-name="sec"]').simulate('blur');

      modal.find('.btn--default').simulate('click');

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(new UrlQuery().prefix + "/api/update", "f=%2Ftest&datetime=1998-12-01T13%3A05%3A05");
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, 'default').mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>
      });

      var handleExitSpy = jest.fn();

      var component = mount(<ModalDatetime subPath="/test.jpg" isOpen={true} handleExit={handleExitSpy} />);

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });


  });

});