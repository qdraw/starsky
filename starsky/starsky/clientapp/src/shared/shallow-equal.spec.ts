import shallowEqual from './shallow-equal';

describe("shallowEqual", function () {

  // eslint-disable-next-line no-sparse-arrays
  const falsey = [, "", 0, false, NaN, null, undefined];

  beforeEach(() => {
    // isolated instances of shallowEqual for each test.
    jest.resetModules();
  });

  // test cases copied from https://github.com/facebook/fbjs/blob/82247de1c33e6f02a199778203643eaee16ea4dc/src/core/__tests__/shallowEqual-test.js
  it("returns false if either argument is null", () => {
    expect(shallowEqual(null, {})).toEqual(false);
    expect(shallowEqual({}, null)).toEqual(false);
  });

  it("returns true if both arguments are null or undefined", () => {
    expect(shallowEqual(null, null)).toEqual(true);
    expect(shallowEqual(undefined, undefined)).toEqual(true);
  });

  it("returns true if arguments are shallow equal", () => {
    expect(shallowEqual({ a: 1, b: 2, c: 3 }, { a: 1, b: 2, c: 3 })).toEqual(
      true
    );
  });

  it("returns false if arguments are not objects and not equal", () => {
    expect(shallowEqual(1, 2)).toEqual(false);
  });

  it("returns false if only one argument is not an object", () => {
    expect(shallowEqual(1, {})).toEqual(false);
  });

  it("returns false if first argument has too many keys", () => {
    expect(shallowEqual({ a: 1, b: 2, c: 3 }, { a: 1, b: 2 })).toEqual(false);
  });

  it("returns false if second argument has too many keys", () => {
    expect(shallowEqual({ a: 1, b: 2 }, { a: 1, b: 2, c: 3 })).toEqual(false);
  });

  it("returns true if values are not primitives but are ===", () => {
    let obj = {};
    expect(
      shallowEqual({ a: 1, b: 2, c: obj }, { a: 1, b: 2, c: obj })
    ).toEqual(true);
  });

  // subsequent test cases are copied from lodash tests
  it("returns false if arguments are not shallow equal", () => {
    expect(shallowEqual({ a: 1, b: 2, c: {} }, { a: 1, b: 2, c: {} })).toEqual(
      false
    );
  });


  it("should handle comparisons if `customizer` returns `undefined`", () => {
    const noop = () => void 0;

    expect(shallowEqual("a", "a", noop)).toEqual(true);
    expect(shallowEqual(["a"], ["a"], noop)).toEqual(true);
    expect(shallowEqual({ "0": "a" }, { "0": "a" }, noop)).toEqual(true);
  });

  it("should not handle comparisons if `customizer` returns `true`", () => {
    const customizer = function (value: any) {
      return typeof value === "string" || undefined;
    };

    expect(shallowEqual("a", "b", customizer)).toEqual(true);
    expect(shallowEqual(["a"], ["b"], customizer)).toEqual(true);
    expect(shallowEqual({ "0": "a" }, { "0": "b" }, customizer)).toEqual(true);
  });

  it("should not handle comparisons if `customizer` returns `false`", () => {
    const customizer = function (value: any) {
      return typeof value === "string" ? false : undefined;
    };

    expect(shallowEqual("a", "a", customizer)).toEqual(false);
    expect(shallowEqual(["a"], ["a"], customizer)).toEqual(false);
    expect(shallowEqual({ "0": "a" }, { "0": "a" }, customizer)).toEqual(false);
  });

});
