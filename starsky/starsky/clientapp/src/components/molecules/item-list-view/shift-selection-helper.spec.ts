import { globalHistory } from "@reach/router";
import {
	newIFileIndexItem,
	newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import { ShiftSelectionHelper } from "./shift-selection-helper";

describe("ShiftSelectionHelper", () => {
	it("items undefined", () => {
		var result = ShiftSelectionHelper(
			globalHistory,
			[],
			"test",
			undefined as any
		);
		expect(result).toBeFalsy();
	});

	it("select undefined", () => {
		var result = ShiftSelectionHelper(
			globalHistory,
			undefined as any,
			"test",
			newIFileIndexItemArray()
		);
		expect(result).toBeFalsy();
	});

	it("filePath not found", () => {
		var result = ShiftSelectionHelper(
			globalHistory,
			[],
			"test",
			newIFileIndexItemArray()
		);
		expect(result).toBeFalsy();
	});

	var exampleItems = [
		{ ...newIFileIndexItem(), fileName: "test0", filePath: "/test0" },
		{ ...newIFileIndexItem(), fileName: "test1", filePath: "/test1" },
		{ ...newIFileIndexItem(), fileName: "test2", filePath: "/test2" },
		{ ...newIFileIndexItem(), fileName: "test3", filePath: "/test3" },
		{ ...newIFileIndexItem(), fileName: "test4", filePath: "/test4" }
	];

	it("add item after and assume first is selected", () => {
		globalHistory.navigate("/");
		var result = ShiftSelectionHelper(
			globalHistory,
			[],
			"/test3",
			exampleItems
		);
		expect(globalHistory.location.search).toBe(
			"?select=test0,test3,test1,test2"
		);
		expect(result).toBeTruthy();
	});

	it("add item before", () => {
		globalHistory.navigate("/");
		var result = ShiftSelectionHelper(
			globalHistory,
			["test4"],
			"/test2",
			exampleItems
		);
		expect(globalHistory.location.search).toBe("?select=test4,test2,test3");
		expect(result).toBeTruthy();
	});

	it("add same item", () => {
		globalHistory.navigate("/");
		var result = ShiftSelectionHelper(
			globalHistory,
			["test4"],
			"/test4",
			exampleItems
		);
		expect(globalHistory.location.search).toBe("?select=test4");
		expect(result).toBeTruthy();
	});
});
