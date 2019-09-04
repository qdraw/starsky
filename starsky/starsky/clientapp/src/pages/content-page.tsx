import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MediaContent from '../containers/media-content';
import { IsLegacy } from '../shared/is-legacy';

interface IContentPageProps {
}

const ContentPage: FunctionComponent<RouteComponentProps<IContentPageProps>> = (props) => {
  // for some reason "name" here is possibly undefined
  if (props && props.location && props.navigate) {
    if (new IsLegacy().IsLegacy()) return <>Internet Explorer is not supported, please try Firefox or Chrome</>
    return (
      <MediaContent />
    );
  }
  return null;
}


export default ContentPage;
