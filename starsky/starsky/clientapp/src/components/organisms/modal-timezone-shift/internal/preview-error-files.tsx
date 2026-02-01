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
                    ⚠️ {item.fileIndexItem.fileName}: {item.warning}
                  </p>
                );
              return (
                <p key={`error-file-${item.fileIndexItem.fileName}`} className="error">
                  ❌ {item.fileIndexItem.fileName}: {item.error}
                </p>
              );
            })
        : null}
    </div>
  );
}
