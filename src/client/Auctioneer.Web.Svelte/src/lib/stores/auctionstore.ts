import type { AuctionModel } from '$lib/models/auction.model';
import {
	createAuction,
	deleteAuctionById,
	getAuctionById,
	getAuctions,
	updateAuction
} from '$lib/models/auction.model';
import { writable } from 'svelte/store';
import type { Guid } from 'guid-typescript';

let auctions: AuctionModel[] = [];
const { subscribe, set, update } = writable(auctions);

//TODO: kanske inte behövs store? bara kalla direkt på API? Tror inte denna behövs
export const auctionStore = {
	subscribe,
	getAuctions: async () => {
		const response = await getAuctions();
		if (response.ok) {
			set(response.val);
		}
	},
	getAuctionById: async (id: Guid) => {
		const response = await getAuctionById(id);
		if (response.ok) {
			set([response.val]);
		}
	},
	createAuction: async (auction: AuctionModel) => {
		const response = await createAuction(auction);
		if (response.ok) {
			update((currentAuctions) => {
				return [auction, ...currentAuctions];
			});
		}
	},
	updateAuction: async (auction: AuctionModel) => {
		const response = await updateAuction(auction);
		if (response.ok) {
			update((auctions) => {
				let currentAuctions = [...auctions];
				currentAuctions.filter((auc) => auc.id !== auction.id);
				return [auction, ...currentAuctions];
			});
		}
	},
	deleteAuction: async (id: Guid) => {
		const response = await deleteAuctionById(id);
		if (response.ok) {
			update((auctions) => auctions.filter((auc) => auc.id !== id));
		}
	}
};
