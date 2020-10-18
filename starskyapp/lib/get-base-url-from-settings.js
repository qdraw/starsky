const appConfig = require('electron-settings');

getBaseUrlFromSettings = () => {
    var defaultUrl = "http://localhost:9609";
    var currentSettings = appConfig.get("settings");
    if (!currentSettings || !currentSettings.remote) return defaultUrl;
    return currentSettings.location;
}

function slugify(text) {
    return text
      .toString()                     // Cast to string
      .toLowerCase()                  // Convert the string to lowercase letters
      .normalize('NFD')       // The normalize() method returns the Unicode Normalization Form of a given string.
      .trim()                         // Remove whitespace from both sides of a string
      .replace(/\s+/g, '-')           // Replace spaces with -
      .replace(/[^\w\-]+/g, '')       // Remove all non-word chars
      .replace(/\-\-+/g, '-');        // Replace multiple - with single -
}

getBaseUrlFromSettingsSlug = () => {
 return slugify(getBaseUrlFromSettings());
}

module.exports = {
    getBaseUrlFromSettingsSlug,
    getBaseUrlFromSettings
}