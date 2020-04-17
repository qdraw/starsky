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
    fetchPostSpy.mockClear();
  });


  it("onClick disabled", () => {
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
    var fetchPostSpy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

    var wrapper = shallow(<ColorClassSelect collections={true} clearAfter={true} isEnabled={false} filePath={"/test1"} onToggle={(value) => { }} />);

    wrapper.find('button.colorclass--2').simulate('click');

    // expect [disabled]
    expect(fetchPostSpy).toHaveBeenCalledTimes(0);

    // Cleanup: To avoid that mocks are shared
    fetchPostSpy.mockClear();
  });

  // xit("test hide 1 second", async () => {
  //   // spy on fetch
  //   // use this import => import * as FetchPost from '../shared/fetch-post';
  //   const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({ statusCode: 200, data: [{ status: IExifStatus.Ok }] as IFileIndexItem[] });
  //   jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockIConnectionDefault);

  //   var wrapper = mount(<ColorClassSelect collections={true} clearAfter={true} isEnabled={true} filePath={"/test1"} onToggle={(value) => { }} />)
  //   console.log(wrapper.html());

  //   // need to await this click
  //   await act(async () => {
  //     await wrapper.find('button.colorclass--3').simulate('click');
  //   })

  //   wrapper.update();



  //   expect(wrapper.exists('button.colorclass--3.active')).toBeTruthy();
  //   jest.advanceTimersByTime(1000)
  //   expect(wrapper.exists('button.colorclass--3.active')).toBeTruthy();

  // });

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

    wrapper.unmount();
    fetchPostSpy.mockClear();
  });

  // it("onClick value return keypress [FAIL]", () => {

  //   const mockDismissCallback = jest.fn();
  //   document.addEventListener('keydown', mockDismissCallback)

  //   const onToggle = jest.fn();

  //   const root = document.createElement('div');
  //   document.body.appendChild(root);
  //   const wrapper = mount(<ColorClassSelect clearAfter={true} isEnabled={true} filePath={"/test2"} onToggle={onToggle}></ColorClassSelect>, { attachTo: root });

  //   const event = new KeyboardEvent('keydown', { keyCode: 49, bubbles: true } as KeyboardEventInit);
  //   const targetNode = wrapper.getDOMNode();
  //   document.dispatchEvent(event);

  //   console.log(document.body.innerHTML);


  //   // expect(onToggle).toHaveBeenCalledTimes(1);

  //   // const KEYBOARD_ESCAPE_CODE = 49;
  //   // const mockDismissCallback = jest.fn();


  //   // var wrapper = mount(<ColorClassSelect clearAfter={true} isEnabled={true} filePath={"/test2"} onToggle={mockDismissCallback}></ColorClassSelect>)

  //   // var element = wrapper.getDOMNode() as HTMLElement;
  //   // const event = new window.KeyboardEvent('keydown', { keyCode: 27, bubbles: true } as KeyboardEventInit);
  //   // document.dispatchEvent(event);

  //   // expect(mockDismissCallback).toHaveBeenCalled();

  //   // // const mockFetchAsXml: Promise<IFileIndexItem[]> = Promise.resolve(newIFileIndexItemArray());
  //   // // var spy = jest.spyOn(Query.prototype, 'queryUpdateApi').mockImplementationOnce(() => mockFetchAsXml);

  //   // var wrapper = shallow(<ColorClassSelect clearAfter={true} isEnabled={true} filePath={"/test2"} onToggle={(value) => {
  //   //   console.log(value);
  //   //   done()
  //   // }}></ColorClassSelect>)
  //   // wrapper.simulate('keydown', { keyCode: 49 }); // keyCode 49 === 1

  //   // // expect(spy).toHaveBeenCalledTimes(1);
  //   // // expect(spy).toHaveBeenCalledWith("/test", "colorClass", "1");
  // });
});
