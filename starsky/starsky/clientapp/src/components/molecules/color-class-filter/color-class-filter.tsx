import React, { memo, useEffect, useState } from "react";
import { ArchiveContext } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { newIArchive } from "../../../interfaces/IArchive";
import { Language } from "../../../shared/language";
import Link from "../../atoms/link/link";
import Preloader from "../../atoms/preloader/preloader";
import { GetFilterUrlColorClass } from "./internal/get-filter-url-color-class.ts";
import { CleanColorClass } from "./internal/clean-color-class.ts";
import { ClassNameContainer } from "./internal/class-name-container.ts";
import localization from "../../../localization/localization.json";
import { IArchiveProps } from "../../../interfaces/IArchiveProps.ts";

//  <ColorClassFilter itemsCount={this.props.collectionsCount} subPath={this.props.subPath}
// colorClassActiveList={this.props.colorClassActiveList} colorClassUsage={this.props.colorClassUsage}></ColorClassFilter>
export interface IColorClassProp {
  subPath: string;
  colorClassActiveList: Array<number>;
  colorClassUsage: Array<number>;
  itemsCount?: number;
  sticky?: boolean;
}

function stateFallback(state: IArchiveProps, props: IColorClassProp) {
  // props is used as default, state only for update
  if (!state) {
    state = {
      ...newIArchive(),
      colorClassUsage: props.colorClassUsage,
      colorClassActiveList: props.colorClassActiveList,
      collectionsCount: props.itemsCount ? props.itemsCount : 0
    };
  }
  return state;
}

const ColorClassFilter: React.FunctionComponent<IColorClassProp> = memo((props) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const colorContent: Array<string> = [
    language.key(localization.ColorClassColour0),
    language.key(localization.ColorClassColour1),
    language.key(localization.ColorClassColour2),
    language.key(localization.ColorClassColour3),
    language.key(localization.ColorClassColour4),
    language.key(localization.ColorClassColour5),
    language.key(localization.ColorClassColour6),
    language.key(localization.ColorClassColour7),
    language.key(localization.ColorClassColour8),
    language.key(localization.ColorClassColourResetFilter)
  ];

  // used for reading current location
  const history = useLocation();

  let { state } = React.useContext(ArchiveContext);
  state = stateFallback(state, props);

  const [colorClassUsage, setColorClassUsage] = useState(props.colorClassUsage);

  useEffect(() => {
    setColorClassUsage(state.colorClassUsage);
    // it should not update when the prop are changing
  }, [state.colorClassUsage]);

  const [colorClassActiveList, setColorClassActiveList] = useState(props.colorClassActiveList);
  useEffect(() => {
    setColorClassActiveList(state.colorClassActiveList);
    // it should not update when the prop are changing
  }, [state.colorClassActiveList]);

  const [collectionsCount, setCollectionsCount] = useState(props.itemsCount);
  useEffect(() => {
    setCollectionsCount(state.collectionsCount);
    // it should not update when the prop are changing
  }, [state.collectionsCount]);

  const [isLoading, setIsLoading] = useState(false);
  // When change-ing page the loader should be gone
  useEffect(() => {
    setIsLoading(false);
  }, [props.colorClassActiveList]);

  const resetButton = (
    <Link
      data-test="color-class-filter-reset"
      to={CleanColorClass(props.subPath, history.location.search)}
      className="btn colorclass colorclass--reset"
    >
      {colorContent[9]}
    </Link>
  );
  const resetButtonDisabled = (
    <div className="btn colorclass colorclass--reset disabled">{colorContent[9]}</div>
  );

  const classNameContainer = ClassNameContainer(props.sticky);

  // there is no content ?
  if (colorClassUsage.length === 1 && colorClassActiveList.length >= 1) {
    return <div className={classNameContainer}> {resetButton}</div>;
  }

  if (collectionsCount === 0 || colorClassUsage.length === 1) {
    return <></>;
  }

  return (
    <div className={classNameContainer}>
      {isLoading ? <Preloader isWhite={false} isOverlay={true} /> : null}
      {props.colorClassActiveList.length !== 0 ? resetButton : resetButtonDisabled}
      {colorClassUsage.map((item) =>
        item >= 0 && item <= 8 ? (
          <Link
            onClick={() => setIsLoading(true)}
            key={item}
            to={GetFilterUrlColorClass(item, history.location.search, state)}
            data-test={"color-class-filter-" + item}
            className={
              props.colorClassActiveList.indexOf(item) >= 0
                ? "btn btn--default colorclass colorclass--" + item + " active"
                : "btn colorclass colorclass--" + item
            }
          >
            <span className="label" />
            <span>{colorContent[item]}</span>{" "}
          </Link>
        ) : (
          <span key={item} />
        )
      )}
      <div className="btn btn--default sort">
        <span className="text">{!state.sort ? "fileName" : state.sort}</span>
        <span className="icon"></span>
      </div>
    </div>
  );
});

export default ColorClassFilter;
