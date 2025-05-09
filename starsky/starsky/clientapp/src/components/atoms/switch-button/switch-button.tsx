import { useEffect, useState } from "react";

interface ISwitchButtonProps {
  onToggle(value: boolean, name?: string): void;
  leftLabel: string;
  rightLabel: string;
  isEnabled?: boolean;
  isOn?: boolean;
  name?: string;
  "data-test"?: string;
}

function SwitchButton(props: Readonly<ISwitchButtonProps>) {
  const [random, setRandom] = useState(0);

  useEffect(() => {
    setRandom(Math.ceil(Math.random() * 100));
  }, []);

  const [checked, setChecked] = useState(props.isOn ?? false);

  useEffect(() => {
    if (props.isOn === undefined) return;
    setChecked(props.isOn);
  }, [props]);

  function onChange() {
    setChecked(!checked);
    props.onToggle(!checked, props.name);
  }

  return (
    <form
      data-test={props["data-test"]}
      className={props.isEnabled !== false ? "switch-field" : "switch-field disabled"}
    >
      <input
        type="radio"
        disabled={props.isEnabled === false}
        id={"switch_left_" + random}
        name={props.name ?? "switchToggle"}
        value={props.leftLabel}
        data-test="switch-button-left"
        onChange={onChange}
        checked={!checked}
      />
      <label htmlFor={"switch_left_" + random}>{props.leftLabel}</label>

      <input
        type="radio"
        id={"switch_right_" + random}
        disabled={props.isEnabled === false}
        name={props.name ?? "switchToggle"}
        value={props.rightLabel}
        data-test="switch-button-right"
        onChange={onChange}
        checked={checked}
      />
      <label htmlFor={"switch_right_" + random}>{props.rightLabel}</label>
    </form>
  );
}

export default SwitchButton;
