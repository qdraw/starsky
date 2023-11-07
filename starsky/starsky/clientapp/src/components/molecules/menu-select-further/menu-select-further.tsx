import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

export interface IMenuSelectFurtherProps {
  select?: string[];
  toggleLabels: () => void;
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
          <div
            className="item item--continue"
            data-test="select-further"
            onClick={() => {
              toggleLabels();
            }}
            onKeyDown={(event) => {
              event.key === "Enter" && toggleLabels();
            }}
          >
            {MessageSelectFurther}
          </div>
        </div>
      ) : (
        ""
      )}
    </>
  );
};
