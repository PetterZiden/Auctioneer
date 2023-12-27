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