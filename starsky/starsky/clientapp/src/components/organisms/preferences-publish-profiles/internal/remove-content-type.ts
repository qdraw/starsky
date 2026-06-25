import { EditableProfile } from "./editable-profile";

export const removeContentType = (
  setProfiles: React.Dispatch<React.SetStateAction<EditableProfile[]>>,
  activeProfileIndex: number,
  itemId: string
) => {
  setProfiles((current) =>
    current.map((profile, index) => {
      if (index !== activeProfileIndex || profile.items.length <= 1) {
        return profile;
      }

      return {
        ...profile,
        items: profile.items.filter((item) => item._id !== itemId)
      };
    })
  );
};
