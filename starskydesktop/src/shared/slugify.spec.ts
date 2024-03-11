import { Slugify } from "./slugify";

describe("Slugify", () => {
  it("test lowercase", () => {
    const slugifyResult1 = Slugify("Test");
    // it should remove the capital at start
    expect(slugifyResult1).toBe("test");
  });

  it("test replace ----", () => {
    const slugifyResult2 = Slugify("test-----test");
    // it should give a slugified result
    expect(slugifyResult2).toBe("test-test");
  });

  it("test trim", () => {
    const slugifyResult3 = Slugify("    test");
    // it should trim before
    expect(slugifyResult3).toBe("test");
  });

  it("test remove $$", () => {
    const slugifyResult4 = Slugify("test$$$test");
    // it should remove the dollar signs
    expect(slugifyResult4).toBe("testtest");
  });

  it("test space", () => {
    const slugifyResult5 = Slugify("test test");
    // it should replace the space with a dash
    expect(slugifyResult5).toBe("test-test");
  });
});
