import { render } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import {
  IRelativeObjects,
  newIRelativeObjects
} from "../../../interfaces/IDetailView";
import ArchivePagination from "./archive-pagination";

describe("ArchivePagination", () => {
  it("renders new object", () => {
    render(
      <MemoryRouter>
        <ArchivePagination relativeObjects={newIRelativeObjects()} />
      </MemoryRouter>
    );
  });

  const relativeObjects = {
    nextFilePath: "next",
    prevFilePath: "prev"
  } as IRelativeObjects;

  it("next page exist", () => {
    const Component = render(
      <MemoryRouter>
        <ArchivePagination relativeObjects={relativeObjects} />
      </MemoryRouter>
    );
    const next = Component.queryByTestId(
      "archive-pagination-next"
    ) as HTMLAnchorElement;
    expect(next.href).toBe("http://localhost/?f=next");
  });

  it("prev page exist", () => {
    const Component = render(
      <MemoryRouter>
        <ArchivePagination relativeObjects={relativeObjects} />
      </MemoryRouter>
    );
    const next = Component.queryByTestId(
      "archive-pagination-prev"
    ) as HTMLAnchorElement;
    expect(next.href).toBe("http://localhost/?f=prev");
  });
});
