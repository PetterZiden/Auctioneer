import { getAuctionById, getAuctions } from '$lib/models/auction.model';

export async function load() {
	const response = await getAuctions();
	if (response.ok) {
		return {
			auctions: response.val
		};
	} else {
		console.log(`Error when fetching auctions with msg: ${response.val}`);
	}
}
