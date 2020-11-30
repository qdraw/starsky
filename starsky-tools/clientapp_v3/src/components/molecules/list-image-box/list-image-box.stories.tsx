import { globalHistory } from "@reach/router";
import { storiesOf } from "@storybook/react";
import React from "react";
import { MemoryRouter } from "react-router-dom";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ListImageBox from "./list-image-box";

var fileIndexItem = {
	fileName: "test.jpg",
	colorClass: 1,
} as IFileIndexItem;

storiesOf("components/molecules/list-image-box", module)
	.add("default", () => {
		globalHistory.navigate("/");
		return (
			<MemoryRouter>
				<ListImageBox item={fileIndexItem} />
			</MemoryRouter>
		);
		// for multiple items on page see: components/molecules/item-list-view
	})
	.add("select", () => {
		globalHistory.navigate("/?select=test.jpg");
		return (
			<MemoryRouter>
				<ListImageBox item={fileIndexItem} />
			</MemoryRouter>
		);
	});
