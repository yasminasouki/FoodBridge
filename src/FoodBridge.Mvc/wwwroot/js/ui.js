// Small progressive-enhancement helpers. Pages work without this file.
(function () {
    "use strict";

    // Show / hide password.
    document.querySelectorAll("[data-toggle-password]").forEach(function (button) {
        var input = button.parentElement.querySelector("input");
        if (!input) {
            return;
        }

        button.addEventListener("click", function () {
            var show = input.type === "password";
            input.type = show ? "text" : "password";
            button.textContent = show ? "Hide" : "Show";
            button.setAttribute("aria-label", show ? "Hide password" : "Show password");
        });
    });

    // Confirm risky actions (cancel, close).
    document.addEventListener("click", function (event) {
        var button = event.target.closest("[data-confirm]");
        if (button && !window.confirm(button.dataset.confirm)) {
            event.preventDefault();
        }
    });

    // Disable the submit button while a valid form is being sent.
    // Runs in the bubble phase so validation has already had its say.
    document.addEventListener("submit", function (event) {
        if (event.defaultPrevented) {
            return;
        }

        var form = event.target;
        var button = form.querySelector("button[type=submit][data-loading-text]");
        if (!button) {
            return;
        }

        setTimeout(function () {
            button.dataset.originalText = button.textContent;
            button.textContent = button.dataset.loadingText;
            button.disabled = true;
        }, 0);
    });

    // Restore buttons when the page comes back from the browser cache.
    window.addEventListener("pageshow", function (event) {
        if (!event.persisted) {
            return;
        }

        document.querySelectorAll("button[data-original-text]").forEach(function (button) {
            button.textContent = button.dataset.originalText;
            button.disabled = false;
        });
    });

    // Click a pickup code to copy it.
    document.addEventListener("click", function (event) {
        var chip = event.target.closest("[data-copy]");
        if (!chip || !navigator.clipboard) {
            return;
        }

        var original = chip.textContent;
        navigator.clipboard.writeText(chip.dataset.copy).then(function () {
            chip.textContent = "Copied";
            chip.classList.add("copied");
            setTimeout(function () {
                chip.textContent = original;
                chip.classList.remove("copied");
            }, 1200);
        }).catch(function () { });
    });
})();
