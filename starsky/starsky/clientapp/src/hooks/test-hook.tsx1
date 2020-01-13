import { memo } from 'react';


interface ICallbackProps {
  callback(): void;
}

export const TestHook: React.FunctionComponent<ICallbackProps> = memo((props) => {
  props.callback();
  return null;
});

