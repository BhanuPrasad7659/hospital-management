/* ======================================================
   COGMEDI PAY THE BILL - INTERACTIVE CHECKOUT & SIMULATION JS
   ====================================================== */

document.addEventListener('DOMContentLoaded', function () {
    console.log('Pay Bill Checkout JS Initialized');

    // 1. PAYMENT METHOD TAB SWITCHING
    const tabBtns = document.querySelectorAll('.tab-btn');
    const methodPanels = document.querySelectorAll('.method-panel');
    const paymentModeInput = document.getElementById('paymentModeInput');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const mode = this.dataset.mode;
            
            // Activate button
            tabBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            // Show panel
            methodPanels.forEach(panel => {
                if (panel.id === `${mode.toLowerCase()}-panel`) {
                    panel.classList.add('active');
                } else {
                    panel.classList.remove('active');
                }
            });

            // Update hidden input
            if (paymentModeInput) {
                paymentModeInput.value = mode;
            }
        });
    });

    // 2. UPI APP DROPDOWN & LOGO CARDS SYNC
    const upiDropdown = document.getElementById('upiAppSelect');
    const logoCards = document.querySelectorAll('.upi-logo-card');
    const qrAppOverlay = document.getElementById('qrAppOverlay');
    const selectedAppLabel = document.getElementById('selectedAppLabel');

    if (upiDropdown) {
        upiDropdown.addEventListener('change', function () {
            const app = this.value;
            selectUpiApp(app);
        });
    }

    logoCards.forEach(card => {
        card.addEventListener('click', function () {
            const app = this.dataset.app;
            if (upiDropdown) {
                upiDropdown.value = app;
            }
            selectUpiApp(app);
        });
    });

    function selectUpiApp(appName) {
        logoCards.forEach(card => {
            if (card.dataset.app === appName) {
                card.classList.add('selected');
            } else {
                card.classList.remove('selected');
            }
        });

        if (qrAppOverlay) {
            qrAppOverlay.textContent = appName;
        }
        if (selectedAppLabel) {
            selectedAppLabel.textContent = appName;
        }
    }

    // 3. UPI MODE TOGGLE (QR vs VPA ID)
    const modeOptions = document.querySelectorAll('.mode-option');
    const qrSection = document.getElementById('upi-qr-section');
    const vpaSection = document.getElementById('upi-vpa-section');

    modeOptions.forEach(opt => {
        opt.addEventListener('click', function () {
            modeOptions.forEach(o => o.classList.remove('active'));
            this.classList.add('active');

            const target = this.dataset.target;
            if (target === 'qr') {
                if (qrSection) qrSection.style.display = 'flex';
                if (vpaSection) vpaSection.style.display = 'none';
            } else {
                if (qrSection) qrSection.style.display = 'none';
                if (vpaSection) vpaSection.style.display = 'flex';
            }
        });
    });

    // 4. VPA VERIFY SIMULATION
    const verifyVpaBtn = document.getElementById('btnVerifyVpa');
    const vpaInput = document.getElementById('upiIdInput');
    const vpaStatus = document.getElementById('vpaStatus');

    if (verifyVpaBtn) {
        verifyVpaBtn.addEventListener('click', function () {
            const val = vpaInput ? vpaInput.value.trim() : '';
            if (!val) {
                alert('Please enter a valid UPI ID (e.g. mobile@ybl)');
                return;
            }
            this.textContent = 'Verifying...';
            setTimeout(() => {
                this.textContent = 'Verified ✓';
                if (vpaStatus) {
                    vpaStatus.style.display = 'flex';
                }
            }, 600);
        });
    }

    // 5. LIVE CASH CHANGE CALCULATOR
    const cashInput = document.getElementById('cashReceivedInput');
    const totalAmount = parseFloat(document.getElementById('totalPayableAmount')?.value || '0');
    const changeAmountDisplay = document.getElementById('changeAmountDisplay');
    const changeBanner = document.getElementById('changeBanner');
    const cashPills = document.querySelectorAll('.cash-pill');

    if (cashInput) {
        cashInput.addEventListener('input', function () {
            calculateCashChange(parseFloat(this.value || '0'));
        });
    }

    cashPills.forEach(pill => {
        pill.addEventListener('click', function () {
            const add = parseFloat(this.dataset.add || '0');
            const mode = this.dataset.mode;
            let current = parseFloat(cashInput.value || '0');

            if (mode === 'exact') {
                cashInput.value = totalAmount;
            } else {
                cashInput.value = current + add;
            }
            calculateCashChange(parseFloat(cashInput.value));
        });
    });

    function calculateCashChange(received) {
        if (!changeAmountDisplay || !changeBanner) return;

        if (isNaN(received) || received <= 0) {
            changeAmountDisplay.textContent = '₹ 0.00';
            changeBanner.classList.remove('insufficient');
            return;
        }

        const change = received - totalAmount;
        if (change >= 0) {
            changeAmountDisplay.textContent = `₹ ${change.toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            changeBanner.classList.remove('insufficient');
            changeBanner.querySelector('.change-label').textContent = 'CHANGE TO RETURN TO PATIENT';
        } else {
            const needed = Math.abs(change);
            changeAmountDisplay.textContent = `₹ ${needed.toLocaleString('en-IN', { minimumFractionDigits: 2 })}`;
            changeBanner.classList.add('insufficient');
            changeBanner.querySelector('.change-label').textContent = 'SHORTAGE / DUE AMOUNT';
        }
    }

    // 6. CARD BRAND DETECTION
    const cardNumberInput = document.getElementById('cardNumberInput');
    const detectedBrand = document.getElementById('detectedBrand');

    if (cardNumberInput) {
        cardNumberInput.addEventListener('input', function (e) {
            let val = e.target.value.replace(/\D/g, '');
            val = val.substring(0, 16);
            
            // Format into 4-digit chunks
            const formatted = val.match(/.{1,4}/g)?.join(' ') || val;
            e.target.value = formatted;

            // Brand detection logic
            if (detectedBrand) {
                if (val.startsWith('4')) {
                    detectedBrand.textContent = 'VISA';
                } else if (/^5[1-5]/.test(val)) {
                    detectedBrand.textContent = 'MASTERCARD';
                } else if (/^(60|65|81|82)/.test(val)) {
                    detectedBrand.textContent = 'RUPAY';
                } else if (/^3[47]/.test(val)) {
                    detectedBrand.textContent = 'AMEX';
                } else if (val.length > 0) {
                    detectedBrand.textContent = 'CARD';
                } else {
                    detectedBrand.textContent = 'DETECTING';
                }
            }
        });
    }

    // 7. REAL-TIME PAYMENT PROCESSING MODAL SIMULATION
    const payForm = document.getElementById('payBillForm');
    const modalOverlay = document.getElementById('paymentModalOverlay');
    const step1 = document.getElementById('step1');
    const step2 = document.getElementById('step2');
    const step3 = document.getElementById('step3');

    if (payForm) {
        payForm.addEventListener('submit', function (e) {
            e.preventDefault();

            // Validate cash if cash mode
            if (paymentModeInput && paymentModeInput.value === 'CASH') {
                const received = parseFloat(cashInput?.value || '0');
                if (received < totalAmount) {
                    if (!confirm(`Cash received (₹${received}) is less than total bill amount (₹${totalAmount}). Do you still want to proceed?`)) {
                        return;
                    }
                }
            }

            // Show Real-time simulation modal
            if (modalOverlay) {
                modalOverlay.classList.add('active');
            }

            // Step 1: Connecting
            setTimeout(() => {
                if (step1) {
                    step1.classList.remove('active');
                    step1.classList.add('completed');
                    step1.querySelector('.step-dot').textContent = '✓';
                }
                if (step2) {
                    step2.classList.add('active');
                }
            }, 800);

            // Step 2: Authorizing
            setTimeout(() => {
                if (step2) {
                    step2.classList.remove('active');
                    step2.classList.add('completed');
                    step2.querySelector('.step-dot').textContent = '✓';
                }
                if (step3) {
                    step3.classList.add('active');
                }
            }, 1800);

            // Step 3: Complete & Submit via API
            setTimeout(() => {
                const billingRecordId = payForm.querySelector("[name='billingRecordId']").value;
                fetch("/api/billing/pay/" + billingRecordId, {
                    method: "PUT"
                })
                .then(res => {
                    if (!res.ok) {
                        throw new Error("Failed to process payment via API.");
                    }
                    return res.json();
                })
                .then(data => {
                    if (step3) {
                        step3.classList.remove('active');
                        step3.classList.add('completed');
                    }
                    alert("Payment processed successfully via Billing API!");
                    window.location.href = "/billing/print-invoice?billingRecordId=" + billingRecordId;
                })
                .catch(err => {
                    alert(err.message);
                    if (modalOverlay) {
                        modalOverlay.classList.remove('active');
                    }
                });
            }, 2600);
        });
    }
});
