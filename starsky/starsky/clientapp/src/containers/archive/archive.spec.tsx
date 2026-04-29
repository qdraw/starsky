import { render, screen } from "@testing-library/react";
import { newIArchive } from "../../interfaces/IArchive";
import { Router } from "../../router-app/router-app";
import Archive from "./archive";

describe("Archive", () => {
  it("renders", () => {
    render(<Archive {...newIArchive()} />);
  });

  it("no colorclass usage", () => {
    const container = render(<Archive {...newIArchive()} />);
    expect(container.container.textContent).toBe("(Archive) = no colorClassLists");
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

    const warningBox = screen.queryByTestId("list-view-no-photos-in-folder") as HTMLDivElement;
    expect(warningBox).toBeTruthy();

    container.unmount();
  });

  it("shows structured filter warning when no items and structured filters are active", () => {
    Router.navigate("/?f=/&dateFrom=2026-04-01");

    const container = render(
      <Archive
        {...newIArchive()}
        colorClassActiveList={[]}
        colorClassUsage={[]}
        fileIndexItems={[]}
        collectionsCount={0}
      />
    );

    const warningBox = screen.queryByTestId("archive-structured-filter-warning") as HTMLDivElement;
    expect(warningBox).toBeTruthy();

    container.unmount();
  });

  it("renders collapsed layout when sidebar query is true", () => {
    Router.navigate("/?sidebar=true");
    const container = render(
      <Archive
        {...newIArchive()}
        colorClassActiveList={[]}
        colorClassUsage={[]}
        fileIndexItems={[]}
      />
    );

    expect(container.container.querySelector(".archive.collapsed")).toBeTruthy();
    container.unmount();
  });

  it("updates url when shared filter changes", () => {
    const navigateSpy = jest.spyOn(Router, "navigate").mockImplementation(jest.fn());
    const container = render(
      <Archive
        {...newIArchive()}
        colorClassActiveList={[]}
        colorClassUsage={[]}
        fileIndexItems={[]}
      />
    );

    screen.getByTestId("shared-filter-toggle").click();

    expect(navigateSpy).toHaveBeenCalled();

    navigateSpy.mockRestore();
    container.unmount();
  });
});
