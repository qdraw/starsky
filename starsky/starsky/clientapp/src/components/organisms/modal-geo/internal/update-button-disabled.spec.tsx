import { render } from "@testing-library/react";
import localization from "../../../../localization/localization.json";
import { Language, SupportedLanguages } from "../../../../shared/language";
import { ILatLong } from "../modal-geo";
import { UpdateButtonDisabled } from "./update-button-disabled";

describe("UpdateButtonActive", () => {
  const location: ILatLong = { latitude: 0, longitude: 0 };
  const language = new Language(SupportedLanguages.en);

  it("[disabledBtn] disables button when location is not updated", () => {
    const { getByText } = render(
      <UpdateButtonDisabled
        language={language}
        latitude={location.latitude}
        longitude={location.longitude}
      />
    );
    const button = getByText(language.key(localization.MessageAddLocation)) as HTMLButtonElement;
    expect(button.disabled).toBe(true);
  });
});
