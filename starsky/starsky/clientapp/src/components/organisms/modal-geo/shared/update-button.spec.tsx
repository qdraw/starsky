import { fireEvent, render, waitFor } from "@testing-library/react";
import localization from "../../../../localization/localization.json";
import { Language, SupportedLanguages } from "../../../../shared/language";
import { ILatLong } from "../modal-geo";
import { UpdateButton } from "./update-button";
import { UpdateGeoLocation } from "./update-geo-location";

jest.mock("./update-geo-location");

describe("UpdateButton", () => {
  const parentDirectory = "parentDirectory";
  const selectedSubPath = "selectedSubPath";
  const location: ILatLong = { latitude: 0, longitude: 0 };
  const setError = jest.fn();
  const setIsLoading = jest.fn();
  const propsCollections = true;
  const handleExit = jest.fn();
  const latitude = 0;
  const longitude = 0;
  const language = new Language(SupportedLanguages.en);

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("calls UpdateGeoLocation when clicked and updates location", async () => {
    (UpdateGeoLocation as jest.Mock).mockResolvedValueOnce({} as any);
    const { getByTestId } = render(
      new UpdateButton(
        parentDirectory,
        selectedSubPath,
        location,
        setError,
        setIsLoading,
        propsCollections
      ).updateButton(true, handleExit, latitude, longitude, language)
    );
    const button = getByTestId("update-geo-location");
    fireEvent.click(button);
    await waitFor(() => expect(UpdateGeoLocation).toHaveBeenCalledTimes(1));
    expect(handleExit).toHaveBeenCalledTimes(1);
  });

  it("disables button when location is not updated", () => {
    const { getByText } = render(
      new UpdateButton(
        parentDirectory,
        selectedSubPath,
        location,
        setError,
        setIsLoading,
        propsCollections
      ).updateButton(false, handleExit, latitude, longitude, language)
    );
    const button = getByText(
      language.key(localization.MessageAddLocation)
    ) as HTMLButtonElement;
    expect(button.disabled).toBe(true);
  });

  it("enables button when location is updated", () => {
    const { getByText } = render(
      new UpdateButton(
        parentDirectory,
        selectedSubPath,
        location,
        setError,
        setIsLoading,
        propsCollections
      ).updateButton(true, handleExit, latitude, longitude, language)
    );
    const button = getByText(
      language.key(localization.MessageAddLocation)
    ) as HTMLButtonElement;

    expect(button.disabled).toBe(false);
  });
});
