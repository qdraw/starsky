import { HistoryLocation } from "@reach/router";
import { IUseLocation } from "../../../hooks/use-location";
import { IDetailView } from "../../../interfaces/IDetailView";
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
});
