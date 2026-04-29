import { render } from "@testing-library/react";
import { act } from "react";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import DropArea from "./drop-area";
import { ChunkUploadHelper } from "./chunk-upload-helper";
import { PostSingleFormData } from "./post-single-form-data";

describe("DropArea", () => {
  it("renders", () => {
    render(<DropArea endpoint="/import" />);
  });

  describe("with events", () => {
    const exampleFile = new Blob(["file contents"], { type: "text/plain" });

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
    });

    afterEach(() => {
      // and clean your room afterwards
      scrollToSpy.mockClear();
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
  });

  describe("PostSingleFormData", () => {
    it("no input", () => {
      const callBackWhenReady = jest.fn();

      PostSingleFormData("/", undefined, [], 0, [], callBackWhenReady, jest.fn());
      expect(callBackWhenReady).toHaveBeenCalled();
      expect(callBackWhenReady).toHaveBeenCalledWith([]);
    });

    it("uses chunk helper above threshold", (done) => {
      const callBackWhenReady = jest.fn();
      const fetchPostSpy = jest.spyOn(FetchPost, "default").mockResolvedValue({
        ...newIConnectionDefault(),
        data: []
      });
      fetchPostSpy.mockClear();
      const chunkUploadSpy = jest
        .spyOn(ChunkUploadHelper.prototype, "uploadFileInChunks")
        .mockResolvedValueOnce([
          {
            fileName: "test.jpg",
            filePath: "/test.jpg",
            fileCollectionName: "",
            fileHash: "",
            parentDirectory: "/",
            status: IExifStatus.Ok,
            isDirectory: false,
            lastEdited: new Date().toISOString()
          }
        ]);

      const file = {
        name: "test.jpg",
        type: "image/jpg",
        size: 3000000000000
      } as File;

      PostSingleFormData("/", undefined, [file], 0, [], (result) => {
        expect(chunkUploadSpy).toHaveBeenCalledWith(file, 0, 1);
        expect(fetchPostSpy).not.toHaveBeenCalled();
        expect(callBackWhenReady).not.toHaveBeenCalled();
        expect(result).toEqual([
          expect.objectContaining({
            fileName: "test.jpg",
            filePath: "/test.jpg",
            status: "Ok"
          })
        ]);
        done();
      }, jest.fn());
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
