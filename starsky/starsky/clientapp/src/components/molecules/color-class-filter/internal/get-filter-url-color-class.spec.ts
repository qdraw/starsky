import { GetFilterUrlColorClass } from "./get-filter-url-color-class";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps";
import { IFileIndexItem, newIFileIndexItem } from "../../../../interfaces/IFileIndexItem";

describe("GetFilterUrlColorClass", () => {
  it("get colorClass back", () => {
    // Mock necessary dependencies
    const historyLocationSearch = "your-history-location-search";
    const state = {
      fileIndexItems: {}
    } as IArchiveProps;

    // Call the function
    const result = GetFilterUrlColorClass(1, historyLocationSearch, state);

    // Assertions
    expect(result).toBe("?colorClass=1");
  });

  it("get select back", () => {
    // Mock necessary dependencies
    const historyLocationSearch = "?select=test.jpg";
    const state = {
      fileIndexItems: [
        {
          ...newIFileIndexItem(),
          fileName: "test.jpg",
          colorClass: 1
        } as IFileIndexItem
      ]
    } as IArchiveProps;

    // Call the function
    const result = GetFilterUrlColorClass(1, historyLocationSearch, state);

    // Assertions
    expect(result).toBe("?select=test.jpg&colorClass=1");
  });
});
