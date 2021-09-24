import { render } from "@testing-library/react";
import React from "react";
import SearchPagination from "../components/molecules/search-pagination/search-pagination";
import { newIArchive } from "../interfaces/IArchive";
import {
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../interfaces/IFileIndexItem";
import Search from "./search";

describe("Search", () => {
  it("renders", () => {
    render(<Search {...newIArchive()} />);
  });

  describe("Results count", () => {
    it("No results", () => {
      var component = render(
        <Search
          {...newIArchive()}
          fileIndexItems={[]}
          pageNumber={0}
          colorClassUsage={[]}
        />
      );
      var text = component.find(".content--header").text();
      expect(text).toBe("No result");
    });

    it("Page 3 of 1 results", () => {
      var component = render(
        <Search
          {...newIArchive()}
          collectionsCount={1}
          fileIndexItems={[]}
          pageNumber={1}
          colorClassUsage={[]}
        />
      );
      var text = component.find(".content--header").text();
      expect(text).toBe("Page 2 of 1 results");
    });

    it("Page 1 of 1 results", () => {
      var component = render(
        <Search
          {...newIArchive()}
          collectionsCount={1}
          fileIndexItems={[]}
          pageNumber={0}
          colorClassUsage={[]}
        />
      );
      var text = component.find(".content--header").text();
      expect(text).toBe("1 results");
    });

    it("SearchPagination exist", () => {
      var numberOfFileIndexItems = newIFileIndexItemArray();
      for (let index = 0; index < 21; index++) {
        numberOfFileIndexItems.push(newIFileIndexItem());
      }
      var component = render(
        <Search
          {...newIArchive()}
          collectionsCount={1}
          fileIndexItems={numberOfFileIndexItems}
          pageNumber={1}
          colorClassUsage={[]}
        />
      );
      var text = component.exists(SearchPagination);
      expect(text).toBeTruthy();
    });
  });
});
