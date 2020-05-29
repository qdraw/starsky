import React, { useEffect } from 'react';

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
    setChecked(props.isOn ? props.isOn : false);
  }, [props])

  return (
    <form className={props.isEnabled !== false ? "switch-field" : "switch-field disabled"}>
      <input
        type="radio"
        disabled={props.isEnabled === false}
        id={"switch_left_" + random}
        name={!props.name ? "switchToggle" : props.name}
        value={props.leftLabel}
        onChange={() => {
          setChecked(!checked);
          props.onToggle(!checked, props.name);
        }}
        checked={!checked}
      />
      <label htmlFor={"switch_left_" + random}>{props.leftLabel}</label>

      <input
        type="radio"
        id={"switch_right_" + random}
        disabled={props.isEnabled === false}
        name={!props.name ? "switchToggle" : props.name}
        value={props.rightLabel}
        onChange={() => {
          setChecked(!checked);
          props.onToggle(!checked, props.name);
        }}
        checked={checked}
      />
      <label htmlFor={"switch_right_" + random}>{props.rightLabel}</label>
    </form>
  );
}

export default SwitchButton;
