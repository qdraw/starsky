import { RouteComponentProps } from "@reach/router";
import { FunctionComponent } from "react";
import Preferences from "../containers/preferences/preferences";

const PreferencesPage: FunctionComponent<RouteComponentProps> = (props) => {
  return <Preferences />;
};

export default PreferencesPage;
