type SelectPropTypes = {
  children?: React.ReactNode;
  selectOptions: string[] | undefined;
  selected?: string;
  callback?(option: string): void;
};

const Select: React.FunctionComponent<SelectPropTypes> = ({
  children,
  selectOptions,
  callback,
  selected
}) => {
  const change = (value: string) => {
    if (!callback) {
      return;
    }
    callback(value);
  };

  if (!selectOptions) {
    return <select className="select"></select>;
  }

  return (
    <select
      defaultValue={selected}
      className="select"
      data-test="select"
      onChange={(e) => change(e.target.value)}
    >
      {selectOptions.map((value) => {
        return (
          <option key={value} value={value}>
            {value}
          </option>
        );
      })}
      {children}
    </select>
  );
};

export default Select;
