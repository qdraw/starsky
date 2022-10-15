import ArrayHelper from "./array-helper";

describe("ArrayHelper", () => {
  const arrayHelper = new ArrayHelper();

  it("undefined return same output", () => {
    const result = arrayHelper.UniqueResults(undefined as any, "test");
    expect(result).toBeUndefined();
  });

  it("Check duplicate values check first", () => {
    const family = [
      { name: "Nancy", age: 15 },
      { name: "Nancy", age: 2 },
      { name: "Mike", age: 10 },
      { name: "Matt", age: 13 },
      { name: "Carl", age: 40 }
    ];

    const unique = arrayHelper.UniqueResults(family, "name");
    expect(unique[0].name).toBe("Nancy");
    expect(unique[0].age).toBe(15);
  });

  it("Check duplicate values length", () => {
    const family = [
      { name: "Nancy", age: 15 },
      { name: "Nancy", age: 2 }
    ];
    const unique = arrayHelper.UniqueResults(family, "name");
    expect(unique.length).toBe(1);
  });
});
