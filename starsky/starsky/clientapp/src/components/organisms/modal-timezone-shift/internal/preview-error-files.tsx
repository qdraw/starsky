import { IExifTimezoneCorrectionResult } from "../../../../interfaces/ITimezone";

type PreviewErrorFilesProps = Readonly<{
  data: ReadonlyArray<IExifTimezoneCorrectionResult>;
}>;

export function PreviewErrorFiles({ data }: PreviewErrorFilesProps) {
  return (
    <div className="preview-error-files">
      {Array.isArray(data)
        ? data
            .filter((x) => x.error || x.warning)
            .map((item) => {
              if (item.warning && !item.error)
                return (
                  <p key={`warning-file-${item.fileIndexItem.fileName}`} className="warning">
                    ⚠️{" "}
                    <span data-test="warning-filename" className="filename">
                      {item.fileIndexItem.fileName}
                    </span>
                    : {item.warning}
                  </p>
                );
              return (
                <p key={`error-file-${item.fileIndexItem.fileName}`} className="error">
                  ❌{" "}
                  <span data-test="error-filename" className="filename">
                    {item.fileIndexItem.fileName}
                  </span>
                  : {item.error}
                </p>
              );
            })
        : null}
    </div>
  );
}
