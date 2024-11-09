import { CommaSeperatedFileList } from "./comma-seperated-filelist";

describe("CommaSeperatedFileList", () => {
  const comma = new CommaSeperatedFileList();

  test("should return empty string for empty input", () => {
    const result = comma.CommaSpaceLastDot([]);
    expect(result).toBe("");
  });

  test("should return last part after dot for single input", () => {
    const result = comma.CommaSpaceLastDot(["example.test"]);
    expect(result).toBe("test");
  });

  test("should return comma-separated last parts after dot for multiple inputs", () => {
    const result = comma.CommaSpaceLastDot(["example.test", "another.example", "final.test"]);
    expect(result).toBe("example, test");
  });

  test("should handle inputs without dots", () => {
    const result = comma.CommaSpaceLastDot(["example", "another", "final"]);
    expect(result).toBe("without extension");
  });

  test("should handle translated fallback for inputs without dots", () => {
    const result = comma.CommaSpaceLastDot(["example", "another", "final"], "__test123__");
    expect(result).toBe("__test123__");
  });

  test("should handle mixed inputs with and without dots", () => {
    const result = comma.CommaSpaceLastDot(["example.test", "another", "final.test"]);
    expect(result).toBe("test, without extension");
  });

  test("should handle mixed inputs with and without dots", () => {
    const result = comma.CommaSpaceLastDot([
      "example.test",
      "/collection/20241106_155823_DSC00339",
      "final.test"
    ]);
    expect(result).toBe("test, without extension");
  });
});
