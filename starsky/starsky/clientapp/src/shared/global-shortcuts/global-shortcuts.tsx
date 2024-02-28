import useHotKeys from "../../hooks/use-keyboard/use-hotkeys";
import useLocation from "../../hooks/use-location/use-location";
import { UrlQuery } from "../url/url-query";

export function GlobalShortcuts() {
  const history = useLocation();

  // used in desktop to route from menu
  // command + shift + k
  useHotKeys(
    { key: "k", shiftKey: true, ctrlKeyOrMetaKey: true },
    () => {
      history.navigate(new UrlQuery().UrlPreferencesPage());
    },
    []
  );
}
