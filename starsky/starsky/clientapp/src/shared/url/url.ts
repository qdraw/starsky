export function IsRelativeUrl(url: string): boolean {
  // Regular expression pattern to match relative URLs
  const relativeUrlRegex = /^(?!https?:\/\/)(?![^/]*\.[^/.]+\/)[^?#\s]+(?:\?[^#\s]*)?(?:#\S*)?$/;

  // Check if the URL matches the relative URL pattern
  return relativeUrlRegex.test(url);
}
