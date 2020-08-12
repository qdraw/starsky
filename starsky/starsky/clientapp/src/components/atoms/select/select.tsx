import React from 'react';

type SelectPropTypes = {
  children?: React.ReactNode;
  selectOptions: string[]
  callback?(option: string): void;
}


const Select: React.FunctionComponent<SelectPropTypes> = ({ children, selectOptions, callback }) => {

  const change = (value: string) => {
    if (!callback) {
      return;
    }
    callback(value);
  };

  return (
    <select className="select" onChange={e => change(e.target.value)}>
      {selectOptions.map((value, index) => {
        return <option key={index} value={value}>{value}</option>
      })}
      {children}
    </select>
  );
};

export default Select
