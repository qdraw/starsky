import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import localization from "../../../localization/localization.json";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import PreferencesImportIndexJson from "./preferences-import-index-json";

describe("PreferencesImportIndexJson", () => {
  it("renders with default state", () => {
    const component = render(<PreferencesImportIndexJson />);

    expect(screen.getByTestId("import-index-json-file-name")).toHaveTextContent(
      localization.MessageImportIndexJsonNoFileSelected.en
    );

    component.unmount();
  });

  it("imports selected json file", async () => {
    const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementation(() =>
      Promise.resolve({
        statusCode: 200,
        data: {}
      })
    );

    const component = render(<PreferencesImportIndexJson />);

    const file = new File(["{\"items\":[]}"], "import-index.json", {
      type: "application/json"
    });

    fireEvent.change(screen.getByTestId("import-index-json-file"), {
      target: { files: [file] }
    });

    fireEvent.click(screen.getByTestId("import-index-json-import-button"));

    await waitFor(() => {
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlImportIndexJsonImport(),
        "{\"items\":[]}",
        "post",
        {
          "Content-Type": "application/json"
        }
      );
    });

    expect(screen.getByTestId("import-index-json-success")).toHaveTextContent(
      localization.MessageImportIndexJsonImportSuccess.en
    );

    component.unmount();
  });

  it("shows admin-only warning when import returns forbidden", async () => {
    jest.spyOn(FetchPost, "default").mockImplementation(() =>
      Promise.resolve({
        statusCode: 403,
        data: null
      })
    );

    const component = render(<PreferencesImportIndexJson />);

    const file = new File(["{\"items\":[]}"], "import-index.json", {
      type: "application/json"
    });

    fireEvent.change(screen.getByTestId("import-index-json-file"), {
      target: { files: [file] }
    });

    fireEvent.click(screen.getByTestId("import-index-json-import-button"));

    await waitFor(() => {
      expect(screen.getByTestId("import-index-json-error")).toHaveTextContent(
        localization.MessageImportIndexJsonAdminOnly.en
      );
    });

    component.unmount();
  });

  it("exports json", async () => {
    const fetchSpy = jest.spyOn(globalThis, "fetch").mockImplementation(() =>
      Promise.resolve({
        ok: true,
        status: 200,
        text: async () => "{\"items\":[]}"
      } as Response)
    );

    const component = render(<PreferencesImportIndexJson />);

    fireEvent.click(screen.getByTestId("import-index-json-export-button"));

    await waitFor(() => {
      expect(fetchSpy).toHaveBeenCalledWith(new UrlQuery().UrlImportIndexJsonExport(), {
        method: "GET",
        credentials: "include",
        headers: {
          Accept: "application/json",
          "X-Requested-With": "XMLHttpRequest"
        }
      });
    });

    expect(screen.getByTestId("import-index-json-success")).toHaveTextContent(
      localization.MessageImportIndexJsonExportSuccess.en
    );

    component.unmount();
  });
});
