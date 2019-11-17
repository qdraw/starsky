import { mount, shallow } from 'enzyme';
import React from 'react';
import DropArea from './drop-area';

// function simulateDragDrop(sourceNode: Element, destinationNode: Element) {
//   var EVENT_TYPES = {
//     DRAG_END: 'dragend',
//     DRAG_START: 'dragstart',
//     DROP: 'drop'
//   }

//   function createCustomEvent(type : string) {
//     var event: CustomEvent & { dataTransfer?: DataTransfer } = new CustomEvent("CustomEvent");

//     const fileContents = "file contents";
//     const file = new Blob([fileContents], { type: "text/plain" });

//     event.initCustomEvent(type, true, true, null)
//     event.dataTransfer = {  files: [file] } as unknown as DataTransfer }
//     return event
//   }

//   function dispatchEvent(node : HTMLElement, type : string, event: Event) {
//     if (node.dispatchEvent) {
//       return node.dispatchEvent(event)
//     }
//     if (node.fireEvent) {
//       return node.fireEvent("on" + type, event)
//     }
//   }

//   // var event = createCustomEvent(EVENT_TYPES.DRAG_START)
//   // dispatchEvent(sourceNode, EVENT_TYPES.DRAG_START, event)

//   // var dropEvent = createCustomEvent(EVENT_TYPES.DROP)
//   // dropEvent.dataTransfer = event.dataTransfer
//   // dispatchEvent(destinationNode, EVENT_TYPES.DROP, dropEvent)

//   // var dragEndEvent = createCustomEvent(EVENT_TYPES.DRAG_END)
//   // dragEndEvent.dataTransfer = event.dataTransfer
//   // dispatchEvent(sourceNode, EVENT_TYPES.DRAG_END, dragEndEvent)
// }

describe("DropArea", () => {

  it("renders", () => {
    shallow(<DropArea></DropArea>)
  });

  it("WIP renders2", () => {

    const scrollToSpy = jest.fn();
    window.scrollTo = scrollToSpy;


    const fileUploaderMock = jest.fn();
    const component = mount(<DropArea></DropArea>);

    component.getDOMNode();
    // const file = {
    //   name: 'test.jpg',
    //   type: 'image/jpg',
    // } as File;

    // const fileList: any = {
    //   length: 1,
    //   item: () => null,
    //   0: file,
    // };


    // Create a non-null file
    var event: CustomEvent & { dataTransfer?: DataTransfer } = new CustomEvent("CustomEvent");

    const fileContents = "file contents";
    const file = new Blob([fileContents], { type: "text/plain" });

    event.initCustomEvent('drop', true, true, null)
    event.dataTransfer = { files: [file] } as unknown as DataTransfer

    window.dispatchEvent(event);

    console.log(document.body.className);


    // component.find(DropArea).simulate("drop", { dataTransfer: { files: [file] } });


    // component.find('[data-name="tags"]').getDOMNode().textContent = "a";
    // component.simulate('input', { key: 'a' })

    // document.dispatchEvent(event);

    // simulateEvent(component, "mousedown", event);

    // const instance = component.instance() as DropArea;

    // instance.handleUploadFile(event);
    // expect(fileUploaderMock).toBeCalledWith(fileList);

    scrollToSpy.mockClear();
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