import { mount, ReactWrapper, shallow } from 'enzyme';
import React from 'react';
import DropArea from './drop-area';

function simulateEvent(component: ReactWrapper, eventName: "mousedown" | "mousemove" | "mouseup", eventData: any) {
  const event = new window['MouseEvent'](eventName, eventData);
  component.getDOMNode().dispatchEvent(event);
}

describe("DropArea", () => {

  it("renders", () => {
    shallow(<DropArea></DropArea>)
  });

  it("WIP renders2", () => {

    const scrollToSpy = jest.fn();
    window.scrollTo = scrollToSpy;


    const fileUploaderMock = jest.fn();
    const component = mount(<DropArea></DropArea>);

    const file = {
      name: 'test.jpg',
      type: 'image/jpg',
    } as File;

    const fileList: any = {
      length: 1,
      item: () => null,
      0: file,
    };

    const event = {
      currentTarget: {
        files: fileList,
      }
    } as React.ChangeEvent<HTMLInputElement>;



    // document.dispatchEvent(event);

    // simulateEvent(component, "mousedown", event);

    // const instance = component.instance() as DropArea;

    // instance.handleUploadFile(event);
    // expect(fileUploaderMock).toBeCalledWith(fileList);

    scrollToSpy.mockClear();
  });
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