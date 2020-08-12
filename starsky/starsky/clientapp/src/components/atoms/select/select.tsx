import React from 'react';

type SelectPropTypes = {
  children?: React.ReactNode;
  selectOptions: string[]
  callback?(option: string): void;
}


const Select: React.FunctionComponent<SelectPropTypes> = ({ children, selectOptions, callback }) => {

  // const close = () => {
  //   const portal = document.getElementById(PortalId);
  //   if (!portal) return;
  //   portal.remove();
  //   if (callback) callback();
  // };

  return (
    <select className="select">
      {selectOptions.map((value, index) => {
        return <option key={index} value={value}>{value}</option>
      })}
    </select>
  );
};

export default Select
