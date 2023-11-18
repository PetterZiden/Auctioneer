process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = 0;
//import type { Guid } from 'guid-typescript';
import { Ok, Err, Result } from 'ts-results';
import type { Update } from 'vite';

const apiUrl: string = 'https://localhost:7298';
const headers = {
	Accept: 'application/json, text/plain',
	'Content-Type': 'application/json'
};

export interface AuctionModel {
	id: string;
	memberId: string;
	title: string;
	description: string;
	startTime: Date;
	endTime: Date;
	startingPrice: number;
	currentPrice: number;
	imgRoute: string;
	bids: BidModel[] | null;
}

export interface BidModel {
	auctionId: string;
	memberId: string;
	bidPrice: number;
	timeStamp: Date;
}

export interface CreateAuctionRequest {
	memberId: string;
	title: string;
	description: string;
	startTime: Date;
	endTime: Date;
	startingPrice: number;
	imgRoute: string;
}

export interface UpdateAuctionRequest {
	auctionId: string;
	title: string | null;
	description: string | null;
	imgRoute: string | null;
}

export async function getAuctions(): Promise<Result<AuctionModel[], Error>> {
	//TryCatch
	const response = await fetch(`${apiUrl}/api/auctions`, {
		method: 'GET',
		headers: headers
	});
	if (response.status === 200) {
		let data = await response.json();
		return Ok(data);
	} else {
		return Err(new Error('No auctions found'));
	}
}

export async function getAuctionById(id: string): Promise<Result<AuctionModel, Error>> {
	//TryCatch
	const response = await fetch(`${apiUrl}/api/auction/${id}`, {
		method: 'GET',
		headers: headers
	});
	if (response.status === 200) {
		let data = await response.json();
		return Ok(data);
	} else {
		return Err(new Error('No auctions found'));
	}
}

export async function createAuction(
	auction: CreateAuctionRequest
): Promise<Result<boolean, Error>> {
	let body = JSON.stringify(auction);
	const response = await fetch(`${apiUrl}/api/auction`, {
		method: 'POST',
		headers: headers,
		body: body
	});

	if (response.status === 200) {
		return Ok(true);
	} else {
		let data = await response.json();
		return Err(new Error(data));
	}
}

export async function updateAuction(
	auction: UpdateAuctionRequest
): Promise<Result<boolean, Error>> {
	let body = JSON.stringify(auction);
	const response = await fetch(`${apiUrl}/api/auction`, {
		method: 'PUT',
		headers: headers,
		body: body
	});

	if (response.status === 200) {
		return Ok(true);
	} else {
		return Err(new Error('Could not update auction'));
	}
}

export async function deleteAuctionById(id: string): Promise<Result<boolean, Error>> {
	//TryCatch
	const response = await fetch(`${apiUrl}/api/auction/${id}`, {
		method: 'DELETE',
		headers: headers
	});
	if (response.status === 200) {
		return Ok(true);
	} else {
		return Err(new Error('Could not delete auction'));
	}
}
