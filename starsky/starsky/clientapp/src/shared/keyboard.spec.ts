import { Keyboard } from './keyboard';

describe("keyboard", () => {
  var keyboard = new Keyboard();

  it("null input", () => {
    var eventTarget: EventTarget = new EventTarget()
    var event = new KeyboardEvent('keydown', { 'keyCode': 37, 'target': eventTarget } as KeyboardEventInit);

    var result = keyboard.isInForm(event);

    expect(result).toBeNull();
  });
});