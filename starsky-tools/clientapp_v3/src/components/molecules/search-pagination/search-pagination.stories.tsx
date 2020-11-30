import { storiesOf } from "@storybook/react";
import React from "react";
import { MemoryRouter } from "react-router-dom";
import SearchPagination from "./search-pagination";

storiesOf("components/molecules/search-pagination", module).add("default", () => {
	return (
		<MemoryRouter>
			<SearchPagination lastPageNumber={2} />
		</MemoryRouter>
	);
});
