import ArrowKeyDown from "./arrow-key-down";

describe("ArrowKeyDown", () => {
  it("not arrow down or up", () => {
    const callback = jest.fn();
    ArrowKeyDown(
      {
        key: "t"
      } as React.KeyboardEvent<HTMLInputElement>,
      0,
      callback,
      document.createElement("input"),
      []
    );

    expect(callback).toHaveBeenCalled();
    expect(callback).toHaveBeenCalledWith(-1);
  });

  it("inputFormControlReferenceCurrent is null", () => {
    const callback = jest.fn();
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as React.KeyboardEvent<HTMLInputElement>,
      0,
      callback,
      null,
      []
    );

    expect(callback).toHaveBeenCalledTimes(0);
  });

  it("suggest has nothing", () => {
    const callback = jest.fn();
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as React.KeyboardEvent<HTMLInputElement>,
      0,
      callback,
      document.createElement("input"),
      []
    );

    expect(callback).toHaveBeenCalledTimes(0);
  });

  it("1 arrow down", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as React.KeyboardEvent<HTMLInputElement>,
      -1,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toHaveBeenCalled();
    expect(callback).toHaveBeenCalledWith(0);
    expect(inputElement.value).toBe("test");
  });

  it("1 arrow up", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowUp"
      } as React.KeyboardEvent<HTMLInputElement>,
      1,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toHaveBeenCalled();
    expect(callback).toHaveBeenCalledWith(0);
    expect(inputElement.value).toBe("test");
  });

  it("end of arrow down list", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowDown"
      } as React.KeyboardEvent<HTMLInputElement>,
      1,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toHaveBeenCalledTimes(0);
  });

  it("before of arrow up list", () => {
    const callback = jest.fn();
    const inputElement = document.createElement("input");
    ArrowKeyDown(
      {
        key: "ArrowUp"
      } as React.KeyboardEvent<HTMLInputElement>,
      0,
      callback,
      inputElement,
      ["test", "test1"]
    );

    expect(callback).toHaveBeenCalledTimes(0);
  });
});
