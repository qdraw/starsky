import React, { memo, useEffect } from 'react';
import useGlobalSettings from '../hooks/use-globalSettings';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import FetchPost from '../shared/fetch-post';
import { Keyboard } from '../shared/keyboard';
import { Language } from '../shared/language';
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

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const colorContent: Array<string> = [
    language.text("Kleurloos", "Colorless"),
    language.text("Paars", "Purple"),
    language.text("Rood", "Red"),
    language.text("Oranje", "Orange"),
    language.text("Geel", "Yellow"),
    language.text("Groen", "Green"),
    language.text("Azuur", "Azure"),
    language.text("Blauw", "Blue"),
    language.text("Grijs", "Grey"),
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

    var updateApiUrl = new UrlQuery().UrlUpdateApi();

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", props.filePath);
    bodyParams.append("colorclass", colorClass.toString());

    FetchPost(updateApiUrl, bodyParams.toString()).then(_ => {
      setCurrentColorClass(colorClass);
      props.onToggle(colorClass);
    });

    if (!props.clearAfter) return;

    setTimeout(function () {
      setCurrentColorClass(undefined);
    }, 1000);
  };

  useKeyboardEvent(/[0-8]/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    handleChange(Number(event.key));
  });

  return (<div className={props.isEnabled ? "colorclass colorclass--select" : "colorclass colorclass--select colorclass--disabled"}>
    {
      colorContent.map((item, index) => (
        <button key={index} onClick={() => { handleChange(index); }}
          className={currentColorClass === index ? "btn btn--default colorclass colorclass--" + index + " active" : "btn colorclass colorclass--" + index}>
          <label /><span>{item}</span>
        </button>
      ))
    }
  </div>)
});

export default ColorClassSelect
