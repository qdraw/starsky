import { IDetailView, IRelativeObjects } from "../interfaces/IDetailView";
import FetchGet from "./fetch-get";
import { URLPath } from "./url-path";
import { UrlQuery } from "./url-query";

export class UpdateRelativeObject {
	/**
	 * UrlSearchRelativeApi using FetchGet
	 */
	public Update(
		state: IDetailView,
		isSearchQuery: boolean,
		historyLocationSearch: string,
		setRelativeObjects: React.Dispatch<React.SetStateAction<IRelativeObjects>>
	): Promise<IRelativeObjects> {
		return new Promise((resolve, rejects) => {
			if (state.subPath === "/" || !isSearchQuery) {
				rejects();
				return;
			}

			FetchGet(
				new UrlQuery().UrlSearchRelativeApi(
					state.subPath,
					new URLPath().StringToIUrl(historyLocationSearch).t,
					new URLPath().StringToIUrl(historyLocationSearch).p
				)
			)
				.then((result) => {
					if (result.statusCode !== 200) {
						rejects();
						return;
					}
					setRelativeObjects(result.data);
					resolve(result.data);
				})
				.catch((err) => {
					console.log(err);
					rejects();
				});
		});
	}
}
