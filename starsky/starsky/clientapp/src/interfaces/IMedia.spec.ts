import { newIMedia } from './IMedia';

describe("IMedia", () => {
  it("newIMedia", () => {
    var media = newIMedia();
    expect(media).toBe(media);
  });
});