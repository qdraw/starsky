import { EditableProfile } from "./editable-profile";
import { EditableProfileItem } from "./editable-profile-item";

export const updateItemField = <K extends keyof EditableProfileItem>(
  setProfiles: React.Dispatch<React.SetStateAction<EditableProfile[]>>,
  activeProfileIndex: number,
  itemId: string,
  field: K,
  value: EditableProfileItem[K]
) => {
  setProfiles((current) =>
    current.map((profile, profileIndex) => {
      if (profileIndex !== activeProfileIndex) {
        return profile;
      }

      return {
        ...profile,
        items: profile.items.map((item) => {
          if (item._id !== itemId) {
            return item;
          }

          return {
            ...item,
            [field]: value
          };
        })
      };
    })
  );
};
