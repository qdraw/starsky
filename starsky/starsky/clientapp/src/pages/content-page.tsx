import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MediaContent from '../containers/media-content';



interface IContentPageProps {
}

const ContentPage: FunctionComponent<RouteComponentProps<IContentPageProps>> = (props) => {
  // for some reason "name" here is possibly undefined
  if (props && props.location && props.navigate) {
    // props.location.search = "/?sidebar=true";
    // props.navigate("/some/where", { replace: true });

    return (
      <MediaContent />
    );
  }
  return null;
}


export default ContentPage;
