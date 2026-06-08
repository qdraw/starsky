import { render } from "@testing-library/react";
import { act } from "react";
import * as DropArea from "../../components/atoms/drop-area/drop-area";
import * as Modal from "../../components/atoms/modal/modal.tsx";
import * as ModalDropAreaFilesAdded from "../../components/molecules/modal-drop-area-files-added/modal-drop-area-files-added.tsx";
import * as PreferencesCloudImport from "../../components/organisms/preferences-cloud-import/preferences-cloud-import";
import { newIFileIndexItem } from "../../interfaces/IFileIndexItem";
import { Import } from "./import";

describe("Import", () => {
  beforeEach(() => {
    jest.spyOn(PreferencesCloudImport, "default").mockImplementation(() => {
      return <div data-test="preferences-cloud-import" />;
    });
  });

  it("clears the drop area upload files list when modal is closed", () => {
    jest.spyOn(Modal, "default").mockImplementationOnce(() => {
      return <div data-test="modal-drop-area-files-added"></div>;
    });

    const modalSpy1 = jest.spyOn(DropArea, "default").mockImplementationOnce((test) => {
      act(() => {
        if (test?.callback) {
          console.log("callback", new Array(newIFileIndexItem()));

          test?.callback(new Array(newIFileIndexItem()));
        }
      });
      return <div className="DropArea"></div>;
    });

    const container = render(<Import />);

    console.log(container.container.innerHTML);

    expect(modalSpy1).toHaveBeenCalledTimes(1);

    jest.spyOn(DropArea, "default").mockReset();

    container.rerender(<Import />);

    expect(container.getByTestId("modal-drop-area-files-added")).toBeTruthy();

    container.unmount();
  });

  it("no content hides modal-drop-area-files-added", () => {
    jest.spyOn(Modal, "default").mockImplementationOnce(() => {
      return <div data-test="modal-drop-area-files-added"></div>;
    });

    const modalSpy1 = jest.spyOn(DropArea, "default").mockImplementationOnce((test) => {
      act(() => {
        if (test?.callback) {
          test?.callback([]);
        }
      });
      return <div className="DropArea"></div>;
    });

    const container = render(<Import />);

    expect(modalSpy1).toHaveBeenCalledTimes(2);

    container.rerender(<Import />);

    expect(container.queryByTestId("modal-drop-area-files-added")).toBeFalsy();

    container.unmount();
  });

  it("handle exit of modal-drop-area-files-added and hide modal again", () => {
    const modalSpy = jest
      .spyOn(ModalDropAreaFilesAdded, "default")
      .mockImplementationOnce((props) => {
        props.handleExit();
        return <div data-test="modal-drop-area-files-added"></div>;
      });

    jest.spyOn(DropArea, "default").mockImplementationOnce((test) => {
      act(() => {
        if (test?.callback) {
          test?.callback(new Array(newIFileIndexItem()));
        }
      });
      return <div className="DropArea"></div>;
    });

    const container = render(<Import />);
    container.rerender(<Import />);

    expect(modalSpy).toHaveBeenCalledTimes(1);

    expect(container.queryByTestId("modal-drop-area-files-added")).toBeFalsy();

    container.unmount();
  });
});
