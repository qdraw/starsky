import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IConnectionDefault, newIConnectionDefault } from '../interfaces/IConnectionDefault';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import * as FetchPost from '../shared/fetch-post';
import { UrlQuery } from '../shared/url-query';
import ColorClassSelect from './color-class-select';
import Notification from './notification';

describe("ColorClassSelect", () => {

  it("renders", () => {
    shallow(<ColorClassSelect collections={true} isEnabled={true} filePath={"/test"} onToggle={() => {
    }} />)
  });

  it("onClick value", () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200, data: [{ status: IExifStatus.Ok }] as IFileIndexItem[] });
    var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = shallow(<ColorClassSelect collections={true} clearAfter={true} isEnabled={true} filePath={"/test1"} onToggle={(value) => {
    }} />)

    wrapper.find('button.colorclass--2').simulate('click');

    // expect
    expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    expect(fetchPostSpy).toHaveBeenCalledWith(new UrlQuery().prefix + "/api/update", "f=%2Ftest1&colorclass=2&collections=true");

    // Cleanup: To avoid that mocks are shared
    fetchPostSpy.mockReset();
  });


  it("onClick disabled", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
    var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = shallow(<ColorClassSelect collections={true} clearAfter={true} isEnabled={false} filePath={"/test1"} onToggle={(value) => { }} />);

    wrapper.find('button.colorclass--2').simulate('click');

    // expect [disabled]
    expect(fetchPostSpy).toHaveBeenCalledTimes(0);

    // Cleanup: To avoid that mocks are shared
    fetchPostSpy.mockReset();
  });

  it("test hide 1 second", async () => {
    jest.useFakeTimers();
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200, data: [{ status: IExifStatus.Ok }] as IFileIndexItem[] });
    var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = mount(<ColorClassSelect collections={true} clearAfter={true} isEnabled={true} filePath={"/test1"} onToggle={(value) => { }} />)

    // need to await this click
    await act(async () => {
      await wrapper.find('button.colorclass--3').simulate('click');
    })

    wrapper.update();

    expect(wrapper.exists('button.colorclass--3.active')).toBeTruthy();

    // need to await this
    await act(async () => {
      await jest.advanceTimersByTime(1200);
    });

    wrapper.update();

    expect(wrapper.exists('button.colorclass--3.active')).toBeFalsy();

    wrapper.unmount();
    fetchPostSpy.mockReset();
    jest.useRealTimers();
  });

  it("onClick readonly file", async () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 404, data: [{ status: IExifStatus.ReadOnly }] as IFileIndexItem[] });
    var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = mount(<ColorClassSelect collections={true} clearAfter={true} isEnabled={true} filePath={"/test1"} onToggle={(value) => { }} />)

    // need to await this click
    await act(async () => {
      await wrapper.find('button.colorclass--2').simulate('click');
    })

    wrapper.update();

    expect(wrapper.exists(Notification)).toBeTruthy();

    act(() => {
      wrapper.unmount();
    })

    fetchPostSpy.mockReset();
  });


  it('Should change value when onChange was called', async () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200, data: [{ status: IExifStatus.Ok }] as IFileIndexItem[] });
    var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault)

    const component = mount(<ColorClassSelect collections={true} clearAfter={true} isEnabled={true} filePath={"/test1"} onToggle={(value) => { }} />);

    var event = new KeyboardEvent("keydown", {
      bubbles: true,
      cancelable: true,
      key: "5",
      shiftKey: true,
    });

    // need to await this
    await act(async () => {
      await window.dispatchEvent(event);
    })

    // expect
    expect(fetchPostSpy).toHaveBeenCalledTimes(1);
    expect(fetchPostSpy).toHaveBeenCalledWith(new UrlQuery().prefix + "/api/update", "f=%2Ftest1&colorclass=5&collections=true");

    component.update();
    expect(component.exists('button.colorclass--5.active')).toBeTruthy();

    // clean
    act(() => {
      component.unmount();
    })

    fetchPostSpy.mockReset();
  });

});
