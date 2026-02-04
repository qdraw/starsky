import AspectRatio from "./aspect-ratio";

describe("AspectRatio", () => {
  const aspectRatio = new AspectRatio();

  it("Dimensions = 1280 x 1024", () => {
    const width = 1280;
    const height = 1024;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(256);
    expect(ratio).toBe("5:4");
  });

  it("1152 x 960", () => {
    const width = 1152;
    const height = 960;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(192);
    expect(ratio).toBe("6:5");
  });

  it("1280 x 960", () => {
    const width = 1280;
    const height = 960;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(320);
    expect(ratio).toBe("4:3");
  });

  it("1920 x 1080", () => {
    const width = 1920;
    const height = 1080;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(120);
    expect(ratio).toBe("16:9");
  });

  it("4032 × 2688", () => {
    const width = 4032;
    const height = 2688;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(1344);
    expect(ratio).toBe("3:2");
  });

  it("4000 × 4000", () => {
    const width = 4000;
    const height = 4000;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(4000);
    expect(ratio).toBe("1:1");
  });

  it("0 × 10", () => {
    const width = 0;
    const height = 10;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(10);
    expect(ratio).toBe("0:1");
  });

  it("10 × 0", () => {
    const width = 10;
    const height = 0;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(10);
    expect(ratio).toBe("1:0");
  });

  it("4240 × 2832 (filter)", () => {
    const width = 4240;
    const height = 2832;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height, true);

    expect(gcd).toBe(16);
    expect(ratio).toBe(null);
  });

  it("4240 × 2832 (no-filter)", () => {
    const width = 4240;
    const height = 2832;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height, false);

    expect(gcd).toBe(16);
    expect(ratio).toBe("265:177");
  });

  it("3758 × 2505", () => {
    const width = 3758;
    const height = 2505;
    const gcd = aspectRatio.gcd(width, height);
    const ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(1);
    expect(ratio).toBe(null);
  });
});
