<script lang="ts">
    import Auction from "$lib/components/AuctionCard.svelte";
    import type { AuctionModel } from "$lib/models/auction.model";
    import { Button } from "flowbite-svelte";

    export let data: { auctions: AuctionModel[] };
</script>

<div class="grid grid-cols-2 gap-4 mt-4">
    <div>
        <h1>Welcome to Auctioneer</h1>
        <p>Below you can see tha newest auctions.</p>
    </div>
    <div>
        <Button class="float-right">
            <a href="/auction/create">Create Auction</a>
        </Button>
    </div>
</div>
<div class="grid grid-cols-4 gap-4 mt-4">
    {#each data.auctions as auction }
        <Auction auction={auction} />
    {/each}
</div>
