import React, { memo, useEffect } from 'react';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import FetchPost from '../shared/fetch-post';
import { Keyboard } from '../shared/keyboard';
import { UrlQuery } from '../shared/url-query';

export interface IColorClassSelectProps {
  currentColorClass?: number;
  isEnabled: boolean;
  filePath: string;
  clearAfter?: boolean;
  onToggle(value: number): void;
}

/**
 * Used to update colorclasses
 */
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

  /** re-render when switching page */
  useEffect(() => {
    setCurrentColorClass(props.currentColorClass)
  }, [props.currentColorClass]);

  /**
   * Used for Updating Colorclasses
   * @param colorClass value to update
   */
  var handleChange = (colorClass: number) => {

    if (!props.isEnabled) return;

    var updateApiUrl = new UrlQuery().UrlQueryUpdateApi();

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", props.filePath);
    bodyParams.append("colorclass", colorClass.toString());

    FetchPost(updateApiUrl, bodyParams.toString()).then(item => {
      setCurrentColorClass(colorClass);
      props.onToggle(colorClass);
    });

    if (!props.clearAfter) return;

    setTimeout(function () {
      setCurrentColorClass(undefined);
    }, 500);
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
