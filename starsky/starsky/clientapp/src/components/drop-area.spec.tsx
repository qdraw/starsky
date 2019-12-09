import { mount, shallow } from 'enzyme';
import React from 'react';
import { act } from 'react-dom/test-utils';
import { IConnectionDefault, newIConnectionDefault } from '../interfaces/IConnectionDefault';
import * as FetchPost from '../shared/fetch-post';
import DropArea from './drop-area';

describe("DropArea", () => {

  it("renders", () => {
    shallow(<DropArea></DropArea>)
  });

  describe("with events", () => {
    const exampleFile = new Blob(["file contents"], { type: "text/plain" });

    function createDnDEvent(eventType: 'dragenter' | 'dragleave' | 'dragover' | 'drop'): CustomEvent & { dataTransfer?: DataTransfer } {
      // Create a non-null file
      var event: CustomEvent & { dataTransfer?: DataTransfer } = new CustomEvent("CustomEvent");
      event.initCustomEvent(eventType, true, true, null)
      event.dataTransfer = { files: [exampleFile] } as unknown as DataTransfer;
      return event;
    }

    var scrollToSpy = jest.fn();

    beforeEach(() => {
      window.scrollTo = scrollToSpy;
    });

    afterEach(() => {
      // and clean your room afterwards
      scrollToSpy.mockClear()
    });

    it("Test Drop a file", () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockFetchAsXml: Promise<IConnectionDefault> = Promise.resolve(newIConnectionDefault());
      var spy = jest.spyOn(FetchPost, 'default').mockImplementationOnce(() => mockFetchAsXml);

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        mount(<DropArea enableDragAndDrop={true}></DropArea>);
      });

      act(() => {
        document.dispatchEvent(createDnDEvent('drop'));
      });

      var compareFormData = new FormData();
      compareFormData.append("files", exampleFile);

      expect(spy).toBeCalled()
      expect(spy).toBeCalledTimes(1);
      expect(spy).toBeCalledWith("/import", compareFormData)

    });

    it("Test dragenter", () => {
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        mount(<DropArea enableDragAndDrop={true}></DropArea>);
      });

      act(() => {
        document.dispatchEvent(createDnDEvent('dragenter'));
      });

      expect(document.body.className).toBe('drag');
    });

    it("Test dragenter and then dragleave", () => {
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        shallow(<DropArea enableDragAndDrop={true}></DropArea>);
      });

      act(() => {
        document.dispatchEvent(createDnDEvent('dragenter'));
      });

      expect(document.body.className).toBe('drag');

      act(() => {
        document.dispatchEvent(createDnDEvent('dragleave'));
      });

      expect(document.body.className).toBe('');
    });

    it("Test dragover", () => {
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        shallow(<DropArea enableDragAndDrop={true}></DropArea>);
      });

      act(() => {
        document.dispatchEvent(createDnDEvent('dragover'));
      });

      expect(document.body.className).toBe('drag');
    });

  });
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