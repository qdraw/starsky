import { HistoryLocation } from "@reach/router";
import { ModalOpenClassName } from "../../../components/atoms/modal/modal";
import { IUseLocation } from "../../../hooks/use-location";
import { IDetailView } from "../../../interfaces/IDetailView";
import { Keyboard } from "../../../shared/keyboard";
import { moveFolderUp } from "./move-folder-up";

describe("statusRemoved", () => {
  it("renders", () => {
    moveFolderUp(
      {} as KeyboardEvent,
      {} as IUseLocation,
      true,
      {} as IDetailView
    );
  });

  it("should navigate [search query on]", () => {
    const history = { location: {} as HistoryLocation } as IUseLocation;
    history.navigate = jest.fn();
    moveFolderUp(new KeyboardEvent("delete"), history, true, {
      fileIndexItem: {
        filePath: "/"
      }
    } as IDetailView);

    expect(history.navigate).toHaveBeenCalled();
  });

  it("should navigate [search query off]", () => {
    const history = { location: {} as HistoryLocation } as IUseLocation;
    history.navigate = jest.fn();
    moveFolderUp(new KeyboardEvent("delete"), history, false, {
      fileIndexItem: {
        filePath: "/"
      }
    } as IDetailView);

    expect(history.navigate).toHaveBeenCalled();
  });

  it("should not navigate due isInForm", () => {
    const history = { location: {} as HistoryLocation } as IUseLocation;
    history.navigate = jest.fn();
    jest
      .spyOn(Keyboard.prototype, "isInForm")
      .mockImplementationOnce(() => true);

    moveFolderUp(new KeyboardEvent("delete"), history, true, {
      fileIndexItem: {
        filePath: "/"
      }
    } as IDetailView);

    expect(history.navigate).toHaveBeenCalledTimes(0);
  });

  it("should not navigate due portal", () => {
    const history = { location: {} as HistoryLocation } as IUseLocation;
    history.navigate = jest.fn();
    jest
      .spyOn(Keyboard.prototype, "isInForm")
      .mockImplementationOnce(() => false);

    const portalDivName = document.createElement("div");
    portalDivName.className = ModalOpenClassName;
    document.body.appendChild(portalDivName);

    moveFolderUp(new KeyboardEvent("delete"), history, true, {
      fileIndexItem: {
        filePath: "/"
      }
    } as IDetailView);

    expect(history.navigate).toHaveBeenCalledTimes(0);
  });
});
