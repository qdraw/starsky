import { Slugify } from "./slugify";

describe("Slugify", () => {
  it("test lowercase", () => {
    const slugifyResult = Slugify("Test");
    expect(slugifyResult).toBe("test");
  });

  it("test replace ----", () => {
    const slugifyResult = Slugify("test-----test");
    expect(slugifyResult).toBe("test-test");
  });

  it("test trim", () => {
    const slugifyResult = Slugify("    test");
    expect(slugifyResult).toBe("test");
  });

  it("test remove $$", () => {
    const slugifyResult = Slugify("test$$$test");
    expect(slugifyResult).toBe("testtest");
  });

  it("test space", () => {
    const slugifyResult = Slugify("test test");
    expect(slugifyResult).toBe("test-test");
  });
});
