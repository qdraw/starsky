// use the: IMAGE_INLINE_SIZE_LIMIT=1 due the fact that data: are not supported by the CSP
import { BrowserDetect } from '../shared/browser-detect';
import detectAutomaticRotationJpeg from '../style/images/detect-automatic-rotation.jpg';

/**
 * Black 2x1 JPEG, with the following meta information set:
 * EXIF Orientation: 6 (Rotated 90° CCW)
 * @see: https://github.com/blueimp/JavaScript-Load-Image
 * @see: https://github.com/davejm/client-compress/issues/4#issuecomment-630109722
 */
export const testAutoOrientationImageURL = 'data:image/jpeg;base64,/9j/4QAiRXhpZgAATU0AKgAAAAgAAQESAAMAAAABAAYAAAA' +
  'AAAD/2wCEAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBA' +
  'QEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQE' +
  'BAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAf/AABEIAAEAAgMBEQACEQEDEQH/x' +
  'ABKAAEAAAAAAAAAAAAAAAAAAAALEAEAAAAAAAAAAAAAAAAAAAAAAQEAAAAAAAAAAAAAAAA' +
  'AAAAAEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/8H//2Q=='

/**
 * check if browser supports automatic image orientation
 * Supported: Chrome 81+ and Safari (Desktop) 13.1+ and Safari Mobile 13+
 * Hacked/Supported though BrowserDetect hack: Safari on iOS 12 and lower
 */
const DetectAutomaticRotation = async (): Promise<boolean> => {

  // iOS 12 and lower don't update the img.width but does rotate the image.
  // this BrowserDetect method detect if the browser is iOS or iPad OS
  if (new BrowserDetect().IsIOS()) {
    return true;
  }

  return new Promise((resolve) => {
    const img = new Image();
    img.onload = () => {
      // Check if browser supports automatic image orientation:
      const supported = img.width === 1 && img.height === 2;
      resolve(supported);
    };
    img.src = detectAutomaticRotationJpeg;
  });
};


/*
Exif orientation values to correctly display the letter F:

  1             2
██████        ██████
██                ██
████            ████
██                ██
██                ██

  3             4
    ██        ██
    ██        ██
  ████        ████
    ██        ██
██████        ██████

  5             6
██████████    ██
██  ██        ██  ██
██            ██████████

  7             8
      ██    ██████████
  ██  ██        ██  ██
██████████            ██

*/


export default DetectAutomaticRotation;
