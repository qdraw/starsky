import { storiesOf } from "@storybook/react";
import React from "react";
import { MemoryRouter } from "react-router-dom";
import NavigateLink from "./navigate-link";

storiesOf("components/atoms/navigate-link", module).add("default", () => {
	return (
		<MemoryRouter>
			<NavigateLink to="1">test</NavigateLink>
		</MemoryRouter>
	);
});
