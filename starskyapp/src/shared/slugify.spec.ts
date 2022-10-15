import { Slugify } from "./slugify";

describe("Slugify", () => {
  it("lowercase", () => {
    const slugifyResult = Slugify("Test");
    expect(slugifyResult).toBe("test");
  });

  it("trim", () => {
    const slugifyResult = Slugify("    test");
    expect(slugifyResult).toBe("test");
  });

  it("space", () => {
    const slugifyResult = Slugify("test test");
    expect(slugifyResult).toBe("test-test");
  });

  it("remove $$", () => {
    const slugifyResult = Slugify("test$$$test");
    expect(slugifyResult).toBe("testtest");
  });

  it("replace ----", () => {
    const slugifyResult = Slugify("test-----test");
    expect(slugifyResult).toBe("test-test");
  });
});
