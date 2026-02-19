import { render } from "@testing-library/react";
import * as useSearchParams from "react-router-dom";
import { PreferencesPage } from "./preferences-page";

describe("PreferencesPage", () => {
  it("renders", () => {
    jest
      .spyOn(useSearchParams, "useSearchParams")
      .mockReturnValue([new URLSearchParams(), jest.fn()]);
    render(<PreferencesPage />);
  });
});
