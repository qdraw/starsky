import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { newIArchive } from "../interfaces/IArchive";
import {
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../interfaces/IFileIndexItem";
import Search from "./search";

describe("Search", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(<Search {...newIArchive()} />);
  });

  describe("Results count", () => {
    it("No results", () => {
      const component = render(
        <Search
          {...newIArchive()}
          fileIndexItems={[]}
          pageNumber={0}
          colorClassUsage={[]}
        />
      );
      const text = screen.queryByTestId("search-content-header")?.textContent;
      expect(text).toBe("No result");

      component.unmount();
    });

    it("Page 3 of 1 results", () => {
      const component = render(
        <Search
          {...newIArchive()}
          collectionsCount={1}
          fileIndexItems={[]}
          pageNumber={1}
          colorClassUsage={[]}
        />
      );
      const text = screen.queryByTestId("search-content-header")?.textContent;
      expect(text).toBe("Page 2 of 1 results");
      component.unmount();
    });

    it("Page 1 of 1 results", () => {
      const component = render(
        <Search
          {...newIArchive()}
          collectionsCount={1}
          fileIndexItems={[]}
          pageNumber={0}
          colorClassUsage={[]}
        />
      );
      const text = screen.queryByTestId("search-content-header")?.textContent;
      expect(text).toBe("1 results");
      component.unmount();
    });

    it("SearchPagination exist", () => {
      const numberOfFileIndexItems = newIFileIndexItemArray();
      for (let index = 0; index < 21; index++) {
        numberOfFileIndexItems.push(newIFileIndexItem());
      }
      const component = render(
        <BrowserRouter>
          <Search
            {...newIArchive()}
            collectionsCount={1}
            fileIndexItems={numberOfFileIndexItems}
            pageNumber={1}
            colorClassUsage={[]}
          />
        </BrowserRouter>
      );
      console.log(component.container.innerHTML);

      const searchPagination = screen.queryAllByTestId("search-pagination");

      expect(searchPagination.length).toEqual(2);
      component.unmount();
    });
  });
});
