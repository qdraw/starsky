import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import localization from "../../../localization/localization.json";
import * as FetchGet from "../../../shared/fetch/fetch-get";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { Language, SupportedLanguages } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import PreferencesCloudImport from "./preferences-cloud-import";

describe("PreferencesCloudImport", () => {
  it("renders providers and status", async () => {
    jest.spyOn(FetchGet, "default").mockImplementationOnce(() =>
      Promise.resolve({
        statusCode: 200,
        data: {
          providers: [
            {
              id: "dropbox-camera-uploads",
              enabled: true,
              provider: "Dropbox",
              remoteFolder: "/Camera Uploads",
              syncFrequencyMinutes: 0,
              syncFrequencyHours: 0,
              deleteAfterImport: false
            }
          ],
          isSyncInProgress: false,
          lastSyncResults: {}
        }
      })
    );

    const component = render(<PreferencesCloudImport />);

    await screen.findByTestId("cloud-import-provider-dropbox-camera-uploads");

    expect(screen.getByTestId("cloud-import-status")?.textContent).toContain("Idle");

    component.unmount();
  });

  it("should disable sync when a sync is in progress", async () => {
    jest.spyOn(FetchGet, "default").mockImplementationOnce(() =>
      Promise.resolve({
        statusCode: 200,
        data: {
          providers: [
            {
              id: "dropbox-camera-uploads",
              enabled: true,
              provider: "Dropbox",
              remoteFolder: "/Camera Uploads",
              syncFrequencyMinutes: 0,
              syncFrequencyHours: 0,
              deleteAfterImport: false
            }
          ],
          isSyncInProgress: true,
          lastSyncResults: {}
        }
      })
    );

    const component = render(<PreferencesCloudImport />);

    const syncButton = await screen.findByTestId("cloud-import-sync-dropbox-camera-uploads");
    expect((syncButton as HTMLButtonElement).disabled).toBe(true);

    component.unmount();
  });

  it("waits for start-sync request before allowing another start", async () => {
    const fetchGetPromise = Promise.resolve({
      statusCode: 200,
      data: {
        providers: [
          {
            id: "dropbox-camera-uploads",
            enabled: true,
            provider: "Dropbox",
            remoteFolder: "/Camera Uploads",
            syncFrequencyMinutes: 0,
            syncFrequencyHours: 0,
            deleteAfterImport: false
          }
        ],
        isSyncInProgress: false,
        lastSyncResults: {}
      }
    });
    jest
      .spyOn(FetchGet, "default")
      .mockImplementationOnce(() => fetchGetPromise)
      .mockImplementationOnce(() => fetchGetPromise);

    let resolvePost: ((value: unknown) => void) | undefined;
    const postPromise = new Promise((resolve) => {
      resolvePost = resolve;
    });

    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementation(() => postPromise as Promise<{ statusCode: number; data: unknown }>);

    const component = render(<PreferencesCloudImport />);

    const syncButton = await screen.findByTestId("cloud-import-sync-dropbox-camera-uploads");
    fireEvent.click(syncButton);

    await waitFor(() => {
      expect(
        (screen.getByTestId("cloud-import-sync-dropbox-camera-uploads") as HTMLButtonElement)
          .disabled
      ).toBe(true);
    });

    fireEvent.click(syncButton);
    expect(fetchPostSpy).toHaveBeenCalledTimes(1);

    resolvePost?.({ statusCode: 200, data: {} });

    await waitFor(() => {
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlCloudImportSync("dropbox-camera-uploads"),
        ""
      );
    });

    component.unmount();
  });

  it("shows error and re-enables button when starting sync fails", async () => {
    jest.spyOn(FetchGet, "default").mockImplementationOnce(() =>
      Promise.resolve({
        statusCode: 200,
        data: {
          providers: [
            {
              id: "dropbox-camera-uploads",
              enabled: true,
              provider: "Dropbox",
              remoteFolder: "/Camera Uploads",
              syncFrequencyMinutes: 0,
              syncFrequencyHours: 0,
              deleteAfterImport: false
            }
          ],
          isSyncInProgress: false,
          lastSyncResults: {}
        }
      })
    );

    const fetchPostSpy = jest.spyOn(FetchPost, "default").mockImplementation(() =>
      Promise.resolve({
        statusCode: 500,
        data: null
      })
    );

    const component = render(<PreferencesCloudImport />);

    const syncButton = (await screen.findByTestId(
      "cloud-import-sync-dropbox-camera-uploads"
    )) as HTMLButtonElement;
    fireEvent.click(syncButton);

    await waitFor(() => {
      expect(fetchPostSpy).toHaveBeenCalledWith(
        new UrlQuery().UrlCloudImportSync("dropbox-camera-uploads"),
        ""
      );
    });

    await waitFor(() => {
      expect(syncButton.disabled).toBe(false);
    });

    const warning = screen.getByText(localization.MessageCloudImportSyncStartFail.en);
    expect(warning).toBeTruthy();

    component.unmount();
  });

  it("should disable sync when a sync is in progress", async () => {
    jest.spyOn(FetchGet, "default").mockImplementation(() =>
      Promise.resolve({
        statusCode: 500,
        data: null
      })
    );
    const language = new Language(SupportedLanguages.en);
    const messageStatusUnavailable = language.key(localization.MessageCloudImportStatusUnavailable);

    const component = render(<PreferencesCloudImport />);

    const errorMessage = await screen.findByTestId("cloud-import-status-error");
    expect(errorMessage).toHaveTextContent(messageStatusUnavailable);

    component.unmount();
  });
});
