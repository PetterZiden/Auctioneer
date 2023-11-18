import { getAuctionById } from '$lib/models/auction.model';

export async function load({ params }) {
	const response = await getAuctionById(params.id);
	if (response.ok) {
		let auction = response.val;
		return { auction };
	} else {
		console.log(`Error when fetching auctions with msg: ${response.val}`);
	}
}
