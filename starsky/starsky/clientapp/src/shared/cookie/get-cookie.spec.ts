import { GetCookie } from "./get-cookie";

describe("GetCookie function", () => {
  // Test case 1: Cookie with the specified name exists
  test("should return the value of the cookie when it exists", () => {
    // Mock document.cookie to simulate the presence of the cookie
    document.cookie = "yourCookieName=yourCookieValue; path=/";

    // Call the function and assert the result
    expect(GetCookie("yourCookieName")).toBe("yourCookieValue");
  });

  // Test case 2: Cookie with the specified name does not exist
  test('should return "" when the cookie does not exist', () => {
    // Mock document.cookie to simulate an empty cookie
    document.cookie = "";

    // Call the function and assert the result
    expect(GetCookie("nonExistentCookie")).toBe("");
  });

  // Test case 3: Cookie with the specified name has an empty value
  test("should return an empty string when the cookie value is empty", () => {
    // Mock document.cookie to simulate a cookie with an empty value
    document.cookie = "emptyCookie=; path=/";

    // Call the function and assert the result
    expect(GetCookie("emptyCookie")).toBe("");
  });

  // Test case 4: Cookie with the specified name has special characters
  test("should return the correct value when the cookie value has special characters", () => {
    // Mock document.cookie to simulate a cookie with special characters
    document.cookie =
      "specialCookie=%24%25%5E%26%2A%28%29%3D%2B%7B%7D%5B%5D%3B%3A%40%23%21%7C%3C%3E%2C%2F%3F; path=/";

    // Call the function and assert the result
    expect(GetCookie("specialCookie")).toBe(
      "%24%25%5E%26%2A%28%29%3D%2B%7B%7D%5B%5D%3B%3A%40%23%21%7C%3C%3E%2C%2F%3F"
    );
  });
});
