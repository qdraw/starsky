import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";

interface IUpdateButtonActiveProps {
  latitude?: number;
  longitude?: number;
  language: Language;
}

export const UpdateButtonDisabled: React.FunctionComponent<IUpdateButtonActiveProps> = ({
  latitude,
  longitude,
  language
}) => {
  return (
    <button className="btn btn--default" disabled={true}>
      {/* disabled */}
      {!latitude && !longitude
        ? language.key(localization.MessageAddLocation)
        : language.key(localization.MessageUpdateLocation)}
    </button>
  );
};
