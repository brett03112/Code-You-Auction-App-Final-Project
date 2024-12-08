"use strict";

// Initialize SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/auctionHub")
    .withAutomaticReconnect()
    .build();

// Handle auction start
connection.on("AuctionStarted", (endTime) => {
    // Update all auction timers
    document.querySelectorAll('.auction-timer').forEach(element => {
        element.dataset.endTime = endTime;
        element.dataset.started = "true";
    });

    // Show all bid sections
    document.querySelectorAll('.bid-section').forEach(section => {
        section.style.display = 'block';
    });
});

// Handle bid updates
connection.on("BidUpdated", (dessertId, bidAmount, bidderId, bidderName) => {
    const priceElement = document.querySelector(`[data-dessert-id="${dessertId}"] .price-value`);
    const winnerElement = document.querySelector(`[data-dessert-id="${dessertId}"] .winner-name`);
    if (priceElement) {
        priceElement.textContent = Number(bidAmount).toFixed(2);
    }
    if (winnerElement) {
        winnerElement.textContent = bidderName;
    }
});

connection.start().catch(err => console.error(err));

// Handle bid submission
async function placeBid(dessertId) {
    const bidInput = document.querySelector(`[data-dessert-id="${dessertId}"] .bid-input`);
    const currentPrice = parseFloat(document.querySelector(`[data-dessert-id="${dessertId}"] .current-price`).textContent.replace('$', ''));
    const bidAmount = parseFloat(bidInput.value);

    // Validate bid amount
    if (isNaN(bidAmount) || bidAmount <= currentPrice) {
        alert('Bid must be higher than the current price');
        return;
    }

    // Validate whole dollar amount
    if (bidAmount % 1 !== 0) {
        alert('Bid must be a whole dollar amount');
        return;
    }

    try {
        const response = await fetch('/Auction/PlaceBid', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                dessertId: dessertId,
                bidAmount: bidAmount
            })
        });

        const result = await response.json();
        if (!result.success) {
            alert(result.message || 'Failed to place bid');
        } else {
            bidInput.value = '';
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to place bid');
    }
}

// Handle auction start button click
async function startAuction() {
    try {
        const response = await fetch('/Home/StartAuction', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        const result = await response.json();
        if (!result.success) {
            alert('Failed to start auction');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to start auction');
    }
}

// Update countdown timer
function updateTimer(element) {
    // Check if auction has started
    if (element.dataset.started !== "true") {
        element.textContent = "Auction not started";
        return false;
    }

    const endTime = new Date(element.dataset.endTime).getTime();
    const now = new Date().getTime();
    const distance = endTime - now;

    if (distance <= 0) {
        element.textContent = "Auction Ended";
        // Hide all bid sections when auction ends
        document.querySelectorAll('.bid-section').forEach(section => {
            section.style.display = 'none';
        });
        return false;
    }

    const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((distance % (1000 * 60)) / 1000);

    element.textContent = `${minutes}m ${seconds}s`;
    return true;
}

// Initialize timers
function initializeTimers() {
    const timerElements = document.querySelectorAll('.auction-timer');
    timerElements.forEach(element => {
        updateTimer(element);
        setInterval(() => updateTimer(element), 1000);
    });
}

// Initialize when document is ready
document.addEventListener('DOMContentLoaded', () => {
    initializeTimers();

    // Add event listener for start auction button
    const startButton = document.getElementById('startAuction');
    if (startButton) {
        startButton.addEventListener('click', startAuction);
    }
});
