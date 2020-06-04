import detectAutomaticRotationJpeg from '../style/images/detect-automatic-rotation.jpg';
// use the: IMAGE_INLINE_SIZE_LIMIT=1 due the fact that data: are not supported by the CSP

export class OrientationHelper {

  /**
   * Black 2x1 JPEG, with the following meta information set:
   * EXIF Orientation: 6 (Rotated 90° CCW)
   * @see: https://github.com/blueimp/JavaScript-Load-Image
   * @see: https://github.com/davejm/client-compress/issues/4#issuecomment-630109722
   */
  public testAutoOrientationImageURL: string =
    'data:image/jpeg;base64,/9j/4QAiRXhpZgAATU0AKgAAAAgAAQESAAMAAAABAAYAAAA' +
    'AAAD/2wCEAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBA' +
    'QEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQE' +
    'BAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAf/AABEIAAEAAgMBEQACEQEDEQH/x' +
    'ABKAAEAAAAAAAAAAAAAAAAAAAALEAEAAAAAAAAAAAAAAAAAAAAAAQEAAAAAAAAAAAAAAAA' +
    'AAAAAEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/8H//2Q==';

  /**
   * Detect if a browser support automatic rotation
   */
  public DetectAutomaticRotation = (): Promise<boolean> => {
    return new Promise((resolve) => {
      const img = new Image();
      img.onload = () => {
        // Check if browser supports automatic image orientation:
        const supported = img.width === 1 && img.height === 2;

        console.log(supported);

        resolve(supported);
      };
      img.src = detectAutomaticRotationJpeg;
    });
  }
}