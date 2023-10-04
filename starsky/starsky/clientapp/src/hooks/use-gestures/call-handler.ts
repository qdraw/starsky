import { ICurrentTouches } from "./ICurrentTouches.types";
import { IHandlers } from "./IHandlers.types";

export const callHandler = (
  eventName: string,
  event: ICurrentTouches,
  handlers: IHandlers | undefined
) => {
  if (!handlers) {
    throw new Error(`handler ${eventName} is missing`);
  }

  if (
    eventName &&
    (handlers as any)[eventName] &&
    typeof (handlers as any)[eventName] === "function"
  ) {
    (handlers as any)[eventName](event);
  }
};
