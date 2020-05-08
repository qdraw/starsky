import { act } from '@testing-library/react';
import { mount, shallow } from 'enzyme';
import React from 'react';
import * as useFetch from '../../../hooks/use-fetch';
import * as useInterval from '../../../hooks/use-interval';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as FetchPost from '../../../shared/fetch-post';
import * as Modal from '../../atoms/modal/modal';
import ModalDownload from './modal-download';

describe("ModalDownload", () => {

  it("renders", () => {
    // interface IModalExportProps {
    //   isOpen: boolean;
    //   select: Array<string> | undefined;
    //   handleExit: Function;
    // }
    shallow(<ModalDownload collections={false} select={["/"]} isOpen={true} handleExit={() => { }}></ModalDownload>)
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

    var modal = mount(<ModalDownload collections={false} select={["/"]} isOpen={true} handleExit={() => { }}></ModalDownload>)

    expect(useFetchSpy).toBeCalled();
    expect(modal.exists('[data-test="thumbnail"]')).toBeTruthy();
    expect(modal.exists('[data-test="orginal"]')).toBeTruthy();

    // and clean afterwards
    act(() => {
      jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
      modal.unmount();
    });
  });

  it("Multiple Files -> click download ", () => {
    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 200, data: null
    } as IConnectionDefault;

    jest.spyOn(useFetch, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    jest.spyOn(useInterval, 'default').mockImplementationOnce(() => { })
      .mockImplementationOnce(() => { })

    // use ==> import * as FetchGet from '../shared/fetch-get';
    const mockFetchGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200, data: null
    } as IConnectionDefault);

    var fetchPostSpy = jest.spyOn(FetchPost, 'default')
      .mockImplementationOnce(() => mockFetchGetIConnectionDefault)

    var modal = mount(<ModalDownload collections={false} select={["/file0", "/file1.jpg"]} isOpen={true} handleExit={() => { }}></ModalDownload>)

    var item = modal.find('[data-test="thumbnail"]');

    act(() => {
      item.simulate('click');
    });

    expect(fetchPostSpy).toBeCalled();

    // and clean afterwards
    jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
    act(() => {
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

    var modal = mount(<ModalDownload collections={false} select={["/"]} isOpen={true} handleExit={() => { }}></ModalDownload>)

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

    jest.spyOn(useInterval, 'default').mockImplementationOnce(() => { });

    // use ==> import * as useFetch from '../hooks/use-fetch';
    const mockGetIConnectionDefault = {
      statusCode: 415, data: null
    } as IConnectionDefault;
    jest.spyOn(useFetch, 'default')
      .mockImplementationOnce(() => mockGetIConnectionDefault)

    var handleExitSpy = jest.fn();

    var modal = mount(<ModalDownload collections={false} select={["/"]} isOpen={true} handleExit={handleExitSpy} />);

    expect(handleExitSpy).toBeCalled();

    // and clean afterwards
    jest.spyOn(window, 'scrollTo').mockImplementationOnce(() => { });
    act(() => {
      modal.unmount();
    });
  });



});