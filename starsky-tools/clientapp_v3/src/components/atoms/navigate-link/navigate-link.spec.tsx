import { shallow } from "enzyme";
import React from "react";
import NavigateLink from "./navigate-link";

describe("NavigateLink", () => {
	it("renders (without state component)", () => {
		shallow(<NavigateLink to="/" />);
	});
});
