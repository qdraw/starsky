import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import * as useFetch from '../hooks/use-fetch';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import * as Modal from './modal';
import ModalExport from './modal-export';

describe("ModalExport", () => {

  it("renders", () => {
    // interface IModalExportProps {
    //   isOpen: boolean;
    //   select: Array<string> | undefined;
    //   handleExit: Function;
    // }
    shallow(<ModalExport select={["/"]} isOpen={true} handleExit={() => { }}></ModalExport>)
  });

  beforeEach(() => {
  })

  it("Single File", () => {
    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200, data: null
    } as IConnectionDefault;
    var useFetchSpy = jest.spyOn(useFetch, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    var modal = mount(<ModalExport select={["/"]} isOpen={true} handleExit={() => { }}></ModalExport>)

    expect(useFetchSpy).toBeCalled();
    expect(modal.exists('[data-test="thumbnail"]')).toBeTruthy();
    expect(modal.exists('[data-test="orginal"]')).toBeTruthy();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });
  });

  it("file type not supported", () => {
    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 415, data: null
    } as IConnectionDefault;
    var useFetchSpy = jest.spyOn(useFetch, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    var modal = mount(<ModalExport select={["/"]} isOpen={true} handleExit={() => { }}></ModalExport>)

    expect(useFetchSpy).toBeCalled();
    expect(modal.exists('[data-test="btn-test"]')).toBeFalsy();
    expect(modal.exists('[data-test="orginal"]')).toBeTruthy();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
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

    var component = mount(<ModalExport select={["/"]} isOpen={true} handleExit={handleExitSpy} />);

    expect(handleExitSpy).toBeCalled();

    // and clean afterwards
    component.unmount();
  });



});