import { render } from "@testing-library/react";
import React from "react";
import { MemoryRouter } from "react-router-dom";
import useLocation from "./use-location";

describe("useLocation", () => {
  const UseLocationComponentTest: React.FunctionComponent = () => {
    useLocation();
    return null;
  };

  it("check if is called once (useLocation)", () => {
    const setState = jest.fn();
    const useStateSpy = jest.spyOn(React, "useState").mockImplementationOnce(() => {
      return [setState, setState];
    });

    render(
      <MemoryRouter>
        <UseLocationComponentTest></UseLocationComponentTest>
      </MemoryRouter>
    );

    expect(useStateSpy).toHaveBeenCalled();
  });
});
