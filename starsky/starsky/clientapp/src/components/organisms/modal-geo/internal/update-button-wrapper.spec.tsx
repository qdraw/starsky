import { fireEvent, render, waitFor } from "@testing-library/react";
import { IGeoLocationModel } from "../../../../interfaces/IGeoLocationModel";
import localization from "../../../../localization/localization.json";
import { Language, SupportedLanguages } from "../../../../shared/language";
import { ILatLong } from "../modal-geo";
import { UpdateButtonWrapper } from "./update-button-wrapper";
import { UpdateGeoLocation } from "./update-geo-location";

jest.mock("./update-geo-location");

describe("UpdateButtonWrapper", () => {
  const parentDirectory = "parentDirectory";
  const selectedSubPath = "selectedSubPath";
  const location: ILatLong = { latitude: 0, longitude: 0 };
  const setError = jest.fn();
  const setIsLoading = jest.fn();
  const propsCollections = true;
  const handleExit = jest.fn();
  const language = new Language(SupportedLanguages.en);

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("[wrapper] calls UpdateGeoLocation when clicked and updates location", async () => {
    (UpdateGeoLocation as jest.Mock).mockResolvedValueOnce({} as Promise<IGeoLocationModel | null>);

    const { getByTestId } = render(
      <UpdateButtonWrapper
        handleExit={handleExit}
        isLocationUpdated={true}
        parentDirectory={parentDirectory}
        location={location}
        selectedSubPath={selectedSubPath}
        setError={setError}
        setIsLoading={setIsLoading}
        propsCollections={propsCollections}
      />
    );
    const button = getByTestId("update-geo-location");
    fireEvent.click(button);
    await waitFor(() => expect(UpdateGeoLocation).toHaveBeenCalledTimes(1));
    expect(handleExit).toHaveBeenCalledTimes(1);
  });

  it("[wrapper] disables button when location is not updated", () => {
    const { getByText } = render(
      <UpdateButtonWrapper
        handleExit={handleExit}
        isLocationUpdated={false}
        parentDirectory={parentDirectory}
        location={location}
        selectedSubPath={selectedSubPath}
        setError={setError}
        setIsLoading={setIsLoading}
        propsCollections={propsCollections}
      />
    );
    const button = getByText(language.key(localization.MessageAddLocation)) as HTMLButtonElement;
    expect(button.disabled).toBe(true);
  });

  it("[wrapper] enables button when location is updated", () => {
    const { getByText } = render(
      <UpdateButtonWrapper
        handleExit={handleExit}
        isLocationUpdated={true}
        parentDirectory={parentDirectory}
        location={location}
        selectedSubPath={selectedSubPath}
        setError={setError}
        setIsLoading={setIsLoading}
        propsCollections={propsCollections}
      />
    );
    const button = getByText(language.key(localization.MessageAddLocation)) as HTMLButtonElement;

    expect(button.disabled).toBe(false);
  });
});
