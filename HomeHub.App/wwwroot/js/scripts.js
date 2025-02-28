/*!
* Start Bootstrap - Shop Homepage v5.0.6 (https://startbootstrap.com/template/shop-homepage)
* Copyright 2013-2023 Start Bootstrap
* Licensed under MIT (https://github.com/StartBootstrap/startbootstrap-shop-homepage/blob/master/LICENSE)
*/
// This file is intentionally blank
// Use this file to add JavaScript to your project

<script>
    document.addEventListener("DOMContentLoaded", function () {
        // Restore the scroll position when the page loads
        if (sessionStorage.getItem("scrollPosition") !== null) {
        window.scrollTo(0, sessionStorage.getItem("scrollPosition"));
        }

        // Save scroll position when user clicks pagination links
        document.querySelectorAll(".pagination-link").forEach(link => {
        link.addEventListener("click", function () {
            sessionStorage.setItem("scrollPosition", window.scrollY);
        });
        });
    });
</script>
