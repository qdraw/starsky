import ArrayHelper from './array-helper';

describe("ArrayHelper", () => {
  var arrayHelper = new ArrayHelper();

  it("Check duplicate values check first", () => {
    var family = [
      { name: "Nancy", age: 15 },
      { name: "Nancy", age: 2 },
      { name: "Mike", age: 10 },
      { name: "Matt", age: 13 },
      { name: "Carl", age: 40 }
    ];

    var unique = arrayHelper.UniqueResults(family, 'name');
    expect(unique[0].name).toBe('Nancy');
    expect(unique[0].age).toBe(15);
  });

  it("Check duplicate values length", () => {
    var family = [
      { name: "Nancy", age: 15 },
      { name: "Nancy", age: 2 }
    ];
    var unique = arrayHelper.UniqueResults(family, 'name');
    expect(unique.length).toBe(1);
  });
});