import { render, screen } from "@testing-library/react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import * as Modal from "../../atoms/modal/modal";
import * as ItemTextListView from "../../molecules/item-text-list-view/item-text-list-view";
import ModalDropAreaFilesAdded from "./modal-drop-area-files-added";

describe("ModalDropAreaFilesAdded", () => {
  it("renders", () => {
    const component = render(
      <ModalDropAreaFilesAdded
        isOpen={true}
        uploadFilesList={[]}
        handleExit={() => {}}
      />
    );
    component.unmount();
  });

  describe("with Context", () => {
    beforeEach(() => {
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    });

    it("list is rendered", () => {
      jest
        .spyOn(ItemTextListView, "default")
        .mockImplementationOnce((props) => {
          return (
            <span data-test="data-test-0">
              {props.fileIndexItems[0].fileName}
            </span>
          );
        });

      const exampleList = [
        {
          fileName: "test.jpg",
          filePath: "/test.jpg"
        } as IFileIndexItem
      ];

      const handleExitSpy = jest.fn();

      const component = render(
        <ModalDropAreaFilesAdded
          isOpen={true}
          uploadFilesList={exampleList}
          handleExit={handleExitSpy}
        />
      );

      const dataTestId = screen.queryAllByTestId("data-test-0")[0];

      expect(dataTestId).toBeTruthy();
      expect(dataTestId.innerHTML).toBe("test.jpg");

      component.unmount();
    });

    it("test if handleExit is called", () => {
      jest.spyOn(ItemTextListView, "default").mockImplementationOnce(() => {
        return <></>;
      });

      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

      const handleExitSpy = jest.fn();

      const component = render(
        <ModalDropAreaFilesAdded
          isOpen={true}
          uploadFilesList={[]}
          handleExit={handleExitSpy}
        />
      );

      expect(handleExitSpy).toBeCalled();

      component.unmount();
    });
  });
});
