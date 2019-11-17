import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IFileIndexItem, newIFileIndexItem } from '../interfaces/IFileIndexItem';
import * as FetchPost from '../shared/fetch-post';
import DropArea from './drop-area';

describe("DropArea", () => {

  it("renders", () => {
    shallow(<DropArea></DropArea>)
  });


  it("Test Drop", () => {

    const scrollToSpy = jest.fn();
    window.scrollTo = scrollToSpy;

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockFetchAsXml: Promise<IFileIndexItem[]> = Promise.resolve([newIFileIndexItem()] as IFileIndexItem[]);
    var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockFetchAsXml);

    act(() => {
      // to use with: => import { act } from 'react-dom/test-utils';
      mount(<DropArea enableDragAndDrop={true}></DropArea>);
    });

    // Create a non-null file
    var event: CustomEvent & { dataTransfer?: DataTransfer } = new CustomEvent("CustomEvent");

    const fileContents = "file contents";
    const file = new Blob([fileContents], { type: "text/plain" });

    event.initCustomEvent('drop', true, true, null)
    event.dataTransfer = { files: [file] } as unknown as DataTransfer

    act(() => {
      document.dispatchEvent(event);
    });

    var compareFormData = new FormData();
    compareFormData.append("files", file);


    expect(spy).toBeCalled()
    expect(spy).toBeCalledTimes(1);
    expect(spy).toBeCalledWith("/import", compareFormData)

    scrollToSpy.mockClear();
  });
  it("Test Drag", () => {

  });

  // it("WIP renders2", () => {

  //   const scrollToSpy = jest.fn();
  //   window.scrollTo = scrollToSpy;


  //   const fileUploaderMock = jest.fn();
  //   const component = mount(<DropArea></DropArea>);

  //   const file = {
  //     name: 'test.jpg',
  //     type: 'image/jpg',
  //   } as File;

  //   const fileList: any = {
  //     length: 1,
  //     item: () => null,
  //     0: file,
  //   };

  //   const event = {
  //     currentTarget: {
  //       files: fileList,
  //     }
  //   } as React.ChangeEvent<HTMLInputElement>;

  //   window.dispatchEvent(event as Event);

  //   component.find('[data-name="tags"]').getDOMNode().textContent = "a";
  //   component.simulate('input', { key: 'a' })

  //   // document.dispatchEvent(event);

  //   // simulateEvent(component, "mousedown", event);

  //   // const instance = component.instance() as DropArea;

  //   // instance.handleUploadFile(event);
  //   // expect(fileUploaderMock).toBeCalledWith(fileList);

  //   scrollToSpy.mockClear();
  // });
});


// test('uploads the file after a click event', () => {
//   const fileUploaderMock = jest.fn();
//   const component = shallow(<DropArea></DropArea>);

//   const file = {
//     name: 'test.jpg',
//     type: 'image/jpg',
//   } as File;

//   const fileList: FileList = {
//     length: 1,
//     item: () => null,
//     0: file,
//   };

//   const event = {
//     currentTarget: {
//       files: fileList,
//     }
//   } as React.ChangeEvent<HTMLInputElement>;

//   const instance = component.instance() as ParentComponent;
//   instance.handleUploadFile(event);
//   expect(fileUploaderMock).toBeCalledWith(fileList);
// });

// https://medium.com/@evanteague/creating-fake-test-events-with-typescript-jest-778018379d1e