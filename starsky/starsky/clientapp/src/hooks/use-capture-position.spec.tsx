import capturePosition, { ICaptionPosition } from './use-capture-position';
import { mountReactHook } from './___tests___/test-hook';

describe("capturePosition", () => {

  let setupComponent;
  let hook: ICaptionPosition;

  beforeEach(() => {
    setupComponent = mountReactHook(capturePosition, []); // Mount a Component with our hook
    hook = setupComponent.componentHook as ICaptionPosition;
  });

  it('default status code', () => {
    hook.freeze();
  });

});