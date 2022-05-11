import { Slugify } from "./slugify";

describe("Slugify", () => {

  it("lowercase", () => {
    var slugifyResult = Slugify("Test");
    expect(slugifyResult).toBe("test");
  });

  it("trim", () => {
    var slugifyResult = Slugify("    test");
    expect(slugifyResult).toBe("test");
  });

  it("space", () => {
    var slugifyResult = Slugify("test test");
    expect(slugifyResult).toBe("test-test");
  });

  it("remove $$", () => {
    var slugifyResult = Slugify("test$$$test");
    expect(slugifyResult).toBe("testtest");
  });

  it("replace ----", () => {
    var slugifyResult = Slugify("test-----test");
    expect(slugifyResult).toBe("test-test");
  });
});
