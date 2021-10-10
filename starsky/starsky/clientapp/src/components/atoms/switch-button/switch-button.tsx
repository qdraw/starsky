import React, { useEffect } from "react";

interface ISwitchButtonProps {
  onToggle(value: boolean, name?: string): void;
  leftLabel: string;
  rightLabel: string;
  isEnabled?: boolean;
  isOn?: boolean;
  name?: string;
}

function SwitchButton(props: ISwitchButtonProps) {
  const [random, setRandom] = React.useState(0);

  useEffect(() => {
    setRandom(Math.ceil(Math.random() * 100));
  }, []);

  const [checked, setChecked] = React.useState(props.isOn ? props.isOn : false);

  useEffect(() => {
    if (props.isOn === undefined) return;
    setChecked(props.isOn);
  }, [props]);

  return (
    <form
      className={
        props.isEnabled !== false ? "switch-field" : "switch-field disabled"
      }
    >
      <input
        type="radio"
        disabled={props.isEnabled === false}
        id={"switch_left_" + random}
        name={!props.name ? "switchToggle" : props.name}
        value={props.leftLabel}
        onChange={() => {
          console.log("--test1");

          setChecked(!checked);
          props.onToggle(!checked, props.name);
        }}
        checked={!checked}
      />
      <label data-test="switch-button-left" htmlFor={"switch_left_" + random}>
        {props.leftLabel}
      </label>

      <input
        type="radio"
        id={"switch_right_" + random}
        disabled={props.isEnabled === false}
        name={!props.name ? "switchToggle" : props.name}
        value={props.rightLabel}
        onChange={() => {
          console.log("--test2");
          setChecked(!checked);
          props.onToggle(!checked, props.name);
        }}
        checked={checked}
      />
      <label data-test="switch-button-right" htmlFor={"switch_right_" + random}>
        {props.rightLabel}
      </label>
    </form>
  );
}

export default SwitchButton;
