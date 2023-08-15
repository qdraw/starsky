import { Link as ReactRouterDomLink } from "react-router-dom";
import { INavigateState } from "../../../interfaces/INavigateState";
interface ILink {
  to: string;
  "data-test"?: string | undefined;
  className?: string | undefined;
  title?: string | undefined;

  children?: React.ReactNode;
  onClick?: React.MouseEventHandler<HTMLAnchorElement> | undefined;
  state?: INavigateState;
}

const Link: React.FunctionComponent<ILink> = (item) => {
  return (
    <ReactRouterDomLink
      data-test={item["data-test"]}
      className={item.className}
      onClick={item.onClick}
      title={item.title}
      to={item.to}
      state={item.state}
    >
      {item.children}
    </ReactRouterDomLink>
  );
};

export default Link;
