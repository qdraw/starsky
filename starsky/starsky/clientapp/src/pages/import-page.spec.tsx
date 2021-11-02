import { render } from "@testing-library/react";
import React from "react";
import * as DropArea from "../components/atoms/drop-area/drop-area";
import * as ModalDropAreaFilesAdded from "../components/atoms/modal-drop-area-files-added/modal-drop-area-files-added";
import * as MenuDefault from "../components/organisms/menu-default/menu-default";
import { newIFileIndexItem } from "../interfaces/IFileIndexItem";
import ImportPage from "./import-page";

describe("ImportPage", () => {
  it("default check if MenuDefault is called", () => {
    var menuDefaultSpy = jest
      .spyOn(MenuDefault, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    var dropAreaSpy = jest
      .spyOn(DropArea, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    const component = render(<ImportPage>t</ImportPage>);
    expect(menuDefaultSpy).toBeCalled();
    expect(dropAreaSpy).toBeCalled();
    component.unmount();
  });

  it("drop area callback", () => {
    var menuDefaultSpy = jest
      .spyOn(MenuDefault, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    var dropAreaSpy = jest
      .spyOn(DropArea, "default")
      .mockImplementationOnce(({ ...props }) => {
        return <></>;
      });
    var modal = jest
      .spyOn(ModalDropAreaFilesAdded, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });

    jest
      .spyOn(React, "useState")
      .mockImplementationOnce(() => [[newIFileIndexItem()], jest.fn()])
      .mockImplementationOnce(() => [[newIFileIndexItem()], jest.fn()]);

    const component = render(<ImportPage>t</ImportPage>);
    expect(menuDefaultSpy).toBeCalled();
    expect(dropAreaSpy).toBeCalled();
    expect(modal).toBeCalled();

    component.unmount();
  });
});
