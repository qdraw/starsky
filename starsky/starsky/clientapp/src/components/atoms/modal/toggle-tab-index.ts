export const toggleTabIndex = (type: "on" | "off", container: Element) => {
  const focusableElements = container.querySelectorAll(
    "button, a, input, textarea, select"
  );
  focusableElements.forEach((element: Element) => {
    if (type === "on") {
      element.removeAttribute("tabindex");
    } else {
      element.setAttribute("tabindex", "-1");
    }
  });
};
