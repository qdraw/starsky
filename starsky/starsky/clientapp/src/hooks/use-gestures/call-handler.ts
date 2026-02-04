import { ICurrentTouches } from "./ICurrentTouches.types";
import { IHandlers, IHandlersMapper } from "./IHandlers.types";

export const callHandler = (
  eventName: string,
  event: ICurrentTouches,
  handlers: IHandlers | undefined | IHandlersMapper
) => {
  if (!handlers) {
    throw new Error(`handler ${eventName} is missing`);
  }

  if (
    eventName &&
    handlers[eventName as keyof IHandlers] &&
    typeof handlers[eventName as keyof IHandlers] === "function"
  ) {
    const handler = handlers[eventName as keyof IHandlers];
    if (handler) {
      (handler as (e: ICurrentTouches) => void)(event);
    }
  }
};
