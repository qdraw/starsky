import React from 'react';

interface ISwitchButtonProps {
  onToggle(value: boolean): void;
  on: string;
  off: string;
}

function SwitchButton(props: ISwitchButtonProps) {

  var random = Math.ceil(Math.random() * 100);
  return (<div className="switch">
    <input type="radio" className="switch-input" name="view" value="true" id={"on_" + random} defaultChecked onClick={() => props.onToggle(false)} />
    <label htmlFor={"on_" + random} className="switch-label switch-label-off">{props.on}</label>
    <input type="radio" className="switch-input" name="view" value="true" id={"off_" + random} onClick={() => props.onToggle(true)} />
    <label htmlFor={"off_" + random} className="switch-label switch-label-on">{props.off}</label>
    <span className="switch-selection"></span>
  </div>);
};
export default SwitchButton




// const Button: React.FunctionComponent<IButtonProps> = memo((props) => {
//   // const [loading, changeLoading] = useState(false);

//   // const onClickHandler = useCallback((event: React.MouseEvent<HTMLButtonElement>) => {
//   //   const promise = props.onClick(event);

//   //   if (promise) {
//   //     changeLoading(true);
//   //     promise.then(() => changeLoading(false));
//   //   }
//   // }, [props.onClick]);

//   return (
//     <button
//       type={props.type || 'button'}
//       disabled={props.disabled}
//       // disabled={props.disabled || loading}
//       className={props.className}
//     // onClick={onClickHandler}
//     >
//       {props.children}
//     </button>
//   );
// });