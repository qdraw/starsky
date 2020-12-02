export interface IClipboardData {
	tags: string;
	description: string;
	title: string;
}

export class ClipboardHelper {
	/**
	 * Name of the sessionStorage
	 */
	private clipBoardName = "starskyClipboardData";

	public Copy(
		tagsReference: React.RefObject<HTMLDivElement>,
		descriptionReference: React.RefObject<HTMLDivElement>,
		titleReference: React.RefObject<HTMLDivElement>
	): boolean {
		if (
			!tagsReference.current ||
			!descriptionReference.current ||
			!titleReference.current
		) {
			return false;
		}

		var tags = tagsReference.current.innerText;
		var description = descriptionReference.current.innerText;
		var title = titleReference.current.innerText;

		sessionStorage.setItem(
			this.clipBoardName,
			JSON.stringify({
				tags,
				description,
				title
			} as IClipboardData)
		);
		return true;
	}

	public Read(): IClipboardData | null {
		var result = {};
		try {
			var resultString = sessionStorage.getItem(this.clipBoardName);
			result = JSON.parse(resultString ? resultString : "");
		} catch (error) {
			return null;
		}
		return result as IClipboardData;
	}

	public Paste(
		tagsReference: React.RefObject<HTMLDivElement>,
		descriptionReference: React.RefObject<HTMLDivElement>,
		titleReference: React.RefObject<HTMLDivElement>
	): boolean {
		if (
			!tagsReference.current ||
			!descriptionReference.current ||
			!titleReference.current
		) {
			return false;
		}
		var readData = this.Read();

		if (!readData) {
			return false;
		}
		var tags = tagsReference.current;
		tags.innerText = readData.tags;
		tags.dispatchEvent(new Event("blur"));

		var description = descriptionReference.current;
		description.innerText = readData.description;
		description.dispatchEvent(new Event("blur"));

		var title = titleReference.current;
		title.innerText = readData.title;
		title.dispatchEvent(new Event("blur"));
		return true;
	}
}
