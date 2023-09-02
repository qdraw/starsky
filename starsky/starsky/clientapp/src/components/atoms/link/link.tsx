import { Link as ReactRouterDomLink } from "react-router-dom";
import { INavigateState } from "../../../interfaces/INavigateState";
interface ILink {
  to: string;
  "data-test"?: string;
  className?: string;
  title?: string;

  children?: React.ReactNode;
  onClick?: React.MouseEventHandler<HTMLAnchorElement>;
  state?: INavigateState;
}

const Link: React.FunctionComponent<ILink> = (item) => {
  const test = () => {
    // do nothing
  };
  return (
    <ReactRouterDomLink
      data-test={item?.["data-test"]}
      className={item?.className}
      onClick={item?.onClick ?? test}
      title={item?.title}
      to={item?.to ?? "/"}
      state={item?.state}
    >
      {item?.children}
    </ReactRouterDomLink>
  );
};

export default Link;
