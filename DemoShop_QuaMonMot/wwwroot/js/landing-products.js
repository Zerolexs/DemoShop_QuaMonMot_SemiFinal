(function () {
    const formatPrice = (value) => `$${Number(value || 0).toFixed(2)}`;

    const updateCard = (card, product) => {
        const image = card.querySelector(".product-img img");
        const title = card.querySelector(".text-center a.h6");
        const price = card.querySelector(".text-center h5");
        const oldPrice = card.querySelector(".text-center del");
        const actions = card.querySelectorAll(".product-action a");

        if (image) {
            image.src = product.imageUrl;
            image.alt = product.name;
            image.onerror = function () {
                this.src = "/Hinh/HangHoa/default.jpg";
            };
        }

        if (title) {
            title.textContent = product.name;
            title.href = product.detailUrl;
        }

        if (price) {
            price.textContent = formatPrice(product.price);
        }

        if (oldPrice) {
            oldPrice.textContent = formatPrice(product.oldPrice);
        }

        if (actions[0]) {
            actions[0].href = product.cartUrl;
        }

        if (actions[3]) {
            actions[3].href = product.detailUrl;
        }
    };

    fetch("/Home/LandingProducts")
        .then((response) => response.ok ? response.json() : [])
        .then((products) => {
            if (!Array.isArray(products) || products.length === 0) {
                return;
            }

            document.querySelectorAll(".product-item").forEach((card, index) => {
                const product = products[index % products.length];
                updateCard(card, product);
            });
        })
        .catch(() => {
            // Giữ nguyên dữ liệu mẫu nếu không gọi được API.
        });
})();
