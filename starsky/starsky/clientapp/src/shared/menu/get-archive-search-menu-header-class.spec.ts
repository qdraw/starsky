import { GetArchiveSearchMenuHeaderClass } from "./get-archive-search-menu-header-class";

describe("GetArchiveSearchMenuHeaderClass", () => {
  it("returns the correct class when sidebar is true", () => {
    const sidebar = true;
    const select = undefined;
    const expectedClass = "header header--main header--select header--edit";
    const result = GetArchiveSearchMenuHeaderClass(sidebar, select);
    expect(result).toEqual(expectedClass);
  });

  it("returns the correct class when select is not empty", () => {
    const sidebar = false;
    const select = ["option1", "option2"];
    const expectedClass = "header header--main header--select";
    const result = GetArchiveSearchMenuHeaderClass(sidebar, select);
    expect(result).toEqual(expectedClass);
  });

  it("returns the correct class when sidebar and select are falsey", () => {
    const sidebar = false;
    const select = undefined;
    const expectedClass = "header header--main";
    const result = GetArchiveSearchMenuHeaderClass(sidebar, select);
    expect(result).toEqual(expectedClass);
  });
});
