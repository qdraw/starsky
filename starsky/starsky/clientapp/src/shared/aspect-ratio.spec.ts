import AspectRatio from './aspect-ratio';

describe("AspectRatio", () => {
  var aspectRatio = new AspectRatio();

  it("Dimensions = 1280 x 1024", () => {
    var width = 1280;
    var height = 1024
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(256);
    expect(ratio).toBe("5:4");
  });

  it("1152 x 960", () => {
    var width = 1152;
    var height = 960
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(192);
    expect(ratio).toBe("6:5");
  });

  it("1280 x 960", () => {
    var width = 1280;
    var height = 960
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(320);
    expect(ratio).toBe("4:3");
  });

  it("1920 x 1080", () => {
    var width = 1920;
    var height = 1080
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(120);
    expect(ratio).toBe("16:9");
  });

  it("4032 × 2688", () => {
    var width = 4032;
    var height = 2688
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(1344);
    expect(ratio).toBe("3:2");
  });

  it("4000 × 4000", () => {
    var width = 4000;
    var height = 4000;
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(4000);
    expect(ratio).toBe("1:1");
  });

  it("0 × 10", () => {
    var width = 0;
    var height = 10;
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(10);
    expect(ratio).toBe("0:1");
  });

  it("10 × 0", () => {
    var width = 10;
    var height = 0;
    var gcd = aspectRatio.gcd(width, height);
    var ratio = aspectRatio.ratio(width, height);

    expect(gcd).toBe(10);
    expect(ratio).toBe("1:0");
  });
});

