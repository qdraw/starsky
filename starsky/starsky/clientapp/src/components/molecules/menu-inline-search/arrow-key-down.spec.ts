import ArrowKeyDown from "./arrow-key-down";

describe("ArrowKeyDown", () => {
  it("not arrow down or up", () => {
    const callback = jest.fn();
    ArrowKeyDown(
      {
        key: "t"
      } as any,
      0,
      callback,
      document.createElement("input"),
      []
    );

    expect(callback).toBeCalled();
    expect(callback).toBeCalledWith(-1);
  });

  it("inputFormControlReferenceCurrent is null", () => {
    const callback = jest.fn();
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as any,
      0,
      callback,
      null,
      []
    );

    expect(callback).toBeCalledTimes(0);
  });

  it("suggest has nothing", () => {
    const callback = jest.fn();
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as any,
      0,
      callback,
      document.createElement("input"),
      []
    );

    expect(callback).toBeCalledTimes(0);
  });

  it("1 arrow down", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as any,
      -1,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toBeCalled();
    expect(callback).toBeCalledWith(0);
    expect(inputElement.value).toBe("test");
  });

  it("1 arrow up", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowUp"
      } as any,
      1,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toBeCalled();
    expect(callback).toBeCalledWith(0);
    expect(inputElement.value).toBe("test");
  });

  it("end of arrow down list", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as any,
      1,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toBeCalledTimes(0);
  });

  it("before of arrow up list", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowUp"
      } as any,
      0,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toBeCalledTimes(0);
  });
});
