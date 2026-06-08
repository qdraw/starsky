import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import DropArea from "./drop-area";
import { PostSingleFormData } from "./post-single-form-data";

describe("DropArea", () => {
  it("renders", () => {
    const item = render(<DropArea endpoint="/import" />);
    expect(item).toBeTruthy();
  });

  it("does not upload selected files when endpoint is missing", () => {
    const fetchPostSpy = jest.spyOn(FetchPost, "default");
    const selectedFile = new File(["file contents"], "test.json", {
      type: "application/json"
    });

    render(<DropArea enableInputButton={true} />);

    fireEvent.change(screen.getByTestId("droparea-file-input"), {
      target: { files: [selectedFile] }
    });

    expect(fetchPostSpy).not.toHaveBeenCalled();
  });

  describe("with events", () => {
    const exampleFile = new Blob(["file contents"], { type: "text/plain" });
    let originalVisibilityState: DocumentVisibilityState;

    function createDnDEvent(
      eventType: "dragenter" | "dragleave" | "dragover" | "drop"
    ): CustomEvent & { dataTransfer?: DataTransfer } {
      // Create a non-null file
      const event: CustomEvent & {
        dataTransfer?: DataTransfer;
      } = new CustomEvent("CustomEvent");
      event.initCustomEvent(eventType, true, true, null);
      event.dataTransfer = {
        files: [exampleFile],
        types: ["", "Files"]
      } as unknown as DataTransfer;
      return event;
    }

    const scrollToSpy = jest.fn();

    beforeEach(() => {
      globalThis.scrollTo = scrollToSpy;
      originalVisibilityState = document.visibilityState;
      Object.defineProperty(document, "visibilityState", {
        configurable: true,
        value: "visible"
      });
    });

    afterEach(() => {
      // and clean your room afterwards
      scrollToSpy.mockClear();
      Object.defineProperty(document, "visibilityState", {
        configurable: true,
        value: originalVisibilityState
      });
      document.body.classList.remove("drag");
    });

    it("Test Drop a file", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: [
          {
            status: IExifStatus.Ok,
            fileName: "rootfilename.jpg",
            fileIndexItem: {
              description: "",
              fileHash: undefined,
              fileName: "test.jpg",
              filePath: "/test.jpg",
              isDirectory: false,
              status: "Ok",
              tags: "",
              title: ""
            }
          }
        ]
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const callbackSpy = jest.fn();
      render(<DropArea callback={callbackSpy} endpoint="/import" enableDragAndDrop={true} />);

      // need to await here
      await act(async () => {
        await document.dispatchEvent(createDnDEvent("drop"));
      });

      const compareFormData = new FormData();
      compareFormData.append("file", exampleFile);

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(fetchPostSpy).toHaveBeenCalledTimes(1);
      expect(fetchPostSpy).toHaveBeenCalledWith("/import", compareFormData, "post", {
        to: undefined
      });

      // callback
      expect(callbackSpy).toHaveBeenCalled();

      expect(callbackSpy).toHaveBeenCalledWith([
        {
          description: "",
          fileHash: undefined,
          fileName: "test.jpg",
          filePath: "/test.jpg",
          isDirectory: false,
          lastEdited: expect.any(String),
          status: "Ok",
          tags: "",
          title: ""
        }
      ]);
    });

    it("Test dragenter", () => {
      render(<DropArea endpoint="/import" enableDragAndDrop={true} />);

      act(() => {
        document.dispatchEvent(createDnDEvent("dragenter"));
      });

      expect(document.body.className).toBe("drag");
    });

    it("Test dragenter and then dragleave", () => {
      // to use with: => import { act } from 'react-dom/test-utils';
      render(<DropArea endpoint="/import" enableDragAndDrop={true} />);

      act(() => {
        document.dispatchEvent(createDnDEvent("dragenter"));
      });

      expect(document.body.className).toBe("drag");

      act(() => {
        document.dispatchEvent(createDnDEvent("dragleave"));
      });

      expect(document.body.className).toBe("");
    });

    it("Test dragover", () => {
      // to use with: => import { act } from 'react-dom/test-utils';
      render(<DropArea endpoint="/import" enableDragAndDrop={true} />);

      act(() => {
        document.dispatchEvent(createDnDEvent("dragover"));
      });

      expect(document.body.className).toBe("drag");
    });

    it("does not activate drag style when tab is hidden", () => {
      render(<DropArea endpoint="/import" enableDragAndDrop={true} />);

      Object.defineProperty(document, "visibilityState", {
        configurable: true,
        value: "hidden"
      });

      act(() => {
        document.dispatchEvent(new Event("visibilitychange"));
      });

      act(() => {
        document.dispatchEvent(createDnDEvent("dragenter"));
      });

      expect(document.body.className).toBe("");
    });
  });

  describe("PostSingleFormData", () => {
    it("no input", () => {
      const callBackWhenReady = jest.fn();

      PostSingleFormData("/", undefined, [], 0, [], callBackWhenReady, jest.fn());
      expect(callBackWhenReady).toHaveBeenCalled();
      expect(callBackWhenReady).toHaveBeenCalledWith([]);
    });

    it("to big", () => {
      const callBackWhenReady = jest.fn();

      const file = {
        name: "test.jpg",
        type: "image/jpg",
        size: 3000000000000
      } as File;

      PostSingleFormData("/", undefined, [file], 0, [], callBackWhenReady, jest.fn());
      expect(callBackWhenReady).toHaveBeenCalled();

      expect(callBackWhenReady).toHaveBeenCalledWith([
        {
          fileName: "test.jpg",
          filePath: "test.jpg",
          status: "ServerError"
        }
      ]);
    });

    it("status Ok", (done) => {
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: [
          {
            status: IExifStatus.Ok,
            fileName: "rootfilename.jpg",
            fileIndexItem: {
              description: "",
              fileHash: undefined,
              fileName: "",
              filePath: "/test.jpg",
              isDirectory: false,
              status: "Ok",
              tags: "",
              title: ""
            }
          }
        ]
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const file = {
        name: "test.jpg",
        type: "image/jpg",
        size: 300
      } as File;

      PostSingleFormData(
        "/",
        undefined,
        [file],
        0,
        [],
        (data) => {
          expect(data).toStrictEqual([
            {
              description: "",
              fileHash: undefined,
              fileName: "",
              filePath: "/test.jpg",
              isDirectory: false,
              lastEdited: expect.any(String),
              status: "Ok",
              tags: "",
              title: ""
            }
          ]);
          expect(fetchPostSpy).toHaveBeenCalled();

          done();
        },
        jest.fn()
      );
    });

    it("no data", (done) => {
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: null
      });

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const file = {
        name: "test.jpg",
        type: "image/jpg",
        size: 300
      } as File;

      PostSingleFormData(
        "/",
        undefined,
        [file],
        0,
        [],
        (data) => {
          expect(data).toStrictEqual([
            {
              fileName: "test.jpg",
              filePath: "test.jpg",
              status: "ServerError"
            }
          ]);
          expect(fetchPostSpy).toHaveBeenCalled();

          done();
        },
        jest.fn()
      );
    });

    it("malformed array", (done) => {
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: [null]
      });

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const file = {
        name: "test.jpg",
        type: "image/jpg",
        size: 300
      } as File;

      PostSingleFormData(
        "/",
        undefined,
        [file],
        0,
        [],
        (data) => {
          expect(data).toStrictEqual([
            {
              fileName: "test.jpg",
              filePath: "test.jpg",
              status: "ServerError"
            }
          ]);
          expect(fetchPostSpy).toHaveBeenCalled();

          done();
        },
        jest.fn()
      );
    });

    it("status Error in response", (done) => {
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: [
          {
            status: IExifStatus.ServerError,
            fileName: "rootfilename.jpg",
            fileIndexItem: {
              description: "",
              fileHash: undefined,
              fileName: "test.jpg",
              filePath: "/test.jpg",
              isDirectory: false,
              status: "ServerError",
              tags: "",
              title: ""
            }
          }
        ]
      });

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const file = {
        name: "test.jpg",
        type: "image/jpg",
        size: 300
      } as File;

      PostSingleFormData(
        "/",
        undefined,
        [file],
        0,
        [],
        (data) => {
          expect(data).toStrictEqual([
            {
              description: "",
              fileHash: undefined,
              fileName: "test.jpg",
              filePath: "/test.jpg",
              isDirectory: false,
              lastEdited: expect.any(String),
              status: "ServerError",
              tags: "",
              title: ""
            }
          ]);
          expect(fetchPostSpy).toHaveBeenCalled();

          done();
        },
        jest.fn()
      );
    });

    it("status Error in response with no FileIndexItem", (done) => {
      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: [
          {
            status: IExifStatus.ServerError
          }
        ]
      });

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const file = {
        name: "test.jpg",
        type: "image/jpg",
        size: 300
      } as File;

      PostSingleFormData(
        "/",
        undefined,
        [file],
        0,
        [],
        (data) => {
          expect(data).toStrictEqual([
            {
              fileName: "test.jpg",
              fileHash: undefined,
              filePath: undefined,
              isDirectory: false,
              status: "ServerError"
            }
          ]);
          expect(fetchPostSpy).toHaveBeenCalled();

          done();
        },
        jest.fn()
      );
    });
  });
});

// https://medium.com/@evanteague/creating-fake-test-events-with-typescript-jest-778018379d1e
