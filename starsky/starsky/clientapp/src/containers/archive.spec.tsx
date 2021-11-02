import { render } from "@testing-library/react";
import React from "react";
import { newIArchive } from "../interfaces/IArchive";
import Archive from "./archive";

describe("Archive", () => {
  it("renders", () => {
    render(<Archive {...newIArchive()} />);
  });

  it("no colorclass usage", () => {
    const container = render(<Archive {...newIArchive()} />);
    expect(container.container.textContent).toBe(
      "(Archive) = no colorClassLists"
    );
  });

  it("check if warning exist with no items in the list", () => {
    jest
      .spyOn(window, "scrollTo")
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {});

    const container = render(
      <Archive
        {...newIArchive()}
        colorClassActiveList={[]}
        colorClassUsage={[]}
        fileIndexItems={[]}
      />
    );

    const warningBox = container.queryByTestId(
      "list-view-no-photos-in-folder"
    ) as HTMLDivElement;
    expect(warningBox).toBeTruthy();
  });
});
