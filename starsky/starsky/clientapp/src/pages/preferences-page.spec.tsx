import { render } from "@testing-library/react";
import React from "react";
import PreferencesPage from "./preferences-page";

describe("PreferencesPage", () => {
  it("renders", () => {
    render(<PreferencesPage />);
  });
});
