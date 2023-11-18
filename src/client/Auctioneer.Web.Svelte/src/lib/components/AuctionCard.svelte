<script lang="ts">
  import { Card, Button, Img } from "flowbite-svelte";
  import { ArrowRightOutline } from "flowbite-svelte-icons";
  import type { AuctionModel } from "$lib/models/auction.model";

  export let auction: AuctionModel;

  let image: Img;

  function bidCount() {
    if (auction.bids) {
      return auction.bids.length == 1 ? "1 bid" : `${auction.bids.length} bids`;
    } else {
      return "0 bids";
    }
  }

  const formatter = new Intl.DateTimeFormat("se", {
    hour12: false, year: "numeric", month: "numeric", day: "numeric", hour: "numeric", minute: "numeric"
  });
</script>

<Card img="/images/auctions/{auction.imgRoute}" class="mb-4">
  <h5 class="mb-2 text-2xl font-bold tracking-tight text-gray-900 dark:text-white">{auction.title}</h5>
  <p
    class="mb-3 font-thin text-gray-700 dark:text-gray-400 leading-tight">{formatter.format(new Date(auction.endTime))}</p>
  <p class="float-left mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">
    <strong>{auction.currentPrice}</strong> kr</p>
  <p class="float-right mb-3 font-normal text-gray-700 dark:text-gray-400 leading-tight">{bidCount()}</p>
  <br />
  <Button class="float-left">
    <a href="/auction/{auction.id.toString()}"> Go to auction</a>
    <ArrowRightOutline class="float-left w-3.5 h-3.5 ml-2 text-white" />
  </Button>
</Card>