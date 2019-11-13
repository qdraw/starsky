import { shallow } from 'enzyme';
import React from 'react';
import { IFileIndexItem, newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import * as FetchPost from '../shared/fetch-post';
import ColorClassSelect from './color-class-select';

describe("ColorClassSelect", () => {

  it("renders", () => {
    shallow(<ColorClassSelect isEnabled={true} filePath={"/test"} onToggle={() => { }}></ColorClassSelect>)
  });

  it("onClick value", () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<IFileIndexItem[]> = Promise.resolve(newIFileIndexItemArray());
    var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var wrapper = shallow(<ColorClassSelect clearAfter={true} isEnabled={true} filePath={"/test1"} onToggle={(value) => { }}></ColorClassSelect>)

    wrapper.find('a.colorclass--2').simulate('click');

    // expect
    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith("/api/update", "filePath=%2Ftest1");

    // Cleanup: To avoid that mocks are shared
    spy.mockClear();
  });


  it("onClick disabled", () => {
    const mockFetchAsXml: Promise<IFileIndexItem[]> = Promise.resolve(newIFileIndexItemArray());
    var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockFetchAsXml);

    var wrapper = shallow(<ColorClassSelect clearAfter={true} isEnabled={false} filePath={"/test1"} onToggle={(value) => { }}></ColorClassSelect>)

    wrapper.find('a.colorclass--2').simulate('click');

    // expect [disabled]
    expect(spy).toHaveBeenCalledTimes(0);

    // Cleanup: To avoid that mocks are shared
    spy.mockClear();
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