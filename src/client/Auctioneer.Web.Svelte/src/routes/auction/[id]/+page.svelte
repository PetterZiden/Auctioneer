<script lang="ts">
  import { Button, Img, Input, Label } from "flowbite-svelte";

  export let data;

  let newBidPrice = 0;

  function bidCount() {
    if (data.auction.bids) {
      return data.auction.bids.length == 1 ? "1 bid" : `${data.auction.bids.length} bids`;
    } else {
      return "0 bids";
    }
  }

  const formatter = new Intl.DateTimeFormat("se", {
    hour12: false, year: "numeric", month: "numeric", day: "numeric", hour: "numeric", minute: "numeric"
  });

  function placeBid(): void {
    //TODO place bid when user cookies exist to extract memberId 
  }
</script>


<div class="grid grid-cols-2 gap-4 mt-4 w-3/5">
  <div class="mr-6">
    <div class="p-4">
      <Img src="/images/auctions/{data.auction.imgRoute}" alt="Picture for auction" size="max-w-lg"
           class="rounded-lg" />
    </div>
    <div class="p-4">
      <h2 class="font-bold">Description</h2>
      <p>{data.auction.description}</p>
    </div>
  </div>
  <div class="ml-6">
    <div class="p-4">
      <h1 class="font-bold">{data.auction.title}</h1>
    </div>
    <div class="p-4">
      <p class="float-left mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">Current leading bid</p>
      <p class="float-right mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">
        <strong>{data.auction.currentPrice}</strong> kr</p>
    </div>
    <div class="p-4">
      <p class="float-left mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">Auction ends</p>
      <p class="float-right mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">
        <strong>{formatter.format(new Date(data.auction.endTime))}</strong></p>
    </div>
    <div class="p-4 mb-8">
      <p class="float-left mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">Number of bids</p>
      <p class="float-right mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">
        <strong>{bidCount()}</strong></p>
    </div>
    <div>
      <div class="w-full p-4">
        <Input type="number" bind:value={newBidPrice} placeholder="Bid" name="bid" autocomplete="off" />
      </div>
      <div class="w-full p-4">
        <Button class="mb-auto w-full" on:click={placeBid}>Place bid</Button>
      </div>
    </div>

  </div>
</div>