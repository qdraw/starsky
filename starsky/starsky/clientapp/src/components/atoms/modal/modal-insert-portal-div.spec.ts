import modalInsertPortalDiv from "./modal-insert-portal-div";

describe("modalInsertPortalDiv", () => {
  it("should add div element", () => {
    modalInsertPortalDiv({ current: null } as any, false, jest.fn() as any, "test-id");

    const element = document.querySelector("#test-id");
    expect(element).not.toBeNull();
  });

  it("should not add div element when already exists", () => {
    const exampleDiv = document.createElement("div");
    exampleDiv.id = "test-id-2";
    exampleDiv.innerHTML = "test";

    document.body.appendChild(exampleDiv);
    modalInsertPortalDiv(
      { current: null } as React.MutableRefObject<HTMLDivElement | null>,
      false,
      jest.fn() as React.Dispatch<React.SetStateAction<boolean>>,
      "test-id-2"
    );

    const element = document.querySelector("#test-id-2");
    expect(element).not.toBeNull();
    expect(element?.innerHTML).not.toBeNull();
    expect(element?.innerHTML).toBe("test");
  });

  it("should setForceUpdate", () => {
    const forceUpdateSpy = jest.fn();
    modalInsertPortalDiv(
      { current: null } as React.MutableRefObject<HTMLDivElement | null>,
      false,
      forceUpdateSpy,
      "test-id"
    );

    expect(forceUpdateSpy).toHaveBeenCalled();
  });

  it("should not setForceUpdate", () => {
    const forceUpdateSpy = jest.fn();
    modalInsertPortalDiv(
      { current: null } as React.MutableRefObject<HTMLDivElement | null>,
      true,
      forceUpdateSpy,
      "test-id"
    );

    expect(forceUpdateSpy).not.toHaveBeenCalled();
  });
});
