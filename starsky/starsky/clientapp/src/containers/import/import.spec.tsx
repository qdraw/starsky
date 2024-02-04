import { render } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import * as DropArea from "../../components/atoms/drop-area/drop-area";
// import * as Modal from "../../components/atoms/modal/modal";
import { newIFileIndexItem } from "../../interfaces/IFileIndexItem";
import { Import } from "./import";

describe("Import", () => {
  it("clears the drop area upload files list when modal is closed", () => {
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

    expect(modalSpy1).toHaveBeenCalledTimes(2);

    container.rerender(<Import />);

    expect(container.getByTestId("modal-drop-area-files-added")).toBeTruthy();

    container.unmount();
  });
});
