<script>
    import "../app.postcss";
    import { Navbar, NavBrand, NavLi, NavUl, NavHamburger } from "flowbite-svelte";
</script>

<Navbar>
    <NavBrand href="/">
        <img src="/images/site/nav-logo.jpg" class="mr-3 h-6 sm:h-9" alt="Auctioneer Logo" />
        <span class="self-center whitespace-nowrap text-xl font-semibold dark:text-white">Auctioneer</span>
    </NavBrand>
    <NavHamburger />
    <NavUl>
        <NavLi href="/">Home</NavLi>
        <NavLi href="/about">About</NavLi>
        <NavLi href="/contact">Contact</NavLi>
    </NavUl>
</Navbar>

<div class="container mx-auto px-4">
    <slot />
</div>

