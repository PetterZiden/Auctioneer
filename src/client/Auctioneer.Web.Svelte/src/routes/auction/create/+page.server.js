import { writeFileSync } from 'fs';
import { createAuction } from '$lib/models/auction.model';
import { redirect } from '@sveltejs/kit';
import { fail } from '@sveltejs/kit';
import { Guid } from 'guid-typescript';

export const actions = {
	default: async ({ request }) => {
		try {
			const formData = Object.fromEntries(await request.formData());
			if (!formData.pictureToUpload.name || formData.pictureToUpload.name === 'undefined') {
				return { success: false, error: 'You must provide a file to upload' };
			}
			const pictureToUpload = formData.pictureToUpload;
			const imgPath = `static/images/auctions/${pictureToUpload.name}`;
			writeFileSync(imgPath, Buffer.from(await pictureToUpload.arrayBuffer()));

			//TODO: get memberId from cookies
			let auction = {
				memberId: 'a08672bf-760c-40ed-889c-d34d0b6842c1',
				title: formData.title,
				description: formData.description,
				startingPrice: formData.price,
				startTime: formData.startTime,
				endTime: formData.endTime,
				imgRoute: pictureToUpload.name
			};

			console.log('CREATE AUCTION');
			console.log(auction);

			let response = await createAuction(auction);
			if (response.ok) {
				console.log('RESPONSE-SUCCESS');
				return { success: true };
			} else {
				console.log('RESPONSE-FAILED');
				return { success: false };
			}
		} catch (error) {
			console.log('CATCH-ERROR');
			console.log(error);
			return { success: false, error: error };
		}
	}
};
