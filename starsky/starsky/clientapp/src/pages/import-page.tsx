
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuSearch from '../components/menu-search';
import useFileHandlers, { IFiles } from '../hooks/use-file-handlers';


const Input = (props: any) => (
  <input
    type="file"
    accept="image/*"
    name="img-loader-input"
    multiple
    {...props}
  />
)

const ImportPage: FunctionComponent<RouteComponentProps> = (props) => {
  const {
    files,
    pending,
    next,
    uploading,
    uploaded,
    status,
    onSubmit,
    onChange,
  } = useFileHandlers()


  return (<div>
    <MenuSearch></MenuSearch>
    <div className="content">
      <div className="container">
        <form className="form" onSubmit={onSubmit}>
          {status === 'FILES_UPLOADED' && (
            <div className="success-container">
              <div>
                <h2>Congratulations!</h2>
                <small>You uploaded your files. Get some rest.</small>
              </div>
            </div>
          )}
          <div>
            <Input onChange={onChange} />
            <button type="submit">Submit</button>
          </div>
          <div>
            {(files as IFiles[]).map(({ file, src, id }, index) => (
              <div
                style={{
                  opacity: uploaded[id] ? 0.2 : 1,
                }}
                key={`thumb${index}`}
                className="thumbnail-wrapper"
              >
                <img className="thumbnail" src={src} alt="" />
                <div className="thumbnail-caption">{file.name}</div>
              </div>
            ))}
          </div>
        </form>
      </div>
      <div className="content--header"><a href="/v1/import">Importeren werkt nog niet in V2</a></div>
      <div className="content--subheader"><a href="/v1/import"><u>Ga naar de oude weergave om te importeren</u></a></div>
    </div>
  </div >
  )
}

export default ImportPage;
