import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MediaContent from '../containers/media-content';
import { BrowserDetect } from '../shared/browser-detect';

interface IContentPageProps {
}

const ContentPage: FunctionComponent<RouteComponentProps<IContentPageProps>> = (props) => {
  // for some reason "name" here is possibly undefined
  if (props && props.location && props.navigate && !new BrowserDetect().IsLegacy()) {
    return (
      <MediaContent />
    );
  }
  return null;
}


export default ContentPage;
