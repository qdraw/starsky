import modalInsertPortalDiv from "./modal-insert-portal-div";

describe("modalInserPortalDiv", () => {
  it("should add div element", () => {
    modalInsertPortalDiv(
      { current: null } as any,
      false,
      jest.fn() as any,
      "test-id"
    );

    const element = document.querySelector("#test-id");
    expect(element).not.toBeNull();
  });

  it("should setForceUpdate", () => {
    const forceUpdateSpy = jest.fn();
    modalInsertPortalDiv(
      { current: null } as any,
      false,
      forceUpdateSpy,
      "test-id"
    );

    expect(forceUpdateSpy).toBeCalled();
  });

  it("should not setForceUpdate", () => {
    const forceUpdateSpy = jest.fn();
    modalInsertPortalDiv(
      { current: null } as any,
      true,
      forceUpdateSpy,
      "test-id"
    );

    expect(forceUpdateSpy).not.toBeCalled();
  });
});
