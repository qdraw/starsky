import React, { memo } from 'react';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import { Keyboard } from '../shared/keyboard';
import { Query } from '../shared/query';

export interface IColorClassSelectProps {
  currentColorClass?: number;
  isEnabled: boolean;
  filePath: string;
}

const ColorClassSelect: React.FunctionComponent<IColorClassSelectProps> = memo((props) => {
  var colorContent: Array<string> = [
    "Geen",
    "Paars",
    "Rood",
    "Oranje",
    "Geel",
    "Groen",
    "Azuur",
    "Blauw",
    "Grijs",
  ];

  const [currentColorClass, setCurrentColorClass] = React.useState(props.currentColorClass);

  var handleChange = (colorClass: number) => {
    if (!props.isEnabled) return;

    // push content to server
    new Query().queryUpdateApi(props.filePath, "colorClass", colorClass.toString()).then(item => {
      setCurrentColorClass(colorClass);
    });
  }

  useKeyboardEvent(/[0-8]/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    handleChange(Number(event.key));
  })

  return (<div className={props.isEnabled ? "colorclass colorclass--select" : "colorclass colorclass--select colorclass--disabled"}>
    {
      colorContent.map((item, index) => (
        <a key={index} onClick={() => { handleChange(index); }}
          className={currentColorClass === index ? "btn btn--default colorclass colorclass--" + index + " active" : "btn colorclass colorclass--" + index}>
          <label></label><span>{item}</span>
        </a>
      ))
    }
  </div>)
});

export default ColorClassSelect
