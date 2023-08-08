import { render } from "@testing-library/react";
import React from "react";
import * as DropArea from "../components/atoms/drop-area/drop-area";
import * as ModalDropAreaFilesAdded from "../components/molecules/modal-drop-area-files-added/modal-drop-area-files-added";
import * as MenuDefault from "../components/organisms/menu-default/menu-default";
import { newIFileIndexItem } from "../interfaces/IFileIndexItem";
import ImportPage from "./import-page";

describe("ImportPage", () => {
  it("default check if MenuDefault is called", () => {
    const menuDefaultSpy = jest
      .spyOn(MenuDefault, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    const dropAreaSpy = jest
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
    const menuDefaultSpy = jest
      .spyOn(MenuDefault, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    const dropAreaSpy = jest
      .spyOn(DropArea, "default")
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      .mockImplementationOnce(() => {
        return <></>;
      });
    const modal = jest
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
