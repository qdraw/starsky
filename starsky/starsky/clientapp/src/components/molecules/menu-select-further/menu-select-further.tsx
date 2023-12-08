import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

export interface IMenuSelectFurtherProps {
  select?: string[];
  toggleLabels: (state: boolean) => void;
}

export const MenuSelectFurther: React.FunctionComponent<
  IMenuSelectFurtherProps
> = ({ select, toggleLabels }) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageSelectFurther = language.key(localization.MessageSelectFurther);

  return (
    <>
      {select ? (
        <div className="header header--sidebar header--border-left">
          <button
            className="item item--continue"
            data-test="select-further"
            onClick={() => {
              toggleLabels(false);
            }}
            onKeyDown={(event) => {
              if (event.key === "Enter") {
                toggleLabels(false);
              }
            }}
          >
            {MessageSelectFurther}
          </button>
        </div>
      ) : (
        ""
      )}
    </>
  );
};
