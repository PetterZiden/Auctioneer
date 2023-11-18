<script lang="ts">
    import { enhance } from "$app/forms";
    import { Label, Fileupload, Input, Button, Img } from "flowbite-svelte";
    import { Guid } from "guid-typescript";
    import type { CreateAuctionRequest } from "$lib/models/auction.model.js";

    let value: any;
    let imgSrc = value ?? "/images/site/image-1@2x.jpg";
    const authorizedImgExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    let newAuction: CreateAuctionRequest = {
        memberId: Guid.create(),
        title: "",
        description: "",
        startingPrice: 0,
        startTime: new Date(),
        endTime: new Date(),
        imgRoute: ""
    };

</script>

<h1 class="font-bold">Create new auction</h1>

<form method="POST" use:enhance enctype="multipart/form-data">
    <div class="grid grid-cols-3 gap-4 mt-4">
        <div class="mr-6">
            <div class="mb-6">
                <Label for="title-input" class="block mb-2">Title:</Label>
                <Input id="title-input" bind:value={newAuction.title} placeholder="Title" name="title"
                       autocomplete="off" />
            </div>
            <div class="mb-6">
                <Label for="description-input" class="block mb-2">Description:</Label>
                <Input id="description-input" bind:value={newAuction.description} placeholder="Description"
                       name="description" autocomplete="off" />
            </div>
            <div class="mb-6">
                <Label for="price-input" class="block mb-2">Starting Price:</Label>
                <Input type="number" id="price-input" bind:value={newAuction.startingPrice} placeholder="Starting price"
                       name="price" autocomplete="off" />
            </div>
            <div class="mb-6">
                <Label class="block mb-2">Start time</Label>
                <div class="inline-flex w-full">
                    <Input type="datetime-local" placeholder="Start" name="startTime"></Input>
                </div>
            </div>
            <div class="mb-6">
                <Label class="block mb-2">End time</Label>
                <div class="inline-flex w-full">
                    <Input type="datetime-local" placeholder="End" name="endTime"></Input>
                </div>
            </div>
            <div class="w-full">
                <Button class="mb-auto w-full" type="submit">
                    <a href="/">Create Auction</a>
                </Button>
            </div>
        </div>
        <div>
            <div class="w-full mb-4">
                <Label class="pb-2" for="picture">Upload image</Label>
                <Img src={imgSrc} alt="upload picture" size="max-w-lg" class="rounded-lg" />
            </div>
            <div class="mb-6">
                <Input type="file" id="picture" name="pictureToUpload" accept="{authorizedImgExtensions.join(',')}"
                       bind:value={value} required />
            </div>
        </div>
    </div>
</form>
