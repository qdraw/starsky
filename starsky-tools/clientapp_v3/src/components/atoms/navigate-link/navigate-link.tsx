import { memo, MouseEventHandler } from "react";
import { Link } from "react-router-dom";

export interface ILink {
	to: string;
	className?: string;
	onClick?: MouseEventHandler;
	title?: string;
}

const NavigateLink: React.FunctionComponent<ILink> = memo((props) => {
	return (
		<Link onClick={props.onClick} className={props.className} to={props.to}>
			{props.children}
		</Link>
	);
});

export default NavigateLink;
