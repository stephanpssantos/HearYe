window.customMethods = {
    carouselTo: function (carouselElementId, index) {
        let carouselElement = document.querySelector(carouselElementId);
        let carousel = bootstrap.Carousel.getOrCreateInstance(carouselElement);
        carousel.to(index);
    }
}