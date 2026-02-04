import { render } from "@testing-library/react";
import * as useGlobalSettingsModule from "../../../../hooks/use-global-settings";
import { PageType } from "../../../../interfaces/IDetailView";
import { SupportedLanguages } from "../../../../shared/language";
import { WarningBoxNoPhotosFilter } from "./warning-box-no-photos-filter";

describe("WarningBoxNoPhotosFilter", () => {
  beforeEach(() => {
    // Mock the useGlobalSettings hook
    jest.spyOn(useGlobalSettingsModule, "default").mockReturnValue({
      language: SupportedLanguages.en
    });
  });

  afterEach(() => {
    // Clear the mock after each test
    jest.restoreAllMocks();
  });

  test("renders no warning box when pageType is Loading", () => {
    const { container } = render(
      <WarningBoxNoPhotosFilter
        pageType={PageType.Loading}
        subPath="/some-path"
        items={[]}
        colorClassUsage={[]}
      />
    );

    // Assert that no warning box is rendered
    expect(container.firstChild).toBeNull();
  });

  // ... (other test cases)

  test("renders warning box when colorClassUsage is not empty", () => {
    const component = render(
      <WarningBoxNoPhotosFilter
        pageType={PageType.Archive}
        subPath="/some-path"
        items={[]}
        colorClassUsage={[1, 2, 3]}
      />
    );

    // Assert that the warning box is rendered with the correct message
    expect(component.getByTestId("list-view-message-items-outside-filter")).toBeTruthy();

    component.unmount();
  });

  test("renders warning box when items are empty", () => {
    const component = render(
      <WarningBoxNoPhotosFilter
        pageType={PageType.Archive}
        subPath="/some-path"
        items={[]}
        colorClassUsage={[]}
      />
    );

    // Assert that the warning box is rendered with the correct message
    expect(component.getByTestId("list-view-no-photos-in-folder")).toBeTruthy();

    component.unmount();
  });
});
