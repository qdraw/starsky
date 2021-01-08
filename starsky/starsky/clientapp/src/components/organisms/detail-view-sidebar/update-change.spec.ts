import { UpdateChange } from "./update-change";

describe("Update Change", () => {
  it("renders", () => {});

  describe("with Context", () => {
    it("renders", () => {
      new UpdateChange({} as any, jest.fn(), jest.fn(), {} as any, {} as any);
    });
  });
});
