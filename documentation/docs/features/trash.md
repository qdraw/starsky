# Trash

When you delete a file in Starsky, it is not permanently removed. Instead, the app moves the file to the system trash (on macOS) or the recycle bin (on Windows). This approach ensures that files can be easily restored using the built-in recovery features of your operating system, providing an extra layer of safety for accidental deletions.

On platforms other than macOS or Windows, or when `useSystemTrash` is disabled, Starsky uses its own internal trash. In this case, deleted files are tagged with `!delete!` and are only recoverable from within the app, not through your operating system.

![Rename](../assets/trash_detailview_v056.gif)

_Screenshot from: internal database_
