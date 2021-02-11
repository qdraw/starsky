import { shallow } from "enzyme";
import React from "react";
import ColorClassSelectKeyboard from "./color-class-select-keyboard";

describe("ColorClassSelectKeyboard", () => {
  it("renders", () => {
    shallow(
      <ColorClassSelectKeyboard
        collections={true}
        isEnabled={true}
        filePath={"/test"}
        onToggle={() => {}}
      />
    );
  });
});
